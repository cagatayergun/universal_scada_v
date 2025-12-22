using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TekstilScada.Core;
using TekstilScada.Core.Core; // ExcelExportHelper için
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using static TekstilScada.Core.Core.ExcelExportHelper;
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
    public DateTime AlarmTime { get; set; } = DateTime.MinValue;
    public string AlarmType { get; set; } = string.Empty;
    public string AlarmDescription { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
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

        // --- EVENT & STATE ---
        public event Action<int, string, string> OnRemoteCommandReceived;
        private DateTime _lastSentTime = DateTime.MinValue;
        private readonly int _sendIntervalMs = 500; // Canlı yayın için hız limiti

        // JSON ayarlarını statik yapıp performansı artırıyoruz
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles, // EF Core Döngüsel referans hatasını önler
            WriteIndented = false, // Veri boyutunu küçültür
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals // NaN ve Infinity desteği
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
            FtpTransferService ftpService)
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

            // SignalR Bağlantı Ayarları
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(jwtToken))
                        options.AccessTokenProvider = () => Task.FromResult(jwtToken);

                    // SSL Bypass (Sadece Geliştirme/Local Test İçin)
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is System.Net.Http.HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };

                    // Büyük veri transfer limitlerini artırıyoruz (Gateway tarafı)
                    options.ApplicationMaxBufferSize = 100 * 1024 * 1024;
                    options.TransportMaxBufferSize = 100 * 1024 * 1024;
                })
                .WithAutomaticReconnect() // Bağlantı koparsa otomatik dene
                .Build();

            RegisterHandlers();
        }

        public async Task StartAsync()
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    Console.WriteLine("[Gateway] SignalR Sunucusuna Bağlandı.");

                    // 1. Kimlik Doğrulama: Ben bir Gateway'im
                    await _connection.InvokeAsync("RegisterGateway");

                    // 2. PLC Servisinden gelen verilere abone ol (Canlı yayın için)
                    _plcService.OnMachineDataRefreshed -= OnLocalDataRefreshed; // Önce çıkar (duplicate önlemi)
                    _plcService.OnMachineDataRefreshed += OnLocalDataRefreshed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gateway] Bağlantı Başarısız: {ex.Message}");
                // Yeniden deneme mantığı burada eklenebilir veya Timer ile denenebilir
            }
        }

        private void RegisterHandlers()
        {
            // --- GELEN İSTEKLERİ İŞLEME (REQUEST HANDLER) ---
            _connection.On<string, string, object[]>("HandleRequest", async (reqId, method, args) =>
            {
                Console.WriteLine($"[Gateway] SİNYAL ALINDI! ID: {reqId}, Metot: '{method}'");
                // -------------------------
                object result = null;
                string errorMessage = null;

                try
                {
                    // İsteği işle ve sonucu al
                    result = await ProcessRequest(method, args);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Console.WriteLine($"[Gateway] Hata ({method}): {ex.Message}");
                }

                // Sonucu (büyük olsa bile) parçalayarak gönder
                await SendLargeDataAsync(reqId, result, errorMessage);
            });

            // --- LOGLAMA (FIRE AND FORGET) ---
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
                Console.WriteLine($"[Gateway] Komut Geldi -> Makine:{machineId}, Komut:{command}");
                OnRemoteCommandReceived?.Invoke(machineId, command, parameters);
            });

            // --- BAĞLANTI OLAYLARI ---
            _connection.Closed += async (error) =>
            {
                Console.WriteLine("[Gateway] Bağlantı Koptu!");
                await Task.CompletedTask;
            };

            _connection.Reconnected += async (connectionId) =>
            {
                Console.WriteLine("[Gateway] Tekrar Bağlandı. Yeniden Register olunuyor...");
                await _connection.InvokeAsync("RegisterGateway");
            };
        }

        // --- KRİTİK BÖLÜM: BÜYÜK VERİ GÖNDERİMİ (CHUNKING) ---
        private async Task SendLargeDataAsync(string reqId, object result, string errorMessage)
        {
            try
            {
                // Hata varsa direkt gönder
                if (!string.IsNullOrEmpty(errorMessage) || result == null)
                {
                    await _connection.InvokeAsync("SendResponseToHub", reqId, result, errorMessage);
                    return;
                }

                // Veriyi JSON string'e çevir
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

                // SignalR limiti genellikle 32KB civarıdır. Biz güvenli olsun diye 30KB parçalara bölelim.
                const int chunkSize = 30 * 1024;

                // Küçük veri ise tek seferde gönder
                if (json.Length <= chunkSize)
                {
                    await _connection.InvokeAsync("SendResponseToHub", reqId, json, null);
                }
                else
                {
                    // Büyük veri: Parçala ve döngüyle gönder
                    int totalLength = json.Length;
                    int offset = 0;

                    while (offset < totalLength)
                    {
                        int remaining = totalLength - offset;
                        int currentChunkSize = Math.Min(remaining, chunkSize);

                        string chunk = json.Substring(offset, currentChunkSize);
                        offset += currentChunkSize;
                        bool isLast = (offset >= totalLength);

                        await _connection.InvokeAsync("ReceiveResponseChunk", reqId, chunk, isLast);

                        // Network buffer'ın şişmesini engellemek için mini bekleme
                        // Çok büyük dosyalarda (örn 50MB) bu, UI donmasını engeller.
                        await Task.Delay(2);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gateway] Gönderim Hatası (Chunking): {ex.Message}");
            }
        }

        // --- CANLI YAYIN (BROADCAST) ---
        private async void OnLocalDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (_connection.State != HubConnectionState.Connected || status == null) return;

            // Çok sık veri gönderip ağı boğmamak için zaman kontrolü (Throttling)
            if ((DateTime.Now - _lastSentTime).TotalMilliseconds < _sendIntervalMs) return;

            try
            {
                await _connection.InvokeAsync("BroadcastFromLocal", status);
                _lastSentTime = DateTime.Now;
            }
            catch
            {
                // Canlı veri hataları akışı kesmemeli, sessizce yutulabilir
            }
        }

        // --- MERKEZİ İSTEK YÖNETİCİSİ ---
        private async Task<object> ProcessRequest(string method, object[] args)
        {
            // NOT: Repository metotlarınızın çoğu senkron (List dönüyor). 
            // Eğer Repository'ler async destekliyse (örn: ToListAsync), bunları await ile çağırmak daha iyidir.
            // Ancak mevcut yapıyı bozmamak için Task.Run içinde veya doğrudan çağırıyoruz.

            switch (method)
            {
                // -- MAKİNE --
                case "GetAllMachines": return _machineRepo.GetAllMachines();
                case "GetMachineStatus":
                    int mId = GetArg<int>(args, 0);
                    var m = _machineRepo.GetAllMachines().Find(x => x.Id == mId);
                    return m != null ? new FullMachineStatus { MachineId = m.Id, MachineName = m.MachineName, MakineTipi = m.MachineSubType } : null;
                case "AddMachine": _machineRepo.AddMachine(GetArg<Machine>(args, 0)); return true;
                case "UpdateMachine": _machineRepo.UpdateMachine(GetArg<Machine>(args, 0)); return true;
                case "DeleteMachine": _machineRepo.DeleteMachine(GetArg<int>(args, 0)); return true;

                // -- MALİYET --
                case "GetCosts": return _costRepo.GetAllParameters();
                case "UpdateParameters": _costRepo.UpdateParameters(GetArg<List<CostParameter>>(args, 0)); return true;

                // -- KULLANICI --
                case "GetAllUsers": return _userRepo.GetAllUsers();
                case "GetAllRoles": return _userRepo.GetAllRoles();
                case "AddUser":
                    var uAdd = GetArg<UserViewModel>(args, 0);
                    var userNew = new User { Username = uAdd.Username, FullName = uAdd.FullName, IsActive = uAdd.IsActive };
                    _userRepo.AddUser(userNew, uAdd.Password, uAdd.SelectedRoleIds);
                    return true;
                case "UpdateUser":
                    var uUpd = GetArg<UserViewModel>(args, 0);
                    var userUpd = new User { Id = uUpd.Id, Username = uUpd.Username, FullName = uUpd.FullName, IsActive = uUpd.IsActive };
                    _userRepo.UpdateUser(userUpd, uUpd.SelectedRoleIds, uUpd.Password);
                    return true;
                case "DeleteUser": _userRepo.DeleteUser(GetArg<int>(args, 0)); return true;

                // -- REÇETE --
                case "GetAllRecipes": return _recipeRepo.GetAllRecipes();
                case "GetRecipeById": return _recipeRepo.GetRecipeById(GetArg<int>(args, 0));
                case "SaveRecipe": _recipeRepo.SaveRecipe(GetArg<ScadaRecipe>(args, 0)); return true;
                case "DeleteRecipe": _recipeRepo.DeleteRecipe(GetArg<int>(args, 0)); return true;
                case "GetRecipeUsageHistory": return _recipeRepo.GetRecipeUsageHistory(GetArg<int>(args, 0));

                // -- DESIGNER --
                case "GetMachineSubTypes": return _configRepo.GetMachineSubTypes();
                case "GetStepTypes":
                    var dt = _configRepo.GetStepTypes();
                    var list = new List<StepTypeDtoDesign>();
                    foreach (System.Data.DataRow r in dt.Rows)
                        list.Add(new StepTypeDtoDesign { Id = Convert.ToInt32(r["Id"]), StepName = r["StepName"].ToString() });
                    return list;
                case "GetLayoutJson": return _configRepo.GetLayoutJson(GetArg<string>(args, 0), GetArg<int>(args, 1));
                case "SaveLayout":
                    var layoutList = GetArg<List<ControlMetadata>>(args, 2);
                    var jsonLayout = JsonSerializer.Serialize(layoutList, _jsonOptions);
                    string subType = GetArg<string>(args, 0);
                    int stepId = GetArg<int>(args, 1);
                    _configRepo.SaveLayout($"{subType} - StepID:{stepId}", subType, stepId, jsonLayout);
                    return true;

                // -- PLC OPERATIONS --
                case "SendRecipeToPlc":
                    int rId = GetArg<int>(args, 0);
                    int mIdPlc = GetArg<int>(args, 1);
                    var recipeToSend = _recipeRepo.GetRecipeById(rId);
                    if (recipeToSend == null) return false;
                    if (_plcService.GetPlcManagers().TryGetValue(mIdPlc, out var mgrSend))
                    {
                        var resultPlc = await mgrSend.WriteRecipeToPlcAsync(recipeToSend);
                        return resultPlc.IsSuccess;
                    }
                    return false;

                case "ReadRecipeFromPlc":
                    int mIdRead = GetArg<int>(args, 0);
                    if (_plcService.GetPlcManagers().TryGetValue(mIdRead, out var mgrRead))
                    {
                        var res = await mgrRead.ReadRecipeFromPlcAsync();
                        if (res.IsSuccess && res.Content != null)
                        {
                            // Basit bir DTO dönüşümü
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

                // -- FTP & HMI --
                case "GetHmiRecipeNames":
                    if (_plcService.GetPlcManagers().TryGetValue(GetArg<int>(args, 0), out var mgrNames))
                    {
                        var res = await mgrNames.ReadRecipeNamesFromPlcAsync();
                        return res.Content ?? new Dictionary<int, string>();
                    }
                    return new Dictionary<int, string>();

                case "GetHmiRecipePreview":
                    int mIdHmi = GetArg<int>(args, 0);
                    string fName = GetArg<string>(args, 1);
                    var mHmi = _machineRepo.GetAllMachines().Find(x => x.Id == mIdHmi);
                    if (mHmi != null)
                    {
                        var ftp = new TekstilScada.Services.FtpService(mHmi.IpAddress, mHmi.FtpUsername, mHmi.FtpPassword);
                        string csv = await ftp.DownloadFileAsync("/" + fName);
                        return RecipeCsvConverter.ToRecipe(csv, fName);
                    }
                    return null;

                case "QueueSequentiallyNamedSendJobs":
                    var qRecipeIds = GetArg<List<int>>(args, 0);
                    var qMachineIds = GetArg<List<int>>(args, 1);
                    int startNum = GetArg<int>(args, 2);
                    var recipesToSend = new List<ScadaRecipe>();
                    foreach (var id in qRecipeIds) { var r = _recipeRepo.GetRecipeById(id); if (r != null) recipesToSend.Add(r); }
                    var allMachines = _machineRepo.GetAllMachines();
                    var machinesToSend = allMachines.FindAll(m => qMachineIds.Contains(m.Id));
                    _ftpService.QueueSequentiallyNamedSendJobs(recipesToSend, machinesToSend, startNum);
                    return true;

                case "QueueReceiveJobs":
                    var qFileNames = GetArg<List<string>>(args, 0);
                    int qRecMachineId = GetArg<int>(args, 1);
                    var recMachine = _machineRepo.GetAllMachines().Find(m => m.Id == qRecMachineId);
                    if (recMachine != null) _ftpService.QueueReceiveJobs(qFileNames, recMachine);
                    return true;

                case "GetActiveFtpJobs":
                    return _ftpService.Jobs.ToList();

                // -- RAPORLAR VE DASHBOARD --
                case "GetProductionReport":
                    Console.WriteLine("[Gateway] GetProductionReport İsteği Geldi.");

                    // 1. Filtreleri Çözümle
                    var filters = GetArg<ReportFilters>(args, 0);

                    if (filters == null)
                    {
                        Console.WriteLine("[Gateway] HATA: Filtreler (ReportFilters) NULL geldi! Parametre okunamadı.");
                        return new List<ProductionReportItem>();
                    }

                    Console.WriteLine($"[Gateway] Filtreler -> Başlangıç: {filters.StartTime}, Bitiş: {filters.EndTime}, MakineId: {filters.MachineId}");

                    // 2. Veritabanından Sorgula
                    List<ProductionReportItem> data = null;
                    try
                    {
                        data = _productionRepo.GetProductionReport(filters);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Gateway] REPO HATASI: Veritabanı sorgusunda hata oluştu: {ex.Message}");
                        throw; // Hatayı Hub'a fırlat ki orada da görelim
                    }

                    // 3. Sonucu Kontrol Et
                    if (data == null)
                    {
                        Console.WriteLine("[Gateway] UYARI: Repo 'null' döndü.");
                        return new List<ProductionReportItem>();
                    }

                    Console.WriteLine($"[Gateway] BAŞARILI: Veritabanından {data.Count} adet kayıt çekildi.");

                    // Veri varsa ilk kaydın dolu olup olmadığını kontrol et (Entity Framework Lazy Loading sorunu olabilir)
                    if (data.Count > 0)
                    {
                        var first = data[0];
                        Console.WriteLine($"[Gateway] Örnek Veri -> Batch: {first.BatchId}, Ürün: {first.MusteriNo}, Miktar: {first.MachineId}");
                    }

                    return data;

                case "GetAlarmReport":
                    var rfAlarm = GetArg<ReportFilters>(args, 0);
                    return _alarmRepo.GetAlarmReport(rfAlarm.StartTime.Date, rfAlarm.EndTime.Date.AddDays(1), rfAlarm.MachineId);

                case "GetTrendData":
                    var rfTrend = GetArg<ReportFilters>(args, 0);
                    if (rfTrend.MachineId == null) return new List<ProcessLogRepository.ProcessDataPoint>();
                    return _processLogRepo.GetLogsForDateRange(rfTrend.MachineId.Value, rfTrend.StartTime.Date, rfTrend.EndTime.Date.AddDays(1));

                case "GetManualConsumptionReport":
                    var rfMan = GetArg<ReportFilters>(args, 0);
                    if (rfMan.MachineId == null) return null;
                    // UTC/Local dönüşümü gerekebilir, burada güvenli tarih kullanıyoruz
                    var manStartTime = DateTime.SpecifyKind(rfMan.StartTime.Date, DateTimeKind.Unspecified);
                    var manEndTime = DateTime.SpecifyKind(rfMan.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                    var machineForMan = _machineRepo.GetAllMachines().Find(m => m.Id == rfMan.MachineId);
                    return _processLogRepo.GetManualConsumptionSummary(rfMan.MachineId.Value, machineForMan?.MachineName ?? "Bilinmeyen", manStartTime, manEndTime);

                case "GetConsumptionTotalsForPeriod":
                    var rfTot = GetArg<ReportFilters>(args, 0);
                    return _productionRepo.GetConsumptionTotalsForPeriod(rfTot.StartTime.Date, rfTot.EndTime.Date.AddDays(1).AddTicks(-1));

                case "GetGeneralDetailedConsumptionReport":
                    var rfGen = GetArg<GeneralDetailedConsumptionFilters>(args, 0);
                    if (rfGen.MachineIds == null || rfGen.MachineIds.Count == 0) return new List<ProductionReportItem>();

                    // Paralel işleme gerek yok, basit döngü yeterli
                    List<ProductionReportItem> combinedResults = new List<ProductionReportItem>();
                    foreach (var currentMachineId in rfGen.MachineIds)
                    {
                        var singleFilter = new ReportFilters { StartTime = rfGen.StartTime, EndTime = rfGen.EndTime, MachineId = currentMachineId };
                        var mReport = _productionRepo.GetProductionReport(singleFilter);
                        combinedResults.AddRange(mReport.FindAll(item => item.EndTime != DateTime.MinValue));
                    }
                    return combinedResults;

                case "GetActionLogs":
                    var rfLog = GetArg<ActionLogFilters>(args, 0);
                    return _userRepo.GetActionLogs(rfLog.StartTime, rfLog.EndTime, rfLog.Username, rfLog.Details);

                case "GetProductionDetail":
                    // Bu mantık karmaşık olduğu için aynen korundu
                    return GetProductionDetailInternal(GetArg<int>(args, 0), GetArg<string>(args, 1));

                // -- DASHBOARD --
                case "GetOeeReport": var rfOee = GetArg<ReportFilters>(args, 0); return _dashboardRepo.GetOeeReport(rfOee.StartTime, rfOee.EndTime, rfOee.MachineId);
                case "GetHourlyFactoryConsumption": return new List<HourlyConsumptionData>(); // TODO: Repo'dan doldur
                case "GetHourlyAverageOee": return new List<HourlyOeeData>(); // TODO: Repo'dan doldur
                case "GetTopAlarmsByFrequency": return _alarmRepo.GetTopAlarmsByFrequency(DateTime.Now.AddDays(-1), DateTime.Now);

                // -- EXCEL EXPORT (DİKKAT: Byte[] döner, chunking çok önemlidir) --
                case "ExportProductionReport": return ExcelExportHelper.ExportProductionReportToExcel(GetArg<List<ProductionReportItem>>(args, 0));
                case "ExportAlarmReport": return ExcelExportHelper.ExportAlarmReportToExcel(GetArg<List<AlarmReportItem>>(args, 0));
                case "ExportOeeReport": return ExcelExportHelper.ExportOeeReportToExcel(GetArg<List<OeeData>>(args, 0));
                case "ExportManualConsumptionReport": return ExcelExportHelper.ExportManualConsumptionReportToExcel(GetArg<ManualConsumptionSummary>(args, 0));
                case "ExportActionLogsReport": return ExcelExportHelper.ExportActionLogsReportToExcel(GetArg<List<ActionLogEntry>>(args, 0));
                case "ExportGeneralDetailedConsumptionReport":
                    var genDetailDto = GetArg<GeneralConsumptionExportDto>(args, 0);
                    return genDetailDto != null ? ExcelExportHelper.ExportGeneralDetailedConsumptionReportToExcel(genDetailDto.Items, genDetailDto.ConsumptionType) : Array.Empty<byte>();

                case "ExportProductionDetailFile":
                    // Detay export işlemi
                    return ExportProductionDetailInternal(GetArg<int>(args, 0), GetArg<string>(args, 1));

                // -- ALARM TANIMLARI --
                case "GetAllAlarmDefinitions": return _alarmRepo.GetAllAlarmDefinitions();
                case "AddAlarmDefinition": _alarmRepo.AddAlarmDefinition(GetArg<AlarmDefinition>(args, 0)); return true;
                case "UpdateAlarmDefinition": _alarmRepo.UpdateAlarmDefinition(GetArg<AlarmDefinition>(args, 0)); return true;
                case "DeleteAlarmDefinition": _alarmRepo.DeleteAlarmDefinition(GetArg<int>(args, 0)); return true;

                // -- PLC OPERATORLERİ --
                case "GetPlcOperators": return _plcOpRepo.GetAll();
                case "SaveOrUpdateOperator": _plcOpRepo.SaveOrUpdate(GetArg<PlcOperator>(args, 0)); return true;
                case "AddDefaultOperator": _plcOpRepo.AddDefaultOperator(); return true;
                case "DeleteOperator": _plcOpRepo.Delete(GetArg<int>(args, 0)); return true;

                default: throw new Exception($"Gateway: Bilinmeyen Metot -> {method}");
            }
        }

        // Yardımcı Metot: Argümanları Güvenli Çevirme
        private T GetArg<T>(object[] args, int index)
        {
            if (args == null || index >= args.Length) return default;

            // SignalR genellikle object[] içindeki kompleks tipleri JsonElement olarak gönderir
            if (args[index] is JsonElement jsonElement)
            {
                // Statik options kullanarak performans kazanıyoruz
                return jsonElement.Deserialize<T>(_jsonOptions);
            }
            return (T)args[index];
        }

        // Yardımcı: Kod tekrarını önlemek için Production Detail mantığını ayırdım
        private ProductionDetailDto GetProductionDetailInternal(int machineId, string batchId)
        {
            var headerFilter = new ReportFilters { MachineId = machineId, BatchNo = batchId, StartTime = DateTime.MinValue, EndTime = DateTime.MaxValue };
            var reportList = _productionRepo.GetProductionReport(headerFilter);
            var reportItem = reportList.Count > 0 ? reportList[0] : null;

            if (reportItem == null) return null;

            // Adımlar
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
                Temperature = 90.5 // TODO: Gerçek veri ile değiştir
            }).ToList();

            // Alarmlar
            var rawAlarms = _alarmRepo.GetAlarmDetailsForBatch(batchId, machineId);
            var alarmDtos = rawAlarms.Select((a, index) => new AlarmDetailDto
            {
                AlarmTime = DateTime.Now.AddMinutes(-index * 5), // TODO: Gerçek zaman
                AlarmType = "Makine Alarmı",
                AlarmDescription = a.AlarmDescription,
                Duration = TimeSpan.FromMinutes(1)
            }).ToList();

            // Loglar (Trend verisi)
            var rawLogs = _processLogRepo.GetLogsForBatch(machineId, batchId);
            var logDtos = rawLogs.Select(p => new TrendDataPoint
            {
                Timestamp = p.Timestamp,
                Temperature = (double)p.Temperature,
                Rpm = (double)p.Rpm,
                WaterLevel = (double)p.WaterLevel
            }).ToList();

            return new ProductionDetailDto { Header = reportItem, Steps = stepDtos, Alarms = alarmDtos, LogData = logDtos };
        }

        // Yardımcı: Excel Export Detay
        private byte[] ExportProductionDetailInternal(int machineId, string batchId)
        {
            var detail = GetProductionDetailInternal(machineId, batchId);
            if (detail == null) return Array.Empty<byte>();

            // DTO dönüşümü (ExcelHelper'ın kendi DTO'su varsa ona maplenmeli)
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
                LogData = new List<ExcelExportHelper.TrendDataPoint>() // Genellikle detay exportta grafik verisi ham olarak istenmez
            };

            return ExcelExportHelper.ExportProductionDetailToExcel(excelDto);
        }
    }
}