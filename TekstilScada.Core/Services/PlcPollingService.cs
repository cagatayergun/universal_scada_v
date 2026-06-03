using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        private ConcurrentDictionary<int, Machine> _activeMachinesConfig;

        // --- OPTİMİZASYON: DB İŞLEM KUYRUĞU VE SCOPE YÖNETİMİ ---
        private readonly Channel<Func<IServiceProvider, Task>> _dbOperationsChannel =
            Channel.CreateBounded<Func<IServiceProvider, Task>>(new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        private readonly IServiceScopeFactory _scopeFactory;

        // --- DATA STRUCTURES & CACHE ---
        private ConcurrentDictionary<int, DateTime> _batchStartDebounce = new ConcurrentDictionary<int, DateTime>();
        private ConcurrentDictionary<int, DateTime> _batchEndDebounce = new ConcurrentDictionary<int, DateTime>();
        private ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>> _activeAlarmsTracker;
        private ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>> _alarmLastSeenTracker;
        private ConcurrentDictionary<int, DateTime?> _alarmZeroSignalTrackers = new ConcurrentDictionary<int, DateTime?>();

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

        private readonly ConcurrentDictionary<int, LiveStepAnalyzer> _liveAnalyzers;
        private readonly ConcurrentDictionary<int, (int machineAlarmSeconds, int operatorPauseSeconds)> _liveAlarmCounters;
        private ConcurrentDictionary<int, DateTime> _lastManualLogTime = new ConcurrentDictionary<int, DateTime>();
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
        private readonly int _loggingIntervalMs = 6000;

        private const int BatchSize = 50;

        private ConcurrentDictionary<int, DateTime> _lastConnectionTime = new ConcurrentDictionary<int, DateTime>();
        private ConcurrentDictionary<int, int> _lastLoggedStepNumber = new ConcurrentDictionary<int, int>();
        private const int StabilizationSeconds = 5;
        private static readonly string[] _stepNameCache = new string[2048];
        private ConcurrentDictionary<int, DateTime> _batchEndDebounceTimers = new ConcurrentDictionary<int, DateTime>();
        private ConcurrentDictionary<int, string> _lastMachineState = new ConcurrentDictionary<int, string>();
        private ConcurrentDictionary<int, long> _activeEfficiencyLogIds = new ConcurrentDictionary<int, long>();
        private ConcurrentDictionary<int, string> _waitingDefinitionsCache;

        public PlcPollingService(
            AlarmRepository alarmRepository,
            ProcessLogRepository processLogRepository,
            ProductionRepository productionRepository,
            RecipeRepository recipeRepository,
            MachineRepository machineRepository,
            ILogger<PlcPollingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _alarmRepository = alarmRepository;
            _processLogRepository = processLogRepository;
            _productionRepository = productionRepository;
            _recipeRepository = recipeRepository;
            _machinerepository = machineRepository;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _waitingDefinitionsCache = new ConcurrentDictionary<int, string>();
            _plcManagers = new ConcurrentDictionary<int, IPlcManager>();
            MachineDataCache = new ConcurrentDictionary<int, FullMachineStatus>();
            _reconnectAttempts = new ConcurrentDictionary<int, DateTime>();
            _connectionStates = new ConcurrentDictionary<int, ConnectionStatus>();
            _activeAlarmsTracker = new ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>>();
            _currentBatches = new ConcurrentDictionary<int, string>();
            _liveAnalyzers = new ConcurrentDictionary<int, LiveStepAnalyzer>();
            _liveAlarmCounters = new ConcurrentDictionary<int, (int, int)>();
            _pollingTasks = new List<Task>();
            _alarmLastSeenTracker = new ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>>();
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
            LoadWaitingDefinitionsCache();

            _ = Task.Run(() => ProcessDbOperationsQueueAsync(_cancellationTokenSource.Token));
            _activeMachinesConfig = new ConcurrentDictionary<int, Machine>(machines.ToDictionary(m => m.Id));

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
                        MakineTipi = machine.MachineSubType,
                        DisplayOrder = machine.DisplayOrder,
                        ConnectionState = ConnectionStatus.Disconnected
                    });

                    _activeAlarmsTracker.TryAdd(machine.Id, new ConcurrentDictionary<int, DateTime>());
                    _currentBatches.TryAdd(machine.Id, null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Makine başlatılırken hata oluştu: {MachineId}", machine.Id);
                }
            }

            var machineBatches = machines
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / BatchSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            foreach (var batch in machineBatches)
            {
                var batchTask = Task.Run(() => PollBatchLoop(batch, _cancellationTokenSource.Token));
                _pollingTasks.Add(batchTask);
            }

            _loggingTimer = new System.Threading.Timer(LoggingTimer_Tick, null, 1000, Timeout.Infinite);
            _logger.LogInformation("{Count} makine için Polling Servisi başlatıldı.", machines.Count);
        }

        public void UpdateMachineConfig(Machine updatedMachine)
        {
            if (_activeMachinesConfig != null && _activeMachinesConfig.ContainsKey(updatedMachine.Id))
            {
                _activeMachinesConfig[updatedMachine.Id] = updatedMachine;
            }
        }

        static PlcPollingService()
        {
            for (int i = 0; i < 2048; i++)
            {
                var stepTypes = new List<string>();
                if ((i & 1) != 0) stepTypes.Add("Take Water");
                if ((i & 2) != 0) stepTypes.Add("Heating");
                if ((i & 4) != 0) stepTypes.Add("Working");
                if ((i & 8) != 0) stepTypes.Add("Dosing");
                if ((i & 16) != 0) stepTypes.Add("Drain");
                if ((i & 32) != 0) stepTypes.Add("Extraction");
                if ((i & 64) != 0) stepTypes.Add("Humidity Working");
                if ((i & 128) != 0) stepTypes.Add("Timed Working");
                if ((i & 256) != 0) stepTypes.Add("Humidity/Timed Working");
                if ((i & 512) != 0) stepTypes.Add("Cooling");
                if ((i & 1024) != 0) stepTypes.Add("Operator Call");

                _stepNameCache[i] = stepTypes.Any() ? string.Join(" + ", stepTypes) : "Waiting....";
            }
        }

        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                try { Task.WaitAll(_pollingTasks.ToArray(), 3000); } catch { }
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
            _generatedBatchIds?.Clear();
            _pollingTasks?.Clear();

            _logger.LogInformation("Polling Servisi durduruldu.");
        }

        private async Task ProcessDbOperationsQueueAsync(CancellationToken token)
        {
            try
            {
                await foreach (var dbOperation in _dbOperationsChannel.Reader.ReadAllAsync(token))
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            await dbOperation(scope.ServiceProvider);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kuyruktan DB'ye yazılırken hata oluştu.");
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task PollBatchLoop(List<Machine> machineBatch, CancellationToken token)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_pollingIntervalMs));
            try
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    var parallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 10,
                        CancellationToken = token
                    };

                    await Parallel.ForEachAsync(machineBatch, parallelOptions, async (machine, ct) =>
                    {
                        if (_plcManagers.TryGetValue(machine.Id, out var manager))
                        {
                            await ProcessSingleMachineAsync(machine, manager, ct);
                        }
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch döngü hatası.");
            }
        }

        private async Task ProcessSingleMachineAsync(Machine machine, IPlcManager manager, CancellationToken token)
        {
            try
            {
                if (!MachineDataCache.TryGetValue(machine.Id, out var status)) return;

                if (status.ConnectionState != ConnectionStatus.Connected)
                {
                    // 🚨 RECONNECT GÜNCELLEMESİ: Reconnect metoduna ham 'machine' konfigürasyonunu da paslıyoruz
                    await HandleReconnectionAsync(machine, manager);
                }
                else
                {
                    var readResult = await manager.ReadLiveStatusDataAsync();

                    if (readResult.IsSuccess)
                    {
                        var newStatus = readResult.Content;
                        newStatus.MachineId = machine.Id;
                        newStatus.MachineName = status.MachineName;
                        newStatus.MakineTipi = status.MakineTipi;

                        if (_activeMachinesConfig.TryGetValue(machine.Id, out var currentMachineConfig))
                        {
                            newStatus.DisplayOrder = currentMachineConfig.DisplayOrder;
                            newStatus.MakineTipi = currentMachineConfig.MachineSubType;
                        }

                        newStatus.ConnectionState = ConnectionStatus.Connected;
                        newStatus.AktifAdimAdi = GetStepTypeName(newStatus.AktifAdimTipiWordu);
                        CheckAndLogEfficiencyState(machine, newStatus);

                        if (newStatus.IsInRecipeMode)
                        {
                            _batchEndDebounceTimers.TryRemove(machine.Id, out _);
                        }
                        else
                        {
                            if (_generatedBatchIds.ContainsKey(machine.Id))
                            {
                                var firstStopMoment = _batchEndDebounceTimers.GetOrAdd(machine.Id, DateTime.Now);
                                if ((DateTime.Now - firstStopMoment).TotalSeconds < 7)
                                {
                                    newStatus.IsInRecipeMode = true;
                                }
                                else
                                {
                                    _batchEndDebounceTimers.TryRemove(machine.Id, out _);
                                }
                            }
                        }

                        if (newStatus.IsInRecipeMode)
                        {
                            if (!_generatedBatchIds.TryGetValue(machine.Id, out string currentBatchId))
                            {
                                currentBatchId = $"{DateTime.Now:yyyyMMddHHmmss}_{machine.Id}";
                                _generatedBatchIds.TryAdd(machine.Id, currentBatchId);
                            }
                            newStatus.BatchNumarasi = currentBatchId;
                        }
                        else
                        {
                            if (_generatedBatchIds.ContainsKey(machine.Id)) _generatedBatchIds.TryRemove(machine.Id, out _);
                            newStatus.BatchNumarasi = "";
                        }

                        PerformLiveAnalysis(machine.Id, newStatus);
                        ProcessLiveStepAnalysis(machine.Id, newStatus);
                        CheckAndLogBatchStartAndEnd(machine.Id, newStatus);
                        CheckAndLogAlarms(machine.Id, newStatus);

                        status = newStatus;
                        UpdateLiveCounters(machine.Id, newStatus);
                    }
                    else
                    {
                        // 🚨 RECONNECT GÜNCELLEMESİ: Hata anında manager bilgisini de gönderiyoruz
                        HandleDisconnection(machine.Id, manager);
                        if (MachineDataCache.ContainsKey(machine.Id)) status = MachineDataCache[machine.Id];
                    }
                }

                if (MachineDataCache.ContainsKey(machine.Id)) MachineDataCache[machine.Id] = status;

                var safeId = machine.Id;
                var safeStatus = status;
                _ = Task.Run(() =>
                {
                    try { OnMachineDataRefreshed?.Invoke(safeId, safeStatus); } catch { }
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Makine işlem hatası: {MachineId}", machine.Id);
            }
        }

        // 🚨 RECONNECT GÜNCELLEMESİ: Kurşun geçirmez Reconnect Durum Makinesi
        private async Task HandleReconnectionAsync(Machine machine, IPlcManager manager)
        {
            // Yeniden bağlanma periyodunu endüstriyel standart gereği 10 saniyeden 5 saniyeye düşürdük
            if (!_reconnectAttempts.ContainsKey(machine.Id) || (DateTime.UtcNow - _reconnectAttempts[machine.Id]).TotalSeconds > 5)
            {
                _reconnectAttempts[machine.Id] = DateTime.UtcNow;

                if (!MachineDataCache.TryGetValue(machine.Id, out var status)) return;

                status.ConnectionState = ConnectionStatus.Connecting;
                _connectionStates[machine.Id] = ConnectionStatus.Connecting;
                OnMachineConnectionStateChanged?.Invoke(machine.Id, status);

                try
                {
                    // KESİN ÇÖZÜM: Eski zombi kütüphane nesnesini imha et!
                    try { manager.Disconnect(); } catch { }

                    // İşletim sisteminin TCP havuzunu boşaltıp sıfırdan yeni soket açması için nesneyi baştan yaratıyoruz
                    var freshManager = PlcManagerFactory.Create(machine);
                    _plcManagers[machine.Id] = freshManager;
                    manager = freshManager; // Bu metodun kalanı için referansı tazeledik

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
                        var verifyRead = await manager.ReadLiveStatusDataAsync();

                        if (verifyRead.IsSuccess)
                        {
                            var initialData = verifyRead.Content;
                            status.IsInRecipeMode = initialData.IsInRecipeMode;
                            status.manuel_status = initialData.manuel_status;
                            status.HasActiveAlarm = initialData.HasActiveAlarm;

                            status.ConnectionState = ConnectionStatus.Connected;
                            _connectionStates[machine.Id] = ConnectionStatus.Connected;
                            _lastConnectionTime[machine.Id] = DateTime.Now;
                            _reconnectAttempts.TryRemove(machine.Id, out _);

                            OnMachineConnectionStateChanged?.Invoke(machine.Id, status);
                            LiveEventAggregator.Instance.Publish(new LiveEvent { Timestamp = DateTime.Now, Source = status.MachineName, Message = "Connection re-established.", Type = EventType.SystemSuccess });

                            _logger.LogInformation($"PLC Elektrik kesintisinden sonra başarıyla kurtarıldı. Makine: {machine.Id}");
                        }
                        else
                        {
                            manager.Disconnect();
                            throw new Exception("Soket el sıkıştı ancak ilk veri okuma paketi başarısız.");
                        }
                    }
                    else
                    {
                        status.ConnectionState = ConnectionStatus.Disconnected;
                        _connectionStates[machine.Id] = ConnectionStatus.Disconnected;
                        OnMachineConnectionStateChanged?.Invoke(machine.Id, status);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Makine {machine.Id} otomatik kurtarma denemesi başarısız (PLC hala kapalı olabilir): {ex.Message}");
                    status.ConnectionState = ConnectionStatus.Disconnected;
                    _connectionStates[machine.Id] = ConnectionStatus.Disconnected;
                    OnMachineConnectionStateChanged?.Invoke(machine.Id, status);
                }
            }
        }

        // 🚨 RECONNECT GÜNCELLEMESİ: Bağlantı koptuğu an eski soketi OS kernel seviyesinde boşa düşürme
        private void HandleDisconnection(int machineId, IPlcManager manager)
        {
            if (!MachineDataCache.TryGetValue(machineId, out var status)) return;

            // Okuma başarısız olduğu an kütüphaneyi hemen kilitlememesi için soketi anında kapatıyoruz
            try { manager.Disconnect(); } catch { }

            status.ConnectionState = ConnectionStatus.ConnectionLost;
            status.ProsesYuzdesi = 0;
            _connectionStates[machineId] = ConnectionStatus.ConnectionLost;

            // Reconnect döngüsünün beklemeden hemen devreye girmesi için zaman sayacını öne çekiyoruz
            _reconnectAttempts[machineId] = DateTime.UtcNow.AddSeconds(-6);

            OnMachineConnectionStateChanged?.Invoke(machineId, status);
            LiveEventAggregator.Instance.Publish(new LiveEvent { Source = status.MachineName, Message = "Connection lost!", Type = EventType.SystemWarning });
        }

        private void CheckAndLogEfficiencyState(Machine machine, FullMachineStatus currentStatus)
        {
            string currentState = "IDLE";
            if (currentStatus.IsInRecipeMode) currentState = "AUTO";
            else if (currentStatus.manuel_status) currentState = "MANUAL";

            _lastMachineState.TryGetValue(machine.Id, out string lastState);

            if (currentState != lastState)
            {
                if (_activeEfficiencyLogIds.TryRemove(machine.Id, out long oldLogId))
                {
                    _dbOperationsChannel.Writer.TryWrite(async (sp) =>
                    {
                        var repo = sp.GetRequiredService<EfficiencyRepository>();
                        await repo.EndLogAsync(oldLogId, DateTime.Now);
                    });
                }

                var newLog = new EfficiencyLog
                {
                    MachineId = machine.Id,
                    MachineSubType = machine.MachineSubType,
                    State = currentState,
                    StartTime = DateTime.Now,
                    RecipeName = currentStatus.IsInRecipeMode ? currentStatus.RecipeName : null
                };

                if (currentState == "IDLE" && currentStatus.WaitingReasonWords != null)
                {
                    var reasons = GetActiveReasons(currentStatus.WaitingReasonWords, _waitingDefinitionsCache);
                    if (reasons.Count > 0) newLog.Reason1 = reasons[0];
                    if (reasons.Count > 1) newLog.Reason2 = reasons[1];
                    if (reasons.Count > 2) newLog.Reason3 = reasons[2];
                    if (reasons.Count > 3) newLog.Reason4 = reasons[3];
                    if (reasons.Count > 4) newLog.Reason5 = reasons[4];
                }

                _dbOperationsChannel.Writer.TryWrite(async (sp) =>
                {
                    var repo = sp.GetRequiredService<EfficiencyRepository>();
                    long newId = await repo.StartNewLogAsync(newLog);
                    _activeEfficiencyLogIds[machine.Id] = newId;
                });

                _lastMachineState[machine.Id] = currentState;
            }
        }

        public List<string> GetActiveReasons(short[] statusWords, IDictionary<int, string> definitions)
        {
            var activeReasons = new List<string>();
            if (statusWords == null || definitions == null) return activeReasons;

            for (int w = 0; w < statusWords.Length; w++)
            {
                for (int b = 0; b < 16; b++)
                {
                    bool isSet = (statusWords[w] & (1 << b)) != 0;
                    if (isSet)
                    {
                        int bitIndex = (w * 16) + b;
                        if (definitions.TryGetValue(bitIndex, out var reasonText))
                        {
                            activeReasons.Add(reasonText);
                        }
                        if (activeReasons.Count >= 5) return activeReasons;
                    }
                }
            }
            return activeReasons;
        }

        public void LoadWaitingDefinitionsCache()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<EfficiencyRepository>();
                    var definitions = repo.GetDowntimeDefinitions();
                    _waitingDefinitionsCache = new ConcurrentDictionary<int, string>(definitions);
                    _logger.LogInformation("{Count} adet bekleme tanımı veritabanından belleğe yüklendi.", definitions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bekleme tanımları veritabanından yüklenirken KRİTİK HATA!");
                _waitingDefinitionsCache = new ConcurrentDictionary<int, string>();
            }
        }

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
                        if (machineStatus.IsInRecipeMode)
                        {
                            batchLogList.Add(machineStatus);
                        }
                        else
                        {
                            if (!_lastManualLogTime.TryGetValue(machineStatus.MachineId, out DateTime lastLog))
                            {
                                lastLog = DateTime.MinValue;
                            }

                            bool shouldLog = false;
                            if (machineStatus.manuel_status)
                            {
                                shouldLog = true;
                            }
                            else
                            {
                                if ((now - lastLog).TotalSeconds >= 16)
                                {
                                    shouldLog = true;
                                }
                            }

                            if (shouldLog)
                            {
                                manualLogList.Add(machineStatus);
                                _lastManualLogTime[machineStatus.MachineId] = now;
                            }
                        }
                    }
                }

                if (batchLogList.Count > 0)
                {
                    var bList = batchLogList.ToList();
                    _dbOperationsChannel.Writer.TryWrite((sp) =>
                    {
                        var repo = sp.GetRequiredService<ProcessLogRepository>();
                        repo.LogBulkData(bList);
                        return Task.CompletedTask;
                    });
                }
                if (manualLogList.Count > 0)
                {
                    var mList = manualLogList.ToList();
                    _dbOperationsChannel.Writer.TryWrite((sp) =>
                    {
                        var repo = sp.GetRequiredService<ProcessLogRepository>();
                        repo.LogBulkManualData(mList);
                        return Task.CompletedTask;
                    });
                }
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

                if (!isSystemStable) return;

                _currentBatches.TryGetValue(machineId, out string lastTrackedBatchId);
                bool isRecipeSignalActive = currentStatus.IsInRecipeMode;

                if (isRecipeSignalActive)
                {
                    _batchEndDebounce.TryRemove(machineId, out _);

                    if (string.IsNullOrEmpty(lastTrackedBatchId))
                    {
                        var firstSignalTime = _batchStartDebounce.GetOrAdd(machineId, DateTime.Now);

                        if ((DateTime.Now - firstSignalTime).TotalSeconds >= 5)
                        {
                            var filter = new ReportFilters
                            {
                                MachineId = machineId,
                                StartTime = DateTime.Now.AddDays(-4),
                                EndTime = DateTime.Now.AddDays(1)
                            };

                            var recentBatches = _productionRepository.GetProductionReport(filter);
                            var lastRecordedBatch = recentBatches.OrderByDescending(b => b.StartTime).FirstOrDefault();

                            string batchIdToUse = currentStatus.BatchNumarasi;
                            bool isResume = false;
                            ProductionReportItem existingActiveBatchItem = null;

                            if (lastRecordedBatch != null && lastRecordedBatch.EndTime == DateTime.MinValue)
                            {
                                existingActiveBatchItem = lastRecordedBatch;
                            }

                            if (existingActiveBatchItem != null)
                            {
                                batchIdToUse = existingActiveBatchItem.BatchId;
                                isResume = true;
                                _logger.LogInformation($"Makine {machineId} için son açık batch bulundu, devam ediliyor. ID: {batchIdToUse}");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(batchIdToUse))
                                    batchIdToUse = $"{DateTime.Now:yyyyMMddHHmmss}_{machineId}";
                            }

                            try
                            {
                                if (!isResume)
                                {
                                    var statToLog = currentStatus;
                                    _dbOperationsChannel.Writer.TryWrite((sp) =>
                                    {
                                        var repo = sp.GetRequiredService<ProductionRepository>();
                                        repo.StartNewBatch(statToLog);
                                        return Task.CompletedTask;
                                    });
                                    _batchStartTimes[machineId] = DateTime.Now;
                                }
                                else
                                {
                                    _batchStartTimes[machineId] = existingActiveBatchItem.StartTime;
                                }

                                _currentBatches[machineId] = batchIdToUse;
                                currentStatus.BatchNumarasi = batchIdToUse;

                                if (_plcManagers.TryGetValue(machineId, out var plcManager))
                                {
                                    try
                                    {
                                        var recipeReadResult = await plcManager.ReadFullRecipeDataAsync();
                                        if (recipeReadResult.IsSuccess && recipeReadResult.Content != null)
                                        {
                                            var fullRecipe = recipeReadResult.Content;
                                            fullRecipe.RecipeName = currentStatus.RecipeName;

                                            _liveAnalyzers[machineId] = new LiveStepAnalyzer(fullRecipe, _productionRepository);
                                            _batchTotalTheoreticalTimes[machineId] = RecipeAnalysis.CalculateTotalTheoreticalTimeSeconds(fullRecipe);
                                            _batchNonProductiveSeconds[machineId] = 0;

                                            if (!isResume)
                                            {
                                                var bId = batchIdToUse;
                                                _dbOperationsChannel.Writer.TryWrite((sp) =>
                                                {
                                                    var repo = sp.GetRequiredService<ProductionRepository>();
                                                    repo.SaveBatchRecipe(machineId, bId, fullRecipe);
                                                    return Task.CompletedTask;
                                                });
                                            }
                                        }
                                    }
                                    catch (Exception recipeEx)
                                    {
                                        _logger.LogError(recipeEx, "Batch reçete detayı okuma hatası: {MachineId}", machineId);
                                    }
                                }

                                _batchStartDebounce.TryRemove(machineId, out _);
                            }
                            catch (Exception dbEx)
                            {
                                _logger.LogError(dbEx, "Yeni parti başlatılırken veritabanı hatası: {MachineId}", machineId);
                            }
                        }
                    }
                    else
                    {
                        _batchStartDebounce.TryRemove(machineId, out _);
                    }
                }
                else
                {
                    _batchStartDebounce.TryRemove(machineId, out _);

                    if (lastTrackedBatchId != null)
                    {
                        var stopSignalTime = _batchEndDebounce.GetOrAdd(machineId, DateTime.Now);

                        if ((DateTime.Now - stopSignalTime).TotalSeconds >= 4)
                        {
                            if (_liveAnalyzers.TryGetValue(machineId, out var analyzer))
                            {
                                FinalizeAndLogActiveStep(analyzer, machineId, lastTrackedBatchId);
                            }

                            _liveAlarmCounters.TryGetValue(machineId, out var finalCounters);
                            _batchTotalTheoreticalTimes.TryGetValue(machineId, out double theoreticalTime);

                            var endBatchId = lastTrackedBatchId;
                            var endStatus = currentStatus;
                            var endAlarms = finalCounters.machineAlarmSeconds;
                            var endPause = finalCounters.operatorPauseSeconds;
                            var endQty = currentStatus.ActualQuantityProduction;
                            var endTotStop = finalCounters.machineAlarmSeconds + finalCounters.operatorPauseSeconds;

                            _dbOperationsChannel.Writer.TryWrite((sp) =>
                            {
                                var repo = sp.GetRequiredService<ProductionRepository>();
                                repo.EndBatch(machineId, endBatchId, endStatus, endAlarms, endPause, endQty, endTotStop, theoreticalTime);
                                return Task.CompletedTask;
                            });

                            _currentBatches[machineId] = null;
                            _batchEndDebounce.TryRemove(machineId, out _);

                            _liveAlarmCounters.TryRemove(machineId, out _);
                            _liveAnalyzers.TryRemove(machineId, out _);
                            _batchTotalTheoreticalTimes.TryRemove(machineId, out _);
                            _batchStartTimes.TryRemove(machineId, out _);
                            _batchNonProductiveSeconds.TryRemove(machineId, out _);
                            _lastLoggedStepNumber.TryRemove(machineId, out _);

                            if (_plcManagers.TryGetValue(machineId, out var plcManager))
                            {
                                _ = Task.Run(async () => {
                                    try
                                    {
                                        var summaryResult = await plcManager.ReadBatchSummaryDataAsync();
                                        if (summaryResult.IsSuccess)
                                        {
                                            var summaryData = summaryResult.Content;
                                            _dbOperationsChannel.Writer.TryWrite((sp) =>
                                            {
                                                var repo = sp.GetRequiredService<ProductionRepository>();
                                                repo.UpdateBatchSummary(machineId, endBatchId, summaryData);
                                                return Task.CompletedTask;
                                            });
                                        }
                                        await plcManager.IncrementProductionCounterAsync();
                                        await plcManager.ResetOeeCountersAsync();
                                    }
                                    catch (Exception ex) { _logger.LogError(ex, "Batch bitişi asenkron hata: {MachineId}", machineId); }
                                });
                            }
                        }
                    }
                    else
                    {
                        _batchEndDebounce.TryRemove(machineId, out _);
                    }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Batch takibi hatası: {MachineId}", machineId); }
        }

        private void FinalizeAndLogActiveStep(LiveStepAnalyzer analyzer, int machineId, string batchId)
        {
            try
            {
                var activeStep = analyzer.AnalyzedSteps.LastOrDefault(s => s.WorkingTime == "Processing...");

                if (activeStep != null)
                {
                    analyzer.FinalizeStep(activeStep.StepNumber, batchId, machineId);
                    _lastLoggedStepNumber.TryGetValue(machineId, out int lastLoggedNo);

                    if (activeStep.StepNumber != lastLoggedNo)
                    {
                        var stepToLog = activeStep;
                        var safeBatchId = batchId;
                        _dbOperationsChannel.Writer.TryWrite((sp) =>
                        {
                            var repo = sp.GetRequiredService<ProductionRepository>();
                            repo.LogSingleStepDetail(stepToLog, machineId, safeBatchId);
                            return Task.CompletedTask;
                        });

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
                    if (analyzer.ProcessData(currentStatus))
                    {
                        var completedStepAnalysis = analyzer.GetLastCompletedStep();
                        if (completedStepAnalysis != null)
                        {
                            if (TimeSpan.TryParse(completedStepAnalysis.WorkingTime, out TimeSpan duration))
                            {
                                if (duration.TotalSeconds < 3) return;
                            }
                            else return;

                            _lastLoggedStepNumber.TryGetValue(machineId, out int lastStepNo);
                            if (lastStepNo == completedStepAnalysis.StepNumber) return;

                            var stepToLog = completedStepAnalysis;
                            var safeBatchId = currentStatus.BatchNumarasi;
                            _dbOperationsChannel.Writer.TryWrite((sp) =>
                            {
                                var repo = sp.GetRequiredService<ProductionRepository>();
                                repo.LogSingleStepDetail(stepToLog, machineId, safeBatchId);
                                return Task.CompletedTask;
                            });

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

                var activeAlarms = _activeAlarmsTracker.GetOrAdd(machineId, new ConcurrentDictionary<int, DateTime>());
                var lastSeenAlarms = _alarmLastSeenTracker.GetOrAdd(machineId, new ConcurrentDictionary<int, DateTime>());

                int currentWordValue = currentStatus.ActiveAlarmNumber;
                DateTime now = DateTime.Now;

                if (currentWordValue > 0)
                {
                    _alarmZeroSignalTrackers.TryRemove(machineId, out _);

                    if (_alarmDefinitionsCache.TryGetValue(currentWordValue, out var alarmDef))
                    {
                        lastSeenAlarms[currentWordValue] = now;

                        if (!activeAlarms.ContainsKey(currentWordValue))
                        {
                            activeAlarms[currentWordValue] = now;

                            var aDefId = alarmDef.Id;
                            _dbOperationsChannel.Writer.TryWrite((sp) =>
                            {
                                var repo = sp.GetRequiredService<AlarmRepository>();
                                repo.WriteAlarmHistoryEvent(machineId, aDefId, "ACTIVE");
                                return Task.CompletedTask;
                            });

                            LiveEventAggregator.Instance.PublishAlarm(currentStatus.MachineName, alarmDef.AlarmText);
                        }
                    }
                }
                else
                {
                    if (!activeAlarms.IsEmpty)
                    {
                        if (!_alarmZeroSignalTrackers.TryGetValue(machineId, out DateTime? zeroStartTime) || zeroStartTime == null)
                        {
                            _alarmZeroSignalTrackers[machineId] = now;
                        }
                        else
                        {
                            if ((now - zeroStartTime.Value).TotalSeconds >= 3)
                            {
                                foreach (var kvp in activeAlarms)
                                {
                                    CloseAlarm(machineId, kvp.Key, currentStatus.MachineName);
                                }
                                activeAlarms.Clear();
                                lastSeenAlarms.Clear();
                                _alarmZeroSignalTrackers.TryRemove(machineId, out _);
                            }
                        }
                    }
                }

                if (!activeAlarms.IsEmpty)
                {
                    var activeKeys = activeAlarms.Keys.ToList();
                    foreach (var alarmId in activeKeys)
                    {
                        if (lastSeenAlarms.TryGetValue(alarmId, out DateTime lastSeenTime))
                        {
                            if ((now - lastSeenTime).TotalSeconds > 300)
                            {
                                CloseAlarm(machineId, alarmId, currentStatus.MachineName);
                                activeAlarms.TryRemove(alarmId, out _);
                                lastSeenAlarms.TryRemove(alarmId, out _);
                            }
                        }
                        else
                        {
                            lastSeenAlarms[alarmId] = now;
                        }
                    }
                }

                currentStatus.HasActiveAlarm = !activeAlarms.IsEmpty;

                if (currentStatus.HasActiveAlarm)
                {
                    int displayId = (currentWordValue > 0) ? currentWordValue : activeAlarms.Keys.LastOrDefault();
                    currentStatus.ActiveAlarmNumber = displayId;
                    if (_alarmDefinitionsCache.TryGetValue(displayId, out var def))
                        currentStatus.ActiveAlarmText = def.AlarmText;
                    else
                        currentStatus.ActiveAlarmText = $"ALARM {displayId}";
                }
                else
                {
                    currentStatus.ActiveAlarmNumber = 0;
                    currentStatus.ActiveAlarmText = "";
                }

                MachineDataCache.TryGetValue(machineId, out var previousStatus);
                if ((previousStatus?.HasActiveAlarm ?? false) != currentStatus.HasActiveAlarm ||
                    (previousStatus?.ActiveAlarmNumber ?? 0) != currentStatus.ActiveAlarmNumber)
                {
                    var safeStatus = currentStatus;
                    _ = Task.Run(() =>
                    {
                        try { OnActiveAlarmStateChanged?.Invoke(machineId, safeStatus); } catch { }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alarm işleme hatası: {MachineId}", machineId);
            }
        }

        private void CloseAlarm(int machineId, int alarmId, string machineName)
        {
            if (_alarmDefinitionsCache.TryGetValue(alarmId, out var closingAlarmDef))
            {
                var aDefId = closingAlarmDef.Id;
                _dbOperationsChannel.Writer.TryWrite((sp) =>
                {
                    var repo = sp.GetRequiredService<AlarmRepository>();
                    repo.WriteAlarmHistoryEvent(machineId, aDefId, "INACTIVE");
                    return Task.CompletedTask;
                });

                if (MachineDataCache.TryGetValue(machineId, out var currentStatus) && currentStatus.IsInRecipeMode)
                {
                    if (_activeAlarmsTracker.TryGetValue(machineId, out var activeAlarms) && activeAlarms.TryGetValue(alarmId, out DateTime startTime))
                    {
                        DateTime endTime = DateTime.Now;
                        int durationMins = (int)Math.Max(1, Math.Round((endTime - startTime).TotalMinutes));
                        _activeMachinesConfig.TryGetValue(machineId, out var machineConfig);
                        string currentScadaUser = CurrentUser.IsLoggedIn && CurrentUser.User != null ? CurrentUser.User.FullName : "";

                        _dbOperationsChannel.Writer.TryWrite(async (sp) =>
                        {
                            using var conn = new MySql.Data.MySqlClient.MySqlConnection(AppConfig.ConnectionString);
                            await conn.OpenAsync();

                            string insertSql = @"
                                INSERT INTO laundry_machine_reports 
                                (Date, Machine_ID, Machine_IP, `Machine Name`, Machine_Type, Start_time, End_Time, Duration_mins, Type, `Reason Type`, Reason, Recipe_id, Factory_Order, telematric_user, machine_operator_id, machine_operator_name)
                                VALUES 
                                (@Date, @Machine_ID, @Machine_IP, @Machine_Name, @Machine_Type, @Start_time, @End_Time, @Duration_mins, @Type, @Reason_Type, @Reason, @Recipe_id, @Factory_Order, @telematric_user, @machine_operator_id, @machine_operator_name);";

                            using var cmd = new MySql.Data.MySqlClient.MySqlCommand(insertSql, conn);
                            cmd.Parameters.AddWithValue("@Date", startTime.Date);
                            cmd.Parameters.AddWithValue("@Machine_ID", machineId.ToString());
                            cmd.Parameters.AddWithValue("@Machine_IP", machineConfig?.IpAddress ?? "0.0.0.0");
                            cmd.Parameters.AddWithValue("@Machine_Name", machineConfig?.MachineName ?? machineName);
                            cmd.Parameters.AddWithValue("@Machine_Type", machineConfig?.MachineSubType ?? "");
                            cmd.Parameters.AddWithValue("@Start_time", startTime.ToString("HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@End_Time", endTime.ToString("HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Duration_mins", durationMins);
                            cmd.Parameters.AddWithValue("@Type", "Unplanned");
                            cmd.Parameters.AddWithValue("@Reason_Type", "Alarm");
                            cmd.Parameters.AddWithValue("@Reason", closingAlarmDef.AlarmText);
                            cmd.Parameters.AddWithValue("@Recipe_id", currentStatus.RecipeName);
                            cmd.Parameters.AddWithValue("@Factory_Order", currentStatus.BatchNumarasi ?? "");
                            cmd.Parameters.AddWithValue("@telematric_user", currentScadaUser);
                            cmd.Parameters.AddWithValue("@machine_operator_id", currentStatus.OperatorIsmi ?? "Empty");
                            cmd.Parameters.AddWithValue("@machine_operator_name", currentStatus.OperatorIsmi ?? "Empty");

                            await cmd.ExecuteNonQueryAsync();
                        });
                    }
                }

                LiveEventAggregator.Instance.Publish(new LiveEvent
                {
                    Type = EventType.SystemInfo,
                    Source = machineName,
                    Message = $"{closingAlarmDef.AlarmText} - CLEARED"
                });
            }
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
            return _stepNameCache[Math.Abs(controlWord) & 2047];
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