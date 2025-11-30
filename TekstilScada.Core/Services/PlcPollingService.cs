// File: TekstilScada.Core/Services/PlcPollingService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // KRİTİK: Loglama için eklendi
using TekstilScada.Core;
using TekstilScada.Core.Services;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.Services
{
    public class PlcPollingService
    {
        // --- EVENTLER ---
        public event Action<int, FullMachineStatus> OnMachineDataRefreshed;
        public event Action<int, FullMachineStatus> OnMachineConnectionStateChanged;
        public event Action<int, FullMachineStatus> OnActiveAlarmStateChanged;

        // --- DEPENDENCIES ---
        private readonly AlarmRepository _alarmRepository;
        private readonly ProcessLogRepository _processLogRepository;
        private readonly ProductionRepository _productionRepository;
        private readonly MachineRepository _machinerepository;
        private readonly RecipeRepository _recipeRepository;
        private readonly ILogger<PlcPollingService> _logger; // KRİTİK: Logger Eklendi

        // --- DATA STRUCTURES & CACHE ---
        private ConcurrentDictionary<int, IPlcManager> _plcManagers;
        public ConcurrentDictionary<int, FullMachineStatus> MachineDataCache { get; private set; }
        private ConcurrentDictionary<int, string> _currentBatches;
        private ConcurrentDictionary<int, DateTime> _reconnectAttempts;
        private ConcurrentDictionary<int, ConnectionStatus> _connectionStates;
        private ConcurrentDictionary<int, AlarmDefinition> _alarmDefinitionsCache;
        private ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>> _activeAlarmsTracker;
        private readonly ConcurrentDictionary<int, LiveStepAnalyzer> _liveAnalyzers;
        private readonly ConcurrentDictionary<int, (int machineAlarmSeconds, int operatorPauseSeconds)> _liveAlarmCounters;

        // --- BATCH TRACKING ---
        private readonly ConcurrentDictionary<int, double> _batchTotalTheoreticalTimes;
        private readonly ConcurrentDictionary<int, DateTime> _batchStartTimes;
        private readonly ConcurrentDictionary<int, double> _batchNonProductiveSeconds;

        // --- THREADING & TIMING ---
        private System.Threading.Timer _loggingTimer;
        private CancellationTokenSource _cancellationTokenSource;
        private List<Task> _pollingTasks;
        private readonly object _timerLock = new object(); // KRİTİK: Timer Kilidi

        private readonly int _pollingIntervalMs = 1000;
        private readonly int _loggingIntervalMs = 5000;
        private ConcurrentDictionary<int, DateTime> _lastConnectionTime = new ConcurrentDictionary<int, DateTime>();
        // PlcPollingService sınıfının en başına, diğer Dictionary tanımlarının altına ekleyin:
        private ConcurrentDictionary<int, int> _lastLoggedStepNumber = new ConcurrentDictionary<int, int>();
        // YENİ: Bağlantı sonrası bekleme süresi (Saniye)
        private const int StabilizationSeconds = 5;
        public PlcPollingService(
            AlarmRepository alarmRepository,
            ProcessLogRepository processLogRepository,
            ProductionRepository productionRepository,
            RecipeRepository recipeRepository,
            MachineRepository machineRepository,
            ILogger<PlcPollingService> logger) // Dependency Injection
        {
            _alarmRepository = alarmRepository;
            _processLogRepository = processLogRepository;
            _productionRepository = productionRepository;
            _recipeRepository = recipeRepository;
            _machinerepository = machineRepository;
            _logger = logger;

            // Koleksiyonların Başlatılması
            _plcManagers = new ConcurrentDictionary<int, IPlcManager>();
            MachineDataCache = new ConcurrentDictionary<int, FullMachineStatus>();
            _reconnectAttempts = new ConcurrentDictionary<int, DateTime>();
            _connectionStates = new ConcurrentDictionary<int, ConnectionStatus>();
            _activeAlarmsTracker = new ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>>();
            _currentBatches = new ConcurrentDictionary<int, string>();
            _liveAnalyzers = new ConcurrentDictionary<int, LiveStepAnalyzer>();
            _liveAlarmCounters = new ConcurrentDictionary<int, (int, int)>();
            _pollingTasks = new List<Task>();

            _batchTotalTheoreticalTimes = new ConcurrentDictionary<int, double>();
            _batchStartTimes = new ConcurrentDictionary<int, DateTime>();
            _batchNonProductiveSeconds = new ConcurrentDictionary<int, double>();
        }

        public void Start(List<Models.Machine> machines)
        {
            Stop(); // Temiz başlangıç için önce durdur
            _cancellationTokenSource = new CancellationTokenSource();

            LoadAlarmDefinitionsCache();

            foreach (var machine in machines)
            {
                try
                {
                    var plcManager = PlcManagerFactory.Create(machine);
                    _plcManagers.TryAdd(machine.Id, plcManager);

                    // Varsayılan durumları ayarla
                    _connectionStates.TryAdd(machine.Id, ConnectionStatus.Disconnected);
                    MachineDataCache.TryAdd(machine.Id, new FullMachineStatus
                    {
                        MachineId = machine.Id,
                        MachineName = machine.MachineName,
                        ConnectionState = ConnectionStatus.Disconnected
                    });
                    _activeAlarmsTracker.TryAdd(machine.Id, new ConcurrentDictionary<int, DateTime>());
                    _currentBatches.TryAdd(machine.Id, null);

                    // Task Başlatma
                    var machineTask = Task.Run(() => PollMachineLoop(machine, plcManager, _cancellationTokenSource.Token));
                    _pollingTasks.Add(machineTask);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Makine başlatılırken hata oluştu: {MachineId} - {MachineName}", machine.Id, machine.MachineName);
                }
            }

            // KRİTİK: Timer'ı Infinite modda başlatıyoruz (Manuel tetikleme)
            _loggingTimer = new System.Threading.Timer(LoggingTimer_Tick, null, 1000, Timeout.Infinite);

            _logger.LogInformation("{Count} makine için Polling Servisi başlatıldı.", machines.Count);
        }

        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();

                try
                {
                    // Taskların bitmesini bekle (3 saniye timeout)
                    Task.WaitAll(_pollingTasks.ToArray(), 3000);
                }
                catch (AggregateException) { /* Beklenen iptal hataları */ }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Polling görevleri durdurulurken hata oluştu.");
                }

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // Timer Temizliği
            lock (_timerLock)
            {
                if (_loggingTimer != null)
                {
                    _loggingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _loggingTimer.Dispose();
                    _loggingTimer = null;
                }
            }

            // PLC Bağlantılarını Kapat
            if (_plcManagers != null && !_plcManagers.IsEmpty)
            {
                foreach (var manager in _plcManagers.Values)
                {
                    try { manager.Disconnect(); } catch { /* Yut */ }
                }
            }

            _plcManagers?.Clear();
            MachineDataCache?.Clear();
            _connectionStates?.Clear();
            _activeAlarmsTracker?.Clear();
            _currentBatches?.Clear();
            _pollingTasks?.Clear();

            _logger.LogInformation("Polling Servisi durduruldu.");
        }

        private async Task PollMachineLoop(Machine machine, IPlcManager manager, CancellationToken token)
        {
            // Ana Döngü
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!MachineDataCache.TryGetValue(machine.Id, out var status))
                    {
                        await Task.Delay(1000, token);
                        continue;
                    }

                    if (status.ConnectionState != ConnectionStatus.Connected)
                    {
                        await HandleReconnectionAsync(machine.Id, manager);
                    }
                    else
                    {
                        // ASENKRON OKUMA
                        var readResult = await manager.ReadLiveStatusDataAsync();

                        if (readResult.IsSuccess)
                        {
                            var newStatus = readResult.Content;
                            newStatus.MachineId = machine.Id;
                            newStatus.MachineName = status.MachineName;
                            newStatus.ConnectionState = ConnectionStatus.Connected;
                            newStatus.AktifAdimAdi = GetStepTypeName(newStatus.AktifAdimTipiWordu);

                            // --- Analiz ve Hesaplamalar ---
                            PerformLiveAnalysis(machine.Id, newStatus);

                            // --- İş Mantığı ---
                            ProcessLiveStepAnalysis(machine.Id, newStatus);
                            CheckAndLogBatchStartAndEnd(machine.Id, newStatus);
                            CheckAndLogAlarms(machine.Id, newStatus);

                            status = newStatus;

                            // --- OEE Sayaçları ---
                            UpdateLiveCounters(machine.Id, newStatus);
                        }
                        else
                        {
                            HandleDisconnection(machine.Id);
                            if (MachineDataCache.ContainsKey(machine.Id))
                                status = MachineDataCache[machine.Id];
                        }
                    }

                    // Cache Güncelleme ve UI Bildirimi
                    if (MachineDataCache.ContainsKey(machine.Id))
                    {
                        MachineDataCache[machine.Id] = status;
                    }
                    OnMachineDataRefreshed?.Invoke(machine.Id, status);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Makine döngü hatası: {MachineId}", machine.Id);
                }

                // Asenkron Bekleme (CPU Dostu)
                try { await Task.Delay(_pollingIntervalMs, token); }
                catch (OperationCanceledException) { break; }
            }
        }

        // --- TIMER & LOGLAMA (KRİTİK OPTİMİZASYON) ---
        private void LoggingTimer_Tick(object state)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested) return;

            // Timer'ı Kilitle ve Durdur
            lock (_timerLock)
            {
                if (_loggingTimer == null) return;
                _loggingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            try
            {
                var batchLogList = new List<FullMachineStatus>();
                var manualLogList = new List<FullMachineStatus>();

                // Thread-Safe Okuma
                foreach (var machineStatus in MachineDataCache.Values)
                {
                    if (machineStatus.ConnectionState == ConnectionStatus.Connected)
                    {
                        if (machineStatus.IsInRecipeMode)
                            batchLogList.Add(machineStatus);
                        else
                            manualLogList.Add(machineStatus);
                    }
                }

                // DB Yazma İşlemleri
                if (batchLogList.Count > 0)
                {
                    _processLogRepository.LogBulkData(batchLogList);
                }

                if (manualLogList.Count > 0)
                {
                    _processLogRepository.LogBulkManualData(manualLogList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu loglama sırasında hata oluştu.");
            }
            finally
            {
                // Timer'ı Tekrar Başlat
                lock (_timerLock)
                {
                    if (_loggingTimer != null && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        _loggingTimer.Change(_loggingIntervalMs, Timeout.Infinite);
                    }
                }
            }
        }

        // --- YARDIMCI METOTLAR ---

        private void PerformLiveAnalysis(int machineId, FullMachineStatus newStatus)
        {
            var analyzer = _liveAnalyzers.TryGetValue(machineId, out var a) ? a : null;

            if (newStatus.IsInRecipeMode && analyzer != null &&
                _batchTotalTheoreticalTimes.TryGetValue(machineId, out double totalTheoreticalTime) && totalTheoreticalTime > 0)
            {
                double timeInCurrentStep = (DateTime.Now - analyzer.CurrentStepStartTime).TotalSeconds;
                var remainingSteps = analyzer.Recipe.Steps.Where(s => s.StepNumber >= newStatus.AktifAdimNo);
                double remainingTheoreticalTime = RecipeAnalysis.CalculateTheoreticalTimeForSteps(remainingSteps);
                double completedStepsTime = totalTheoreticalTime - remainingTheoreticalTime;
                double totalProgressSeconds = completedStepsTime + timeInCurrentStep;
                double percentage = (totalProgressSeconds / totalTheoreticalTime) * 100.0;
                newStatus.ProsesYuzdesi = (short)Math.Min(100.0, Math.Max(0.0, percentage));
            }
            else
            {
                newStatus.ProsesYuzdesi = 0;
            }
        }

        private void UpdateLiveCounters(int machineId, FullMachineStatus newStatus)
        {
            if (_currentBatches.TryGetValue(machineId, out var activeBatch) && activeBatch != null)
            {
                if (_liveAlarmCounters.TryGetValue(machineId, out var counters))
                {
                    if (newStatus.HasActiveAlarm)
                    {
                        counters.machineAlarmSeconds += _pollingIntervalMs / 1000;
                    }
                    else if (newStatus.IsPaused)
                    {
                        counters.operatorPauseSeconds += _pollingIntervalMs / 1000;
                    }
                    _liveAlarmCounters[machineId] = counters;
                }
            }
        }

        private async Task HandleReconnectionAsync(int machineId, IPlcManager manager)
        {
            if (!_reconnectAttempts.ContainsKey(machineId) || (DateTime.UtcNow - _reconnectAttempts[machineId]).TotalSeconds > 10)
            {
                _reconnectAttempts[machineId] = DateTime.UtcNow;

                if (!MachineDataCache.TryGetValue(machineId, out var status)) return;

                status.ConnectionState = ConnectionStatus.Connecting;
                status.ProsesYuzdesi = 0;
                _connectionStates[machineId] = ConnectionStatus.Connecting;
                OnMachineConnectionStateChanged?.Invoke(machineId, status);

                try
                {
                    var connectResult = await manager.ConnectAsync();

                    if (connectResult.IsSuccess)
                    {
                        status.ConnectionState = ConnectionStatus.Connected;
                        _connectionStates[machineId] = ConnectionStatus.Connected;
                        _lastConnectionTime[machineId] = DateTime.Now;
                        _reconnectAttempts.TryRemove(machineId, out _);
                        OnMachineConnectionStateChanged?.Invoke(machineId, status);

                        _logger.LogInformation("Makine {MachineId} bağlantısı yeniden sağlandı.", machineId);

                        LiveEventAggregator.Instance.Publish(new LiveEvent
                        {
                            Timestamp = DateTime.Now,
                            Source = status.MachineName,
                            Message = "Connection re-established.",
                            Type = EventType.SystemSuccess
                        });
                    }
                    else
                    {
                        status.ConnectionState = ConnectionStatus.Disconnected;
                        _connectionStates[machineId] = ConnectionStatus.Disconnected;
                        OnMachineConnectionStateChanged?.Invoke(machineId, status);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Makine {MachineId} yeniden bağlanma hatası.", machineId);
                }
            }
        }

        private void HandleDisconnection(int machineId)
        {
            if (!MachineDataCache.TryGetValue(machineId, out var status)) return;

            status.ConnectionState = ConnectionStatus.ConnectionLost;
            status.ProsesYuzdesi = 0;
            _connectionStates[machineId] = ConnectionStatus.ConnectionLost;
            _reconnectAttempts.TryAdd(machineId, DateTime.UtcNow);

            OnMachineConnectionStateChanged?.Invoke(machineId, status);
            LiveEventAggregator.Instance.Publish(new LiveEvent { Source = status.MachineName, Message = "Connection lost!", Type = EventType.SystemWarning });
        }

        private async void CheckAndLogBatchStartAndEnd(int machineId, FullMachineStatus currentStatus)
        {
            try
            {
                // 1. KARARLILIK KONTROLÜ
                // Bağlantı yoksa veya yeni geldiyse (ilk 5 sn) Batch durumunu değiştirme.
                bool isSystemStable = true;
                if (currentStatus.ConnectionState != ConnectionStatus.Connected) isSystemStable = false;

                if (_lastConnectionTime.TryGetValue(machineId, out DateTime connectTime))
                {
                    if ((DateTime.Now - connectTime).TotalSeconds < StabilizationSeconds) isSystemStable = false;
                }

                _currentBatches.TryGetValue(machineId, out string lastTrackedBatchId);

                // --- SENARYO 1: YENİ BATCH BAŞLANGICI ---
                if (currentStatus.IsInRecipeMode && !string.IsNullOrEmpty(currentStatus.BatchNumarasi) && currentStatus.BatchNumarasi != lastTrackedBatchId)
                {
                    // Eğer sistem kararsızsa yeni batch başlatma, bir sonraki döngüyü bekle.
                    if (!isSystemStable) return;

                    // Önceki batch varsa bitir
                    if (lastTrackedBatchId != null)
                    {
                        if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                        {
                            var lastStep = analyzer.GetLastCompletedStep();
                            if (lastStep != null && lastStep.WorkingTime == "Processing...")
                            {
                                analyzer.FinalizeStep(lastStep.StepNumber, lastTrackedBatchId, machineId);
                            }
                        }

                        int actualProducedQuantity = currentStatus.ActualQuantityProduction;
                        _liveAlarmCounters.TryGetValue(machineId, out var finalCounters);
                        int totalDowntimeFromScada = finalCounters.machineAlarmSeconds + finalCounters.operatorPauseSeconds;
                        _batchTotalTheoreticalTimes.TryGetValue(machineId, out double theoreticalTime);

                        _productionRepository.EndBatch(
                          machineId, lastTrackedBatchId, currentStatus,
                          finalCounters.machineAlarmSeconds, finalCounters.operatorPauseSeconds,
                          actualProducedQuantity, totalDowntimeFromScada, theoreticalTime);
                    }

                    // Yeni batch başlat
                    _currentBatches[machineId] = currentStatus.BatchNumarasi;
                    _productionRepository.StartNewBatch(currentStatus);

                    if (_plcManagers.TryGetValue(machineId, out var plcManager))
                    {
                        var recipeReadResult = await plcManager.ReadFullRecipeDataAsync();
                        if (recipeReadResult.IsSuccess && recipeReadResult.Content != null)
                        {
                            var fullRecipe = recipeReadResult.Content;
                            fullRecipe.RecipeName = currentStatus.RecipeName;
                            _liveAnalyzers[machineId] = new LiveStepAnalyzer(fullRecipe, _productionRepository);

                            double totalSeconds = RecipeAnalysis.CalculateTotalTheoreticalTimeSeconds(fullRecipe);
                            _batchTotalTheoreticalTimes[machineId] = totalSeconds;
                            _batchStartTimes[machineId] = DateTime.Now;
                            _batchNonProductiveSeconds[machineId] = 0;

                            _productionRepository.SaveBatchRecipe(machineId, currentStatus.BatchNumarasi, fullRecipe);
                        }
                    }
                }
                // --- SENARYO 2: BATCH BİTİŞİ ---
                else if (!currentStatus.IsInRecipeMode && lastTrackedBatchId != null)
                {
                    // KRİTİK NOKTA:
                    // Eğer sistem kararsızsa (bağlantı anlık gidip geldiyse),
                    // "IsInRecipeMode = false" bilgisini görmezden gel ve Batch'i BİTİRME.
                    if (!isSystemStable) return;

                    if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                    {
                        var lastStep = analyzer.GetLastCompletedStep();
                        if (lastStep != null && lastStep.WorkingTime == "Processing...")
        {
             // Eğer bu adım zaten ProcessLiveStepAnalysis tarafından az önce kaydedildiyse tekrar kaydetme
             _lastLoggedStepNumber.TryGetValue(machineId, out int lastLoggedNo);
             
             if (lastLoggedNo != lastStep.StepNumber)
             {
                 analyzer.FinalizeStep(lastStep.StepNumber, lastTrackedBatchId, machineId);
                 // FinalizeStep zaten LogSingleStepDetail çağırıyorsa burası tamamdır, 
                 // çağırmıyorsa (ki LiveStepAnalyzer yapısına bağlı) burada loglama yapılıyor olabilir.
                 // Önemli olan çift çağırmamak.
             }
        }
                    }

                    int actualProducedQuantity = currentStatus.ActualQuantityProduction;
                    _liveAlarmCounters.TryGetValue(machineId, out var finalCounters);
                    int totalDowntimeFromScada = finalCounters.machineAlarmSeconds + finalCounters.operatorPauseSeconds;
                    _batchTotalTheoreticalTimes.TryGetValue(machineId, out double theoreticalTime);

                    _productionRepository.EndBatch(
                      machineId, lastTrackedBatchId, currentStatus,
                      finalCounters.machineAlarmSeconds, finalCounters.operatorPauseSeconds,
                      actualProducedQuantity, totalDowntimeFromScada, theoreticalTime);

                    _currentBatches[machineId] = null;
                    _liveAlarmCounters.TryRemove(machineId, out _);
                    _liveAnalyzers.TryRemove(machineId, out _);
                    _batchTotalTheoreticalTimes.TryRemove(machineId, out _);
                    _batchStartTimes.TryRemove(machineId, out _);
                    _batchNonProductiveSeconds.TryRemove(machineId, out _);

                    if (_plcManagers.TryGetValue(machineId, out var plcManager))
                    {
                        _ = Task.Run(async () => {
                            try
                            {
                                var summaryResult = await plcManager.ReadBatchSummaryDataAsync();
                                if (summaryResult.IsSuccess)
                                {
                                    _productionRepository.UpdateBatchSummary(machineId, lastTrackedBatchId, summaryResult.Content);
                                }
                                await plcManager.IncrementProductionCounterAsync();
                                await plcManager.ResetOeeCountersAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Batch bitişi asenkron işlemlerinde hata: {MachineId}", machineId);
                            }
                        });
                    }
                }
                _lastLoggedStepNumber.TryRemove(machineId, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch takibi sırasında hata: {MachineId}", machineId);
            }
        }
        private void ProcessLiveStepAnalysis(int machineId, FullMachineStatus currentStatus)
        {
            try
            {
                // 1. GÜVENLİK
                if (!currentStatus.IsInRecipeMode || string.IsNullOrEmpty(currentStatus.BatchNumarasi)) return;

                // 2. KARARLILIK KONTROLÜ
                if (_lastConnectionTime.TryGetValue(machineId, out DateTime connectTime))
                {
                    if ((DateTime.Now - connectTime).TotalSeconds < StabilizationSeconds) return;
                }

                if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                {
                    // Analyzer veriyi işlesin
                    if (analyzer.ProcessData(currentStatus))
                    {
                        var completedStepAnalysis = analyzer.GetLastCompletedStep();

                        if (completedStepAnalysis != null)
                        {
                            // --- HATA DÜZELTMESİ BURADA ---
                            // 'WorkingTime' string olduğu için (örn: "00:00:05") önce TimeSpan'a çeviriyoruz.
                            // Çeviremezse veya süre 3 saniyeden kısaysa (glitch) kaydetmiyoruz.
                            if (TimeSpan.TryParse(completedStepAnalysis.WorkingTime, out TimeSpan duration))
                            {
                                if (duration.TotalSeconds < 3)
                                {
                                    _logger.LogWarning($"Makine {machineId} - Kısa süreli parazit adım ({duration.TotalSeconds} sn) atlandı. Adım: {completedStepAnalysis.StepNumber}");
                                    return;
                                }
                            }
                            else
                            {
                                // Format bozuksa veya "Processing..." kaldıysa da kaydetme
                                return;
                            }

                            // --- ÇİFT KAYIT ÖNLEME ---
                            _lastLoggedStepNumber.TryGetValue(machineId, out int lastStepNo);

                            if (lastStepNo == completedStepAnalysis.StepNumber)
                            {
                                // Zaten kaydedilmiş
                                return;
                            }

                            // Veritabanına kaydet
                            _productionRepository.LogSingleStepDetail(completedStepAnalysis, machineId, currentStatus.BatchNumarasi);

                            // Son kaydedilen olarak işaretle
                            _lastLoggedStepNumber[machineId] = completedStepAnalysis.StepNumber;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adım analizi hatası: {MachineId}", machineId);
            }
        }

        private void CheckAndLogAlarms(int machineId, FullMachineStatus currentStatus)
        {
            try
            {
                // 1. GÜVENLİK KONTROLÜ: Bağlantı durumu
                if (currentStatus.ConnectionState != ConnectionStatus.Connected) return;

                // 2. KARARLILIK KONTROLÜ: Bağlantı yeni mi geldi?
                bool isSystemStable = true;
                if (_lastConnectionTime.TryGetValue(machineId, out DateTime connectTime))
                {
                    // Eğer bağlantı kurulalı 5 saniyeden az olduysa sistem kararsızdır.
                    if ((DateTime.Now - connectTime).TotalSeconds < StabilizationSeconds)
                    {
                        isSystemStable = false;
                    }
                }

                // Alarm takip listesini al veya oluştur
                if (_activeAlarmsTracker == null || !_activeAlarmsTracker.TryGetValue(machineId, out var machineActiveAlarms))
                {
                    _activeAlarmsTracker?.TryAdd(machineId, new ConcurrentDictionary<int, DateTime>());
                    return;
                }

                MachineDataCache.TryGetValue(machineId, out var previousStatus);
                int previousAlarmNumber = previousStatus?.ActiveAlarmNumber ?? 0;
                int currentAlarmNumber = currentStatus.ActiveAlarmNumber;

                // --- ALARM BAŞLANGIÇ MANTIĞI (Her zaman çalışır) ---
                // Yeni bir alarm geldiyse bunu hemen kaydetmek isteriz, beklemeye gerek yok.
                if (currentAlarmNumber > 0)
                {
                    if (!machineActiveAlarms.ContainsKey(currentAlarmNumber) && _alarmDefinitionsCache.TryGetValue(currentAlarmNumber, out var newAlarmDef))
                    {
                        _alarmRepository.WriteAlarmHistoryEvent(machineId, newAlarmDef.Id, "ACTIVE");
                        LiveEventAggregator.Instance.PublishAlarm(currentStatus.MachineName, newAlarmDef.AlarmText);
                    }
                    // Alarmın son görülme zamanını güncelle
                    machineActiveAlarms[currentAlarmNumber] = DateTime.Now;
                }

                // --- ALARM BİTİŞ/SİLME MANTIĞI (Sadece Kararlıysa Çalışır) ---
                // Eğer bağlantı yeni geldiyse (isSystemStable = false), alarm bitirme işlemlerini yapma!
                // Çünkü PLC ilk açılışta yanlışlıkla '0' (Alarm Yok) göndermiş olabilir.
                if (isSystemStable)
                {
                    // A) Zaman aşımına uğrayan alarmları temizle (30 saniye boyunca gelmeyenler)
                    var timedOutAlarms = machineActiveAlarms.Where(kvp => (DateTime.Now - kvp.Value).TotalSeconds > 30).ToList();
                    foreach (var timedOutAlarm in timedOutAlarms)
                    {
                        if (_alarmDefinitionsCache.TryGetValue(timedOutAlarm.Key, out var oldAlarmDef))
                        {
                            _alarmRepository.WriteAlarmHistoryEvent(machineId, oldAlarmDef.Id, "INACTIVE");
                            LiveEventAggregator.Instance.Publish(new LiveEvent { Type = EventType.SystemInfo, Source = currentStatus.MachineName, Message = $"{oldAlarmDef.AlarmText} - CLEARED" });
                        }
                        machineActiveAlarms.TryRemove(timedOutAlarm.Key, out _);
                    }

                    // B) Eğer şu an alarm '0' geldiyse ve listemiz doluysa, muhtemelen operatör resetledi. Hepsini temizle.
                    if (currentAlarmNumber == 0 && !machineActiveAlarms.IsEmpty)
                    {
                        foreach (var activeAlarm in machineActiveAlarms)
                        {
                            if (_alarmDefinitionsCache.TryGetValue(activeAlarm.Key, out var oldAlarmDef))
                            {
                                _alarmRepository.WriteAlarmHistoryEvent(machineId, oldAlarmDef.Id, "INACTIVE");
                            }
                        }
                        machineActiveAlarms.Clear();
                    }
                }

                // --- DURUM GÜNCELLEME ---
                currentStatus.HasActiveAlarm = !machineActiveAlarms.IsEmpty;
                if (currentStatus.HasActiveAlarm)
                {
                    // En son gelen alarmı ana ekranda göster
                    currentStatus.ActiveAlarmNumber = machineActiveAlarms.OrderByDescending(kvp => kvp.Value).First().Key;
                    if (_alarmDefinitionsCache.TryGetValue(currentStatus.ActiveAlarmNumber, out var def))
                    {
                        currentStatus.ActiveAlarmText = def.AlarmText;
                    }
                    else
                    {
                        currentStatus.ActiveAlarmText = $"UNDEFINED ALARM ({currentStatus.ActiveAlarmNumber})";
                    }
                }
                else
                {
                    currentStatus.ActiveAlarmNumber = 0;
                    currentStatus.ActiveAlarmText = "";
                }

                // Değişiklik varsa event fırlat
                if ((previousStatus?.HasActiveAlarm ?? false) != currentStatus.HasActiveAlarm || previousAlarmNumber != currentStatus.ActiveAlarmNumber)
                {
                    OnActiveAlarmStateChanged?.Invoke(machineId, currentStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alarm işleme hatası: {MachineId}", machineId);
            }
        }

        private void LoadAlarmDefinitionsCache()
        {
            try
            {
                var definitions = _alarmRepository.GetAllAlarmDefinitions();
                _alarmDefinitionsCache = new ConcurrentDictionary<int, AlarmDefinition>(
                  definitions.ToDictionary(def => def.AlarmNumber, def => def)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alarm tanımları yüklenirken hata oluştu.");
                _alarmDefinitionsCache = new ConcurrentDictionary<int, AlarmDefinition>();
            }
        }

        private string GetStepTypeName(short controlWord)
        {
            var stepTypes = new List<string>();
            // TODO: Bu stringler localization servisinden çekilmeli
            if ((controlWord & 1) != 0) stepTypes.Add("Su Alma");
            if ((controlWord & 2) != 0) stepTypes.Add("Isıtma");
            if ((controlWord & 4) != 0) stepTypes.Add("Çalışma");
            if ((controlWord & 8) != 0) stepTypes.Add("Dozaj");
            if ((controlWord & 16) != 0) stepTypes.Add("Boşaltma");
            if ((controlWord & 32) != 0) stepTypes.Add("Sıkma");
            if ((controlWord & 64) != 0) stepTypes.Add("Nemli Çalışma");
            if ((controlWord & 128) != 0) stepTypes.Add("Zamanlı Çalışma");
            if ((controlWord & 256) != 0) stepTypes.Add("Nem/Zaman Çalışma");
            if ((controlWord & 512) != 0) stepTypes.Add("Soğutma");

            return stepTypes.Any() ? string.Join(" + ", stepTypes) : "Bekliyor...";
        }

        public List<AlarmDefinition> GetActiveAlarmsForMachine(int machineId)
        {
            var activeAlarms = new List<AlarmDefinition>();
            if (_activeAlarmsTracker.TryGetValue(machineId, out var machineActiveAlarms) && !machineActiveAlarms.IsEmpty)
            {
                foreach (var alarmNumber in machineActiveAlarms.Keys)
                {
                    if (_alarmDefinitionsCache.TryGetValue(alarmNumber, out var alarmDef))
                    {
                        activeAlarms.Add(alarmDef);
                    }
                }
            }
            return activeAlarms.OrderByDescending(a => a.Severity).ThenBy(a => a.AlarmNumber).ToList();
        }

        public Dictionary<int, IPlcManager> GetPlcManagers()
        {
            return new Dictionary<int, IPlcManager>(_plcManagers);
        }
    }
}