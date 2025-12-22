using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;
using static TekstilScada.Core.Core.ExcelExportHelper;
// Diğer gerekli namespace'ler (Core altında değilse ekleyin)
// using TekstilScada.Repositories; // Artık kullanılmıyor
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

namespace TekstilScada.WebAPI.Hubs
{
    public class ScadaHub : Hub
    {
        // --- GATEWAY YÖNETİMİ ---
        private static readonly ConcurrentDictionary<string, StringBuilder> _chunkBuffers = new();
        // Gateway (Windows Forms) ConnectionId
        private static string? _gatewayConnectionId;

        // Bekleyen istekler: <RequestId, TaskCompletionSource>
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<object?>> _pendingRequests = new();

        // 1. Gateway Kaydı: Windows Forms bağlanınca çağırır
        public void RegisterGateway()
        {
            _gatewayConnectionId = Context.ConnectionId;
            System.Diagnostics.Debug.WriteLine($"[ScadaHub] Gateway Kayıt Oldu: {_gatewayConnectionId}");
        }

        // Bağlantı Koptuğunda
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.ConnectionId == _gatewayConnectionId)
            {
                _gatewayConnectionId = null;
                System.Diagnostics.Debug.WriteLine("[ScadaHub] Gateway Koptu! İstekler iptal ediliyor.");

                foreach (var item in _pendingRequests)
                {
                    item.Value.TrySetException(new Exception("Gateway bağlantısı koptu."));
                }
                _pendingRequests.Clear();
            }
            return base.OnDisconnectedAsync(exception);
        }

        // --- İSTEK YÖNLENDİRME MOTORU (CORE) ---

        private async Task<T?> InvokeOnGateway<T>(string targetMethod, params object[] args)
        {
            Console.WriteLine($"[Hub] InvokeOnGateway Çağrıldı. Hedef Metot: {targetMethod}");

            if (string.IsNullOrEmpty(_gatewayConnectionId))
            {
                throw new Exception("Gateway (Ana Makine) bağlı değil.");
            }

            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Raporlar için süreyi artırdık
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            cts.Token.Register(() =>
            {
                if (_pendingRequests.TryRemove(requestId, out var pendingTcs))
                {
                    pendingTcs.TrySetException(new TimeoutException("Gateway'den cevap alınamadı (Timeout)."));
                }
            }, useSynchronizationContext: false);

            _pendingRequests[requestId] = tcs;

            try
            {
                await Clients.Client(_gatewayConnectionId).SendAsync("HandleRequest", requestId, targetMethod, args);

                var result = await tcs.Task;

                if (result == null) return default;

                // --- JSON STRING ÇÖZÜMLEME (GÜNCELLENMİŞ KISIM) ---
                if (result is string jsonString)
                {
                    try
                    {
                        // Gateway ile BİREBİR AYNI ayarları kullanmalıyız
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,

                            // --- BU İKİ SATIR EKSİK OLDUĞU İÇİN HATA ALIYORSUNUZ ---
                            ReferenceHandler = ReferenceHandler.IgnoreCycles, // Döngüsel referans hatasını çözer
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Türkçe karakterleri (İ, ş, ğ) kabul eder
                                                                                  // -------------------------------------------------------
                        };

                        return JsonSerializer.Deserialize<T>(jsonString, options);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ScadaHub] JSON Çevirme Hatası ({targetMethod}): {ex.Message}");
                        // Hatanın nerede olduğunu görmek için JSON'un bir kısmını yazdıralım
                        if (jsonString.Length > 200)
                            Console.WriteLine($"Hatalı JSON (Başlangıç): {jsonString.Substring(0, 200)}...");
                        else
                            Console.WriteLine($"Hatalı JSON: {jsonString}");

                        return default;
                    }
                }
                // --------------------------------------------------

                if (result is JsonElement jsonElement)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Buraya da ekleyin
                    };
                    return jsonElement.Deserialize<T>(options);
                }

                return (T)result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScadaHub] Invoke Hatası: {ex.Message}");
                return default;
            }
            finally
            {
                _pendingRequests.TryRemove(requestId, out _);
                _chunkBuffers.TryRemove(requestId, out _);
                cts.Dispose();
            }
        }

        // --- YENİ EKLENECEK METOT: PARÇA ALICI ---
        // Gateway bu metodu çağırarak veriyi dilim dilim gönderecek
        public void ReceiveResponseChunk(string requestId, string chunk, bool isLast)
        {
            // 1. Buffer'ı al veya oluştur
            var buffer = _chunkBuffers.GetOrAdd(requestId, _ => new StringBuilder());

            // 2. Gelen parçayı ekle
            lock (buffer)
            {
                buffer.Append(chunk);
            }

            // 3. Son parça mı?
            if (isLast)
            {
                if (_pendingRequests.TryGetValue(requestId, out var tcs))
                {
                    // Veriyi tamamlanmış string olarak teslim et
                    // InvokeOnGateway içindeki JSON Deserialize bunu yakalayacak
                    tcs.TrySetResult(buffer.ToString());
                }
                // Buffer temizliği finally bloğunda yapılacak
            }
        }

        // 2. Gateway Cevabı: WinForms işlemi bitirince buraya döner
        public void SendResponseToHub(string requestId, object? data, string? errorMessage)
        {
            if (_pendingRequests.TryGetValue(requestId, out var tcs))
            {
                if (!string.IsNullOrEmpty(errorMessage))
                    tcs.TrySetException(new Exception(errorMessage));
                else
                    tcs.TrySetResult(data);
            }
        }
        // --- MAKİNE YÖNETİMİ ---
        public async Task<List<Machine>> GetAllMachines()
            => await InvokeOnGateway<List<Machine>>("GetAllMachines") ?? new List<Machine>();

        public async Task<FullMachineStatus?> GetMachineStatus(int id)
            => await InvokeOnGateway<FullMachineStatus>("GetMachineStatus", id);

        public async Task AddMachine(Machine machine)
        {
            await InvokeOnGateway<bool>("AddMachine", machine);
            await Clients.All.SendAsync("MachineListUpdated");
        }
        public async Task UpdateMachine(Machine machine)
        {
            await InvokeOnGateway<bool>("UpdateMachine", machine);
            await Clients.All.SendAsync("MachineListUpdated");
        }
        public async Task DeleteMachine(int id)
        {
            await InvokeOnGateway<bool>("DeleteMachine", id);
            await Clients.All.SendAsync("MachineListUpdated");
        }

        // --- KULLANICI YÖNETİMİ ---
        public async Task<List<User>> GetUsers()
            => await InvokeOnGateway<List<User>>("GetAllUsers") ?? new List<User>();

        public async Task<List<Role>> GetRoles()
            => await InvokeOnGateway<List<Role>>("GetAllRoles") ?? new List<Role>();

        public async Task AddUser(UserViewModel model) => await InvokeOnGateway<bool>("AddUser", model);
        public async Task UpdateUser(UserViewModel model) => await InvokeOnGateway<bool>("UpdateUser", model);
        public async Task DeleteUser(int id) => await InvokeOnGateway<bool>("DeleteUser", id);

        // --- MALİYET YÖNETİMİ ---
        public async Task<List<CostParameter>> GetCosts()
            => await InvokeOnGateway<List<CostParameter>>("GetCosts") ?? new List<CostParameter>();

        public async Task UpdateCosts(List<CostParameter> costs) => await InvokeOnGateway<bool>("UpdateParameters", costs);

        // --- ALARM TANIMLARI (Yönetim) ---
        public async Task<List<AlarmDefinition>> GetAlarms()
            => await InvokeOnGateway<List<AlarmDefinition>>("GetAllAlarmDefinitions") ?? new List<AlarmDefinition>();

        public async Task AddAlarm(AlarmDefinition alarm) => await InvokeOnGateway<bool>("AddAlarmDefinition", alarm);
        public async Task UpdateAlarm(AlarmDefinition alarm) => await InvokeOnGateway<bool>("UpdateAlarmDefinition", alarm);
        public async Task DeleteAlarm(int id) => await InvokeOnGateway<bool>("DeleteAlarmDefinition", id);
        // --- REÇETE İŞLEMLERİ ---
        public async Task<List<ScadaRecipe>> GetRecipes()
            => await InvokeOnGateway<List<ScadaRecipe>>("GetAllRecipes") ?? new List<ScadaRecipe>();

        public async Task<ScadaRecipe?> GetRecipeDetails(int id)
            => await InvokeOnGateway<ScadaRecipe>("GetRecipeById", id);

        public async Task SaveRecipe(ScadaRecipe recipe) => await InvokeOnGateway<bool>("SaveRecipe", recipe);
        public async Task DeleteRecipe(int id) => await InvokeOnGateway<bool>("DeleteRecipe", id);

        public async Task<List<ProductionReportItem>> GetRecipeConsumptionHistory(int recipeId)
            => await InvokeOnGateway<List<ProductionReportItem>>("GetRecipeUsageHistory", recipeId) ?? new List<ProductionReportItem>();

        // --- PLC İLE REÇETE TRANSFERİ ---
        public async Task<bool> SendRecipeToPlc(int recipeId, int machineId)
            => await InvokeOnGateway<bool>("SendRecipeToPlc", recipeId, machineId);

        public async Task<ScadaRecipe?> ReadRecipeFromPlc(int machineId)
            => await InvokeOnGateway<ScadaRecipe>("ReadRecipeFromPlc", machineId);

        // --- REÇETE TASARIMCISI (DESIGNER) ---
        // Daha önce 'json' controller ve RecipeConfigurationRepository ile yapılan işler

        public async Task<List<string>> GetMachineSubTypesDesign()
            => await InvokeOnGateway<List<string>>("GetMachineSubTypes");

        // DTO tanımları Client ve Gateway'de ortak olmalı
        public async Task<List<StepTypeDtoDesign>> GetStepTypesDesign()
            => await InvokeOnGateway<List<StepTypeDtoDesign>>("GetStepTypes");

        public async Task<List<ControlMetadata>> GetLayoutDesign(string subType, int stepTypeId)
            => await InvokeOnGateway<List<ControlMetadata>>("GetLayoutJson", subType, stepTypeId) ?? new List<ControlMetadata>();

        public async Task<bool> SaveLayoutDesign(string subType, int stepTypeId, List<ControlMetadata> layout)
            => await InvokeOnGateway<bool>("SaveLayout", subType, stepTypeId, layout);
        // --- PLC OPERATÖRLERİ ---
        public async Task<List<PlcOperator>> GetPlcOperators()
            => await InvokeOnGateway<List<PlcOperator>>("GetPlcOperators") ?? new List<PlcOperator>();

        public async Task SavePlcOperator(PlcOperator op) => await InvokeOnGateway<bool>("SaveOrUpdateOperator", op);
        public async Task AddDefaultPlcOperator() => await InvokeOnGateway<bool>("AddDefaultOperator");
        public async Task DeletePlcOperator(int id) => await InvokeOnGateway<bool>("DeleteOperator", id);

        // --- FTP ve DOSYA TRANSFER ---
        public async Task<Dictionary<int, string>> GetHmiRecipeNames(int machineId)
            => await InvokeOnGateway<Dictionary<int, string>>("GetHmiRecipeNames", machineId) ?? new Dictionary<int, string>();

        public async Task<ScadaRecipe?> GetHmiRecipePreview(int machineId, string fileName)
            => await InvokeOnGateway<ScadaRecipe>("GetHmiRecipePreview", machineId, fileName);

        public async Task<bool> QueueSequentiallyNamedSendJobs(List<int> recipeIds, List<int> machineIds, int startNumber)
            => await InvokeOnGateway<bool>("QueueSequentiallyNamedSendJobs", recipeIds, machineIds, startNumber);

        public async Task<bool> QueueReceiveJobs(List<string> fileNames, int machineId)
            => await InvokeOnGateway<bool>("QueueReceiveJobs", fileNames, machineId);

        public async Task<List<TransferJob>> GetActiveJobs()
            => await InvokeOnGateway<List<TransferJob>>("GetActiveFtpJobs") ?? new List<TransferJob>();

        // --- DASHBOARD VERİLERİ ---
        public async Task<List<OeeData>> GetOeeReport(ReportFilters filters)
            => await InvokeOnGateway<List<OeeData>>("GetOeeReport", filters) ?? new List<OeeData>();

        public async Task<List<HourlyConsumptionData>> GetHourlyConsumption()
            => await InvokeOnGateway<List<HourlyConsumptionData>>("GetHourlyFactoryConsumption") ?? new List<HourlyConsumptionData>();

        public async Task<List<HourlyOeeData>> GetHourlyOee()
            => await InvokeOnGateway<List<HourlyOeeData>>("GetHourlyAverageOee") ?? new List<HourlyOeeData>();

        public async Task<List<TopAlarmData>> GetTopAlarms()
            => await InvokeOnGateway<List<TopAlarmData>>("GetTopAlarmsByFrequency") ?? new List<TopAlarmData>();
        // --- RAPORLAR ---
        public async Task<List<ProductionReportItem>> GetProductionReport(ReportFilters filters)
        {
            // --- BU LOGU EKLEYİN ---
            System.Diagnostics.Debug.WriteLine("[Hub] HALKA AÇIK METOT TETİKLENDİ: GetProductionReport");
            System.Diagnostics.Debug.WriteLine($"[Hub] Gelen Filtre: MakineID={filters?.MachineId}");
            // -----------------------

            // Eski tek satırlık kodu buraya taşıyoruz:
            return await InvokeOnGateway<List<ProductionReportItem>>("GetProductionReport", filters)
                   ?? new List<ProductionReportItem>();
        }
        public async Task<List<AlarmReportItem>> GetAlarmReport(ReportFilters filters)
            => await InvokeOnGateway<List<AlarmReportItem>>("GetAlarmReport", filters) ?? new List<AlarmReportItem>();

        // object veya dynamic dönüyoruz çünkü Gateway'den List<ProcessDataPoint> gelecek
        public async Task<object> GetTrendData(ReportFilters filters)
            => await InvokeOnGateway<object>("GetTrendData", filters) ?? new List<object>();

        public async Task<ManualConsumptionSummary?> GetManualConsumptionReport(ReportFilters filters)
            => await InvokeOnGateway<ManualConsumptionSummary>("GetManualConsumptionReport", filters);

        public async Task<ConsumptionTotals?> GetConsumptionTotals(ReportFilters filters)
            => await InvokeOnGateway<ConsumptionTotals>("GetConsumptionTotalsForPeriod", filters);

        public async Task<List<ProductionReportItem>> GetGeneralDetailedConsumptionReport(GeneralDetailedConsumptionFilters filters)
            => await InvokeOnGateway<List<ProductionReportItem>>("GetGeneralDetailedConsumptionReport", filters) ?? new List<ProductionReportItem>();

        public async Task<List<ActionLogEntry>> GetActionLogs(ActionLogFilters filters)
            => await InvokeOnGateway<List<ActionLogEntry>>("GetActionLogs", filters) ?? new List<ActionLogEntry>();

        public async Task<ProductionDetailDto?> GetProductionDetail(int machineId, string batchId)
            => await InvokeOnGateway<ProductionDetailDto>("GetProductionDetail", machineId, batchId);

        // --- EXPORT İŞLEMLERİ (Gateway byte[] üretip döner) ---
        public async Task<byte[]> ExportProductionReport(List<ProductionReportItem> items) => await InvokeOnGateway<byte[]>("ExportProductionReport", items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportAlarmReport(List<AlarmReportItem> items) => await InvokeOnGateway<byte[]>("ExportAlarmReport", items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportOeeReport(List<OeeData> items) => await InvokeOnGateway<byte[]>("ExportOeeReport", items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportManualConsumptionReport(ManualConsumptionSummary summary) => await InvokeOnGateway<byte[]>("ExportManualConsumptionReport", summary) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportGeneralDetailedConsumptionReport(GeneralConsumptionExportDto data) => await InvokeOnGateway<byte[]>("ExportGeneralDetailedConsumptionReport", data) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportActionLogsReport(List<ActionLogEntry> logs) => await InvokeOnGateway<byte[]>("ExportActionLogsReport", logs) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportProductionDetailFile(int machineId, string batchId) => await InvokeOnGateway<byte[]>("ExportProductionDetailFile", machineId, batchId) ?? Array.Empty<byte>();

        // --- LOGLAMA ve YAYIN ---

        // Loglama tek yönlüdür (Fire-and-forget), cevap beklenmez
        public async Task LogAction(ActionLogEntry entry)
        {
            if (!string.IsNullOrEmpty(_gatewayConnectionId))
            {
                await Clients.Client(_gatewayConnectionId).SendAsync("HandleLogAction", entry);
            }
        }

        // Canlı veri yayını (Gateway -> Web Clientlar)
        public async Task BroadcastFromLocal(FullMachineStatus status)
        {
            if (status == null) return;
            await Clients.Others.SendAsync("ReceiveMachineUpdate", status);
        }

        // Komut Gönderimi (Web -> Gateway) - Doğrudan sinyal gönderimi
        public async Task SendCommandToLocal(int machineId, string command, string parameters)
        {
            await Clients.All.SendAsync("ReceiveCommand", machineId, command, parameters);
        }
    }
}