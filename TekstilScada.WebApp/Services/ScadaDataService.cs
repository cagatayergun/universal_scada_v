using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;
using static TekstilScada.Repositories.AlarmRepository;

// --- DTO Sınıfları (Global) ---
public class TrendDataPoint { public DateTime Timestamp { get; set; } public double Temperature { get; set; } public double Rpm { get; set; } public double WaterLevel { get; set; } }
public class ProductionStepDetailDto : TekstilScada.Models.ProductionStepDetail { public double TheoreticalDurationSeconds { get; set; } = 0; public double Temperature { get; set; } = 0; public string StepDescription => StepName; }
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

public class ProductionDetailDto { public TekstilScada.Models.ProductionReportItem Header { get; set; } = new(); public List<ProductionStepDetailDto> Steps { get; set; } = new(); public List<AlarmDetailDto> Alarms { get; set; } = new(); public List<TrendDataPoint> LogData { get; set; } = new(); public List<TrendDataPoint> TheoreticalData { get; set; } = new(); }
public class GeneralDetailedConsumptionFilters { public DateTime StartTime { get; set; } public DateTime EndTime { get; set; } public List<int>? MachineIds { get; set; } }
public class ActionLogFilters { public DateTime StartTime { get; set; } public DateTime EndTime { get; set; } public string? Username { get; set; } public string? Details { get; set; } }
public class HourlyConsumptionData { public double Saat { get; set; } public double ToplamElektrik { get; set; } public double ToplamSu { get; set; } public double ToplamBuhar { get; set; } }
public class HourlyOeeData { public double Saat { get; set; } public double AverageOEE { get; set; } }
public class ReportFilters1 { public DateTime StartTime { get; set; } public DateTime EndTime { get; set; } public int? MachineId { get; set; } public string? BatchNo { get; set; } }
public class JsReadyTrendDataPoint { public DateTime Timestamp { get; set; } public double TimestampOADate { get; set; } public double Temperature { get; set; } public double Rpm { get; set; } public double WaterLevel { get; set; } }
public class StepTypeDto { public int Id { get; set; } public string Name { get; set; } }
public class SaveLayoutRequest { public string LayoutName { get; set; } public string MachineSubType { get; set; } public int StepTypeId { get; set; } public string LayoutJson { get; set; } }
public class GeneralConsumptionExportDto { public List<ProductionReportItem>? Items { get; set; } public string? ConsumptionType { get; set; } }
public class CentralFactoryDto { public int Id { get; set; } public string FactoryName { get; set; } }
public class LoginResponse { public string Token { get; set; } public string FullName { get; set; } public string Role { get; set; } public string AllowedFactories { get; set; } }

namespace TekstilScada.WebApp.Services
{
    public class ScadaDataService : IAsyncDisposable
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly JsonSerializerOptions _serializerOptions;

        private HubConnection? _hubConnection;
        public HubConnection? HubConnection => _hubConnection;

        private Action<int, FullMachineStatus>? _onMachineUpdate;
        public event Action? OnDataUpdated;
        public event Action? OnFactoryChanged;
        public event Action<TransferJob>? OnFtpProgressReceived;

        // Hata Bildirim Event'i
        public event Action<string>? OnError;

        public List<int> UserAllowedFactoryIds { get; private set; } = new();
        public string UserRole { get; private set; } = "";
        private int _currentSelectedFactoryId = 0;
        // --- YENİ EKLENEN SATIR ---
        // Bu özellik sayesinde VncMonitor sayfası seçili fabrika ID'sine ulaşabilecek.
        public int CurrentFactoryId => _currentSelectedFactoryId;
        public string CurrentFactoryName { get; private set; } = "";
        public int TotalFactoriesCount { get; private set; } = 0;
        public List<CentralFactoryDto> CachedFactories { get; private set; } = new();

        public ConcurrentDictionary<int, FullMachineStatus> MachineData { get; private set; } = new();
        public ConcurrentDictionary<int, Machine> MachineDetailsCache { get; private set; } = new();
        private string _accessToken = string.Empty;

        public ScadaDataService(HttpClient httpClient, ILocalStorageService localStorage, IConfiguration config)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _config = config;
            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public async Task InitializeAsync()
        {
            try
            {
                _accessToken = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(_accessToken)) return;

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
                {
                    await SetupSignalRConnection();
                }
            }
            catch (Exception ex) { Console.WriteLine($"[ScadaService] Init Hata: {ex.Message}"); }
        }

        public async Task InitializeForBackgroundServiceAsync()
        {
            try
            {
                if (!_httpClient.DefaultRequestHeaders.Contains("X-Service-Key"))
                    _httpClient.DefaultRequestHeaders.Add("X-Service-Key", "UniversalScadaServiceKey_2024");

                try { await LoginAndGetTokenAsync(); if (!string.IsNullOrEmpty(_accessToken)) _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken); }
                catch { }

                if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected) await SetupSignalRConnection();
            }
            catch (Exception ex) { Console.WriteLine($"[ScadaService] BG Init Hata: {ex.Message}"); }
        }

        private async Task SetupSignalRConnection()
        {
            var hubUrl = new Uri(_httpClient.BaseAddress!, "/scadaHub");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_accessToken);
                    options.HttpMessageHandlerFactory = (h) => { if (h is HttpClientHandler c) c.ServerCertificateCustomValidationCallback = (s, c2, ch, e) => true; return h; };
                    options.WebSocketConfiguration = w => { w.RemoteCertificateValidationCallback = (s, c, ch, e) => true; };

                    // --- DEĞİŞİKLİK 1: 500 Makine için Buffer Artırımı (10MB) ---
                    // Standart 32KB yetersiz kalabilir, büyük paketler için artırıyoruz.
                    options.ApplicationMaxBufferSize = 10 * 1024 * 1024;
                    options.TransportMaxBufferSize = 10 * 1024 * 1024;
                })
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true; })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<TransferJob>("ReceiveFtpProgress", (job) => { if (job != null) OnFtpProgressReceived?.Invoke(job); });

            // --- ESKİ YÖNTEM (Tekli Güncelleme - Geri Uyumluluk) ---
            _hubConnection.On<int, FullMachineStatus>("ReceiveMachineUpdate", (factoryId, status) =>
            {
                if (status != null)
                {
                    _onMachineUpdate?.Invoke(factoryId, status);
                    if (_currentSelectedFactoryId == factoryId || _currentSelectedFactoryId == 0)
                    {
                        MachineData[status.MachineId] = status;
                        if (!MachineDetailsCache.ContainsKey(status.MachineId))
                        {
                            MachineDetailsCache.TryAdd(status.MachineId, new Machine { Id = status.MachineId, MachineName = status.MachineName, MachineSubType = string.IsNullOrEmpty(status.MakineTipi) ? "Standart" : status.MakineTipi });
                        }
                        OnDataUpdated?.Invoke();
                    }
                }
            });

            // --- DEĞİŞİKLİK 2: YENİ Toplu Güncelleme (Batch Update) Listener ---
            // Gateway'den gelen 500 makinelik paket burada karşılanır.
            _hubConnection.On<int, List<FullMachineStatus>>("ReceiveMachineBatch", (factoryId, statusList) =>
            {
                if (statusList != null && statusList.Count > 0)
                {
                    // Sadece seçili fabrika (veya hepsi) ise işle
                    if (_currentSelectedFactoryId == factoryId || _currentSelectedFactoryId == 0)
                    {
                        foreach (var status in statusList)
                        {
                            // 1. Canlı veriyi güncelle
                            MachineData[status.MachineId] = status;

                            // 2. Cache'te olmayan makine varsa ekle (Otomatik keşif)
                            if (!MachineDetailsCache.ContainsKey(status.MachineId))
                            {
                                MachineDetailsCache.TryAdd(status.MachineId, new Machine
                                {
                                    Id = status.MachineId,
                                    MachineName = status.MachineName,
                                    MachineSubType = string.IsNullOrEmpty(status.MakineTipi) ? "Standart" : status.MakineTipi
                                });
                            }
                        }

                        // Döngü bittikten sonra tek bir güncelleme eventi fırlat (UI donmasını engeller)
                        OnDataUpdated?.Invoke();
                    }
                }
            });

            _hubConnection.Reconnected += async (c) => { if (_currentSelectedFactoryId > 0) try { await _hubConnection.InvokeAsync("SubscribeToFactories", new List<int> { _currentSelectedFactoryId }); } catch { } };
            await _hubConnection.StartAsync();
        }

        private async Task LoginAndGetTokenAsync()
        {
            var username = _config["ServiceAccount:Username"] ?? "admin";
            var password = _config["ServiceAccount:Password"] ?? "1234";
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                _accessToken = result?.Token ?? string.Empty;
            }
        }

        public async Task<List<CentralFactoryDto>> GetMyFactoriesAsync()
        {
            try { var f = await _httpClient.GetFromJsonAsync<List<CentralFactoryDto>>("api/factory/my-factories"); CachedFactories = f ?? new(); TotalFactoriesCount = CachedFactories.Count; OnFactoryChanged?.Invoke(); return CachedFactories; } catch { return new List<CentralFactoryDto>(); }
        }

        public async Task<List<CentralFactoryDto>> GetAllFactoriesAsync() => await GetMyFactoriesAsync();

        public async Task<List<int>> GetOnlineFactoryIdsAsync()
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return new List<int>();
            try { return await _hubConnection.InvokeAsync<List<int>>("GetOnlineFactoryIds"); } catch { return new List<int>(); }
        }

        public async Task SelectFactoryAndSubscribeAsync(int factoryId, string factoryName)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            var onlineIds = await GetOnlineFactoryIdsAsync();
            if (!onlineIds.Contains(factoryId)) throw new Exception("Fabrika çevrimdışı.");

            MachineData.Clear(); MachineDetailsCache.Clear();
            CurrentFactoryName = factoryName; _currentSelectedFactoryId = factoryId;
            OnDataUpdated?.Invoke();
            await _hubConnection.InvokeAsync("SubscribeToFactories", new List<int> { factoryId });
            OnFactoryChanged?.Invoke();
        }

        public void SubscribeToLiveUpdates(Action<int, FullMachineStatus> handler) { _onMachineUpdate = handler; }
        private int ResolveId(int factoryId) => factoryId > 0 ? factoryId : _currentSelectedFactoryId;

        // ====================================================================================
        // GENERIC WRAPPER METOTLARI
        // ====================================================================================

        private async Task<T> InvokeSafeAsync<T>(string methodName, int factoryId, T defaultValue, params object[] args)
        {
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected) return defaultValue;

            try
            {
                int targetFactoryId = ResolveId(factoryId);
                var fullArgs = new object[args.Length + 1];
                fullArgs[0] = targetFactoryId;
                Array.Copy(args, 0, fullArgs, 1, args.Length);

                return await _hubConnection.InvokeCoreAsync<T>(methodName, fullArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScadaService] Hata ({methodName}): {ex.Message}");
                OnError?.Invoke($"Veri alma hatası ({methodName}): {ex.Message}");
                return defaultValue;
            }
        }

        private async Task InvokeSafeActionAsync(string methodName, int factoryId, params object[] args)
        {
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                int targetFactoryId = ResolveId(factoryId);
                var fullArgs = new object[args.Length + 1];
                fullArgs[0] = targetFactoryId;
                Array.Copy(args, 0, fullArgs, 1, args.Length);

                await _hubConnection.InvokeCoreAsync(methodName, fullArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScadaService] İşlem Hatası ({methodName}): {ex.Message}");
                OnError?.Invoke($"İşlem başarısız ({methodName}): {ex.Message}");
            }
        }

        // ====================================================================================
        // HUB METOTLARI (DÜZELTİLMİŞ İSİMLER)
        // ====================================================================================

        // DÜZELTME 1: Hub'daki metot adı "GetUsers"
        public async Task<List<User>> GetUsersAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetUsers", factoryId, new List<User>());

        // DÜZELTME 2: Hub'daki metot adı "GetRoles"
        public async Task<List<Role>> GetRolesAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetRoles", factoryId, new List<Role>());

        public async Task AddUserAsync(UserViewModel u, int factoryId = 0)
            => await InvokeSafeActionAsync("AddUser", factoryId, u);

        public async Task UpdateUserAsync(UserViewModel u, int factoryId = 0)
            => await InvokeSafeActionAsync("UpdateUser", factoryId, u);

        public async Task DeleteUserAsync(int id, int factoryId = 0)
            => await InvokeSafeActionAsync("DeleteUser", factoryId, id);

        public async Task<List<Machine>> GetMachinesAsync(int factoryId = 0)
        {
            var machines = await InvokeSafeAsync("GetAllMachines", factoryId, new List<Machine>());
            if (machines.Any())
            {
                foreach (var x in machines) MachineDetailsCache.TryAdd(x.Id, x);
                return machines;
            }
            return MachineDetailsCache.Values.ToList();
        }

        public async Task<Machine?> AddMachineAsync(Machine m, int factoryId = 0)
        {
            await InvokeSafeActionAsync("AddMachine", factoryId, m);
            return m;
        }

        public async Task<bool> UpdateMachineAsync(Machine m, int factoryId = 0)
            => await InvokeSafeAsync("UpdateMachine", factoryId, false, m);

        public async Task<bool> DeleteMachineAsync(int id, int factoryId = 0)
            => await InvokeSafeAsync("DeleteMachine", factoryId, false, id);

        public async Task<FullMachineStatus?> GetMachineStatusAsync(int id, int factoryId = 0)
            => await InvokeSafeAsync<FullMachineStatus?>("GetMachineStatus", factoryId, null, id);

        public async Task<List<FullMachineStatus>> GetLiveMachineStatusByFactoryId(int factoryId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return new List<FullMachineStatus>();
            try { return await _hubConnection.InvokeAsync<List<FullMachineStatus>>("GetLiveMachineStatusByFactoryId", factoryId); }
            catch { return new List<FullMachineStatus>(); }
        }

        public async Task<List<ScadaRecipe>> GetRecipesAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetRecipes", factoryId, new List<ScadaRecipe>());

        public async Task<ScadaRecipe?> GetRecipeDetailsAsync(int recipeId, int factoryId = 0)
            => await InvokeSafeAsync<ScadaRecipe?>("GetRecipeDetails", factoryId, null, recipeId);

        public async Task<ScadaRecipe?> SaveRecipeAsync(ScadaRecipe recipe, int factoryId = 0)
        {
            await InvokeSafeActionAsync("SaveRecipe", factoryId, recipe);
            return recipe;
        }

        public async Task<bool> DeleteRecipeAsync(int recipeId, int factoryId = 0)
            => await InvokeSafeAsync("DeleteRecipe", factoryId, false, recipeId);

        public async Task<bool> SendRecipeToPlcAsync(int rId, int mId, int factoryId = 0)
            => await InvokeSafeAsync("SendRecipeToPlc", factoryId, false, rId, mId);

        public async Task<ScadaRecipe?> ReadRecipeFromPlcAsync(int mId, int factoryId = 0)
            => await InvokeSafeAsync<ScadaRecipe?>("ReadRecipeFromPlc", factoryId, null, mId);

        // DÜZELTME 3: Hub'daki metot adı "GetRecipeConsumptionHistory"
        public async Task<List<ProductionReportItem>?> GetRecipeConsumptionHistoryAsync(int rId, int factoryId = 0)
            => await InvokeSafeAsync("GetRecipeConsumptionHistory", factoryId, new List<ProductionReportItem>(), rId);

        // --- DESIGNER ---
        public async Task<List<ControlMetadata>> GetLayoutAsync(string subType, int stepTypeId, int factoryId = 0)
            => await InvokeSafeAsync("GetLayoutDesign", factoryId, new List<ControlMetadata>(), subType, stepTypeId);

        public async Task<List<string>> GetMachineSubTypesAsyncDesign(int factoryId = 0)
            => await InvokeSafeAsync("GetMachineSubTypesDesign", factoryId, new List<string> { "DEFAULT" });

        public async Task<List<StepTypeDtoDesign>> GetStepTypesAsyncDesign(int factoryId = 0)
            => await InvokeSafeAsync("GetStepTypesDesign", factoryId, new List<StepTypeDtoDesign>());

        // DÜZELTME 4: Hub'daki metot adı "SaveLayoutDesign"
        public async Task SaveLayoutAsync(string subType, int stepTypeId, List<ControlMetadata> layout, int factoryId = 0)
            => await InvokeSafeAsync<bool>("SaveLayoutDesign", factoryId, false, subType, stepTypeId, layout);

        public async Task<List<AlarmDefinition>> GetAlarmsAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetAlarms", factoryId, new List<AlarmDefinition>());

        public async Task AddAlarmAsync(AlarmDefinition a, int factoryId = 0)
            => await InvokeSafeActionAsync("AddAlarm", factoryId, a);

        public async Task UpdateAlarmAsync(AlarmDefinition a, int factoryId = 0)
            => await InvokeSafeActionAsync("UpdateAlarm", factoryId, a);

        public async Task DeleteAlarmAsync(int id, int factoryId = 0)
            => await InvokeSafeActionAsync("DeleteAlarm", factoryId, id);

        public async Task<List<CostParameter>> GetCostsAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetCosts", factoryId, new List<CostParameter>());

        // DÜZELTME 5: Hub'daki metot adı "UpdateCosts"
        public async Task UpdateCostsAsync(List<CostParameter> c, int factoryId = 0)
            => await InvokeSafeActionAsync("UpdateCosts", factoryId, c);

        public async Task<List<PlcOperator>> GetPlcOperatorsAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetPlcOperators", factoryId, new List<PlcOperator>());

        // DÜZELTME 6: Hub'daki metot adı "SavePlcOperator"
        public async Task SavePlcOperatorAsync(PlcOperator o, int factoryId = 0)
            => await InvokeSafeActionAsync("SavePlcOperator", factoryId, o);

        public async Task AddDefaultPlcOperatorAsync(int factoryId = 0)
            => await InvokeSafeActionAsync("AddDefaultPlcOperator", factoryId);

        // DÜZELTME 7: Hub'daki metot adı "DeletePlcOperator"
        public async Task DeletePlcOperatorAsync(int id, int factoryId = 0)
            => await InvokeSafeActionAsync("DeletePlcOperator", factoryId, id);

        // --- RAPORLAR & VERİLER (Timeout Süreleri ile) ---
        public async Task<List<ProductionReportItem>> GetProductionReportAsync(ReportFilters f, int factoryId = 0)
            => await InvokeSafeAsync("GetProductionReport", factoryId, new List<ProductionReportItem>(), f, 120);

        public async Task<List<AlarmReportItem>> GetAlarmReportAsync(ReportFilters f, int factoryId = 0)
            => await InvokeSafeAsync("GetAlarmReport", factoryId, new List<AlarmReportItem>(), f, 120);

        public async Task<List<object>?> GetTrendDataAsync(ReportFilters f, int factoryId = 0)
            => await InvokeSafeAsync<List<object>?>("GetTrendData", factoryId, null, f, 120);

        public async Task<ManualConsumptionSummary?> GetManualConsumptionReportAsync(ReportFilters f, int factoryId = 0)
            => await InvokeSafeAsync<ManualConsumptionSummary?>("GetManualConsumptionReport", factoryId, null, f, 120);

        public async Task<ConsumptionTotals?> GetConsumptionTotalsAsync(ReportFilters f, int factoryId = 0)
            => await InvokeSafeAsync<ConsumptionTotals?>("GetConsumptionTotals", factoryId, null, f, 120);
        // ScadaDataService.cs içine ekleyin
        public async Task<List<object>> GetManualTrendDataAsync(ReportFilters f, int factoryId = 0)
        {
            return await InvokeSafeAsync("GetManualTrendData", factoryId, new List<object>(), f);
        }
        public async Task<List<ProductionReportItem>?> GetGeneralDetailedConsumptionReportAsync(GeneralDetailedConsumptionFilters f, int factoryId = 0)
            => await InvokeSafeAsync<List<ProductionReportItem>?>("GetGeneralDetailedConsumptionReport", factoryId, null, f, 180);

        public async Task<List<TekstilScada.Core.Models.ActionLogEntry>> GetActionLogsAsync(ActionLogFilters f, int factoryId = 0)
            => await InvokeSafeAsync("GetActionLogs", factoryId, new List<TekstilScada.Core.Models.ActionLogEntry>(), f, 60);

        public async Task<ProductionDetailDto?> GetProductionDetailAsync(int mId, string bId, int factoryId = 0)
            => await InvokeSafeAsync<ProductionDetailDto?>("GetProductionDetail", factoryId, null, mId, bId);

        public async Task<List<OeeData>> GetOeeReportAsync(ReportFilters f, int factoryId = 0)
            => await InvokeSafeAsync("GetOeeReport", factoryId, new List<OeeData>(), f);

        public async Task<List<HourlyConsumptionData>?> GetHourlyConsumptionAsync(int factoryId = 0)
            => await InvokeSafeAsync<List<HourlyConsumptionData>?>("GetHourlyConsumption", factoryId, null);

        public async Task<List<HourlyOeeData>?> GetHourlyOeeAsync(int factoryId = 0)
            => await InvokeSafeAsync<List<HourlyOeeData>?>("GetHourlyOee", factoryId, null);

        public async Task<List<TopAlarmData>?> GetTopAlarmsAsync(int factoryId = 0)
            => await InvokeSafeAsync<List<TopAlarmData>?>("GetTopAlarms", factoryId, null);

        // --- EXPORT METOTLARI ---
        public async Task<byte[]> ExportProductionReportAsync(List<ProductionReportItem> i, int factoryId = 0)
            => await InvokeSafeAsync("ExportProductionReport", factoryId, Array.Empty<byte>(), i);

        public async Task<byte[]> ExportAlarmReportAsync(List<AlarmReportItem> i, int factoryId = 0)
            => await InvokeSafeAsync("ExportAlarmReport", factoryId, Array.Empty<byte>(), i);

        public async Task<byte[]> ExportOeeReportAsync(List<OeeData> i, int factoryId = 0)
            => await InvokeSafeAsync("ExportOeeReport", factoryId, Array.Empty<byte>(), i);

        public async Task<byte[]> ExportManualConsumptionReportAsync(ManualConsumptionSummary s, int factoryId = 0)
            => await InvokeSafeAsync("ExportManualConsumptionReport", factoryId, Array.Empty<byte>(), s);

        public async Task<byte[]> ExportGeneralDetailedConsumptionReportAsync(GeneralConsumptionExportDto d, int factoryId = 0)
            => await InvokeSafeAsync("ExportGeneralDetailedConsumptionReport", factoryId, Array.Empty<byte>(), d);

        public async Task<byte[]> ExportActionLogsReportAsync(List<TekstilScada.Core.Models.ActionLogEntry> l, int factoryId = 0)
            => await InvokeSafeAsync("ExportActionLogsReport", factoryId, Array.Empty<byte>(), l);

        public async Task<byte[]> ExportProductionDetailFileAsync(int mId, string bId, int factoryId = 0)
            => await InvokeSafeAsync("ExportProductionDetailFile", factoryId, Array.Empty<byte>(), mId, bId);

        // --- DİĞER ---
        public async Task<Dictionary<int, string>?> GetHmiRecipeNamesAsync(int mId, int factoryId = 0)
            => await InvokeSafeAsync<Dictionary<int, string>?>("GetHmiRecipeNames", factoryId, null, mId);

        public async Task<ScadaRecipe?> GetHmiRecipePreviewAsync(int mId, string f, int factoryId = 0)
            => await InvokeSafeAsync<ScadaRecipe?>("GetHmiRecipePreview", factoryId, null, mId, f);

        public async Task<bool> QueueSequentiallyNamedSendJobsAsync(List<int> r, List<int> m, int s, int factoryId = 0)
            => await InvokeSafeAsync("QueueSequentiallyNamedSendJobs", factoryId, false, r, m, s);

        public async Task<bool> QueueReceiveJobsAsync(List<string> f, int m, int factoryId = 0)
            => await InvokeSafeAsync("QueueReceiveJobs", factoryId, false, f, m);

        public async Task<List<TransferJob>> GetActiveFtpJobsAsync(int factoryId = 0)
            => await InvokeSafeAsync("GetActiveJobs", factoryId, new List<TransferJob>());

        public async Task<string> GetGatewayIpAsync(int mId, int factoryId = 0)
            => await InvokeSafeAsync("GetGatewayIpForMachine", factoryId, "localhost:5901", mId);

        public async Task LogUserActionAsync(int uId, string t, string d, int factoryId = 0)
        {
            var entry = new TekstilScada.Core.Models.ActionLogEntry { UserId = uId, ActionType = t, Details = d, Timestamp = DateTime.Now };
            await InvokeSafeActionAsync("LogAction", factoryId, entry); // Düzeltme: Hub'daki metot adı "LogAction"
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null) { try { await _hubConnection.StopAsync(); await _hubConnection.DisposeAsync(); } catch { } _hubConnection = null; }
        }
        public void ExitFactory()
        {
            CurrentFactoryName = ""; _currentSelectedFactoryId = 0; MachineData.Clear(); MachineDetailsCache.Clear(); OnFactoryChanged?.Invoke();
        }
        public async Task<PagedResult<AlarmReportItem>> GetAlarmReportPagedAsync(ReportFilters f, int pageNumber, int pageSize)
        {
            return await InvokeSafeAsync("GetAlarmReportPaged", 0, new PagedResult<AlarmReportItem>(), f, pageNumber, pageSize);
        }
    }
}