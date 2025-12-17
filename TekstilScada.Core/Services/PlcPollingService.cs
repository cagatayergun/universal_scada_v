// File: TekstilScada.Core/Services/PlcPollingService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PlcPollingService> _logger;

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
        private ConcurrentDictionary<int, DateTime> _lastManualLogTime = new ConcurrentDictionary<int, DateTime>();
        // --- YENİ: SCADA Tarafından Oluşturulan Batch ID'leri Tutmak İçin ---
        private ConcurrentDictionary<int, string> _generatedBatchIds;

        // --- BATCH TRACKING ---
        private readonly ConcurrentDictionary<int, double> _batchTotalTheoreticalTimes;
        private readonly ConcurrentDictionary<int, DateTime> _batchStartTimes;
        private readonly ConcurrentDictionary<int, double> _batchNonProductiveSeconds;

        // --- THREADING & TIMING ---
        private System.Threading.Timer _loggingTimer;
        private CancellationTokenSource _cancellationTokenSource;
        private List<Task> _pollingTasks;
        private readonly object _timerLock = new object();

        private readonly int _pollingIntervalMs = 1000;
        private readonly int _loggingIntervalMs = 1000;
        private ConcurrentDictionary<int, DateTime> _lastConnectionTime = new ConcurrentDictionary<int, DateTime>();
        private ConcurrentDictionary<int, int> _lastLoggedStepNumber = new ConcurrentDictionary<int, int>();
        private const int StabilizationSeconds = 5;
      
        private ConcurrentDictionary<int, DateTime> _batchEndDebounceTimers = new ConcurrentDictionary<int, DateTime>();
        //
        public PlcPollingService(
            AlarmRepository alarmRepository,
            ProcessLogRepository processLogRepository,
            ProductionRepository productionRepository,
            RecipeRepository recipeRepository,
            MachineRepository machineRepository,
            ILogger<PlcPollingService> logger)
        {
            _alarmRepository = alarmRepository;
            _processLogRepository = processLogRepository;
            _productionRepository = productionRepository;
            _recipeRepository = recipeRepository;
            _machinerepository = machineRepository;
            _logger = logger;

            _plcManagers = new ConcurrentDictionary<int, IPlcManager>();
            MachineDataCache = new ConcurrentDictionary<int, FullMachineStatus>();
            _reconnectAttempts = new ConcurrentDictionary<int, DateTime>();
            _connectionStates = new ConcurrentDictionary<int, ConnectionStatus>();
            _activeAlarmsTracker = new ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>>();
            _currentBatches = new ConcurrentDictionary<int, string>();
            _liveAnalyzers = new ConcurrentDictionary<int, LiveStepAnalyzer>();
            _liveAlarmCounters = new ConcurrentDictionary<int, (int, int)>();
            _pollingTasks = new List<Task>();

            // YENİ: Sözlüğü başlat
            _generatedBatchIds = new ConcurrentDictionary<int, string>();

            _batchTotalTheoreticalTimes = new ConcurrentDictionary<int, double>();
            _batchStartTimes = new ConcurrentDictionary<int, DateTime>();
            _batchNonProductiveSeconds = new ConcurrentDictionary<int, double>();
        }

        public void Start(List<Models.Machine> machines)
        {
            Stop();
            _cancellationTokenSource = new CancellationTokenSource();

            LoadAlarmDefinitionsCache();

            foreach (var machine in machines)
            {
                try
                {
                    var plcManager = PlcManagerFactory.Create(machine);
                    _plcManagers.TryAdd(machine.Id, plcManager);

                    _connectionStates.TryAdd(machine.Id, ConnectionStatus.Disconnected);
                    MachineDataCache.TryAdd(machine.Id, new FullMachineStatus
                    {
                        MachineId = machine.Id,
                        MachineName = machine.MachineName,
                        ConnectionState = ConnectionStatus.Disconnected
                    });
                    _activeAlarmsTracker.TryAdd(machine.Id, new ConcurrentDictionary<int, DateTime>());
                    _currentBatches.TryAdd(machine.Id, null);

                    var machineTask = Task.Run(() => PollMachineLoop(machine, plcManager, _cancellationTokenSource.Token));
                    _pollingTasks.Add(machineTask);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Makine başlatılırken hata oluştu: {MachineId} - {MachineName}", machine.Id, machine.MachineName);
                }
            }

            _loggingTimer = new System.Threading.Timer(LoggingTimer_Tick, null, 1000, Timeout.Infinite);
            _logger.LogInformation("{Count} makine için Polling Servisi başlatıldı.", machines.Count);
        }

        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                try { Task.WaitAll(_pollingTasks.ToArray(), 3000); }
                catch { }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            lock (_timerLock)
            {
                if (_loggingTimer != null)
                {
                    _loggingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _loggingTimer.Dispose();
                    _loggingTimer = null;
                }
            }

            if (_plcManagers != null && !_plcManagers.IsEmpty)
            {
                foreach (var manager in _plcManagers.Values) { try { manager.Disconnect(); } catch { } }
            }

            _plcManagers?.Clear();
            MachineDataCache?.Clear();
            _connectionStates?.Clear();
            _activeAlarmsTracker?.Clear();
            _currentBatches?.Clear();
            _generatedBatchIds?.Clear(); // Temizlik
            _pollingTasks?.Clear();

            _logger.LogInformation("Polling Servisi durduruldu.");
        }

        private async Task PollMachineLoop(Machine machine, IPlcManager manager, CancellationToken token)
        {
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
                        var readResult = await manager.ReadLiveStatusDataAsync();

                        if (readResult.IsSuccess)
                        {
                            var newStatus = readResult.Content;
                            newStatus.MachineId = machine.Id;
                            newStatus.MachineName = status.MachineName;
                            newStatus.ConnectionState = ConnectionStatus.Connected;
                            newStatus.AktifAdimAdi = GetStepTypeName(newStatus.AktifAdimTipiWordu);

                            // --- BATCH ID YÖNETİMİ (YENİ EKLEME) ---
                            if (newStatus.IsInRecipeMode)
                            {
                                _batchEndDebounceTimers.TryRemove(machine.Id, out _);
                            }
                            else
                            {
                                // Makine "Durdum" diyor. Ama gerçekten durdu mu yoksa sinyal mi gitti?
                                // Eğer halihazırda devam eden bir Batch varsa (ID üretilmişse) hemen bitirme.
                                if (_generatedBatchIds.ContainsKey(machine.Id))
                                {
                                    // İlk kez "Durdum" dediyse zamanı kaydet
                                    var firstStopMoment = _batchEndDebounceTimers.GetOrAdd(machine.Id, DateTime.Now);

                                    // 15 Saniye (veya ihtiyaca göre artırın) boyunca sinyal gelmezse gerçekten bitir.
                                    // Bu süre içinde sinyal "1" olursa yukarıdaki if bloğu bu sayacı siler.
                                    if ((DateTime.Now - firstStopMoment).TotalSeconds < 7)
                                    {
                                        // SİSTEMİ KANDIR: Henüz 15 saniye olmadı, Batch devam ediyor say!
                                        newStatus.IsInRecipeMode = true;
                                    }
                                    else
                                    {
                                        // 15 saniyedir ses yok, gerçekten bitmiş.
                                        // Sayacı temizle ki bir sonraki batch için hazır olsun
                                        _batchEndDebounceTimers.TryRemove(machine.Id, out _);
                                    }
                                }
                            }
                            // ------------------------------------------------

                            // --- BATCH ID YÖNETİMİ (MEVCUT KODUNUZUN GÜNCELLENMİŞ HALİ) ---
                            if (newStatus.IsInRecipeMode)
                            {
                                // Eğer makine çalışıyor (veya biz yukarıda çalışıyor saydık) ama ID yoksa:
                                if (!_generatedBatchIds.TryGetValue(machine.Id, out string currentBatchId))
                                {
                                    // Benzersiz ID oluştur: YYYYMMDDHHmmss_MakineID
                                    currentBatchId = $"{DateTime.Now:yyyyMMddHHmmss}_{machine.Id}";
                                    _generatedBatchIds.TryAdd(machine.Id, currentBatchId);
                                    _logger.LogInformation($"Makine {machine.Id} için yeni Batch ID oluşturuldu: {currentBatchId}");
                                }

                                // Oluşturulan (veya var olan) ID'yi status nesnesine ata
                                newStatus.BatchNumarasi = currentBatchId;
                            }
                            else
                            {
                                // Makine gerçekten durduysa (15 sn bekleme süresi de dolduysa):
                                if (_generatedBatchIds.ContainsKey(machine.Id))
                                {
                                    _generatedBatchIds.TryRemove(machine.Id, out _);
                                }
                                newStatus.BatchNumarasi = "";
                            }
                            // ---------------------------------------

                            PerformLiveAnalysis(machine.Id, newStatus);
                            ProcessLiveStepAnalysis(machine.Id, newStatus);
                            CheckAndLogBatchStartAndEnd(machine.Id, newStatus);
                            CheckAndLogAlarms(machine.Id, newStatus);

                            status = newStatus;
                            UpdateLiveCounters(machine.Id, newStatus);
                        }
                        else
                        {
                            HandleDisconnection(machine.Id);
                            if (MachineDataCache.ContainsKey(machine.Id))
                                status = MachineDataCache[machine.Id];
                        }
                    }

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

                try { await Task.Delay(_pollingIntervalMs, token); }
                catch (OperationCanceledException) { break; }
            }
        }

        // --- DİĞER METOTLAR (DEĞİŞİKLİK YOK) ---

        private void LoggingTimer_Tick(object state)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested) return;

            lock (_timerLock)
            {
                if (_loggingTimer == null) return;
                _loggingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            try
            {
                var batchLogList = new List<FullMachineStatus>();
                var manualLogList = new List<FullMachineStatus>();
                DateTime now = DateTime.Now;

                foreach (var machineStatus in MachineDataCache.Values)
                {
                    if (machineStatus.ConnectionState == ConnectionStatus.Connected)
                    {

                        
                        
                        // --- REÇETE (BATCH) MODU ---
                        if (machineStatus.IsInRecipeMode)
                        {
                            // Batch logları genelde sabit 5sn veya 10sn de bir alınır.
                            // Eğer Batch için de 1sn çok fazlaysa buraya da bir kontrol koyabilirsiniz.
                            // Şimdilik her saniye alacak şekilde bırakıyorum veya mevcut _loggingIntervalMs mantığınıza göre düzenleyebilirsiniz.
                            batchLogList.Add(machineStatus);
                        }
                        // --- MANUEL MOD ---
                        else
                        {
                            // Makinenin son log zamanını al, yoksa MinValue olsun
                            if (!_lastManualLogTime.TryGetValue(machineStatus.MachineId, out DateTime lastLog))
                            {
                                lastLog = DateTime.MinValue;
                            }

                            // MANTIK BURADA:
                            bool shouldLog = false;

                            if (machineStatus.manuel_status) // <-- PLC'den gelen "Manuel Çalışma Aktif" biti
                            {
                                // Manuel Status VARSA: Her 1 saniyede bir log al (Zaten timer 1sn çalışıyor)
                                shouldLog = true;
                            }
                            else
                            {
                                // Manuel Status YOKSA (Bekleme): 7 saniyede bir log al
                                if ((now - lastLog).TotalSeconds >= 7)
                                {
                                    shouldLog = true;
                                }
                            }

                            if (shouldLog)
                            {
                                manualLogList.Add(machineStatus);
                                _lastManualLogTime[machineStatus.MachineId] = now; // Son log zamanını güncelle
                            }
                        }
                    }
                }

                if (batchLogList.Count > 0) _processLogRepository.LogBulkData(batchLogList);
                if (manualLogList.Count > 0) _processLogRepository.LogBulkManualData(manualLogList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu loglama sırasında hata oluştu.");
            }
            finally
            {
                lock (_timerLock)
                {
                    if (_loggingTimer != null && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        // Timer'ı tekrar 1 saniye sonrasına kur
                        _loggingTimer.Change(_loggingIntervalMs, Timeout.Infinite);
                    }
                }
            }
        }


        private void PerformLiveAnalysis(int machineId, FullMachineStatus newStatus)
        {
            var analyzer = _liveAnalyzers.TryGetValue(machineId, out var a) ? a : null;

            if (newStatus.IsInRecipeMode && analyzer != null)
            {
                if (newStatus.AktifAdimDataWords != null)
                {
                    analyzer.SyncActiveStepParameters(newStatus.AktifAdimDataWords, newStatus.AktifAdimNo);
                }

                double totalDynamicDuration = analyzer.RecalculateTotalDuration(newStatus.AktifAdimNo);
                double timeInCurrentStep = (DateTime.Now - analyzer.CurrentStepStartTime).TotalSeconds;
                double completedDuration = analyzer.GetCompletedStepsDuration();
                double totalProgressSeconds = completedDuration + timeInCurrentStep;

                double percentage = 0;
                if (totalDynamicDuration > 0)
                {
                    percentage = (totalProgressSeconds / totalDynamicDuration) * 100.0;
                }
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
                    if (newStatus.HasActiveAlarm) counters.machineAlarmSeconds += _pollingIntervalMs / 1000;
                    else if (newStatus.IsPaused) counters.operatorPauseSeconds += _pollingIntervalMs / 1000;
                    _liveAlarmCounters[machineId] = counters;
                }
            }
        }

        private async Task HandleReconnectionAsync(int machineId, IPlcManager manager)
        {
            // Kural: Son denemenin üzerinden 10 saniye geçti mi?
            if (!_reconnectAttempts.ContainsKey(machineId) || (DateTime.UtcNow - _reconnectAttempts[machineId]).TotalSeconds > 10)
            {
                _reconnectAttempts[machineId] = DateTime.UtcNow;

                if (!MachineDataCache.TryGetValue(machineId, out var status)) return;

                // UI'da "Bağlanıyor..." (Sarı) göstermek istiyorsanız burası kalabilir
                // Ancak "Boşta" (Yeşil) görünmemesi için Connecting durumunun UI'daki karşılığını kontrol edin.
                status.ConnectionState = ConnectionStatus.Connecting;
                _connectionStates[machineId] = ConnectionStatus.Connecting;

                // Bu event UI'da sarı ikon yakar, sorun yok
                OnMachineConnectionStateChanged?.Invoke(machineId, status);

                try
                {
                    // ADIM 1: Eski bağlantı kalıntılarını temizle
                    try { manager.Disconnect(); } catch { }

                    // ADIM 2: Bağlantı Denemesi
                    var connectTask = manager.ConnectAsync();
                    var timeoutTask = Task.Delay(3000);

                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Bağlantı isteği zaman aşımına uğradı (3sn).");
                    }

                    var connectResult = await connectTask;

                    if (connectResult.IsSuccess)
                    {
                        // --- DÜZELTME BAŞLANGICI ---

                        // HATA BURADAYDI: Hemen "Connected" yapıyorduk.
                        // YENİ MANTIK: Bağlantı var ama veri okuyabiliyor muyuz? Teyit et.

                        var verifyRead = await manager.ReadLiveStatusDataAsync();

                        if (verifyRead.IsSuccess)
                        {
                            // Hem bağlantı var HEM DE veri okunabiliyor. Şimdi Connected yapabiliriz.

                            // Okunan ilk veriyi hemen status'e işle ki "Boşta" görünmesin (Eğer çalışıyorsa çalışıyor görünsün)
                            var initialData = verifyRead.Content;
                            status.IsInRecipeMode = initialData.IsInRecipeMode;
                            status.manuel_status = initialData.manuel_status;
                            status.HasActiveAlarm = initialData.HasActiveAlarm;
                            // Diğer kritik verileri de eşitleyebilirsiniz...

                            status.ConnectionState = ConnectionStatus.Connected;
                            _connectionStates[machineId] = ConnectionStatus.Connected;
                            _lastConnectionTime[machineId] = DateTime.Now;
                            _reconnectAttempts.TryRemove(machineId, out _);

                            OnMachineConnectionStateChanged?.Invoke(machineId, status);
                            LiveEventAggregator.Instance.Publish(new LiveEvent { Timestamp = DateTime.Now, Source = status.MachineName, Message = "Connection re-established.", Type = EventType.SystemSuccess });

                            _logger.LogInformation($"Makine {machineId} bağlantısı ve veri akışı sağlandı.");
                        }
                        else
                        {
                            // Bağlantı başarılı ama veri okunamadı.
                            // Bu durumda "Connected" yapma, bağlantıyı kopar ve başarısız say.
                            manager.Disconnect();
                            throw new Exception("Socket bağlandı ancak ilk veri okunamadı.");
                        }

                        // --- DÜZELTME BİTİŞİ ---
                    }
                    else
                    {
                        // Bağlantı reddedildi
                        status.ConnectionState = ConnectionStatus.Disconnected;
                        _connectionStates[machineId] = ConnectionStatus.Disconnected;
                        OnMachineConnectionStateChanged?.Invoke(machineId, status);
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumu
                    status.ConnectionState = ConnectionStatus.Disconnected;
                    _connectionStates[machineId] = ConnectionStatus.Disconnected;
                    OnMachineConnectionStateChanged?.Invoke(machineId, status);
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
                    if (!isSystemStable) return;

                    // Eğer önceki batch düzgün kapanmadıysa kapat (Önceki işten kalan son adımı kaydet)
                    if (lastTrackedBatchId != null)
                    {
                        if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                        {
                            // Önceki batch'in son aktif adımını bul ve kaydet
                            FinalizeAndLogActiveStep(analyzer, machineId, lastTrackedBatchId);
                        }

                        _liveAlarmCounters.TryGetValue(machineId, out var finalCounters);
                        _batchTotalTheoreticalTimes.TryGetValue(machineId, out double theoreticalTime);
                        _productionRepository.EndBatch(machineId, lastTrackedBatchId, currentStatus, finalCounters.machineAlarmSeconds, finalCounters.operatorPauseSeconds, currentStatus.ActualQuantityProduction, finalCounters.machineAlarmSeconds + finalCounters.operatorPauseSeconds, theoreticalTime);
                    }

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

                            _batchTotalTheoreticalTimes[machineId] = RecipeAnalysis.CalculateTotalTheoreticalTimeSeconds(fullRecipe);
                            _batchStartTimes[machineId] = DateTime.Now;
                            _batchNonProductiveSeconds[machineId] = 0;
                            _productionRepository.SaveBatchRecipe(machineId, currentStatus.BatchNumarasi, fullRecipe);
                        }
                    }
                }
                // --- SENARYO 2: BATCH BİTİŞİ (Beklenmedik veya Normal) ---
                else if (!currentStatus.IsInRecipeMode && lastTrackedBatchId != null)
                {
                    if (!isSystemStable) return;

                    if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                    {
                        // KRİTİK DÜZELTME:
                        // Sadece "LastCompletedStep" değil, o an çalışmakta olan "ActiveStep"i kapatıp kaydediyoruz.
                        // Bu sayede son adım kaybolmaz ve N-1 tekrar yazılmaz.
                        FinalizeAndLogActiveStep(analyzer, machineId, lastTrackedBatchId);
                    }

                    _liveAlarmCounters.TryGetValue(machineId, out var finalCounters);
                    _batchTotalTheoreticalTimes.TryGetValue(machineId, out double theoreticalTime);
                    _productionRepository.EndBatch(machineId, lastTrackedBatchId, currentStatus, finalCounters.machineAlarmSeconds, finalCounters.operatorPauseSeconds, currentStatus.ActualQuantityProduction, finalCounters.machineAlarmSeconds + finalCounters.operatorPauseSeconds, theoreticalTime);

                    // Temizlik
                    _currentBatches[machineId] = null;
                    _liveAlarmCounters.TryRemove(machineId, out _);
                    _liveAnalyzers.TryRemove(machineId, out _);
                    _batchTotalTheoreticalTimes.TryRemove(machineId, out _);
                    _batchStartTimes.TryRemove(machineId, out _);
                    _batchNonProductiveSeconds.TryRemove(machineId, out _);
                    _lastLoggedStepNumber.TryRemove(machineId, out _); // Son loglanan numarasını temizle

                    if (_plcManagers.TryGetValue(machineId, out var plcManager))
                    {
                        _ = Task.Run(async () => {
                            try
                            {
                                var summaryResult = await plcManager.ReadBatchSummaryDataAsync();
                                if (summaryResult.IsSuccess) _productionRepository.UpdateBatchSummary(machineId, lastTrackedBatchId, summaryResult.Content);
                                await plcManager.IncrementProductionCounterAsync();
                                await plcManager.ResetOeeCountersAsync();
                            }
                            catch (Exception ex) { _logger.LogError(ex, "Batch bitişi asenkron işlemlerinde hata: {MachineId}", machineId); }
                        });
                    }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Batch takibi sırasında hata: {MachineId}", machineId); }
        }

        // --- YENİ YARDIMCI METOT: AKTİF ADIMI BUL, KAPAT VE KAYDET ---
        private void FinalizeAndLogActiveStep(LiveStepAnalyzer analyzer, int machineId, string batchId)
        {
            try
            {
                // DÜZELTME: Eski kod 'GetLastCompletedStep()' kullanıyordu, bu da bitmiş adımları getiriyordu.
                // Batch biterken son adım hala "Processing..." durumundadır.
                // Bu yüzden listedeki "Processing..." olan son adımı çekiyoruz.
                var activeStep = analyzer.AnalyzedSteps.LastOrDefault(s => s.WorkingTime == "Processing...");

                if (activeStep != null)
                {
                    // 1. Adımı bellek üzerinde sonlandır (Bitiş zamanını şu an olarak hesaplar ve günceller)
                    analyzer.FinalizeStep(activeStep.StepNumber, batchId, machineId);

                    // 2. ÇİFT KAYIT KONTROLÜ
                    _lastLoggedStepNumber.TryGetValue(machineId, out int lastLoggedNo);

                    // Eğer bu adım numarası daha önce veritabanına yazılmadıysa YAZ.
                    if (activeStep.StepNumber != lastLoggedNo)
                    {
                        _productionRepository.LogSingleStepDetail(activeStep, machineId, batchId);
                        _lastLoggedStepNumber[machineId] = activeStep.StepNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Son adım kaydedilirken hata oluştu.");
            }
        }

        private void ProcessLiveStepAnalysis(int machineId, FullMachineStatus currentStatus)
        {
            try
            {
                if (!currentStatus.IsInRecipeMode || string.IsNullOrEmpty(currentStatus.BatchNumarasi)) return;
                if (_lastConnectionTime.TryGetValue(machineId, out DateTime connectTime))
                {
                    if ((DateTime.Now - connectTime).TotalSeconds < StabilizationSeconds) return;
                }

                if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                {
                    // Analyzer veriyi işlediğinde bir adım tamamlandıysa true döner
                    if (analyzer.ProcessData(currentStatus))
                    {
                        var completedStepAnalysis = analyzer.GetLastCompletedStep();
                        if (completedStepAnalysis != null)
                        {
                            // Çok kısa süren gürültü adımları filtrele (3 saniye)
                            if (TimeSpan.TryParse(completedStepAnalysis.WorkingTime, out TimeSpan duration))
                            {
                                if (duration.TotalSeconds < 3) return;
                            }
                            else return;

                            // ÇİFT KAYIT KONTROLÜ
                            _lastLoggedStepNumber.TryGetValue(machineId, out int lastStepNo);
                            if (lastStepNo == completedStepAnalysis.StepNumber) return;

                            _productionRepository.LogSingleStepDetail(completedStepAnalysis, machineId, currentStatus.BatchNumarasi);
                            _lastLoggedStepNumber[machineId] = completedStepAnalysis.StepNumber;
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Adım analizi hatası: {MachineId}", machineId); }
        }

        private void CheckAndLogAlarms(int machineId, FullMachineStatus currentStatus)
        {
            try
            {
                if (currentStatus.ConnectionState != ConnectionStatus.Connected) return;
                bool isSystemStable = true;
                if (_lastConnectionTime.TryGetValue(machineId, out DateTime connectTime))
                {
                    if ((DateTime.Now - connectTime).TotalSeconds < StabilizationSeconds) isSystemStable = false;
                }

                if (_activeAlarmsTracker == null || !_activeAlarmsTracker.TryGetValue(machineId, out var machineActiveAlarms))
                {
                    _activeAlarmsTracker?.TryAdd(machineId, new ConcurrentDictionary<int, DateTime>());
                    return;
                }

                MachineDataCache.TryGetValue(machineId, out var previousStatus);
                int previousAlarmNumber = previousStatus?.ActiveAlarmNumber ?? 0;
                int currentAlarmNumber = currentStatus.ActiveAlarmNumber;

                if (currentAlarmNumber > 0)
                {
                    // ÖNEMLİ: Alarm numarası (örn: 5) önbellekte (veritabanında) tanımlı mı?
                    // Tanımlı DEĞİLSE, bu bloğa girmez ve alarm hiç var olmamış gibi davranılır.
                    if (_alarmDefinitionsCache.TryGetValue(currentAlarmNumber, out var newAlarmDef))
                    {
                        // Alarm takip listesinde yoksa (yeni geldiyse)
                        if (!machineActiveAlarms.ContainsKey(currentAlarmNumber))
                        {
                            _alarmRepository.WriteAlarmHistoryEvent(machineId, newAlarmDef.Id, "ACTIVE");
                            LiveEventAggregator.Instance.PublishAlarm(currentStatus.MachineName, newAlarmDef.AlarmText);
                        }
                        // Alarmı aktif listesine ekle/güncelle
                        machineActiveAlarms[currentAlarmNumber] = DateTime.Now;
                    }
                    // ELSE: Alarm tanımı yoksa (örn: 5 silindiyse) hiçbir şey yapma, yoksay.
                }

                if (isSystemStable)
                {
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

                currentStatus.HasActiveAlarm = !machineActiveAlarms.IsEmpty;
                if (currentStatus.HasActiveAlarm)
                {
                    currentStatus.ActiveAlarmNumber = machineActiveAlarms.OrderByDescending(kvp => kvp.Value).First().Key;
                    if (_alarmDefinitionsCache.TryGetValue(currentStatus.ActiveAlarmNumber, out var def)) currentStatus.ActiveAlarmText = def.AlarmText;
                    else currentStatus.ActiveAlarmText = $"UNDEFINED ALARM ({currentStatus.ActiveAlarmNumber})";
                }
                else
                {
                    currentStatus.ActiveAlarmNumber = 0;
                    currentStatus.ActiveAlarmText = "";
                }

                if ((previousStatus?.HasActiveAlarm ?? false) != currentStatus.HasActiveAlarm || previousAlarmNumber != currentStatus.ActiveAlarmNumber)
                {
                    OnActiveAlarmStateChanged?.Invoke(machineId, currentStatus);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Alarm işleme hatası: {MachineId}", machineId); }
        }

        private void LoadAlarmDefinitionsCache()
        {
            try
            {
                var definitions = _alarmRepository.GetAllAlarmDefinitions();
                _alarmDefinitionsCache = new ConcurrentDictionary<int, AlarmDefinition>(definitions.ToDictionary(def => def.AlarmNumber, def => def));
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
            if ((controlWord & 1) != 0) stepTypes.Add("Take Water");
            if ((controlWord & 2) != 0) stepTypes.Add("Heating");
            if ((controlWord & 4) != 0) stepTypes.Add("Working");
            if ((controlWord & 8) != 0) stepTypes.Add("Dosing");
            if ((controlWord & 16) != 0) stepTypes.Add("Drain");
            if ((controlWord & 32) != 0) stepTypes.Add("Extraction");
            if ((controlWord & 64) != 0) stepTypes.Add("Humidity Working");
            if ((controlWord & 128) != 0) stepTypes.Add("Timed Working");
            if ((controlWord & 256) != 0) stepTypes.Add("Humidity/Timed Working");
            if ((controlWord & 512) != 0) stepTypes.Add("Cooling");

            return stepTypes.Any() ? string.Join(" + ", stepTypes) : "Waiting....";
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