using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.WebAPI.Repositories;
using static TekstilScada.Core.Core.ExcelExportHelper;

// --- DTO Sınıfları (Aynen korunuyor) ---
public class ReportFilters
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? MachineId { get; set; }
    public string? BatchNo { get; set; }
}

public enum TransferType { Send, Receive }
public enum TransferStatus { Pending, Transferring, Successful, Failed }

public class TransferJob
{
    public Guid Id { get; set; }
    public Machine Machine { get; set; }
    public ScadaRecipe? LocalRecipe { get; set; }
    public string? RemoteFileName { get; set; }
    public string TargetFileName { get; set; }
    public int RecipeNumber { get; set; }
    public TransferType OperationType { get; set; }
    public TransferStatus Status { get; set; }
    public int Progress { get; set; }
    public string ErrorMessage { get; set; }
    public string MachineName => Machine?.MachineName ?? "";
    public string RecipeName => OperationType == TransferType.Send
                                ? (!string.IsNullOrEmpty(TargetFileName) ? $"{LocalRecipe?.RecipeName} -> {TargetFileName}" : LocalRecipe?.RecipeName)
                                : RemoteFileName;
}
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
        // --- TEMEL BAĞIMLILIKLAR ---
        private readonly CentralFactoryRepository _factoryRepo;

        // --- GATEWAY YÖNETİMİ ---
        // Hangi ConnectionId, hangi Fabrika ID'sine ait?
        private static readonly ConcurrentDictionary<string, int> _gatewayConnections = new();

        // Parçalı veri transferi için buffer
        private static readonly ConcurrentDictionary<string, StringBuilder> _chunkBuffers = new();

        // Bekleyen istekler: <RequestId, TaskCompletionSource>
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<object?>> _pendingRequests = new();
        private static readonly ConcurrentDictionary<int, string> _factoryIps = new();
        // --- TEK VE GEÇERLİ CONSTRUCTOR ---
        public ScadaHub(CentralFactoryRepository factoryRepo)
        {
            _factoryRepo = factoryRepo;
        }

        // --- 1. GATEWAY KAYDI (WINFORMS) ---
        // Gateway açıldığında Donanım Anahtarını gönderir
        public async Task RegisterGateway(string hardwareKey, string gatewayIp)
        {
            // A. Veritabanından bu anahtarı doğrula
            var factory = _factoryRepo.GetFactoryByHardwareKey(hardwareKey);
            _factoryIps[factory.Id] = gatewayIp;
            if (factory == null)
            {
                //($"[Hub] Yetkisiz Giriş Denemesi! Key: {hardwareKey}");
                Context.Abort();
                return;
            }

            // B. Bağlantıyı Kaydet
            _gatewayConnections[Context.ConnectionId] = factory.Id;

            // C. Gateway'i Kendi Fabrika Grubuna Ekle
            string groupName = $"Factory_{factory.Id}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            //($"[Hub] Gateway Onaylandı: {factory.FactoryName} (ID: {factory.Id})");
        }

        // --- 2. WEB KULLANICI ABONELİĞİ (BLAZOR) ---
        public string GetGatewayIpForMachine(int machineId)
        {
            // Basitlik adına: Kullanıcının yetkili olduğu ilk fabrikanın IP'sini dönüyoruz.
            // Daha gelişmiş sistemde MachineId -> FactoryId sorgusu yapılır.

            var targetConnectionId = GetTargetGatewayForCurrentUser();
            if (targetConnectionId != null && _gatewayConnections.TryGetValue(targetConnectionId, out int factoryId))
            {
                if (_factoryIps.TryGetValue(factoryId, out string ip))
                {
                    return ip;
                }
            }
            return "localhost:5901"; // Bulunamazsa varsayılan
        }
        public async Task SubscribeToFactories(List<int> factoryIds)
        {
            foreach (var fid in factoryIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Factory_{fid}");
                //($"[Hub] Web Kullanıcısı Fabrika {fid} kanalına abone oldu.");
            }
        }

        // --- 3. CANLI VERİ YAYINI (GATEWAY -> WEB) ---
        public async Task BroadcastFromLocal(FullMachineStatus status)
        {
            // Gönderen kim? Hangi fabrikadan geliyor?
            if (_gatewayConnections.TryGetValue(Context.ConnectionId, out int factoryId))
            {
                // Sadece o fabrikanın grubuna yayın yap
                await Clients.Group($"Factory_{factoryId}").SendAsync("ReceiveMachineUpdate", status);
            }
        }

        // --- 4. KOMUT GÖNDERİMİ (WEB -> GATEWAY) ---
        public async Task SendCommandToLocal(int machineId, string command, string parameters)
        {
            string? targetId = GetTargetGatewayForCurrentUser();
            if (targetId != null)
            {
                await Clients.Client(targetId).SendAsync("ReceiveCommand", machineId, command, parameters);
            }
            else
            {
                //("[Hub] Hata: Komut gönderilecek aktif Gateway bulunamadı.");
            }
        }

        // --- BAĞLANTI KOPMA ---
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_gatewayConnections.TryRemove(Context.ConnectionId, out int factoryId))
            {
                //($"[Hub] Fabrika {factoryId} Gateway bağlantısı koptu.");
            }
            return base.OnDisconnectedAsync(exception);
        }

        // --- İSTEK YÖNLENDİRME MOTORU (CORE) ---
        // --- GÜNCELLENMİŞ İSTEK YÖNLENDİRME MOTORU ---
        // --- İSTEK YÖNLENDİRME (DEBUG MODU) ---
        // --- İSTEK YÖNLENDİRME (JSON RAW LOG MODU) ---
        // --- İSTEK YÖNLENDİRME (AKILLI ÇÖZÜMLEYİCİ) ---
        // --- KUTU AÇICI (UNWRAPPER) MODLU YÖNLENDİRME ---
        // --- İSTEK YÖNLENDİRME (NİHAİ ÇÖZÜM) ---
        private async Task<T?> InvokeOnGateway<T>(string targetMethod, params object[] args)
        {
            string? targetConnectionId = GetTargetGatewayForCurrentUser();
            if (string.IsNullOrEmpty(targetConnectionId)) return default;

            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => {
                if (_pendingRequests.TryRemove(requestId, out var pendingTcs))
                    pendingTcs.TrySetException(new TimeoutException("Gateway zaman aşımı."));
            }, useSynchronizationContext: false);

            _pendingRequests[requestId] = tcs;

            try
            {
                await Clients.Client(targetConnectionId).SendAsync("HandleRequest", requestId, targetMethod, args);

                var result = await tcs.Task;
                if (result == null) return default;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };

                string jsonString = "";
                if (result is string s) jsonString = s;
                else if (result is JsonElement e) jsonString = e.GetRawText();
                else return (T)result;

                // --- LOG: Veriyi görelim ---
                // //($"[HUB] Veri Geldi: {jsonString.Substring(0, Math.Min(jsonString.Length, 100))}");

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    // DURUM 1: Veri doğrudan bir LİSTE [...] ise (İdeal Durum)
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<T>(jsonString, options);
                    }
                    // DURUM 2: Veri bir NESNE {...} ise (Kutu içinde olabilir)
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("$values", out JsonElement valuesElement))
                            return JsonSerializer.Deserialize<T>(valuesElement.GetRawText(), options);

                        if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement))
                            return JsonSerializer.Deserialize<T>(resultElement.GetRawText(), options);

                        // Kutu değilse normal nesne olarak dene
                        return JsonSerializer.Deserialize<T>(jsonString, options);
                    }
                    // DURUM 3: Veri bir STRING "..." ise (ÇİFT PAKETLEME SORUNU BURADA ÇÖZÜLÜYOR)
                    else if (doc.RootElement.ValueKind == JsonValueKind.String)
                    {
                        // String'in içindeki asıl JSON'ı al (Unescape yap)
                        string innerJson = doc.RootElement.GetString();

                        // İçindeki veriyi tekrar parse etmeye çalış (Recursive gibi düşünmeyelim, direkt çevirelim)
                        if (!string.IsNullOrEmpty(innerJson))
                        {
                            return JsonSerializer.Deserialize<T>(innerJson, options);
                        }
                    }
                }

                return default;
            }
            catch (Exception ex)
            {
                //($"[HUB KRİTİK HATA] {ex.Message}");
                return default;
            }
            finally
            {
                _pendingRequests.TryRemove(requestId, out _);
                _chunkBuffers.TryRemove(requestId, out _);
            }
        }
        public Task<List<int>> GetOnlineFactoryIds()
        {
            // Bağlı olan tüm gateway'lerin Fabrika ID'lerini benzersiz olarak listele
            var onlineIds = _gatewayConnections.Values.Distinct().ToList();
            return Task.FromResult(onlineIds);
        }
        // Yardımcı: Şu anki kullanıcının yetkisine uygun bir Gateway bul
        private string? GetTargetGatewayForCurrentUser()
        {
            var user = Context.User;
            if (user == null) return null;

            var allowedIdsStr = user.FindFirst("AllowedFactoryIds")?.Value;
            if (string.IsNullOrEmpty(allowedIdsStr)) return null;

            // Bağlı gateway'leri tara
            foreach (var kvp in _gatewayConnections)
            {
                int factoryId = kvp.Value;
                // "ALL" yetkisi varsa veya ID listesinde varsa
                if (allowedIdsStr == "ALL" || allowedIdsStr.Split(',').Contains(factoryId.ToString()))
                {
                    return kvp.Key; // ConnectionId döner
                }
            }
            return null;
        }

        // --- VERİ PARÇA ALICI (CHUNKING) ---
        public void ReceiveResponseChunk(string requestId, string chunk, bool isLast)
        {
            var buffer = _chunkBuffers.GetOrAdd(requestId, _ => new StringBuilder());
            lock (buffer) { buffer.Append(chunk); }

            if (isLast)
            {
                if (_pendingRequests.TryGetValue(requestId, out var tcs))
                {
                    tcs.TrySetResult(buffer.ToString());
                }
            }
        }

        public void SendResponseToHub(string requestId, object? data, string? errorMessage)
        {
            if (_pendingRequests.TryGetValue(requestId, out var tcs))
            {
                if (!string.IsNullOrEmpty(errorMessage)) tcs.TrySetException(new Exception(errorMessage));
                else tcs.TrySetResult(data);
            }
        }

        // --- LOGLAMA ---
        public async Task LogAction(ActionLogEntry entry)
        {
            string? targetId = GetTargetGatewayForCurrentUser();
            if (targetId != null)
            {
                await Clients.Client(targetId).SendAsync("HandleLogAction", entry);
            }
        }

        // --- PUBLIC METOTLAR (WEB CLIENT İÇİN) ---
        public async Task<List<Machine>> GetAllMachines() => await InvokeOnGateway<List<Machine>>("GetAllMachines") ?? new List<Machine>();
        public async Task<FullMachineStatus?> GetMachineStatus(int id) => await InvokeOnGateway<FullMachineStatus>("GetMachineStatus", id);
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

        public async Task<List<User>> GetUsers() => await InvokeOnGateway<List<User>>("GetAllUsers") ?? new List<User>();
        public async Task<List<Role>> GetRoles() => await InvokeOnGateway<List<Role>>("GetAllRoles") ?? new List<Role>();
        public async Task AddUser(UserViewModel model) => await InvokeOnGateway<bool>("AddUser", model);
        public async Task UpdateUser(UserViewModel model) => await InvokeOnGateway<bool>("UpdateUser", model);
        public async Task DeleteUser(int id) => await InvokeOnGateway<bool>("DeleteUser", id);

        public async Task<List<CostParameter>> GetCosts() => await InvokeOnGateway<List<CostParameter>>("GetCosts") ?? new List<CostParameter>();
        public async Task UpdateCosts(List<CostParameter> costs) => await InvokeOnGateway<bool>("UpdateParameters", costs);

        public async Task<List<AlarmDefinition>> GetAlarms() => await InvokeOnGateway<List<AlarmDefinition>>("GetAllAlarmDefinitions") ?? new List<AlarmDefinition>();
        public async Task AddAlarm(AlarmDefinition alarm) => await InvokeOnGateway<bool>("AddAlarmDefinition", alarm);
        public async Task UpdateAlarm(AlarmDefinition alarm) => await InvokeOnGateway<bool>("UpdateAlarmDefinition", alarm);
        public async Task DeleteAlarm(int id) => await InvokeOnGateway<bool>("DeleteAlarmDefinition", id);

        public async Task<List<ScadaRecipe>> GetRecipes() => await InvokeOnGateway<List<ScadaRecipe>>("GetAllRecipes") ?? new List<ScadaRecipe>();
        public async Task<ScadaRecipe?> GetRecipeDetails(int id) => await InvokeOnGateway<ScadaRecipe>("GetRecipeById", id);
        public async Task SaveRecipe(ScadaRecipe recipe) => await InvokeOnGateway<bool>("SaveRecipe", recipe);
        public async Task DeleteRecipe(int id) => await InvokeOnGateway<bool>("DeleteRecipe", id);
        public async Task<List<ProductionReportItem>> GetRecipeConsumptionHistory(int recipeId) => await InvokeOnGateway<List<ProductionReportItem>>("GetRecipeUsageHistory", recipeId) ?? new List<ProductionReportItem>();

        public async Task<bool> SendRecipeToPlc(int recipeId, int machineId) => await InvokeOnGateway<bool>("SendRecipeToPlc", recipeId, machineId);
        public async Task<ScadaRecipe?> ReadRecipeFromPlc(int machineId) => await InvokeOnGateway<ScadaRecipe>("ReadRecipeFromPlc", machineId);

        public async Task<List<string>> GetMachineSubTypesDesign() => await InvokeOnGateway<List<string>>("GetMachineSubTypes");
        public async Task<List<StepTypeDtoDesign>> GetStepTypesDesign() => await InvokeOnGateway<List<StepTypeDtoDesign>>("GetStepTypes");
        public async Task<List<ControlMetadata>> GetLayoutDesign(string subType, int stepTypeId) => await InvokeOnGateway<List<ControlMetadata>>("GetLayoutJson", subType, stepTypeId) ?? new List<ControlMetadata>();
        public async Task<bool> SaveLayoutDesign(string subType, int stepTypeId, List<ControlMetadata> layout) => await InvokeOnGateway<bool>("SaveLayout", subType, stepTypeId, layout);
        public async Task<string> GetStepLayout(string subType, int stepTypeId)
        {
            var list = await InvokeOnGateway<List<ControlMetadata>>("GetLayoutJson", subType, stepTypeId);
            if (list == null) return string.Empty;
            return JsonSerializer.Serialize(list);
        }

        public async Task<List<PlcOperator>> GetPlcOperators() => await InvokeOnGateway<List<PlcOperator>>("GetPlcOperators") ?? new List<PlcOperator>();
        public async Task SavePlcOperator(PlcOperator op) => await InvokeOnGateway<bool>("SaveOrUpdateOperator", op);
        public async Task AddDefaultPlcOperator() => await InvokeOnGateway<bool>("AddDefaultOperator");
        public async Task DeletePlcOperator(int id) => await InvokeOnGateway<bool>("DeleteOperator", id);

        public async Task<Dictionary<int, string>> GetHmiRecipeNames(int machineId) => await InvokeOnGateway<Dictionary<int, string>>("GetHmiRecipeNames", machineId) ?? new Dictionary<int, string>();
        public async Task<ScadaRecipe?> GetHmiRecipePreview(int machineId, string fileName) => await InvokeOnGateway<ScadaRecipe>("GetHmiRecipePreview", machineId, fileName);
        public async Task<bool> QueueSequentiallyNamedSendJobs(List<int> recipeIds, List<int> machineIds, int startNumber) => await InvokeOnGateway<bool>("QueueSequentiallyNamedSendJobs", recipeIds, machineIds, startNumber);
        public async Task<bool> QueueReceiveJobs(List<string> fileNames, int machineId) => await InvokeOnGateway<bool>("QueueReceiveJobs", fileNames, machineId);
        public async Task<List<TransferJob>> GetActiveJobs() => await InvokeOnGateway<List<TransferJob>>("GetActiveFtpJobs") ?? new List<TransferJob>();

        public async Task<List<OeeData>> GetOeeReport(ReportFilters filters) => await InvokeOnGateway<List<OeeData>>("GetOeeReport", filters) ?? new List<OeeData>();
        public async Task<List<HourlyConsumptionData>> GetHourlyConsumption() => await InvokeOnGateway<List<HourlyConsumptionData>>("GetHourlyFactoryConsumption") ?? new List<HourlyConsumptionData>();
        public async Task<List<HourlyOeeData>> GetHourlyOee() => await InvokeOnGateway<List<HourlyOeeData>>("GetHourlyAverageOee") ?? new List<HourlyOeeData>();
        public async Task<List<TopAlarmData>> GetTopAlarms() => await InvokeOnGateway<List<TopAlarmData>>("GetTopAlarmsByFrequency") ?? new List<TopAlarmData>();

        public async Task<List<ProductionReportItem>> GetProductionReport(ReportFilters filters)
        {
            //("[Hub] HALKA AÇIK METOT TETİKLENDİ: GetProductionReport");
            return await InvokeOnGateway<List<ProductionReportItem>>("GetProductionReport", filters) ?? new List<ProductionReportItem>();
        }
        public async Task<List<AlarmReportItem>> GetAlarmReport(ReportFilters filters) => await InvokeOnGateway<List<AlarmReportItem>>("GetAlarmReport", filters) ?? new List<AlarmReportItem>();
        public async Task<object> GetTrendData(ReportFilters filters) => await InvokeOnGateway<object>("GetTrendData", filters) ?? new List<object>();
        public async Task<ManualConsumptionSummary?> GetManualConsumptionReport(ReportFilters filters) => await InvokeOnGateway<ManualConsumptionSummary>("GetManualConsumptionReport", filters);
        public async Task<ConsumptionTotals?> GetConsumptionTotals(ReportFilters filters) => await InvokeOnGateway<ConsumptionTotals>("GetConsumptionTotalsForPeriod", filters);
        public async Task<List<ProductionReportItem>> GetGeneralDetailedConsumptionReport(GeneralDetailedConsumptionFilters filters) => await InvokeOnGateway<List<ProductionReportItem>>("GetGeneralDetailedConsumptionReport", filters) ?? new List<ProductionReportItem>();
        public async Task<List<ActionLogEntry>> GetActionLogs(ActionLogFilters filters) => await InvokeOnGateway<List<ActionLogEntry>>("GetActionLogs", filters) ?? new List<ActionLogEntry>();
        public async Task<ProductionDetailDto?> GetProductionDetail(int machineId, string batchId) => await InvokeOnGateway<ProductionDetailDto>("GetProductionDetail", machineId, batchId);

        public async Task<byte[]> ExportProductionReport(List<ProductionReportItem> items) => await InvokeOnGateway<byte[]>("ExportProductionReport", items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportAlarmReport(List<AlarmReportItem> items) => await InvokeOnGateway<byte[]>("ExportAlarmReport", items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportOeeReport(List<OeeData> items) => await InvokeOnGateway<byte[]>("ExportOeeReport", items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportManualConsumptionReport(ManualConsumptionSummary summary) => await InvokeOnGateway<byte[]>("ExportManualConsumptionReport", summary) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportGeneralDetailedConsumptionReport(GeneralConsumptionExportDto data) => await InvokeOnGateway<byte[]>("ExportGeneralDetailedConsumptionReport", data) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportActionLogsReport(List<ActionLogEntry> logs) => await InvokeOnGateway<byte[]>("ExportActionLogsReport", logs) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportProductionDetailFile(int machineId, string batchId) => await InvokeOnGateway<byte[]>("ExportProductionDetailFile", machineId, batchId) ?? Array.Empty<byte>();
    }
}