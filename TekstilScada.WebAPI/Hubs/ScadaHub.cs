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

            if (factory == null)
            {
                Console.WriteLine($"[Hub] Yetkisiz Giriş Denemesi! Tanımsız Key: {hardwareKey}");
                Context.Abort(); // Bağlantıyı reddet
                return;
            }

            // IP adresini hafızaya kaydet (Web tarafında "Bağlantı Var/Yok" kontrolü için)
            _factoryIps[factory.Id] = gatewayIp;

            // B. Bağlantıyı Kaydet
            _gatewayConnections[Context.ConnectionId] = factory.Id;

            // C. Gateway'i Kendi Fabrika Grubuna Ekle
            string groupName = $"Factory_{factory.Id}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Console.WriteLine($"[Hub] Gateway Onaylandı: {factory.FactoryName} (ID: {factory.Id})");
        }

        // --- 2. YENİ EKLENEN METOT (Arka Plan Servisi İçin) ---
        public async Task<List<FullMachineStatus>> GetLiveMachineStatusByFactoryId(int factoryId)
        {
            // Bu Fabrika ID'sine ait bağlı Gateway'i bul
            string? targetConnectionId = _gatewayConnections.FirstOrDefault(x => x.Value == factoryId).Key;

            if (string.IsNullOrEmpty(targetConnectionId))
            {
                // Gateway bağlı değil, boş dön
                return new List<FullMachineStatus>();
            }

            // Gateway'e istek at ve cevabı bekle
            var result = await SendRequestToGateway<List<FullMachineStatus>>(targetConnectionId, "GetAllMachineStatuses");

            return result ?? new List<FullMachineStatus>();
        }

        // --- 3. MERKEZİ İSTEK GÖNDERME YARDIMCISI (Gateway'e direkt gönderim) ---
        private async Task<T?> SendRequestToGateway<T>(string targetConnectionId, string targetMethod, params object[] args)
        {
            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Timeout artırıldı
            cts.Token.Register(() => {
                if (_pendingRequests.TryRemove(requestId, out var pendingTcs))
                    pendingTcs.TrySetException(new TimeoutException("Gateway cevap vermedi."));
            });

            _pendingRequests[requestId] = tcs;

            try
            {
                // Gateway'e gönder
                await Clients.Client(targetConnectionId).SendAsync("HandleRequest", requestId, targetMethod, args);

                // Cevabı bekle
                var result = await tcs.Task;
                if (result == null) return default;

                return DeserializeResult<T>(result);
            }
            catch
            {
                return default;
            }
            finally
            {
                _pendingRequests.TryRemove(requestId, out _);
                _chunkBuffers.TryRemove(requestId, out _);
            }
        }

        // --- 4. WEB KULLANICI ABONELİĞİ (BLAZOR) ---
        public string GetGatewayIpForMachine(int factoryId, int machineId)
        {
            // Kullanıcı bu factoryId'ye erişebilir mi?
            var targetConnectionId = GetTargetGateway(factoryId);

            if (targetConnectionId != null)
            {
                if (_factoryIps.TryGetValue(factoryId, out string ip))
                {
                    return ip;
                }
            }
            return "localhost:5901";
        }

        public async Task SendScreenImage(int machineId, string base64Image)
        {
            // NOT: Burada hangi fabrikadan geldiği bilgisi eksik olabilir.
            // Ancak genellikle sadece izleyenlere gönderildiği için All yerine Group kullanılabilir.
            // Şimdilik eski yapı korunuyor ancak geliştirilebilir.
            await Clients.All.SendAsync("ReceiveScreenImage", machineId, base64Image);
        }

        public async Task SubscribeToFactories(List<int> factoryIds)
        {
            // Güvenlik: Kullanıcının token'ındaki yetkileri kontrol et
            var user = Context.User;
            var allowedIdsStr = user?.FindFirst("AllowedFactoryIds")?.Value;

            if (string.IsNullOrEmpty(allowedIdsStr)) return;

            List<int> authorizedIds = new List<int>();

            if (allowedIdsStr == "ALL")
            {
                authorizedIds = factoryIds;
            }
            else
            {
                var allowedList = allowedIdsStr.Split(',').Select(int.Parse).ToList();
                // Talep edilenlerle izin verilenlerin kesişimi
                authorizedIds = factoryIds.Intersect(allowedList).ToList();
            }

            foreach (var fid in authorizedIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Factory_{fid}");
            }
        }

        // --- 5. CANLI VERİ YAYINI (GATEWAY -> WEB) ---
        // DÜZELTME: Veriyi gönderirken yanına FactoryId ekliyoruz.
        public async Task BroadcastFromLocal(FullMachineStatus status)
        {
            if (_gatewayConnections.TryGetValue(Context.ConnectionId, out int factoryId))
            {
                // Konsola Log
                // Console.WriteLine($"[CANLI AKIŞ] Fabrika ID: {factoryId} | Makine: {status.MachineName} ({status.MachineId}) -> Gruba Dağıtılıyor...");

                // İstemci tarafında imza: (int factoryId, FullMachineStatus status)
                await Clients.Group($"Factory_{factoryId}").SendAsync("ReceiveMachineUpdate", factoryId, status);
            }
            else
            {
                Console.WriteLine($"[HATA] Veri geldi ama gönderen Gateway Tanımsız! ConnectionId: {Context.ConnectionId}");
            }
        }

        // --- 6. KOMUT GÖNDERİMİ (WEB -> GATEWAY) ---
        public async Task SendCommandToLocal(int factoryId, int machineId, string command, string parameters)
        {
            string? targetId = GetTargetGateway(factoryId);
            if (targetId != null)
            {
                await Clients.Client(targetId).SendAsync("ReceiveCommand", machineId, command, parameters);
            }
            else
            {
                // Gateway bulunamadı veya yetki yok
            }
        }

        // --- BAĞLANTI KOPMA ---
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_gatewayConnections.TryRemove(Context.ConnectionId, out int factoryId))
            {
                // Gateway koptu
            }
            return base.OnDisconnectedAsync(exception);
        }

        // --- 7. İSTEK YÖNLENDİRME MOTORU (REVİZE EDİLDİ) ---
        // factoryId parametresi zorunlu hale getirildi.
        private async Task<T?> InvokeOnGateway<T>(int factoryId, string targetMethod, params object[] args)
        {
            string? targetConnectionId = GetTargetGateway(factoryId);
            if (string.IsNullOrEmpty(targetConnectionId)) return default;

            // SendRequestToGateway metodunu yeniden kullanıyoruz (Kod tekrarını önlemek için)
            return await SendRequestToGateway<T>(targetConnectionId, targetMethod, args);
        }

        public Task<List<int>> GetOnlineFactoryIds()
        {
            var onlineIds = _gatewayConnections.Values.Distinct().ToList();
            return Task.FromResult(onlineIds);
        }

        // --- YENİ HELPER: ID'ye göre Gateway Bulucu ---
        private string? GetTargetGateway(int targetFactoryId)
        {
            var user = Context.User;
            if (user == null) return null;

            var allowedIdsStr = user.FindFirst("AllowedFactoryIds")?.Value;
            if (string.IsNullOrEmpty(allowedIdsStr)) return null;

            // 1. Yetki Kontrolü
            bool isAuthorized = allowedIdsStr == "ALL" ||
                                allowedIdsStr.Split(',').Select(int.Parse).Contains(targetFactoryId);

            if (!isAuthorized) return null;

            // 2. Bağlantı Kontrolü
            var gatewayEntry = _gatewayConnections.FirstOrDefault(x => x.Value == targetFactoryId);

            return string.IsNullOrEmpty(gatewayEntry.Key) ? null : gatewayEntry.Key;
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

        // --- JSON DESERIALIZER HELPER ---
        private T? DeserializeResult<T>(object result)
        {
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

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<T>(jsonString, options);
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("$values", out JsonElement valuesElement))
                            return JsonSerializer.Deserialize<T>(valuesElement.GetRawText(), options);

                        if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement))
                            return JsonSerializer.Deserialize<T>(resultElement.GetRawText(), options);

                        return JsonSerializer.Deserialize<T>(jsonString, options);
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.String)
                    {
                        string innerJson = doc.RootElement.GetString();
                        if (!string.IsNullOrEmpty(innerJson))
                        {
                            return JsonSerializer.Deserialize<T>(innerJson, options);
                        }
                    }
                }
            }
            catch { return default; }

            return default;
        }

        // --- LOGLAMA ---
        public async Task LogAction(int factoryId, ActionLogEntry entry)
        {
            string? targetId = GetTargetGateway(factoryId);
            if (targetId != null)
            {
                await Clients.Client(targetId).SendAsync("HandleLogAction", entry);
            }
        }

        // --- PUBLIC METOTLAR (WEB CLIENT İÇİN - GÜNCELLENMİŞ) ---
        // Tüm metotlara "int factoryId" eklendi.

        public async Task<List<Machine>> GetAllMachines(int factoryId)
            => await InvokeOnGateway<List<Machine>>(factoryId, "GetAllMachines") ?? new List<Machine>();

        public async Task<FullMachineStatus?> GetMachineStatus(int factoryId, int id)
            => await InvokeOnGateway<FullMachineStatus>(factoryId, "GetMachineStatus", id);

        public async Task AddMachine(int factoryId, Machine machine)
        {
            await InvokeOnGateway<bool>(factoryId, "AddMachine", machine);
            // Sadece o fabrikanın grubuna haber ver
            await Clients.Group($"Factory_{factoryId}").SendAsync("MachineListUpdated");
        }

        public async Task UpdateMachine(int factoryId, Machine machine)
        {
            await InvokeOnGateway<bool>(factoryId, "UpdateMachine", machine);
            await Clients.Group($"Factory_{factoryId}").SendAsync("MachineListUpdated");
        }

        public async Task DeleteMachine(int factoryId, int id)
        {
            await InvokeOnGateway<bool>(factoryId, "DeleteMachine", id);
            await Clients.Group($"Factory_{factoryId}").SendAsync("MachineListUpdated");
        }

        public async Task<List<User>> GetUsers(int factoryId)
            => await InvokeOnGateway<List<User>>(factoryId, "GetAllUsers") ?? new List<User>();

        public async Task<List<Role>> GetRoles(int factoryId)
            => await InvokeOnGateway<List<Role>>(factoryId, "GetAllRoles") ?? new List<Role>();

        public async Task AddUser(int factoryId, UserViewModel model)
            => await InvokeOnGateway<bool>(factoryId, "AddUser", model);

        public async Task UpdateUser(int factoryId, UserViewModel model)
            => await InvokeOnGateway<bool>(factoryId, "UpdateUser", model);

        public async Task DeleteUser(int factoryId, int id)
            => await InvokeOnGateway<bool>(factoryId, "DeleteUser", id);

        public async Task<List<CostParameter>> GetCosts(int factoryId)
            => await InvokeOnGateway<List<CostParameter>>(factoryId, "GetCosts") ?? new List<CostParameter>();

        public async Task UpdateCosts(int factoryId, List<CostParameter> costs)
            => await InvokeOnGateway<bool>(factoryId, "UpdateParameters", costs);

        public async Task<List<AlarmDefinition>> GetAlarms(int factoryId)
            => await InvokeOnGateway<List<AlarmDefinition>>(factoryId, "GetAllAlarmDefinitions") ?? new List<AlarmDefinition>();

        public async Task AddAlarm(int factoryId, AlarmDefinition alarm)
            => await InvokeOnGateway<bool>(factoryId, "AddAlarmDefinition", alarm);

        public async Task UpdateAlarm(int factoryId, AlarmDefinition alarm)
            => await InvokeOnGateway<bool>(factoryId, "UpdateAlarmDefinition", alarm);

        public async Task DeleteAlarm(int factoryId, int id)
            => await InvokeOnGateway<bool>(factoryId, "DeleteAlarmDefinition", id);

        public async Task<List<ScadaRecipe>> GetRecipes(int factoryId)
            => await InvokeOnGateway<List<ScadaRecipe>>(factoryId, "GetAllRecipes") ?? new List<ScadaRecipe>();

        public async Task<ScadaRecipe?> GetRecipeDetails(int factoryId, int id)
            => await InvokeOnGateway<ScadaRecipe>(factoryId, "GetRecipeById", id);

        public async Task SaveRecipe(int factoryId, ScadaRecipe recipe)
            => await InvokeOnGateway<bool>(factoryId, "SaveRecipe", recipe);

        public async Task DeleteRecipe(int factoryId, int id)
            => await InvokeOnGateway<bool>(factoryId, "DeleteRecipe", id);

        public async Task<List<ProductionReportItem>> GetRecipeConsumptionHistory(int factoryId, int recipeId)
            => await InvokeOnGateway<List<ProductionReportItem>>(factoryId, "GetRecipeUsageHistory", recipeId) ?? new List<ProductionReportItem>();

        public async Task<bool> SendRecipeToPlc(int factoryId, int recipeId, int machineId)
            => await InvokeOnGateway<bool>(factoryId, "SendRecipeToPlc", recipeId, machineId);

        public async Task<ScadaRecipe?> ReadRecipeFromPlc(int factoryId, int machineId)
            => await InvokeOnGateway<ScadaRecipe>(factoryId, "ReadRecipeFromPlc", machineId);

        public async Task<List<string>> GetMachineSubTypesDesign(int factoryId)
            => await InvokeOnGateway<List<string>>(factoryId, "GetMachineSubTypes");

        public async Task<List<StepTypeDtoDesign>> GetStepTypesDesign(int factoryId)
            => await InvokeOnGateway<List<StepTypeDtoDesign>>(factoryId, "GetStepTypes");

        public async Task<List<ControlMetadata>> GetLayoutDesign(int factoryId, string subType, int stepTypeId)
            => await InvokeOnGateway<List<ControlMetadata>>(factoryId, "GetLayoutJson", subType, stepTypeId) ?? new List<ControlMetadata>();

        public async Task<bool> SaveLayoutDesign(int factoryId, string subType, int stepTypeId, List<ControlMetadata> layout)
            => await InvokeOnGateway<bool>(factoryId, "SaveLayout", subType, stepTypeId, layout);

        public async Task<string> GetStepLayout(int factoryId, string subType, int stepTypeId)
        {
            var list = await InvokeOnGateway<List<ControlMetadata>>(factoryId, "GetLayoutJson", subType, stepTypeId);
            if (list == null) return string.Empty;
            return JsonSerializer.Serialize(list);
        }

        public async Task<List<PlcOperator>> GetPlcOperators(int factoryId)
            => await InvokeOnGateway<List<PlcOperator>>(factoryId, "GetPlcOperators") ?? new List<PlcOperator>();

        public async Task SavePlcOperator(int factoryId, PlcOperator op)
            => await InvokeOnGateway<bool>(factoryId, "SaveOrUpdateOperator", op);

        public async Task AddDefaultPlcOperator(int factoryId)
            => await InvokeOnGateway<bool>(factoryId, "AddDefaultOperator");

        public async Task DeletePlcOperator(int factoryId, int id)
            => await InvokeOnGateway<bool>(factoryId, "DeleteOperator", id);

        public async Task<Dictionary<int, string>> GetHmiRecipeNames(int factoryId, int machineId)
            => await InvokeOnGateway<Dictionary<int, string>>(factoryId, "GetHmiRecipeNames", machineId) ?? new Dictionary<int, string>();

        public async Task<ScadaRecipe?> GetHmiRecipePreview(int factoryId, int machineId, string fileName)
            => await InvokeOnGateway<ScadaRecipe>(factoryId, "GetHmiRecipePreview", machineId, fileName);

        public async Task<bool> QueueSequentiallyNamedSendJobs(int factoryId, List<int> recipeIds, List<int> machineIds, int startNumber)
            => await InvokeOnGateway<bool>(factoryId, "QueueSequentiallyNamedSendJobs", recipeIds, machineIds, startNumber);

        public async Task<bool> QueueReceiveJobs(int factoryId, List<string> fileNames, int machineId)
            => await InvokeOnGateway<bool>(factoryId, "QueueReceiveJobs", fileNames, machineId);

        public async Task<List<TransferJob>> GetActiveJobs(int factoryId)
            => await InvokeOnGateway<List<TransferJob>>(factoryId, "GetActiveFtpJobs") ?? new List<TransferJob>();

        public async Task<List<OeeData>> GetOeeReport(int factoryId, ReportFilters filters)
            => await InvokeOnGateway<List<OeeData>>(factoryId, "GetOeeReport", filters) ?? new List<OeeData>();

        public async Task<List<HourlyConsumptionData>> GetHourlyConsumption(int factoryId)
            => await InvokeOnGateway<List<HourlyConsumptionData>>(factoryId, "GetHourlyFactoryConsumption") ?? new List<HourlyConsumptionData>();

        public async Task<List<HourlyOeeData>> GetHourlyOee(int factoryId)
            => await InvokeOnGateway<List<HourlyOeeData>>(factoryId, "GetHourlyAverageOee") ?? new List<HourlyOeeData>();

        public async Task<List<TopAlarmData>> GetTopAlarms(int factoryId)
            => await InvokeOnGateway<List<TopAlarmData>>(factoryId, "GetTopAlarmsByFrequency") ?? new List<TopAlarmData>();

        // KRİTİK: Raporlar karışıyordu, şimdi factoryId ile filtreleniyor.
        public async Task<List<ProductionReportItem>> GetProductionReport(int factoryId, ReportFilters filters)
        {
            return await InvokeOnGateway<List<ProductionReportItem>>(factoryId, "GetProductionReport", filters) ?? new List<ProductionReportItem>();
        }

        public async Task<List<AlarmReportItem>> GetAlarmReport(int factoryId, ReportFilters filters)
            => await InvokeOnGateway<List<AlarmReportItem>>(factoryId, "GetAlarmReport", filters) ?? new List<AlarmReportItem>();

        public async Task<object> GetTrendData(int factoryId, ReportFilters filters)
            => await InvokeOnGateway<object>(factoryId, "GetTrendData", filters) ?? new List<object>();

        public async Task<ManualConsumptionSummary?> GetManualConsumptionReport(int factoryId, ReportFilters filters)
            => await InvokeOnGateway<ManualConsumptionSummary>(factoryId, "GetManualConsumptionReport", filters);

        public async Task<ConsumptionTotals?> GetConsumptionTotals(int factoryId, ReportFilters filters)
            => await InvokeOnGateway<ConsumptionTotals>(factoryId, "GetConsumptionTotalsForPeriod", filters);

        public async Task<List<ProductionReportItem>> GetGeneralDetailedConsumptionReport(int factoryId, GeneralDetailedConsumptionFilters filters)
            => await InvokeOnGateway<List<ProductionReportItem>>(factoryId, "GetGeneralDetailedConsumptionReport", filters) ?? new List<ProductionReportItem>();

        public async Task<List<ActionLogEntry>> GetActionLogs(int factoryId, ActionLogFilters filters)
            => await InvokeOnGateway<List<ActionLogEntry>>(factoryId, "GetActionLogs", filters) ?? new List<ActionLogEntry>();

        public async Task<ProductionDetailDto?> GetProductionDetail(int factoryId, int machineId, string batchId)
            => await InvokeOnGateway<ProductionDetailDto>(factoryId, "GetProductionDetail", machineId, batchId);

        public async Task<byte[]> ExportProductionReport(int factoryId, List<ProductionReportItem> items)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportProductionReport", items) ?? Array.Empty<byte>();

        public async Task<byte[]> ExportAlarmReport(int factoryId, List<AlarmReportItem> items)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportAlarmReport", items) ?? Array.Empty<byte>();

        public async Task<byte[]> ExportOeeReport(int factoryId, List<OeeData> items)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportOeeReport", items) ?? Array.Empty<byte>();

        public async Task<byte[]> ExportManualConsumptionReport(int factoryId, ManualConsumptionSummary summary)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportManualConsumptionReport", summary) ?? Array.Empty<byte>();

        public async Task<byte[]> ExportGeneralDetailedConsumptionReport(int factoryId, GeneralConsumptionExportDto data)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportGeneralDetailedConsumptionReport", data) ?? Array.Empty<byte>();

        public async Task<byte[]> ExportActionLogsReport(int factoryId, List<ActionLogEntry> logs)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportActionLogsReport", logs) ?? Array.Empty<byte>();

        public async Task<byte[]> ExportProductionDetailFile(int factoryId, int machineId, string batchId)
            => await InvokeOnGateway<byte[]>(factoryId, "ExportProductionDetailFile", machineId, batchId) ?? Array.Empty<byte>();
    }
}