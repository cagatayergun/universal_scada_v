using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Data; // EKLENDİ: DataTable işlemleri için gerekli
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TekstilScada.Core;
using TekstilScada.Core.Core; // ExcelExportHelper için
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services; // Namespace düzeltmesi
using static TekstilScada.Core.Core.ExcelExportHelper;

// --- DTO SINIFLARI (Kaybolmaması için aynen korundu) ---
public class HourlyConsumptionData
{
    public double Saat { get; set; }
    public double ToplamElektrik { get; set; }
    public double ToplamSu { get; set; }
    public double ToplamBuhar { get; set; }
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
    // Alarmın başlangıç zamanı
    public DateTime AlarmTime { get; set; } = DateTime.MinValue;

    // Kritik: Alarm ID'si (0-499: Makine, 500-600: Operatör ayrımı için şart)
    public int AlarmNumber { get; set; }

    // Alarmın tipi (Warning, Error vb.)
    public string AlarmType { get; set; } = string.Empty;

    // Alarm açıklaması
    public string AlarmDescription { get; set; } = string.Empty;

    // Alarm süresi
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    // Zaman kesişim analizi (Interval Merging) yaparken hassasiyet için EndTime eklendi
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

        // --- SERVICES ---
        private readonly PlcPollingService _plcService;
        private readonly FtpTransferService _ftpService;
        private string _myApiKey;

        // --- EVENT & STATE ---
        public event Action<int, string, string> OnRemoteCommandReceived;
        private DateTime _lastSentTime = DateTime.MinValue;
        private readonly int _sendIntervalMs = 500; // Canlı yayın için hız limiti

        // --- DISPATCHER (YENİ: Command Pattern için Sözlük) ---
        // Metot isminden çalıştırılacak fonksiyona haritalama yapar.
        private readonly Dictionary<string, Func<object[], Task<object>>> _requestHandlers;

        // JSON ayarlarını statik yapıp performansı artırıyoruz
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles, // EF Core Döngüsel referans hatasını önler
            WriteIndented = false, // Veri boyutunu küçültür
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals, // NaN ve Infinity desteği
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public SignalRGatewayService(
            string hubUrl,
            string jwtToken,
            MachineRepository machineRepo,
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

            // --- HANDLER KAYITLARI (Switch Case Yerine) ---
            _requestHandlers = new Dictionary<string, Func<object[], Task<object>>>(StringComparer.OrdinalIgnoreCase);
            RegisterHandlers(); // Tüm metotları sözlüğe ekle

            // SignalR Bağlantı Ayarları
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(jwtToken))
                        options.AccessTokenProvider = () => Task.FromResult(jwtToken);

                    // SSL Bypass
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is System.Net.Http.HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };

                    // Büyük veri transfer limitleri
                    options.ApplicationMaxBufferSize = 100 * 1024 * 1024;
                    options.TransportMaxBufferSize = 100 * 1024 * 1024;
                })
                .WithAutomaticReconnect()
                .Build();

            RegisterSignalRListeners();
        }

        private void RegisterSignalRListeners()
        {
            // --- GELEN İSTEKLERİ İŞLEME ---
            _connection.On<string, string, object[]>("HandleRequest", async (reqId, method, args) =>
            {
                object result = null;
                string errorMessage = null;

                try
                {
                    // Sözlükten metodu bul ve çalıştır (Performanslı Yöntem)
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

            // --- LOGLAMA ---
            _connection.On<ActionLogEntry>("HandleLogAction", (entry) =>
            {
                Task.Run(() =>
                {
                    try { _userRepo.LogAction(entry.UserId, entry.ActionType, entry.Details); } catch { }
                });
            });

            // --- KOMUT ALMA ---
            _connection.On<int, string, string>("ReceiveCommand", (machineId, command, parameters) =>
            {
                OnRemoteCommandReceived?.Invoke(machineId, command, parameters);
            });

            // --- BAĞLANTI YÖNETİMİ ---
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

        // --- HANDLER KAYIT MERKEZİ (BURASI PROJENİN KALP ATIŞIDIR) ---
        private void RegisterHandlers()
        {
            // Yardımcı: DB işlemlerini Thread Pool'a atarak SignalR'ın bloklanmasını önler
            Task<T> RunDb<T>(Func<T> action) => Task.Run(action);

            // -- MAKİNE İŞLEMLERİ --
            _requestHandlers["GetAllMachineStatuses"] = async _ => await RunDb(() => _plcService.MachineDataCache.Values.ToList());
            _requestHandlers["GetAllMachines"] = async _ => await RunDb(() => _machineRepo.GetAllMachines());
            _requestHandlers["GetMachineStatus"] = async args =>
            {
                int mId = GetArg<int>(args, 0);
                return await RunDb(() => {
                    var m = _machineRepo.GetAllMachines().Find(x => x.Id == mId);
                    return m != null ? new FullMachineStatus { MachineId = m.Id, MachineName = m.MachineName, MakineTipi = m.MachineSubType } : null;
                });
            };
            _requestHandlers["AddMachine"] = async args => await RunDb(() => { _machineRepo.AddMachine(GetArg<Machine>(args, 0)); return true; });
            _requestHandlers["UpdateMachine"] = async args => await RunDb(() => { _machineRepo.UpdateMachine(GetArg<Machine>(args, 0)); return true; });
            _requestHandlers["DeleteMachine"] = async args => await RunDb(() => { _machineRepo.DeleteMachine(GetArg<int>(args, 0)); return true; });

            // -- MALİYET --
            _requestHandlers["GetCosts"] = async _ => await RunDb(() => _costRepo.GetAllParameters());
            _requestHandlers["UpdateParameters"] = async args => await RunDb(() => { _costRepo.UpdateParameters(GetArg<List<CostParameter>>(args, 0)); return true; });

            // -- KULLANICI --
            _requestHandlers["GetAllUsers"] = async _ => await RunDb(() => _userRepo.GetAllUsers());
            _requestHandlers["GetAllRoles"] = async _ => await RunDb(() => _userRepo.GetAllRoles());
            _requestHandlers["AddUser"] = async args => await RunDb(() =>
            {
                var u = GetArg<UserViewModel>(args, 0);
                var userNew = new User { Username = u.Username, FullName = u.FullName, IsActive = u.IsActive };
                _userRepo.AddUser(userNew, u.Password, u.SelectedRoleIds);
                return true;
            });
            _requestHandlers["UpdateUser"] = async args => await RunDb(() =>
            {
                var u = GetArg<UserViewModel>(args, 0);
                var userUpd = new User { Id = u.Id, Username = u.Username, FullName = u.FullName, IsActive = u.IsActive };
                _userRepo.UpdateUser(userUpd, u.SelectedRoleIds, u.Password);
                return true;
            });
            _requestHandlers["DeleteUser"] = async args => await RunDb(() => { _userRepo.DeleteUser(GetArg<int>(args, 0)); return true; });

            // -- REÇETE --
            _requestHandlers["GetAllRecipes"] = async _ => await RunDb(() => _recipeRepo.GetAllRecipes());
            _requestHandlers["GetRecipeById"] = async args => await RunDb(() => _recipeRepo.GetRecipeById(GetArg<int>(args, 0)));
            _requestHandlers["SaveRecipe"] = async args => await RunDb(() => { _recipeRepo.SaveRecipe(GetArg<ScadaRecipe>(args, 0)); return true; });
            _requestHandlers["DeleteRecipe"] = async args => await RunDb(() => { _recipeRepo.DeleteRecipe(GetArg<int>(args, 0)); return true; });
            _requestHandlers["GetRecipeUsageHistory"] = async args => await RunDb(() => _recipeRepo.GetRecipeUsageHistory(GetArg<int>(args, 0)));

            // -- DESIGNER & LAYOUT --
            _requestHandlers["GetMachineSubTypes"] = async _ => await RunDb(() => _configRepo.GetMachineSubTypes());
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
                // PLC yazma işlemleri zaten async olduğu için RunDb içinde await ediyoruz
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
                // PLC okuma async
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
            _requestHandlers["GetAlarmReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                return _alarmRepo.GetAlarmReport(rf.StartTime.Date, rf.EndTime.Date.AddDays(1), rf.MachineId);
            });
            _requestHandlers["GetTrendData"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                if (rf.MachineId == null) return new List<ProcessLogRepository.ProcessDataPoint>();
                return _processLogRepo.GetLogsForDateRange(rf.MachineId.Value, rf.StartTime.Date, rf.EndTime.Date.AddDays(1));
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

            // -- DASHBOARD METOTLARI (DÜZELTİLDİ) --
            _requestHandlers["GetOeeReport"] = async args => await RunDb(() =>
            {
                var rf = GetArg<ReportFilters>(args, 0);
                return _dashboardRepo.GetOeeReport(rf.StartTime, rf.EndTime, rf.MachineId);
            });

            // GÜNCELLENDİ: Windows Forms mantığı ile birebir aynı veri çekme işlemi
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
                            // WinForms'ta olduğu gibi 1000'e bölerek gönderiyoruz
                            ToplamElektrik = row.IsNull("ToplamElektrik") ? 0 : (Convert.ToDouble(row["ToplamElektrik"]) / 1000.0),
                            ToplamSu = row.IsNull("ToplamSu") ? 0 : (Convert.ToDouble(row["ToplamSu"]) / 1000.0),
                            ToplamBuhar = row.IsNull("ToplamBuhar") ? 0 : (Convert.ToDouble(row["ToplamBuhar"]) / 1000.0)
                        });
                    }
                }
                return list;
            });

            // GÜNCELLENDİ: OEE Verisi
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

            // -- EXCEL EXPORT (Ağır İşlemler) --
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

            // -- ALARM TANIMLARI --
            _requestHandlers["GetAllAlarmDefinitions"] = async _ => await RunDb(() => _alarmRepo.GetAllAlarmDefinitions());
            _requestHandlers["AddAlarmDefinition"] = async args => await RunDb(() => { _alarmRepo.AddAlarmDefinition(GetArg<AlarmDefinition>(args, 0)); return true; });
            _requestHandlers["UpdateAlarmDefinition"] = async args => await RunDb(() => { _alarmRepo.UpdateAlarmDefinition(GetArg<AlarmDefinition>(args, 0)); return true; });
            _requestHandlers["DeleteAlarmDefinition"] = async args => await RunDb(() => { _alarmRepo.DeleteAlarmDefinition(GetArg<int>(args, 0)); return true; });

            // -- PLC OPERATORLERİ --
            _requestHandlers["GetPlcOperators"] = async _ => await RunDb(() => _plcOpRepo.GetAll());
            _requestHandlers["SaveOrUpdateOperator"] = async args => await RunDb(() => { _plcOpRepo.SaveOrUpdate(GetArg<PlcOperator>(args, 0)); return true; });
            _requestHandlers["AddDefaultOperator"] = async _ => await RunDb(() => { _plcOpRepo.AddDefaultOperator(); return true; });
            _requestHandlers["DeleteOperator"] = async args => await RunDb(() => { _plcOpRepo.Delete(GetArg<int>(args, 0)); return true; });
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

        private async void OnLocalDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (_connection.State != HubConnectionState.Connected || status == null) return;
            if ((DateTime.Now - _lastSentTime).TotalMilliseconds < _sendIntervalMs) return;

            try
            {
                await _connection.InvokeAsync("BroadcastFromLocal", status);
                _lastSentTime = DateTime.Now;
            }
            catch { }
        }

        private T GetArg<T>(object[] args, int index)
        {
            if (args == null || index >= args.Length) return default;
            if (args[index] is JsonElement jsonElement)
                return jsonElement.Deserialize<T>(_jsonOptions);
            return (T)args[index];
        }

        // Yardımcı Metotlar (Private)
        private ProductionDetailDto GetProductionDetailInternal(int machineId, string batchId)
        {
            // 1. Header (Başlık) Bilgisi
            var headerFilter = new ReportFilters { MachineId = machineId, BatchNo = batchId, StartTime = DateTime.MinValue, EndTime = DateTime.MaxValue };
            var reportList = _productionRepo.GetProductionReport(headerFilter);
            var reportItem = reportList.Count > 0 ? reportList[0] : null;

            if (reportItem == null) return null;

            // 2. Adım Detayları (Aynen korundu)
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
               // Temperature = s.Temperature
            }).ToList();

            // 3. ALARMLAR (BURASI DÜZELTİLDİ)
            var rawAlarms = _alarmRepo.GetAlarmDetailsForBatch(batchId, machineId);

            // Batch bitiş zamanı (Eğer üretim devam ediyorsa şu anı al)
            DateTime batchEndTime = reportItem.EndTime == DateTime.MinValue ? DateTime.Now : reportItem.EndTime;

            var alarmDtos = rawAlarms.Select(a =>
            {
                // Bitiş zamanı yoksa (hala aktifse) veya null ise batch bitişini/şu anı kullan
                DateTime effectiveEnd = a.EndTime ?? batchEndTime;

                // Süreyi hesapla
                TimeSpan duration = effectiveEnd - a.StartTime;
                if (duration < TimeSpan.Zero) duration = TimeSpan.Zero; // Negatif süre koruması

                return new AlarmDetailDto
                {
                    AlarmTime = a.StartTime,          // GERÇEK BAŞLANGIÇ
                    AlarmNumber = a.AlarmNumber,      // GERÇEK ID (0-499 Makine, 500+ Operatör ayrımı için şart)
                    AlarmType = (a.AlarmNumber >= 500) ? "Operatör" : "Makine",
                    AlarmDescription = a.AlarmDescription,
                    Duration = duration               // GERÇEK HESAPLANAN SÜRE
                };
            }).ToList();

            // 4. LOG (TREND) VERİLERİ (Aynen korundu)
            // Gateway'den veri çekerken büyük veri setlerinde kısıtlama yapmak gerekebilir
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