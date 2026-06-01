using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TekstilScada.Core;
using TekstilScada.Core.Core;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;
using static TekstilScada.Core.Core.ExcelExportHelper;

// --- DTO SINIFLARI (Kaybolmaması için aynen korundu) ---
public class HourlyConsumptionData
{
    public double Saat { get; set; }
    public double ToplamElektrik { get; set; }
    public double ToplamSu { get; set; }
    public double ToplamBuhar { get; set; }
}
public class EfficiencyReportFilters
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? MachineId { get; set; }
    public string? SubType { get; set; }
}
public class HourlyOeeData
{
    public double Saat { get; set; }
    public double AverageOEE { get; set; }
}
public class SaveLayoutRequest
{
    public string LayoutName { get; set; }
    public string MachineSubType { get; set; }
    public int StepTypeId { get; set; }
    public string LayoutJson { get; set; }
}
public class StepTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
public class TrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public double Rpm { get; set; }
    public double WaterLevel { get; set; }
}

public class ProductionStepDetailDto : ProductionStepDetail
{
    public double TheoreticalDurationSeconds { get; set; } = 0;
    public double Temperature { get; set; } = 0;
    public string StepDescription => StepName;
}

public class AlarmDetailDto
{
    public DateTime AlarmTime { get; set; } = DateTime.MinValue;
    public int AlarmNumber { get; set; }
    public string AlarmType { get; set; } = string.Empty;
    public string AlarmDescription { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public DateTime EndTime => AlarmTime.Add(Duration);
}

public class ProductionDetailDto
{
    public ProductionReportItem Header { get; set; } = new();
    public List<ProductionStepDetailDto> Steps { get; set; } = new();
    public List<AlarmDetailDto> Alarms { get; set; } = new();
    public List<TrendDataPoint> LogData { get; set; } = new();
    public List<TrendDataPoint> TheoreticalData { get; set; } = new();
}

public class GeneralDetailedConsumptionFilters
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<int>? MachineIds { get; set; }
}

public class GeneralConsumptionExportDto
{
    public List<ProductionReportItem>? Items { get; set; }
    public string? ConsumptionType { get; set; }
}
public class ActionLogFilters
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Username { get; set; }
    public string? Details { get; set; }
}

namespace TekstilScada.Services
{
    public class SignalRGatewayService
    {
        private readonly HubConnection _connection;

        // --- REPOSITORIES ---
        private readonly MachineRepository _machineRepo;
        private readonly RecipeRepository _recipeRepo;
        private readonly UserRepository _userRepo;
        private readonly CostRepository _costRepo;
        private readonly AlarmRepository _alarmRepo;
        private readonly DashboardRepository _dashboardRepo;
        private readonly ProductionRepository _productionRepo;
        private readonly ProcessLogRepository _processLogRepo;
        private readonly RecipeConfigurationRepository _configRepo;
        private readonly PlcOperatorRepository _plcOpRepo;
        private readonly EfficiencyRepository _efficiencyRepo;
        // --- SERVICES ---
        private readonly PlcPollingService _plcService;
        private readonly FtpTransferService _ftpService;
        private string _myApiKey;

        // --- EVENT & STATE ---
        public event Action<int, string, string> OnRemoteCommandReceived;
        private DateTime _lastSentTime = DateTime.MinValue;
        private readonly int _sendIntervalMs = 500;

        // --- DISPATCHER ---
        private readonly Dictionary<string, Func<object[], Task<object>>> _requestHandlers;
        private readonly ConcurrentDictionary<int, FullMachineStatus> _bufferedMachineStatuses = new();
        private readonly ConcurrentDictionary<int, string> _machineHallCache = new ConcurrentDictionary<int, string>();

        // --- 5. ADIM: GATEWAY RAM CACHE DEĞİŞKENLERİ ---
        private readonly ConcurrentDictionary<string, (object Data, DateTime Expiry)> _gatewayCache = new();

        private CancellationTokenSource _loopCts;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public SignalRGatewayService(
            string hubUrl,
            string jwtToken,
            MachineRepository machineRepo,
            EfficiencyRepository efficiencyRepo,
            RecipeRepository recipeRepo,
            UserRepository userRepo,
            CostRepository costRepo,
            AlarmRepository alarmRepo,
            DashboardRepository dashboardRepo,
            ProductionRepository productionRepo,
            ProcessLogRepository processLogRepo,
            RecipeConfigurationRepository configRepo,
            PlcOperatorRepository plcOpRepo,
            PlcPollingService plcService,
            FtpTransferService ftpService, string apiKey)
        {
            _machineRepo = machineRepo;
            _efficiencyRepo = efficiencyRepo;
            _recipeRepo = recipeRepo;
            _userRepo = userRepo;
            _costRepo = costRepo;
            _alarmRepo = alarmRepo;
            _dashboardRepo = dashboardRepo;
            _productionRepo = productionRepo;
            _processLogRepo = processLogRepo;
            _configRepo = configRepo;
            _plcOpRepo = plcOpRepo;
            _plcService = plcService;
            _ftpService = ftpService;
            _myApiKey = apiKey;

            _requestHandlers = new Dictionary<string, Func<object[], Task<object>>>(StringComparer.OrdinalIgnoreCase);
            RegisterHandlers();

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(jwtToken))
                        options.AccessTokenProvider = () => Task.FromResult(jwtToken);

                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is System.Net.Http.HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };

                    options.ApplicationMaxBufferSize = 100 * 1024 * 1024;
                    options.TransportMaxBufferSize = 100 * 1024 * 1024;
                })
                .WithAutomaticReconnect()
                .Build();

            RegisterSignalRListeners();
            _loopCts = new CancellationTokenSource();
            _ = StartBatchSenderLoopAsync(_loopCts.Token);
        }

        // --- 5. ADIM: CACHE YARDIMCI METOTLARI ---
        private async Task<T> GetOrAddCacheAsync<T>(string cacheKey, Func<Task<T>> dbFetchTask, TimeSpan expiration)
        {
            if (_gatewayCache.TryGetValue(cacheKey, out var cacheEntry))
            {
                if (DateTime.Now < cacheEntry.Expiry)
                {
                    return (T)cacheEntry.Data;
                }
            }

            T data = await dbFetchTask();
            _gatewayCache[cacheKey] = (data, DateTime.Now.Add(expiration));
            return data;
        }

        private void InvalidateCache(string cacheKey)
        {
            _gatewayCache.TryRemove(cacheKey, out _);
        }
        // ----------------------------------------

        private void RegisterSignalRListeners()
        {
            _connection.On<string, string, object[]>("HandleRequest", async (reqId, method, args) =>
            {
                object result = null;
                string errorMessage = null;

                try
                {
                    if (_requestHandlers.TryGetValue(method, out var handler))
                    {
                        result = await handler(args);
                    }
                    else
                    {
                        errorMessage = $"Gateway: Bilinmeyen Metot -> {method}";
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }

                await SendLargeDataAsync(reqId, result, errorMessage);
            });

            _connection.On<ActionLogEntry>("HandleLogAction", (entry) =>
            {
                Task.Run(() =>
                {
                    try { _userRepo.LogAction(entry.UserId, entry.ActionType, entry.Details); } catch { }
                });
            });

            _connection.On<int, string, string>("ReceiveCommand", (machineId, command, parameters) =>
            {
                OnRemoteCommandReceived?.Invoke(machineId, command, parameters);
            });

            _connection.Closed += async (error) => await ConnectWithRetryAsync();
            _connection.Reconnected += async (connectionId) =>
            {
                try
                {
                    string localIp = GetLocalIpAddress();
                    await _connection.InvokeAsync("RegisterGateway", _myApiKey, localIp + ":5901");
                }
                catch { }
            };
        }

        private void RegisterHandlers()
        {
            Task<T> RunDb<T>(Func<T> action) => Task.Run(action);

            // -- MAKİNE İŞLEMLERİ --
            _requestHandlers["GetAllMachineStatuses"] = async _ =>
            {
                var list = _plcService.MachineDataCache.Values.ToList();
                foreach (var status in list)
                {
                    if (string.IsNullOrWhiteSpace(status.MachineHall) && _machineHallCache.TryGetValue(status.MachineId, out var hall))
                        status.MachineHall = hall;
                }
                return await Task.Run(() => list);
            };
            _requestHandlers["GetAllMachines"] = async _ => await RunDb(() => _machineRepo.GetAllMachines());
            _requestHandlers["GetMachineStatus"] = async args =>
            {
                int mId = GetArg<int>(args, 0);
                return await RunDb(() => {
                    var m = _machineRepo.GetAllMachines().Find(x => x.Id == mId);
                    return m != null ? new FullMachineStatus
                    {
                        MachineId = m.Id,
                        MachineName = m.MachineName,
                        MakineTipi = m.MachineSubType,
                        DisplayOrder = m.DisplayOrder,
                        MachineHall = m.MachineHall
                    } : null;
                });
            };
            _requestHandlers["AddMachine"] = async args => await RunDb(() => { _machineRepo.AddMachine(GetArg<Machine>(args, 0)); return true; });
            _requestHandlers["UpdateMachine"] = async args => await RunDb(() => {
                var m = GetArg<Machine>(args, 0);
                if (m != null) _machineHallCache[m.Id] = m.MachineHall;
                _machineRepo.UpdateMachine(m);
                return true;
            });
            _requestHandlers["DeleteMachine"] = async args => await RunDb(() => {
                int id = GetArg<int>(args, 0);
                _machineHallCache.TryRemove(id, out _);
                _machineRepo.DeleteMachine(id);
                return true;
            });

            // -- MALİYET (CACHE EKLENDİ) --
            _requestHandlers["GetCosts"] = async _ =>
                await GetOrAddCacheAsync("CostsCache", async () => await RunDb(() => _costRepo.GetAllParameters()), TimeSpan.FromMinutes(10));

            _requestHandlers["UpdateParameters"] = async args => await RunDb(() => {
                _costRepo.UpdateParameters(GetArg<List<CostParameter>>(args, 0));
                InvalidateCache("CostsCache"); // Cache'i Temizle
                return true;
            });

            // -- KULLANICI (CACHE EKLENDİ) --
            _requestHandlers["GetAllUsers"] = async _ =>
                await GetOrAddCacheAsync("UsersCache", async () => await RunDb(() => _userRepo.GetAllUsers()), TimeSpan.FromMinutes(5));

            _requestHandlers["GetAllRoles"] = async _ =>
                await GetOrAddCacheAsync("RolesCache", async () => await RunDb(() => _userRepo.GetAllRoles()), TimeSpan.FromMinutes(30));

            _requestHandlers["AddUser"] = async args => await RunDb(() =>
            {
                var u = GetArg<UserViewModel>(args, 0);
                var userNew = new User { Username = u.Username, FullName = u.FullName, IsActive = u.IsActive };
                _userRepo.AddUser(userNew, u.Password, u.SelectedRoleIds);
                InvalidateCache("UsersCache"); // Cache'i Temizle
                return true;
            });

            _requestHandlers["UpdateUser"] = async args => await RunDb(() =>
            {
                var u = GetArg<UserViewModel>(args, 0);
                var userUpd = new User { Id = u.Id, Username = u.Username, FullName = u.FullName, IsActive = u.IsActive };
                _userRepo.UpdateUser(userUpd, u.SelectedRoleIds, u.Password);
                InvalidateCache("UsersCache"); // Cache'i Temizle
                return true;
            });

            _requestHandlers["DeleteUser"] = async args => await RunDb(() => {
                _userRepo.DeleteUser(GetArg<int>(args, 0));
                InvalidateCache("UsersCache"); // Cache'i Temizle
                return true;
            });

            // -- REÇETE --
            _requestHandlers["GetAllRecipes"] = async _ => await RunDb(() => _recipeRepo.GetAllRecipes());
            _requestHandlers["GetRecipeById"] = async args => await RunDb(() => _recipeRepo.GetRecipeById(GetArg<int>(args, 0)));
            _requestHandlers["SaveRecipe"] = async args => await RunDb(() => { _recipeRepo.SaveRecipe(GetArg<ScadaRecipe>(args, 0)); return true; });
            _requestHandlers["DeleteRecipe"] = async args => await RunDb(() => { _recipeRepo.DeleteRecipe(GetArg<int>(args, 0)); return true; });
            _requestHandlers["GetRecipeUsageHistory"] = async args => await RunDb(() => _recipeRepo.GetRecipeUsageHistory(GetArg<int>(args, 0)));

            // -- DESIGNER & LAYOUT (CACHE EKLENDİ) --
            _requestHandlers["GetMachineSubTypes"] = async _ =>
                await GetOrAddCacheAsync("MachineSubTypesCache", async () => await RunDb(() => _configRepo.GetMachineSubTypes()), TimeSpan.FromHours(1));

            _requestHandlers["GetStepTypes"] = async _ => await RunDb(() =>
            {
                var dt = _configRepo.GetStepTypes();
                var list = new List<StepTypeDtoDesign>();
                foreach (System.Data.DataRow r in dt.Rows)
                    list.Add(new StepTypeDtoDesign { Id = Convert.ToInt32(r["Id"]), StepName = r["StepName"].ToString() });
                return list;
            });
            _requestHandlers["GetLayoutJson"] = async args => await RunDb(() =>
            {
                string rawJson = _configRepo.GetLayoutJson(GetArg<string>(args, 0), GetArg<int>(args, 1));
                if (string.IsNullOrEmpty(rawJson)) return new List<ControlMetadata>();
                return JsonSerializer.Deserialize<List<ControlMetadata>>(rawJson, _jsonOptions);
            });
            _requestHandlers["SaveLayout"] = async args => await RunDb(() =>
            {
                var layoutList = GetArg<List<ControlMetadata>>(args, 2);
                var jsonLayout = JsonSerializer.Serialize(layoutList, _jsonOptions);
                string subType = GetArg<string>(args, 0);
                int stepId = GetArg<int>(args, 1);
                _configRepo.SaveLayout($"{subType} - StepID:{stepId}", subType, stepId, jsonLayout);
                return true;
            });

            // -- PLC OPERATIONS --
            _requestHandlers["SendRecipeToPlc"] = async args =>
            {
                int rId = GetArg<int>(args, 0);
                int mId = GetArg<int>(args, 1);
                return await RunDb(async () =>
                {
                    var recipe = _recipeRepo.GetRecipeById(rId);
                    if (recipe == null) return false;
                    if (_plcService.GetPlcManagers().TryGetValue(mId, out var mgr))
                    {
                        var res = await mgr.WriteRecipeToPlcAsync(recipe);
                        return res.IsSuccess;
                    }
                    return false;
                });
            };
            _requestHandlers["ReadRecipeFromPlc"] = async args =>
            {
                int mId = GetArg<int>(args, 0);
                if (_plcService.GetPlcManagers().TryGetValue(mId, out var mgr))
                {
                    var res = await mgr.ReadRecipeFromPlcAsync();
                    if (res.IsSuccess && res.Content != null)
                    {
                        var newR = new ScadaRecipe { RecipeName = $"PLC_{DateTime.Now:HHmm}", Steps = new List<ScadaRecipeStep>() };
                        int stepSize = 25;
                        int stepCount = res.Content.Length / stepSize;
                        for (int i = 0; i < stepCount; i++)
                        {
                            var step = new ScadaRecipeStep { StepNumber = i + 1 };
                            Array.Copy(res.Content, i * stepSize, step.StepDataWords, 0, stepSize);
                            newR.Steps.Add(step);
                        }
                        return newR;
                    }
                }
                return null;
            };

            // -- FTP & HMI --
            _requestHandlers["GetHmiRecipeNames"] = async args =>
            {
                int mId = GetArg<int>(args, 0);
                if (_plcService.GetPlcManagers().TryGetValue(mId, out var mgr))
                {
                    var res = await mgr.ReadRecipeNamesFromPlcAsync();
                    return res.Content ?? new Dictionary<int, string>();
                }
                return new Dictionary<int, string>();
            };
            _requestHandlers["GetHmiRecipePreview"] = async args =>
            {
                int mId = GetArg<int>(args, 0);
                string fName = GetArg<string>(args, 1);
                return await RunDb(async () => {
                    var mHmi = _machineRepo.GetAllMachines().Find(x => x.Id == mId);
                    if (mHmi != null)
                    {
                        var ftp = new TekstilScada.Services.FtpService(mHmi.IpAddress, mHmi.FtpUsername, mHmi.FtpPassword);
                        string csv = await ftp.DownloadFileAsync("/" + fName);
                        return RecipeCsvConverter.ToRecipe(csv, fName);
                    }
                    return null;
                });
            };
            _requestHandlers["QueueSequentiallyNamedSendJobs"] = async args => await RunDb(() =>
            {
                var qRecipeIds = GetArg<List<int>>(args, 0);
                var qMachineIds = GetArg<List<int>>(args, 1);
                int startNum = GetArg<int>(args, 2);
                var recipesToSend = new List<ScadaRecipe>();
                foreach (var id in qRecipeIds) { var r = _recipeRepo.GetRecipeById(id); if (r != null) recipesToSend.Add(r); }
                var allMachines = _machineRepo.GetAllMachines();
                var machinesToSend = allMachines.FindAll(m => qMachineIds.Contains(m.Id));
                _ftpService.QueueSequentiallyNamedSendJobs(recipesToSend, machinesToSend, startNum);
                return true;
            });
            _requestHandlers["QueueReceiveJobs"] = async args => await RunDb(() =>
            {
                var qFileNames = GetArg<List<string>>(args, 0);
                int qRecMachineId = GetArg<int>(args, 1);
                var recMachine = _machineRepo.GetAllMachines().Find(m => m.Id == qRecMachineId);
                if (recMachine != null) _ftpService.QueueReceiveJobs(qFileNames, recMachine);
                return true;
            });
            _requestHandlers["GetActiveFtpJobs"] = async _ => await Task.FromResult(_ftpService.Jobs.ToList());

            // -- RAPORLAR VE DASHBOARD --
            _requestHandlers["GetProductionReport"] = async args => await RunDb(() => _productionRepo.GetProductionReport(GetArg<ReportFilters>(args, 0) ?? new ReportFilters()));
            _requestHandlers["GetEfficiencyReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<EfficiencyReportFilters>(args, 0);
                return _efficiencyRepo.GetEfficiencyReportAsync(rf.StartTime, rf.EndTime, rf.MachineId, rf.SubType)
                                      .GetAwaiter().GetResult().ToList();
            });
            _requestHandlers["GetAlarmReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                return _alarmRepo.GetAlarmReport(rf.StartTime, rf.EndTime, rf.MachineId);
            });
            _requestHandlers["GetTrendData"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                if (rf.MachineId == null) return new List<ProcessLogRepository.ProcessDataPoint>();
                return _processLogRepo.GetLogsForDateRange(rf.MachineId.Value, rf.StartTime, rf.EndTime);
            });
            _requestHandlers["GetManualConsumptionReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                if (rf.MachineId == null) return null;
                var s = DateTime.SpecifyKind(rf.StartTime.Date, DateTimeKind.Unspecified);
                var e = DateTime.SpecifyKind(rf.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                var m = _machineRepo.GetAllMachines().Find(x => x.Id == rf.MachineId);
                return _processLogRepo.GetManualConsumptionSummary(rf.MachineId.Value, m?.MachineName ?? "Bilinmeyen", s, e);
            });
            _requestHandlers["GetManualTrendData"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                if (rf == null || rf.MachineId == null) return new List<ProcessLogRepository.ProcessDataPoint>();
                return _processLogRepo.GetManualLogs(rf.MachineId.Value, rf.StartTime, rf.EndTime);
            });
            _requestHandlers["GetConsumptionTotalsForPeriod"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                return _productionRepo.GetConsumptionTotalsForPeriod(rf.StartTime.Date, rf.EndTime.Date.AddDays(1).AddTicks(-1));
            });
            _requestHandlers["GetGeneralDetailedConsumptionReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<GeneralDetailedConsumptionFilters>(args, 0);
                if (rf.MachineIds == null || rf.MachineIds.Count == 0) return new List<ProductionReportItem>();
                List<ProductionReportItem> combined = new List<ProductionReportItem>();
                foreach (var mid in rf.MachineIds)
                {
                    var f = new ReportFilters { StartTime = rf.StartTime, EndTime = rf.EndTime, MachineId = mid };
                    var r = _productionRepo.GetProductionReport(f);
                    combined.AddRange(r.FindAll(item => item.EndTime != DateTime.MinValue));
                }
                return combined;
            });
            _requestHandlers["GetActionLogs"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ActionLogFilters>(args, 0);
                return _userRepo.GetActionLogs(rf.StartTime, rf.EndTime, rf.Username, rf.Details);
            });
            _requestHandlers["GetProductionDetail"] = async args => await RunDb(() => GetProductionDetailInternal(GetArg<int>(args, 0), GetArg<string>(args, 1)));

            _requestHandlers["GetOeeReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                return _dashboardRepo.GetOeeReport(rf.StartTime, rf.EndTime, rf.MachineId);
            });

            _requestHandlers["GetHourlyFactoryConsumption"] = async _ => await RunDb(() =>
            {
                var dt = _dashboardRepo.GetHourlyFactoryConsumption(DateTime.Today);
                var list = new List<HourlyConsumptionData>();
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new HourlyConsumptionData
                        {
                            Saat = Convert.ToDouble(row["Saat"]),
                            ToplamElektrik = row.IsNull("ToplamElektrik") ? 0 : (Convert.ToDouble(row["ToplamElektrik"]) / 1000.0),
                            ToplamSu = row.IsNull("ToplamSu") ? 0 : (Convert.ToDouble(row["ToplamSu"]) / 1000.0),
                            ToplamBuhar = row.IsNull("ToplamBuhar") ? 0 : (Convert.ToDouble(row["ToplamBuhar"]) / 1000.0)
                        });
                    }
                }
                return list;
            });

            _requestHandlers["GetHourlyAverageOee"] = async _ => await RunDb(() =>
            {
                var dt = _dashboardRepo.GetHourlyAverageOee(DateTime.Today);
                var list = new List<HourlyOeeData>();
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new HourlyOeeData
                        {
                            Saat = Convert.ToDouble(row["Saat"]),
                            AverageOEE = row.IsNull("AverageOEE") ? 0 : Convert.ToDouble(row["AverageOEE"])
                        });
                    }
                }
                return list;
            });

            _requestHandlers["GetTopAlarmsByFrequency"] = async _ => await RunDb(() => _alarmRepo.GetTopAlarmsByFrequency(DateTime.Now.AddDays(-1), DateTime.Now));

            _requestHandlers["ExportProductionReport"] = async args => await RunDb(() => ExcelExportHelper.ExportProductionReportToExcel(GetArg<List<ProductionReportItem>>(args, 0)));
            _requestHandlers["ExportAlarmReport"] = async args => await RunDb(() => ExcelExportHelper.ExportAlarmReportToExcel(GetArg<List<AlarmReportItem>>(args, 0)));
            _requestHandlers["ExportOeeReport"] = async args => await RunDb(() => ExcelExportHelper.ExportOeeReportToExcel(GetArg<List<OeeData>>(args, 0)));
            _requestHandlers["ExportManualConsumptionReport"] = async args => await RunDb(() => ExcelExportHelper.ExportManualConsumptionReportToExcel(GetArg<ManualConsumptionSummary>(args, 0)));
            _requestHandlers["ExportActionLogsReport"] = async args => await RunDb(() => ExcelExportHelper.ExportActionLogsReportToExcel(GetArg<List<ActionLogEntry>>(args, 0)));
            _requestHandlers["ExportGeneralDetailedConsumptionReport"] = async args => await RunDb(() =>
            {
                var d = GetArg<GeneralConsumptionExportDto>(args, 0);
                return d != null ? ExcelExportHelper.ExportGeneralDetailedConsumptionReportToExcel(d.Items, d.ConsumptionType) : Array.Empty<byte>();
            });
            _requestHandlers["ExportProductionDetailFile"] = async args => await RunDb(() => ExportProductionDetailInternal(GetArg<int>(args, 0), GetArg<string>(args, 1)));

            // -- ALARM TANIMLARI (CACHE EKLENDİ) --
            _requestHandlers["GetAllAlarmDefinitions"] = async _ =>
                await GetOrAddCacheAsync("AlarmDefsCache", async () => await RunDb(() => _alarmRepo.GetAllAlarmDefinitions()), TimeSpan.FromMinutes(30));

            _requestHandlers["AddAlarmDefinition"] = async args => await RunDb(() => {
                _alarmRepo.AddAlarmDefinition(GetArg<AlarmDefinition>(args, 0));
                InvalidateCache("AlarmDefsCache"); // Cache'i Temizle
                return true;
            });
            _requestHandlers["UpdateAlarmDefinition"] = async args => await RunDb(() => {
                _alarmRepo.UpdateAlarmDefinition(GetArg<AlarmDefinition>(args, 0));
                InvalidateCache("AlarmDefsCache"); // Cache'i Temizle
                return true;
            });
            _requestHandlers["DeleteAlarmDefinition"] = async args => await RunDb(() => {
                _alarmRepo.DeleteAlarmDefinition(GetArg<int>(args, 0));
                InvalidateCache("AlarmDefsCache"); // Cache'i Temizle
                return true;
            });

            _requestHandlers["GetPlcOperators"] = async _ => await RunDb(() => _plcOpRepo.GetAll());
            _requestHandlers["SaveOrUpdateOperator"] = async args => await RunDb(() => { _plcOpRepo.SaveOrUpdate(GetArg<PlcOperator>(args, 0)); return true; });
            _requestHandlers["AddDefaultOperator"] = async _ => await RunDb(() => { _plcOpRepo.AddDefaultOperator(); return true; });
            _requestHandlers["DeleteOperator"] = async args => await RunDb(() => { _plcOpRepo.Delete(GetArg<int>(args, 0)); return true; });
            _requestHandlers["GetAlarmReportPaged"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                int pageNumber = GetArg<int>(args, 1);
                int pageSize = GetArg<int>(args, 2);
                return _alarmRepo.GetAlarmReportPaged(rf, pageNumber, pageSize);
            });
        }

        private string GetLocalIpAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
                }
            }
            catch { }
            return "localhost";
        }

        public async Task StartAsync()
        {
            _ = ConnectWithRetryAsync();
            await Task.CompletedTask;
        }

        private async Task ConnectWithRetryAsync()
        {
            if (_connection.State == HubConnectionState.Connected) return;

            while (true)
            {
                try
                {
                    await _connection.StartAsync();
                    string localIp = GetLocalIpAddress();
                    await _connection.InvokeAsync("RegisterGateway", _myApiKey, localIp + ":5901");

                    _plcService.OnMachineDataRefreshed -= OnLocalDataRefreshed;
                    _plcService.OnMachineDataRefreshed += OnLocalDataRefreshed;
                    return;
                }
                catch (Exception)
                {
                    await Task.Delay(5000);
                }
            }
        }

        private async Task SendLargeDataAsync(string reqId, object result, string errorMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(errorMessage) || result == null)
                {
                    await _connection.InvokeAsync("SendResponseToHub", reqId, result, errorMessage);
                    return;
                }

                string json;
                try
                {
                    json = JsonSerializer.Serialize(result, _jsonOptions);
                }
                catch (Exception ex)
                {
                    await _connection.InvokeAsync("SendResponseToHub", reqId, null, $"Serialization Error: {ex.Message}");
                    return;
                }

                const int chunkSize = 256 * 1024;
                if (json.Length <= chunkSize)
                {
                    await _connection.InvokeAsync("SendResponseToHub", reqId, json, null);
                }
                else
                {
                    int totalLength = json.Length;
                    int offset = 0;
                    while (offset < totalLength)
                    {
                        int remaining = totalLength - offset;
                        int currentChunkSize = Math.Min(remaining, chunkSize);
                        string chunk = json.Substring(offset, currentChunkSize);
                        offset += currentChunkSize;
                        await _connection.InvokeAsync("ReceiveResponseChunk", reqId, chunk, (offset >= totalLength));
                    }
                }
            }
            catch { }
        }

        private void OnLocalDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (status == null) return;

            if (string.IsNullOrWhiteSpace(status.MachineHall))
            {
                if (!_machineHallCache.TryGetValue(machineId, out var hall))
                {
                    try
                    {
                        var m = _machineRepo.GetAllMachines().Find(x => x.Id == machineId);
                        hall = m != null && !string.IsNullOrWhiteSpace(m.MachineHall) ? m.MachineHall : "Genel Hol";
                        _machineHallCache.TryAdd(machineId, hall);
                    }
                    catch { hall = "Genel Hol"; }
                }
                status.MachineHall = hall;
            }

            _bufferedMachineStatuses.AddOrUpdate(machineId, status, (key, oldValue) => status);
        }

        private async Task StartBatchSenderLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, token);

                    if (_connection.State == HubConnectionState.Connected && !_bufferedMachineStatuses.IsEmpty)
                    {
                        var packageToSend = _bufferedMachineStatuses.Values.ToList();
                        await _connection.InvokeAsync("BroadcastMachineBatch", packageToSend, token);
                        Console.WriteLine($"[Gateway] {packageToSend.Count} makine verisi toplu gönderildi.");
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Gateway] Toplu gönderim hatası: {ex.Message}");
                }
            }
        }

        private T GetArg<T>(object[] args, int index)
        {
            if (args == null || index >= args.Length) return default;
            if (args[index] is JsonElement jsonElement)
                return jsonElement.Deserialize<T>(_jsonOptions);
            return (T)args[index];
        }

        private ProductionDetailDto GetProductionDetailInternal(int machineId, string batchId)
        {
            var headerFilter = new ReportFilters { MachineId = machineId, BatchNo = batchId, StartTime = DateTime.MinValue, EndTime = DateTime.MaxValue };
            var reportList = _productionRepo.GetProductionReport(headerFilter);
            var reportItem = reportList.Count > 0 ? reportList[0] : null;

            if (reportItem == null) return null;

            var rawSteps = _productionRepo.GetProductionStepDetails(batchId, machineId);
            var stepDtos = rawSteps.Select(s => new ProductionStepDetailDto
            {
                StepNumber = s.StepNumber,
                StepName = s.StepName,
                TheoreticalTime = s.TheoreticalTime,
                WorkingTime = s.WorkingTime,
                StopTime = s.StopTime,
                DeflectionTime = s.DeflectionTime,
                TheoreticalDurationSeconds = TimeSpan.TryParse(s.TheoreticalTime, out var tt) ? tt.TotalSeconds : 0,
            }).ToList();

            var rawAlarms = _alarmRepo.GetAlarmDetailsForBatch(batchId, machineId);
            DateTime batchEndTime = reportItem.EndTime == DateTime.MinValue ? DateTime.Now : reportItem.EndTime;

            var alarmDtos = rawAlarms.Select(a =>
            {
                DateTime effectiveEnd = a.EndTime ?? batchEndTime;
                TimeSpan duration = effectiveEnd - a.StartTime;
                if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;

                return new AlarmDetailDto
                {
                    AlarmTime = a.StartTime,
                    AlarmNumber = a.AlarmNumber,
                    AlarmType = (a.AlarmNumber >= 500) ? "Operatör" : "Makine",
                    AlarmDescription = a.AlarmDescription,
                    Duration = duration
                };
            }).ToList();

            var rawLogs = _processLogRepo.GetLogsForBatch(machineId, batchId);
            var logDtos = rawLogs.Select(p => new TrendDataPoint
            {
                Timestamp = p.Timestamp,
                Temperature = (double)p.Temperature,
                Rpm = (double)p.Rpm,
                WaterLevel = (double)p.WaterLevel
            }).ToList();

            return new ProductionDetailDto
            {
                Header = reportItem,
                Steps = stepDtos,
                Alarms = alarmDtos,
                LogData = logDtos
            };
        }

        private byte[] ExportProductionDetailInternal(int machineId, string batchId)
        {
            var detail = GetProductionDetailInternal(machineId, batchId);
            if (detail == null) return Array.Empty<byte>();

            var excelDto = new ExcelExportHelper.ProductionDetailDto
            {
                Header = detail.Header,
                Steps = detail.Steps.Select(s => new ExcelExportHelper.ProductionStepDetailDto
                {
                    StepNumber = s.StepNumber,
                    StepName = s.StepName,
                    TheoreticalTime = s.TheoreticalTime,
                    WorkingTime = s.WorkingTime,
                    StopTime = s.StopTime,
                    DeflectionTime = s.DeflectionTime,
                    Temperature = s.Temperature
                }).ToList(),
                Alarms = detail.Alarms.Select(a => new ExcelExportHelper.AlarmDetailDto
                {
                    AlarmTime = a.AlarmTime,
                    AlarmType = a.AlarmType,
                    AlarmDescription = a.AlarmDescription,
                    Duration = a.Duration
                }).ToList(),
                LogData = new List<ExcelExportHelper.TrendDataPoint>()
            };

            return ExcelExportHelper.ExportProductionDetailToExcel(excelDto);
        }

        public async Task SendScreenImageAsync(int machineId, string base64Image)
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
                await _connection.InvokeAsync("SendScreenImage", machineId, base64Image);
        }
    }
}