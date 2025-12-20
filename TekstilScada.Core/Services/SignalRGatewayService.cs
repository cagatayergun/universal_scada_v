using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq; // EKLENDİ: ToList() ve LINQ sorguları için şart
using System.Text.Json;
using System.Threading.Tasks;
using TekstilScada.Core;
using TekstilScada.Core.Core;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Text.Json.Serialization;
using static TekstilScada.Core.Core.ExcelExportHelper;

// NOT: Yukarıdaki "HourlyConsumptionData" vb. sınıflar projenizde zaten varsa 
// burada tekrar tanımlamayın. Eğer yoksa, onları ayrı bir "Models" klasörüne 
// taşımanız en sağlıklı yöntemdir.
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

        // Repository Referansları
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

        // Servis Referansları
        private readonly PlcPollingService _plcService;
        private readonly FtpTransferService _ftpService;

        // CloudSync (Canlı Yayın) Özellikleri
        public event Action<int, string, string> OnRemoteCommandReceived;
        private DateTime _lastSentTime = DateTime.MinValue;
        private readonly int _sendIntervalMs = 500; // Veri gönderme sıklığı

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

            // Bağlantı Oluşturma
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    // Token null ise ekleme (Hata almamak için)
                    if (!string.IsNullOrEmpty(jwtToken))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(jwtToken);
                    }

                    // SSL Bypass (Geliştirme ortamı için)
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is System.Net.Http.HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };

                    // Gateway tarafında da büyük veri alıp göndermek için limitleri artırıyoruz
                    options.ApplicationMaxBufferSize = 100 * 1024 * 1024; // 100 MB
                    options.TransportMaxBufferSize = 100 * 1024 * 1024;   // 100 MB
                })
                .WithAutomaticReconnect()
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
                    Console.WriteLine("[Gateway] SignalR Bağlandı.");

                    // 1. Kendini Gateway olarak tanıt
                    await _connection.InvokeAsync("RegisterGateway");

                    // 2. Canlı Veri Akışına Abone Ol
                    _plcService.OnMachineDataRefreshed += OnLocalDataRefreshed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gateway] Bağlantı Hatası: {ex.Message}");
            }
        }

        private void RegisterHandlers()
        {
            // İSTEK YÖNETİMİ
            _connection.On<string, string, object[]>("HandleRequest", async (reqId, method, args) =>
            {
                object result = null;
                string errorMessage = null;

                try
                {
                    result = await ProcessRequest(method, args);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Console.WriteLine($"[Gateway] İşlem Hatası ({method}): {ex.Message}");
                }

                // Chunking (Parçalama) destekli gönderim
                await SendLargeDataAsync(reqId, result, errorMessage);
            });

            // LOGLAMA
            _connection.On<ActionLogEntry>("HandleLogAction", (entry) =>
            {
                try { _userRepo.LogAction(entry.UserId, entry.ActionType, entry.Details); } catch { }
            });

            // KOMUT ALMA
            _connection.On<int, string, string>("ReceiveCommand", (machineId, command, parameters) =>
            {
                Console.WriteLine($"[Gateway] Komut Alındı: {command}");
                OnRemoteCommandReceived?.Invoke(machineId, command, parameters);
            });

            // BAĞLANTI DURUMLARI
            _connection.Closed += async (error) => {
                Console.WriteLine("[Gateway] Bağlantı Koptu.");
                await Task.CompletedTask;
            };

            _connection.Reconnected += async (connectionId) => {
                Console.WriteLine("[Gateway] Tekrar Bağlandı. Gateway Yeniden Kaydediliyor...");
                await _connection.InvokeAsync("RegisterGateway");
            };
        }

        // --- CHUNKING (PARÇALAMA) MANTIĞI ---
        private async Task SendLargeDataAsync(string reqId, object result, string errorMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(errorMessage) || result == null)
                {
                    await _connection.InvokeAsync("SendResponseToHub", reqId, result, errorMessage);
                    return;
                }

                // --- DÜZELTME BURADA ---
                // İlişkili tablolarda sonsuz döngü hatasını engellemek için ayar yapıyoruz
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, // <--- KRİTİK AYAR
                    WriteIndented = false,
                    PropertyNameCaseInsensitive = true
                };

                // Veriyi JSON String'e çevir (Bu ayarlar ile)
                string json = JsonSerializer.Serialize(result, options);
                // -----------------------

                const int chunkSize = 30 * 1024; // 30 KB

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
                        bool isLast = (offset >= totalLength);

                        await _connection.InvokeAsync("ReceiveResponseChunk", reqId, chunk, isLast);
                        await Task.Delay(2); // Gecikmeyi biraz azalttık (Hız için)
                    }
                    // Console.WriteLine($"[Gateway] Veri parçalı gönderildi. ID: {reqId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gateway] Chunk Gönderim Hatası: {ex.Message}");
                // Hatayı Hub'a bildir ki sonsuza kadar beklemesin
                await _connection.InvokeAsync("SendResponseToHub", reqId, null, "Serialization Error: " + ex.Message);
            }
        }

        // --- CANLI YAYIN ---
        private async void OnLocalDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (_connection.State != HubConnectionState.Connected || status == null) return;

            // Throttling
            if ((DateTime.Now - _lastSentTime).TotalMilliseconds < _sendIntervalMs) return;

            try
            {
                await _connection.InvokeAsync("BroadcastFromLocal", status);
                _lastSentTime = DateTime.Now;
            }
            catch { /* Sessiz Hata */ }
        }

        // --- İSTEK İŞLEME MERKEZİ ---
        private async Task<object> ProcessRequest(string method, object[] args)
        {
            switch (method)
            {
                case "GetAllMachines": return _machineRepo.GetAllMachines();
                case "GetMachineStatus":
                    int mId = GetArg<int>(args, 0);
                    var m = _machineRepo.GetAllMachines().Find(x => x.Id == mId);
                    if (m == null) return null;
                    return new FullMachineStatus { MachineId = m.Id, MachineName = m.MachineName, MakineTipi = m.MachineSubType };

                case "AddMachine": _machineRepo.AddMachine(GetArg<Machine>(args, 0)); return true;
                case "UpdateMachine": _machineRepo.UpdateMachine(GetArg<Machine>(args, 0)); return true;
                case "DeleteMachine": _machineRepo.DeleteMachine(GetArg<int>(args, 0)); return true;

                case "GetCosts": return _costRepo.GetAllParameters();
                case "UpdateParameters": _costRepo.UpdateParameters(GetArg<List<CostParameter>>(args, 0)); return true;

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

                case "GetAllRecipes": return _recipeRepo.GetAllRecipes();
                case "GetRecipeById": return _recipeRepo.GetRecipeById(GetArg<int>(args, 0));
                case "SaveRecipe": _recipeRepo.SaveRecipe(GetArg<ScadaRecipe>(args, 0)); return true;
                case "DeleteRecipe": _recipeRepo.DeleteRecipe(GetArg<int>(args, 0)); return true;
                case "GetRecipeUsageHistory": return _recipeRepo.GetRecipeUsageHistory(GetArg<int>(args, 0));

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
                    var json = JsonSerializer.Serialize(layoutList, new JsonSerializerOptions { WriteIndented = true });
                    string subType = GetArg<string>(args, 0);
                    int stepId = GetArg<int>(args, 1);
                    _configRepo.SaveLayout($"{subType} - StepID:{stepId}", subType, stepId, json);
                    return true;

                case "SendRecipeToPlc":
                    int rId = GetArg<int>(args, 0);
                    int mIdPlc = GetArg<int>(args, 1);
                    var recipeToSend = _recipeRepo.GetRecipeById(rId);
                    if (recipeToSend == null) return false;
                    if (_plcService.GetPlcManagers().TryGetValue(mIdPlc, out var mgrSend))
                    {
                        var result = await mgrSend.WriteRecipeToPlcAsync(recipeToSend);
                        return result.IsSuccess;
                    }
                    return false;

                case "ReadRecipeFromPlc":
                    int mIdRead = GetArg<int>(args, 0);
                    if (_plcService.GetPlcManagers().TryGetValue(mIdRead, out var mgrRead))
                    {
                        var res = await mgrRead.ReadRecipeFromPlcAsync();
                        if (res.IsSuccess && res.Content != null)
                        {
                            var newR = new ScadaRecipe { RecipeName = $"PLC_OKUNAN_{DateTime.Now:HHmm}", Steps = new List<ScadaRecipeStep>() };
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
                    // ToList() için System.Linq gereklidir.
                    return _ftpService.Jobs.ToList();

                case "GetProductionReport":
                    return _productionRepo.GetProductionReport(GetArg<ReportFilters>(args, 0));

                case "GetAlarmReport":
                    var rfAlarm = GetArg<ReportFilters>(args, 0);
                    var alarmEndTime = rfAlarm.EndTime.Date.AddDays(1);
                    return _alarmRepo.GetAlarmReport(rfAlarm.StartTime.Date, alarmEndTime, rfAlarm.MachineId);

                case "GetTrendData":
                    var rfTrend = GetArg<ReportFilters>(args, 0);
                    if (rfTrend.MachineId == null) return new List<ProcessLogRepository.ProcessDataPoint>();
                    var trendEndTime = rfTrend.EndTime.Date.AddDays(1);
                    return _processLogRepo.GetLogsForDateRange(rfTrend.MachineId.Value, rfTrend.StartTime.Date, trendEndTime);

                case "GetManualConsumptionReport":
                    var rfMan = GetArg<ReportFilters>(args, 0);
                    if (rfMan.MachineId == null) return null;
                    var manStartTime = DateTime.SpecifyKind(rfMan.StartTime.Date, DateTimeKind.Unspecified);
                    var manEndTime = DateTime.SpecifyKind(rfMan.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                    var machineForMan = _machineRepo.GetAllMachines().Find(m => m.Id == rfMan.MachineId);
                    string machineName = machineForMan?.MachineName ?? "Bilinmeyen Makine";
                    return _processLogRepo.GetManualConsumptionSummary(rfMan.MachineId.Value, machineName, manStartTime, manEndTime);

                case "GetConsumptionTotalsForPeriod":
                    var rfTot = GetArg<ReportFilters>(args, 0);
                    var totStartTime = DateTime.SpecifyKind(rfTot.StartTime.Date, DateTimeKind.Unspecified);
                    var totEndTime = DateTime.SpecifyKind(rfTot.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                    return _productionRepo.GetConsumptionTotalsForPeriod(totStartTime, totEndTime);

                case "GetGeneralDetailedConsumptionReport":
                    var rfGen = GetArg<GeneralDetailedConsumptionFilters>(args, 0);
                    if (rfGen.MachineIds == null || rfGen.MachineIds.Count == 0) return new List<ProductionReportItem>();
                    var genStartTime = DateTime.SpecifyKind(rfGen.StartTime.Date, DateTimeKind.Unspecified);
                    var genEndTime = DateTime.SpecifyKind(rfGen.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                    List<ProductionReportItem> combinedResults = new List<ProductionReportItem>();
                    foreach (var currentMachineId in rfGen.MachineIds)
                    {
                        var singleFilter = new ReportFilters { StartTime = genStartTime, EndTime = genEndTime, MachineId = currentMachineId };
                        var mReport = _productionRepo.GetProductionReport(singleFilter);
                        combinedResults.AddRange(mReport.FindAll(item => item.EndTime != DateTime.MinValue));
                    }
                    return combinedResults;

                case "GetActionLogs":
                    var rfLog = GetArg<ActionLogFilters>(args, 0);
                    var logStartTime = DateTime.SpecifyKind(rfLog.StartTime.Date, DateTimeKind.Unspecified);
                    var logEndTime = DateTime.SpecifyKind(rfLog.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                    return _userRepo.GetActionLogs(logStartTime, logEndTime, rfLog.Username, rfLog.Details);

                case "GetProductionDetail":
                    int pDetailMachineId = GetArg<int>(args, 0);
                    string pDetailBatchId = GetArg<string>(args, 1);
                    var headerFilter = new ReportFilters { MachineId = pDetailMachineId, BatchNo = pDetailBatchId, StartTime = DateTime.MinValue, EndTime = DateTime.MaxValue };
                    var reportItem = _productionRepo.GetProductionReport(headerFilter).Find(x => true);
                    if (reportItem == null) return null;

                    var rawSteps = _productionRepo.GetProductionStepDetails(pDetailBatchId, pDetailMachineId);
                    var stepDtos = new List<ProductionStepDetailDto>();
                    foreach (var s in rawSteps)
                    {
                        double duration = TimeSpan.TryParse(s.TheoreticalTime, out var tt) ? tt.TotalSeconds : 0;
                        stepDtos.Add(new ProductionStepDetailDto
                        {
                            StepNumber = s.StepNumber,
                            StepName = s.StepName,
                            TheoreticalTime = s.TheoreticalTime,
                            WorkingTime = s.WorkingTime,
                            StopTime = s.StopTime,
                            DeflectionTime = s.DeflectionTime,
                            TheoreticalDurationSeconds = duration,
                            Temperature = 90.5
                        });
                    }

                    var rawAlarms = _alarmRepo.GetAlarmDetailsForBatch(pDetailBatchId, pDetailMachineId);
                    var alarmDtos = new List<AlarmDetailDto>();
                    int alarmIndex = 0;
                    foreach (var a in rawAlarms)
                    {
                        alarmDtos.Add(new AlarmDetailDto
                        {
                            AlarmTime = DateTime.Now.AddMinutes(-alarmIndex * 5),
                            AlarmType = "Makine Alarmı",
                            AlarmDescription = a.AlarmDescription,
                            Duration = TimeSpan.FromMinutes(alarmIndex + 1)
                        });
                        alarmIndex++;
                    }

                    var rawLogs = _processLogRepo.GetLogsForBatch(pDetailMachineId, pDetailBatchId);
                    var logDtos = new List<TrendDataPoint>();
                    foreach (var p in rawLogs)
                        logDtos.Add(new TrendDataPoint { Timestamp = p.Timestamp, Temperature = (double)p.Temperature, Rpm = (double)p.Rpm, WaterLevel = (double)p.WaterLevel });

                    return new ProductionDetailDto { Header = reportItem, Steps = stepDtos, Alarms = alarmDtos, LogData = logDtos, TheoreticalData = new List<TrendDataPoint>() };

                case "GetOeeReport": var rfOee = GetArg<ReportFilters>(args, 0); return _dashboardRepo.GetOeeReport(rfOee.StartTime, rfOee.EndTime, rfOee.MachineId);
                case "GetHourlyFactoryConsumption": return new List<HourlyConsumptionData>();
                case "GetTopAlarmsByFrequency": return _alarmRepo.GetTopAlarmsByFrequency(DateTime.Now.AddDays(-1), DateTime.Now);

                case "ExportProductionReport": return ExcelExportHelper.ExportProductionReportToExcel(GetArg<List<ProductionReportItem>>(args, 0));
                case "ExportAlarmReport": return ExcelExportHelper.ExportAlarmReportToExcel(GetArg<List<AlarmReportItem>>(args, 0));
                case "ExportOeeReport": return ExcelExportHelper.ExportOeeReportToExcel(GetArg<List<OeeData>>(args, 0));
                case "ExportManualConsumptionReport": return ExcelExportHelper.ExportManualConsumptionReportToExcel(GetArg<ManualConsumptionSummary>(args, 0));
                case "ExportGeneralDetailedConsumptionReport":
                    var genDetailDto = GetArg<GeneralConsumptionExportDto>(args, 0);
                    if (genDetailDto == null) return Array.Empty<byte>();
                    return ExcelExportHelper.ExportGeneralDetailedConsumptionReportToExcel(genDetailDto.Items, genDetailDto.ConsumptionType);
                case "ExportActionLogsReport": return ExcelExportHelper.ExportActionLogsReportToExcel(GetArg<List<ActionLogEntry>>(args, 0));

                case "ExportProductionDetailFile":
                    int expMachineId = GetArg<int>(args, 0);
                    string expBatchId = GetArg<string>(args, 1);
                    var expHeaderFilter = new ReportFilters { MachineId = expMachineId, BatchNo = expBatchId, StartTime = DateTime.MinValue, EndTime = DateTime.MaxValue };
                    var expHeader = _productionRepo.GetProductionReport(expHeaderFilter).Find(x => true);
                    if (expHeader == null) return Array.Empty<byte>();
                    var expRawSteps = _productionRepo.GetProductionStepDetails(expBatchId, expMachineId);
                    var expStepDtos = new List<ExcelExportHelper.ProductionStepDetailDto>();
                    foreach (var s in expRawSteps)
                    {
                        double duration = TimeSpan.TryParse(s.TheoreticalTime, out var tt) ? tt.TotalSeconds : 0;
                        expStepDtos.Add(new ExcelExportHelper.ProductionStepDetailDto { StepNumber = s.StepNumber, StepName = s.StepName, TheoreticalTime = s.TheoreticalTime, WorkingTime = s.WorkingTime, StopTime = s.StopTime, DeflectionTime = s.DeflectionTime, TheoreticalDurationSeconds = duration, Temperature = 90.5 });
                    }
                    var expRawAlarms = _alarmRepo.GetAlarmDetailsForBatch(expBatchId, expMachineId);
                    var expAlarmDtos = new List<ExcelExportHelper.AlarmDetailDto>();
                    int expAlarmIndex = 0;
                    foreach (var a in expRawAlarms)
                    {
                        expAlarmDtos.Add(new ExcelExportHelper.AlarmDetailDto { AlarmTime = DateTime.Now.AddMinutes(-expAlarmIndex * 5), AlarmType = "Makine Alarmı", AlarmDescription = a.AlarmDescription, Duration = TimeSpan.FromMinutes(expAlarmIndex + 1) });
                        expAlarmIndex++;
                    }
                    var excelDetailDto = new ExcelExportHelper.ProductionDetailDto { Header = expHeader, Steps = expStepDtos, Alarms = expAlarmDtos, LogData = new List<ExcelExportHelper.TrendDataPoint>(), TheoreticalData = new List<ExcelExportHelper.TrendDataPoint>() };
                    return ExcelExportHelper.ExportProductionDetailToExcel(excelDetailDto);

                case "GetAllAlarmDefinitions": return _alarmRepo.GetAllAlarmDefinitions();
                case "AddAlarmDefinition": _alarmRepo.AddAlarmDefinition(GetArg<AlarmDefinition>(args, 0)); return true;
                case "UpdateAlarmDefinition": _alarmRepo.UpdateAlarmDefinition(GetArg<AlarmDefinition>(args, 0)); return true;
                case "DeleteAlarmDefinition": _alarmRepo.DeleteAlarmDefinition(GetArg<int>(args, 0)); return true;

                case "GetPlcOperators": return _plcOpRepo.GetAll();
                case "SaveOrUpdateOperator": _plcOpRepo.SaveOrUpdate(GetArg<PlcOperator>(args, 0)); return true;
                case "AddDefaultOperator": _plcOpRepo.AddDefaultOperator(); return true;
                case "DeleteOperator": _plcOpRepo.Delete(GetArg<int>(args, 0)); return true;

                default: throw new Exception($"Bilinmeyen Metot: {method}");
            }
        }

        private T GetArg<T>(object[] args, int index)
        {
            if (args == null || index >= args.Length) return default;
            if (args[index] is JsonElement jsonElement)
                return jsonElement.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (T)args[index];
        }
    }
}