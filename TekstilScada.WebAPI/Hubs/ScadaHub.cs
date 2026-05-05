using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using TekstilScada.API.Services;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.WebAPI.Repositories;
using static TekstilScada.Core.Core.ExcelExportHelper;

// --- DTO Sınıfları (Aynen Korunuyor) ---
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

namespace TekstilScada.WebAPI.Hubs
{
    public class ScadaHub : Hub
    {
        private readonly CentralFactoryRepository _factoryRepo;

        // --- GATEWAY YÖNETİMİ ---
        private static readonly ConcurrentDictionary<string, int> _gatewayConnections = new();
        private static readonly ConcurrentDictionary<string, StringBuilder> _chunkBuffers = new();
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<object?>> _pendingRequests = new();
        private static readonly ConcurrentDictionary<int, string> _factoryIps = new();
        private readonly TelegramService _telegramService; // YENİ EKLENDİ
                                                           // Gateway Yönetimi değişkenlerinin hemen altına ekleyin
                                                           // DİKKAT: Buradaki 'static' kelimesi hayati önem taşır! 
                                                           // Olmazsa hafıza her saniye sıfırlanır ve spam mesaj atar.
                                                           // YENİ HAFIZA YAPISI: MakineID -> (AlarmNumarası -> SonGönderimZamanı)
                                                           // =========================================================================================
                                                           // TELEGRAM SPAM ENGELLEYİCİ HAFIZA DEĞİŞKENLERİ (STATIC OLMAK ZORUNDA)
                                                           // =========================================================================================
                                                           // MakineID -> (AlarmNumarası -> SonGönderimZamanı)
                                                           // --- EKLENECEK HAFIZA DEĞİŞKENLERİ ---
                                                           // Sözlüğün yüklenip yüklenmediğini kontrol eder
        
        // Fabrika İsimlerini Tutacak Hafıza (Telegram mesajı için)
       
        // ŞU AN ALDIĞINIZ HATAYI ÇÖZECEK OLAN EKSİK SATIR BURASI:
        private static readonly ConcurrentDictionary<int, DateTime> _machineLastActiveAlarmTime = new();
        // Spam Koruması (Hangi alarm, ne zaman Telegram'a atıldı?)
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>> _machineSentAlarms = new();
        // Fabrika bazlı Alarm İsimleri Sözlüğü (FactoryId -> (AlarmNo -> AlarmText))
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, string>> _factoryAlarmsCache = new();
        // YENİ: HUB İÇİ "AKTİF ALARM LİSTESİ" (MakineID -> (Alarm No -> Son Görülme Zamanı))
        // Gateway tek tek gönderse bile, Hub bunları burada biriktirip liste haline getirecek.
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, DateTime>> _hubActiveAlarms = new();
        // --- TELEGRAM HAFIZA VE LİSTE YÖNETİMİ DEĞİŞKENLERİ ---
        private static readonly ConcurrentDictionary<int, string> _factoryNames = new();
        private static readonly ConcurrentDictionary<int, bool> _factoryAlarmsLoaded = new();

        // Makinenin o anki HAM alarm listesi (Artma/Azalma tespiti için)
        private static readonly ConcurrentDictionary<int, HashSet<int>> _machineCurrentAlarms = new();

        // Telegram'a en son fırlatılan (bildirilen) alarm listesi
        private static readonly ConcurrentDictionary<int, HashSet<int>> _machineReportedAlarms = new();

        // Alarm listesinin son değişme zamanı (5 sn bekleme süresi için)
        private static readonly ConcurrentDictionary<int, DateTime> _machineStateChangeTime = new();
        public ScadaHub(CentralFactoryRepository factoryRepo, TelegramService telegramService)
        {
            _factoryRepo = factoryRepo;
            _telegramService = telegramService;
        }

        // --- 1. GATEWAY KAYDI ---
        public async Task RegisterGateway(string hardwareKey, string gatewayIp)
        {
            var factory = _factoryRepo.GetFactoryByHardwareKey(hardwareKey);
            if (factory == null) { Context.Abort(); return; }

            _factoryIps[factory.Id] = gatewayIp;
            _gatewayConnections[Context.ConnectionId] = factory.Id;

            // Fabrika adını hafızaya al
            _factoryNames[factory.Id] = factory.FactoryName ?? $"Fabrika {factory.Id}";

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Factory_{factory.Id}");
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000); // Gateway'in tam bağlanması için 3 saniye bekle
                try
                {
                    var alarms = await InvokeOnGateway<List<AlarmDefinition>>(factory.Id, "GetAllAlarmDefinitions", 30);
                    if (alarms != null)
                    {
                        var cache = _factoryAlarmsCache.GetOrAdd(factory.Id, _ => new ConcurrentDictionary<int, string>());
                        foreach (var a in alarms)
                        {
                            cache[a.AlarmNumber] = a.AlarmText;
                        }
                    }
                }
                catch { /* Hata olursa sistemi durdurmaması için yutuyoruz */ }
            });
        }

        // --- 2. CANLI DURUM SORGUSU ---
        public async Task<List<FullMachineStatus>> GetLiveMachineStatusByFactoryId(int factoryId)
        {
            string? targetConnectionId = _gatewayConnections.FirstOrDefault(x => x.Value == factoryId).Key;
            if (string.IsNullOrEmpty(targetConnectionId)) return new List<FullMachineStatus>();

            var result = await SendRequestToGateway<List<FullMachineStatus>>(targetConnectionId, 5, "GetAllMachineStatuses");
            return result ?? new List<FullMachineStatus>();
        }

        // --- 3. MERKEZİ İSTEK YARDIMCISI (TIMEOUT & MEMORY FIX) ---
        private async Task<T?> SendRequestToGateway<T>(string targetConnectionId, int timeoutSeconds, string targetMethod, params object[] args)
        {
            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            cts.Token.Register(() => {
                if (_pendingRequests.TryRemove(requestId, out var pendingTcs))
                    pendingTcs.TrySetException(new TimeoutException($"Gateway cevap vermedi ({timeoutSeconds}sn)."));
            });

            _pendingRequests[requestId] = tcs;

            try
            {
                await Clients.Client(targetConnectionId).SendAsync("HandleRequest", requestId, targetMethod, args);
                var result = await tcs.Task;
                if (result == null) return default;
                return DeserializeResult<T>(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hub] İstek Hatası ({targetMethod}): {ex.Message}");
                return default;
            }
            finally
            {
                // Hafıza temizliği (Memory Leak Fix)
                _pendingRequests.TryRemove(requestId, out _);
                if (_chunkBuffers.TryRemove(requestId, out var buffer)) buffer.Clear();
            }
        }

        // --- 4. ABONELİK VE GÖRÜNTÜ ---
        public string GetGatewayIpForMachine(int factoryId, int machineId)
        {
            var targetConnectionId = GetTargetGateway(factoryId);
            if (targetConnectionId != null && _factoryIps.TryGetValue(factoryId, out string ip))
                return ip;
            return "localhost:5901";
        }

        public async Task SendScreenImage(int machineId, string base64Image)
        {
            await Clients.All.SendAsync("ReceiveScreenImage", machineId, base64Image);
        }

        public async Task SubscribeToFactories(List<int> factoryIds)
        {
            var user = Context.User;
            var allowedIdsStr = user?.FindFirst("AllowedFactoryIds")?.Value;
            if (string.IsNullOrEmpty(allowedIdsStr)) return;

            List<int> authorizedIds;
            if (allowedIdsStr == "ALL") authorizedIds = factoryIds;
            else
            {
                var allowedList = allowedIdsStr.Split(',').Select(int.Parse).ToList();
                authorizedIds = factoryIds.Intersect(allowedList).ToList();
            }

            foreach (var fid in authorizedIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Factory_{fid}");
        }
        // Add this inside your ScadaHub class
        public async Task SendAlarmReset(int machineId)
        {
            // This sends a "ReceiveAlarmReset" message to all connected clients (specifically the Gateway)
            // with the target machineId.
            await Clients.All.SendAsync("ReceiveAlarmReset", machineId);
        }
        // --- 5. CANLI VERİ YAYINI (GARANTİLİ SÖZLÜK VE TOPLU LİSTE) ---
        public async Task BroadcastFromLocal(FullMachineStatus status)
        {
            if (_gatewayConnections.TryGetValue(Context.ConnectionId, out int factoryId))
            {
                // 1. Arayüz güncellemesi (Blazor her zaman güncel kalır)
                await Clients.Group($"Factory_{factoryId}").SendAsync("ReceiveMachineUpdate", factoryId, status);

                string factoryName = _factoryNames.TryGetValue(factoryId, out string fName) ? fName : $"Fabrika {factoryId}";

                // Sunucu tarafındaki sözlüğü al veya oluştur
                var dict = _factoryAlarmsCache.GetOrAdd(factoryId, _ => new ConcurrentDictionary<int, string>());

                // --- ADIM 1: AKTİF NUMARALARI TESPİT ET (BİT PARÇALAMA) ---
                HashSet<int> activeAlarmNumbers = new HashSet<int>();
                if (status.HasActiveAlarm)
                {
                    if (status.ActiveAlarmWords != null && status.ActiveAlarmWords.Length > 0)
                    {
                        for (int wordIndex = 0; wordIndex < status.ActiveAlarmWords.Length; wordIndex++)
                        {
                            short currentWord = status.ActiveAlarmWords[wordIndex];
                            for (int bitIndex = 0; bitIndex < 16; bitIndex++)
                            {
                                if ((currentWord & (1 << bitIndex)) != 0)
                                    activeAlarmNumbers.Add((wordIndex * 16) + bitIndex + 1);
                            }
                        }
                    }
                    else if (status.ActiveAlarmNumber > 0)
                    {
                        activeAlarmNumbers.Add(status.ActiveAlarmNumber);
                    }
                }

                // --- ADIM 2: SÖZLÜĞÜ ZORLA DOLDUR (EKSİK VARSA) ---
                // Eğer listede olup da sözlükte ismi olmayan BİR TANE bile alarm varsa, veri tabanına git
                bool requiresUpdate = activeAlarmNumbers.Any(n => !dict.ContainsKey(n));

                if (requiresUpdate || dict.IsEmpty)
                {
                    try
                    {
                        // NOT: Hub içindeki mevcut GetAlarms metodunu kullanarak veri tabanından çekiyoruz
                        var dbAlarms = await GetAlarms(factoryId);
                        if (dbAlarms != null && dbAlarms.Any())
                        {
                            foreach (var a in dbAlarms)
                            {
                                dict[a.AlarmNumber] = a.AlarmText;
                            }
                            _factoryAlarmsLoaded[factoryId] = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Hub] Alarm sözlüğü çekilirken hata: {ex.Message}");
                    }
                }

                // --- ADIM 3: LİSTE DEĞİŞİM TAKİBİ ---
                var currentKnownAlarms = _machineCurrentAlarms.GetOrAdd(status.MachineId, _ => new HashSet<int>());
                var lastReportedAlarms = _machineReportedAlarms.GetOrAdd(status.MachineId, _ => new HashSet<int>());

                if (!currentKnownAlarms.SetEquals(activeAlarmNumbers))
                {
                    _machineCurrentAlarms[status.MachineId] = new HashSet<int>(activeAlarmNumbers);
                    _machineStateChangeTime[status.MachineId] = DateTime.Now;
                }

                // --- ADIM 4: GECİKMELİ GÖNDERİM VE İSİM EŞLEŞTİRME ---
                var stateChangeTime = _machineStateChangeTime.GetValueOrDefault(status.MachineId, DateTime.Now);

                // Değişimden sonra en az 5 saniye geçmeli (Sinyaller otursun)
                if (!lastReportedAlarms.SetEquals(_machineCurrentAlarms[status.MachineId]) &&
                    (DateTime.Now - stateChangeTime).TotalSeconds >= 5)
                {
                    var alarmsToReport = _machineCurrentAlarms[status.MachineId];

                    if (alarmsToReport.Count > 0)
                    {
                        List<string> messageLines = new List<string>();
                        bool stillHasUndefined = false;

                        foreach (var no in alarmsToReport)
                        {
                            string alarmName = "";

                            // 1. Sözlükte varsa al (En güvenli yol)
                            if (dict.TryGetValue(no, out var foundName))
                            {
                                alarmName = foundName;
                            }
                            // 2. Sözlükte yoksa ama PLC'den gelen ana metin bu numaraya aitse onu kullan
                            else if (no == status.ActiveAlarmNumber && !string.IsNullOrEmpty(status.ActiveAlarmText) && !status.ActiveAlarmText.Contains("Tanımsız"))
                            {
                                alarmName = status.ActiveAlarmText;
                            }
                            // 3. Hala yoksa "Tanımsız" de ve bayrağı kaldır
                            else
                            {
                                alarmName = $"Tanımsız Alarm";
                                stillHasUndefined = true;
                            }

                            messageLines.Add($"• {alarmName} (Kod: {no})");
                        }

                        // EĞER HALA TANIMSIZ VARSA: Telegram'a eksik bilgiyle mesaj atma! 
                        // Bir sonraki döngüde (1 saniye sonra) tekrar isimleri çekmeye çalışacak.
                        if (stillHasUndefined && (DateTime.Now - stateChangeTime).TotalSeconds < 15)
                        {
                            return;
                        }

                        await _telegramService.SendAlarmListAsync(factoryName, status.MachineName, string.Join("\n", messageLines));
                    }
                    else
                    {
                        // Liste boşsa temizleme mesajı
                        await _telegramService.SendAlarmListAsync(factoryName, status.MachineName, "✅ Tüm alarmlar giderildi.");
                    }

                    // Gönderilen listeyi kaydet
                    _machineReportedAlarms[status.MachineId] = new HashSet<int>(alarmsToReport);
                }
            }
        }

        // --- 6. KOMUT GÖNDERİMİ ---
        public async Task SendCommandToLocal(int factoryId, int machineId, string command, string parameters)
        {
            string? targetId = GetTargetGateway(factoryId);
            if (targetId != null)
                await Clients.Client(targetId).SendAsync("ReceiveCommand", machineId, command, parameters);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _gatewayConnections.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        // --- 7. İSTEK YÖNLENDİRME (WRAPPER) ---
        private async Task<T?> InvokeOnGateway<T>(int factoryId, string targetMethod, int timeoutSeconds = 30, params object[] args)
        {
            string? targetConnectionId = GetTargetGateway(factoryId);
            if (string.IsNullOrEmpty(targetConnectionId)) return default;
            return await SendRequestToGateway<T>(targetConnectionId, timeoutSeconds, targetMethod, args);
        }
        public async Task<PagedResult<AlarmReportItem>> GetAlarmReportPaged(int factoryId, ReportFilters filters, int pageNumber, int pageSize)
        {
            // Gateway'deki "GetAlarmReportPaged" handler'ını çağırıyoruz
            var result = await InvokeOnGateway<PagedResult<AlarmReportItem>>(
                factoryId,
                "GetAlarmReportPaged",
                60, // Timeout süresi (60sn yeterli olabilir)
                filters, pageNumber, pageSize
            );

            return result ?? new PagedResult<AlarmReportItem>();
        }
        public Task<List<int>> GetOnlineFactoryIds()
        {
            var onlineIds = _gatewayConnections.Values.Distinct().ToList();
            return Task.FromResult(onlineIds);
        }

        private string? GetTargetGateway(int targetFactoryId)
        {
            var user = Context.User;
            if (user == null) return null;
            var allowedIdsStr = user.FindFirst("AllowedFactoryIds")?.Value;
            if (string.IsNullOrEmpty(allowedIdsStr)) return null;

            bool isAuthorized = allowedIdsStr == "ALL" || allowedIdsStr.Split(',').Select(int.Parse).Contains(targetFactoryId);
            if (!isAuthorized) return null;

            var gatewayEntry = _gatewayConnections.FirstOrDefault(x => x.Value == targetFactoryId);
            return string.IsNullOrEmpty(gatewayEntry.Key) ? null : gatewayEntry.Key;
        }

        // --- CHUNKING YÖNETİMİ ---
        public void ReceiveResponseChunk(string requestId, string chunk, bool isLast)
        {
            if (!_pendingRequests.ContainsKey(requestId)) return;

            var buffer = _chunkBuffers.GetOrAdd(requestId, _ => new StringBuilder());

            lock (buffer) // Tüm süreci kilitle
            {
                buffer.Append(chunk);

                if (isLast)
                {
                    if (_pendingRequests.TryRemove(requestId, out var tcs))
                    {
                        var finalJson = buffer.ToString();
                        _chunkBuffers.TryRemove(requestId, out _); // Temizliği hemen yap
                        tcs.TrySetResult(finalJson);
                    }
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
                    if (doc.RootElement.ValueKind == JsonValueKind.Array) return JsonSerializer.Deserialize<T>(jsonString, options);
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
                        if (!string.IsNullOrEmpty(innerJson)) return JsonSerializer.Deserialize<T>(innerJson, options);
                    }
                }
            }
            catch { return default; }
            return default;
        }

        public async Task LogAction(int factoryId, ActionLogEntry entry)
        {
            string? targetId = GetTargetGateway(factoryId);
            if (targetId != null) await Clients.Client(targetId).SendAsync("HandleLogAction", entry);
        }

        // --- PUBLIC METOTLAR (EKSİKSİZ LİSTE) ---

        public async Task<List<Machine>> GetAllMachines(int factoryId) => await InvokeOnGateway<List<Machine>>(factoryId, "GetAllMachines") ?? new List<Machine>();
        public async Task<FullMachineStatus?> GetMachineStatus(int factoryId, int id) => await InvokeOnGateway<FullMachineStatus>(factoryId, "GetMachineStatus", 30, id);

        public async Task AddMachine(int factoryId, Machine machine)
        {
            await InvokeOnGateway<bool>(factoryId, "AddMachine", 30, machine);
            await Clients.Group($"Factory_{factoryId}").SendAsync("MachineListUpdated");
        }
        public async Task UpdateMachine(int factoryId, Machine machine)
        {
            await InvokeOnGateway<bool>(factoryId, "UpdateMachine", 30, machine);
            await Clients.Group($"Factory_{factoryId}").SendAsync("MachineListUpdated");
        }
        public async Task DeleteMachine(int factoryId, int id)
        {
            await InvokeOnGateway<bool>(factoryId, "DeleteMachine", 30, id);
            await Clients.Group($"Factory_{factoryId}").SendAsync("MachineListUpdated");
        }

        public async Task<List<User>> GetUsers(int factoryId) => await InvokeOnGateway<List<User>>(factoryId, "GetAllUsers") ?? new List<User>();
        public async Task<List<Role>> GetRoles(int factoryId) => await InvokeOnGateway<List<Role>>(factoryId, "GetAllRoles") ?? new List<Role>();
        public async Task AddUser(int factoryId, UserViewModel model) => await InvokeOnGateway<bool>(factoryId, "AddUser", 30, model);
        public async Task UpdateUser(int factoryId, UserViewModel model) => await InvokeOnGateway<bool>(factoryId, "UpdateUser", 30, model);
        public async Task DeleteUser(int factoryId, int id) => await InvokeOnGateway<bool>(factoryId, "DeleteUser", 30, id);

        public async Task<List<CostParameter>> GetCosts(int factoryId) => await InvokeOnGateway<List<CostParameter>>(factoryId, "GetCosts") ?? new List<CostParameter>();
        public async Task UpdateCosts(int factoryId, List<CostParameter> costs) => await InvokeOnGateway<bool>(factoryId, "UpdateParameters", 30, costs);

        public async Task<List<AlarmDefinition>> GetAlarms(int factoryId) => await InvokeOnGateway<List<AlarmDefinition>>(factoryId, "GetAllAlarmDefinitions") ?? new List<AlarmDefinition>();
        public async Task AddAlarm(int factoryId, AlarmDefinition alarm) => await InvokeOnGateway<bool>(factoryId, "AddAlarmDefinition", 30, alarm);
        public async Task UpdateAlarm(int factoryId, AlarmDefinition alarm) => await InvokeOnGateway<bool>(factoryId, "UpdateAlarmDefinition", 30, alarm);
        public async Task DeleteAlarm(int factoryId, int id) => await InvokeOnGateway<bool>(factoryId, "DeleteAlarmDefinition", 30, id);

        public async Task<List<ScadaRecipe>> GetRecipes(int factoryId) => await InvokeOnGateway<List<ScadaRecipe>>(factoryId, "GetAllRecipes") ?? new List<ScadaRecipe>();
        public async Task<ScadaRecipe?> GetRecipeDetails(int factoryId, int id) => await InvokeOnGateway<ScadaRecipe>(factoryId, "GetRecipeById", 30, id);
        public async Task SaveRecipe(int factoryId, ScadaRecipe recipe) => await InvokeOnGateway<bool>(factoryId, "SaveRecipe", 30, recipe);
        public async Task DeleteRecipe(int factoryId, int id) => await InvokeOnGateway<bool>(factoryId, "DeleteRecipe", 30, id);
        public async Task<List<ProductionReportItem>> GetRecipeConsumptionHistory(int factoryId, int recipeId) => await InvokeOnGateway<List<ProductionReportItem>>(factoryId, "GetRecipeUsageHistory", 30, recipeId) ?? new List<ProductionReportItem>();

        public async Task<bool> SendRecipeToPlc(int factoryId, int recipeId, int machineId) => await InvokeOnGateway<bool>(factoryId, "SendRecipeToPlc", 30, recipeId, machineId);
        public async Task<ScadaRecipe?> ReadRecipeFromPlc(int factoryId, int machineId) => await InvokeOnGateway<ScadaRecipe>(factoryId, "ReadRecipeFromPlc", 30, machineId);

        public async Task<List<string>> GetMachineSubTypesDesign(int factoryId) => await InvokeOnGateway<List<string>>(factoryId, "GetMachineSubTypes");
        public async Task<List<StepTypeDtoDesign>> GetStepTypesDesign(int factoryId) => await InvokeOnGateway<List<StepTypeDtoDesign>>(factoryId, "GetStepTypes");
        public async Task<List<ControlMetadata>> GetLayoutDesign(int factoryId, string subType, int stepTypeId) => await InvokeOnGateway<List<ControlMetadata>>(factoryId, "GetLayoutJson", 30, subType, stepTypeId) ?? new List<ControlMetadata>();
        public async Task<bool> SaveLayoutDesign(int factoryId, string subType, int stepTypeId, List<ControlMetadata> layout) => await InvokeOnGateway<bool>(factoryId, "SaveLayout", 30, subType, stepTypeId, layout);
        public async Task<string> GetStepLayout(int factoryId, string subType, int stepTypeId)
        {
            var list = await InvokeOnGateway<List<ControlMetadata>>(factoryId, "GetLayoutJson", 30, subType, stepTypeId);
            if (list == null) return string.Empty;
            return JsonSerializer.Serialize(list);
        }

        public async Task<List<PlcOperator>> GetPlcOperators(int factoryId) => await InvokeOnGateway<List<PlcOperator>>(factoryId, "GetPlcOperators") ?? new List<PlcOperator>();
        public async Task SavePlcOperator(int factoryId, PlcOperator op) => await InvokeOnGateway<bool>(factoryId, "SaveOrUpdateOperator", 30, op);
        public async Task AddDefaultPlcOperator(int factoryId) => await InvokeOnGateway<bool>(factoryId, "AddDefaultOperator");
        public async Task DeletePlcOperator(int factoryId, int id) => await InvokeOnGateway<bool>(factoryId, "DeleteOperator", 30, id);

        public async Task<Dictionary<int, string>> GetHmiRecipeNames(int factoryId, int machineId) => await InvokeOnGateway<Dictionary<int, string>>(factoryId, "GetHmiRecipeNames", 30, machineId) ?? new Dictionary<int, string>();
        public async Task<ScadaRecipe?> GetHmiRecipePreview(int factoryId, int machineId, string fileName) => await InvokeOnGateway<ScadaRecipe>(factoryId, "GetHmiRecipePreview", 30, machineId, fileName);

        public async Task<bool> QueueSequentiallyNamedSendJobs(int factoryId, List<int> recipeIds, List<int> machineIds, int startNumber) => await InvokeOnGateway<bool>(factoryId, "QueueSequentiallyNamedSendJobs", 30, recipeIds, machineIds, startNumber);
        public async Task<bool> QueueReceiveJobs(int factoryId, List<string> fileNames, int machineId) => await InvokeOnGateway<bool>(factoryId, "QueueReceiveJobs", 30, fileNames, machineId);
        public async Task<List<TransferJob>> GetActiveJobs(int factoryId) => await InvokeOnGateway<List<TransferJob>>(factoryId, "GetActiveFtpJobs") ?? new List<TransferJob>();

        public async Task<List<OeeData>> GetOeeReport(int factoryId, ReportFilters filters) => await InvokeOnGateway<List<OeeData>>(factoryId, "GetOeeReport", 60, filters) ?? new List<OeeData>();
        public async Task<List<HourlyConsumptionData>> GetHourlyConsumption(int factoryId) => await InvokeOnGateway<List<HourlyConsumptionData>>(factoryId, "GetHourlyFactoryConsumption") ?? new List<HourlyConsumptionData>();
        public async Task<List<HourlyOeeData>> GetHourlyOee(int factoryId) => await InvokeOnGateway<List<HourlyOeeData>>(factoryId, "GetHourlyAverageOee") ?? new List<HourlyOeeData>();
        public async Task<List<TopAlarmData>> GetTopAlarms(int factoryId) => await InvokeOnGateway<List<TopAlarmData>>(factoryId, "GetTopAlarmsByFrequency") ?? new List<TopAlarmData>();
        public async Task<List<object>> GetManualTrendData(int factoryId, ReportFilters filters)
        {
            // Gateway'deki "GetManualTrendData" handler'ını çağırır
            var result = await InvokeOnGateway<List<object>>(factoryId, "GetManualTrendData", 60, filters);
            return result ?? new List<object>();
        }
        // --- RAPORLAR (Timeout Süreleri Artırıldı: 120sn) ---
        public async Task<List<ProductionReportItem>> GetProductionReport(int factoryId, ReportFilters filters, int timeout = 120) => await InvokeOnGateway<List<ProductionReportItem>>(factoryId, "GetProductionReport", timeout, filters) ?? new List<ProductionReportItem>();
        public async Task<List<AlarmReportItem>> GetAlarmReport(int factoryId, ReportFilters filters, int timeout = 120) => await InvokeOnGateway<List<AlarmReportItem>>(factoryId, "GetAlarmReport", timeout, filters) ?? new List<AlarmReportItem>();
        public async Task<object> GetTrendData(int factoryId, ReportFilters filters, int timeout = 120) => await InvokeOnGateway<object>(factoryId, "GetTrendData", timeout, filters) ?? new List<object>();
        public async Task<ManualConsumptionSummary?> GetManualConsumptionReport(int factoryId, ReportFilters filters, int timeout = 120) => await InvokeOnGateway<ManualConsumptionSummary>(factoryId, "GetManualConsumptionReport", timeout, filters);
        public async Task<ConsumptionTotals?> GetConsumptionTotals(int factoryId, ReportFilters filters, int timeout = 120) => await InvokeOnGateway<ConsumptionTotals>(factoryId, "GetConsumptionTotalsForPeriod", timeout, filters);
        public async Task<List<ProductionReportItem>> GetGeneralDetailedConsumptionReport(int factoryId, GeneralDetailedConsumptionFilters filters, int timeout = 180) => await InvokeOnGateway<List<ProductionReportItem>>(factoryId, "GetGeneralDetailedConsumptionReport", timeout, filters) ?? new List<ProductionReportItem>();
        public async Task<List<ActionLogEntry>> GetActionLogs(int factoryId, ActionLogFilters filters, int timeout = 60) => await InvokeOnGateway<List<ActionLogEntry>>(factoryId, "GetActionLogs", timeout, filters) ?? new List<ActionLogEntry>();
        public async Task<ProductionDetailDto?> GetProductionDetail(int factoryId, int machineId, string batchId) => await InvokeOnGateway<ProductionDetailDto>(factoryId, "GetProductionDetail", 60, machineId, batchId);

        // --- EXPORT METOTLARI (Byte[] Olarak Kaldı) ---
        public async Task<byte[]> ExportProductionReport(int factoryId, List<ProductionReportItem> items) => await InvokeOnGateway<byte[]>(factoryId, "ExportProductionReport", 180, items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportAlarmReport(int factoryId, List<AlarmReportItem> items) => await InvokeOnGateway<byte[]>(factoryId, "ExportAlarmReport", 180, items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportOeeReport(int factoryId, List<OeeData> items) => await InvokeOnGateway<byte[]>(factoryId, "ExportOeeReport", 180, items) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportManualConsumptionReport(int factoryId, ManualConsumptionSummary summary) => await InvokeOnGateway<byte[]>(factoryId, "ExportManualConsumptionReport", 180, summary) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportGeneralDetailedConsumptionReport(int factoryId, GeneralConsumptionExportDto data) => await InvokeOnGateway<byte[]>(factoryId, "ExportGeneralDetailedConsumptionReport", 300, data) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportActionLogsReport(int factoryId, List<ActionLogEntry> logs) => await InvokeOnGateway<byte[]>(factoryId, "ExportActionLogsReport", 180, logs) ?? Array.Empty<byte>();
        public async Task<byte[]> ExportProductionDetailFile(int factoryId, int machineId, string batchId) => await InvokeOnGateway<byte[]>(factoryId, "ExportProductionDetailFile", 180, machineId, batchId) ?? Array.Empty<byte>();
    }
}