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
using TekstilScada.Repositories; // Gerekirse
using TekstilScada.Services;     // Gerekirse

// --- DTO Sınıfları (Global) ---
public class TrendDataPoint { public DateTime Timestamp { get; set; } public double Temperature { get; set; } public double Rpm { get; set; } public double WaterLevel { get; set; } }
public class ProductionStepDetailDto : TekstilScada.Models.ProductionStepDetail { public double TheoreticalDurationSeconds { get; set; } = 0; public double Temperature { get; set; } = 0; public string StepDescription => StepName; }
public class AlarmDetailDto { public DateTime AlarmTime { get; set; } = DateTime.MinValue; public string AlarmType { get; set; } = string.Empty; public string AlarmDescription { get; set; } = string.Empty; public TimeSpan Duration { get; set; } = TimeSpan.Zero; }
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
        public event Action<TransferJob> OnFtpProgressReceived;

        public List<int> UserAllowedFactoryIds { get; private set; } = new();
        public string UserRole { get; private set; } = "";
        private int _currentSelectedFactoryId = 0;
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
                })
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true; })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<TransferJob>("ReceiveFtpProgress", (job) => { if (job != null) OnFtpProgressReceived?.Invoke(job); });
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

        // HATA GİDERİLDİ: Alias metot eklendi
        public async Task<List<CentralFactoryDto>> GetAllFactoriesAsync() => await GetMyFactoriesAsync();

        // HATA GİDERİLDİ: Metot Eklendi
        public async Task<List<int>> GetOnlineFactoryIdsAsync()
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return new List<int>();
            try { return await _hubConnection.InvokeAsync<List<int>>("GetOnlineFactoryIds"); } catch { return new List<int>(); }
        }

        // HATA GİDERİLDİ: Metot Eklendi
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

        // --- HUB METOTLARI (DÜZELTİLMİŞ İSİMLER & EKLENEN EKSİKLER) ---

        public async Task<List<User>> GetUsersAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<User>(); try { return await _hubConnection.InvokeAsync<List<User>>("GetAllUsers", ResolveId(factoryId)); } catch { return new List<User>(); } }
        public async Task<List<Role>> GetRolesAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<Role>(); try { return await _hubConnection.InvokeAsync<List<Role>>("GetAllRoles", ResolveId(factoryId)); } catch { return new List<Role>(); } }
        public async Task AddUserAsync(UserViewModel u, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("AddUser", ResolveId(factoryId), u); }
        public async Task UpdateUserAsync(UserViewModel u, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("UpdateUser", ResolveId(factoryId), u); }
        public async Task DeleteUserAsync(int id, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("DeleteUser", ResolveId(factoryId), id); }

        public async Task<List<Machine>> GetMachinesAsync(int factoryId = 0)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return MachineDetailsCache.Values.ToList();
            try { var m = await _hubConnection.InvokeAsync<List<Machine>>("GetAllMachines", ResolveId(factoryId)); foreach (var x in m) MachineDetailsCache.TryAdd(x.Id, x); return m; } catch { return MachineDetailsCache.Values.ToList(); }
        }
        public async Task<Machine?> AddMachineAsync(Machine m, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { await _hubConnection.InvokeAsync("AddMachine", ResolveId(factoryId), m); return m; } catch { return null; } }
        public async Task<bool> UpdateMachineAsync(Machine m, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return false; try { return await _hubConnection.InvokeAsync<bool>("UpdateMachine", ResolveId(factoryId), m); } catch { return false; } }
        public async Task<bool> DeleteMachineAsync(int id, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return false; try { return await _hubConnection.InvokeAsync<bool>("DeleteMachine", ResolveId(factoryId), id); } catch { return false; } }
        public async Task<FullMachineStatus?> GetMachineStatusAsync(int id, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<FullMachineStatus>("GetMachineStatus", ResolveId(factoryId), id); } catch { return null; } }
        public async Task<List<FullMachineStatus>> GetLiveMachineStatusByFactoryId(int factoryId) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<FullMachineStatus>(); try { return await _hubConnection.InvokeAsync<List<FullMachineStatus>>("GetLiveMachineStatusByFactoryId", factoryId); } catch { return new List<FullMachineStatus>(); } }

        public async Task<List<ScadaRecipe>> GetRecipesAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<ScadaRecipe>(); try { return await _hubConnection.InvokeAsync<List<ScadaRecipe>>("GetRecipes", ResolveId(factoryId)); } catch { return new List<ScadaRecipe>(); } }
        public async Task<ScadaRecipe?> GetRecipeDetailsAsync(int recipeId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<ScadaRecipe>("GetRecipeDetails", ResolveId(factoryId), recipeId); } catch { return null; } }
        public async Task<ScadaRecipe?> SaveRecipeAsync(ScadaRecipe recipe, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { await _hubConnection.InvokeAsync("SaveRecipe", ResolveId(factoryId), recipe); return recipe; } catch { return null; } }
        public async Task<bool> DeleteRecipeAsync(int recipeId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return false; try { return await _hubConnection.InvokeAsync<bool>("DeleteRecipe", ResolveId(factoryId), recipeId); } catch { return false; } }
        public async Task<bool> SendRecipeToPlcAsync(int rId, int mId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return false; try { return await _hubConnection.InvokeAsync<bool>("SendRecipeToPlc", ResolveId(factoryId), rId, mId); } catch { return false; } }
        public async Task<ScadaRecipe?> ReadRecipeFromPlcAsync(int mId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<ScadaRecipe>("ReadRecipeFromPlc", ResolveId(factoryId), mId); } catch { return null; } }
        public async Task<List<ProductionReportItem>?> GetRecipeConsumptionHistoryAsync(int rId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<ProductionReportItem>(); try { return await _hubConnection.InvokeAsync<List<ProductionReportItem>>("GetRecipeUsageHistory", ResolveId(factoryId), rId); } catch { return new List<ProductionReportItem>(); } }

        // --- DESIGNER (HATA GİDERİLDİ: GetLayoutAsync & Diğerleri) ---
        public async Task<List<ControlMetadata>> GetLayoutAsync(string subType, int stepTypeId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<ControlMetadata>(); try { return await _hubConnection.InvokeAsync<List<ControlMetadata>>("GetLayoutDesign", ResolveId(factoryId), subType, stepTypeId); } catch { return new List<ControlMetadata>(); } }

        // HATA GİDERİLDİ: Metot Eklendi
        public async Task<List<string>> GetMachineSubTypesAsyncDesign(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<string> { "DEFAULT" }; try { return await _hubConnection.InvokeAsync<List<string>>("GetMachineSubTypesDesign", ResolveId(factoryId)); } catch { return new List<string> { "DEFAULT" }; } }

        // HATA GİDERİLDİ: Metot Eklendi
        public async Task<List<StepTypeDtoDesign>> GetStepTypesAsyncDesign(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<StepTypeDtoDesign>(); try { return await _hubConnection.InvokeAsync<List<StepTypeDtoDesign>>("GetStepTypesDesign", ResolveId(factoryId)); } catch { return new List<StepTypeDtoDesign>(); } }

        // HATA GİDERİLDİ: 4 Parametreli Metot Eklendi (ReceteAdimTasarimcisi için)
        public async Task SaveLayoutAsync(string subType, int stepTypeId, List<ControlMetadata> layout, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return; try { await _hubConnection.InvokeAsync("SaveLayoutDesign", ResolveId(factoryId), subType, stepTypeId, layout); } catch { } }

        public async Task<List<AlarmDefinition>> GetAlarmsAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<AlarmDefinition>(); try { return await _hubConnection.InvokeAsync<List<AlarmDefinition>>("GetAlarms", ResolveId(factoryId)); } catch { return new List<AlarmDefinition>(); } }
        public async Task AddAlarmAsync(AlarmDefinition a, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("AddAlarm", ResolveId(factoryId), a); }
        public async Task UpdateAlarmAsync(AlarmDefinition a, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("UpdateAlarm", ResolveId(factoryId), a); }
        public async Task DeleteAlarmAsync(int id, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("DeleteAlarm", ResolveId(factoryId), id); }

        public async Task<List<CostParameter>> GetCostsAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<CostParameter>(); try { return await _hubConnection.InvokeAsync<List<CostParameter>>("GetCosts", ResolveId(factoryId)); } catch { return new List<CostParameter>(); } }
        public async Task UpdateCostsAsync(List<CostParameter> c, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("UpdateParameters", ResolveId(factoryId), c); }

        public async Task<List<PlcOperator>> GetPlcOperatorsAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<PlcOperator>(); try { return await _hubConnection.InvokeAsync<List<PlcOperator>>("GetPlcOperators", ResolveId(factoryId)); } catch { return new List<PlcOperator>(); } }
        public async Task SavePlcOperatorAsync(PlcOperator o, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("SaveOrUpdateOperator", ResolveId(factoryId), o); }
        public async Task AddDefaultPlcOperatorAsync(int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("AddDefaultOperator", ResolveId(factoryId)); }
        public async Task DeletePlcOperatorAsync(int id, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) await _hubConnection.InvokeAsync("DeleteOperator", ResolveId(factoryId), id); }

        public async Task<List<ProductionReportItem>> GetProductionReportAsync(ReportFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<ProductionReportItem>(); try { return await _hubConnection.InvokeAsync<List<ProductionReportItem>>("GetProductionReport", ResolveId(factoryId), f); } catch { return new List<ProductionReportItem>(); } }
        public async Task<List<AlarmReportItem>> GetAlarmReportAsync(ReportFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<AlarmReportItem>(); try { return await _hubConnection.InvokeAsync<List<AlarmReportItem>>("GetAlarmReport", ResolveId(factoryId), f); } catch { return new List<AlarmReportItem>(); } }
        public async Task<List<object>?> GetTrendDataAsync(ReportFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<List<object>>("GetTrendData", ResolveId(factoryId), f); } catch { return null; } }
        public async Task<ManualConsumptionSummary?> GetManualConsumptionReportAsync(ReportFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<ManualConsumptionSummary>("GetManualConsumptionReport", ResolveId(factoryId), f); } catch { return null; } }
        public async Task<ConsumptionTotals?> GetConsumptionTotalsAsync(ReportFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<ConsumptionTotals>("GetConsumptionTotals", ResolveId(factoryId), f); } catch { return null; } }
        public async Task<List<ProductionReportItem>?> GetGeneralDetailedConsumptionReportAsync(GeneralDetailedConsumptionFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<List<ProductionReportItem>>("GetGeneralDetailedConsumptionReport", ResolveId(factoryId), f); } catch { return null; } }
        public async Task<List<TekstilScada.Core.Models.ActionLogEntry>> GetActionLogsAsync(ActionLogFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<TekstilScada.Core.Models.ActionLogEntry>(); try { return await _hubConnection.InvokeAsync<List<TekstilScada.Core.Models.ActionLogEntry>>("GetActionLogs", ResolveId(factoryId), f); } catch { return new List<TekstilScada.Core.Models.ActionLogEntry>(); } }
        public async Task<ProductionDetailDto?> GetProductionDetailAsync(int mId, string bId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<ProductionDetailDto>("GetProductionDetail", ResolveId(factoryId), mId, bId); } catch { return null; } }

        public async Task<List<OeeData>> GetOeeReportAsync(ReportFilters f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<OeeData>(); try { return await _hubConnection.InvokeAsync<List<OeeData>>("GetOeeReport", ResolveId(factoryId), f); } catch { return new List<OeeData>(); } }
        public async Task<List<HourlyConsumptionData>?> GetHourlyConsumptionAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<List<HourlyConsumptionData>>("GetHourlyFactoryConsumption", ResolveId(factoryId)); } catch { return null; } }
        public async Task<List<HourlyOeeData>?> GetHourlyOeeAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<List<HourlyOeeData>>("GetHourlyAverageOee", ResolveId(factoryId)); } catch { return null; } }
        public async Task<List<TopAlarmData>?> GetTopAlarmsAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<List<TopAlarmData>>("GetTopAlarms", ResolveId(factoryId)); } catch { return null; } }

        public async Task<byte[]> ExportProductionReportAsync(List<ProductionReportItem> i, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportProductionReport", ResolveId(factoryId), i); } catch { return Array.Empty<byte>(); } }
        public async Task<byte[]> ExportAlarmReportAsync(List<AlarmReportItem> i, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportAlarmReport", ResolveId(factoryId), i); } catch { return Array.Empty<byte>(); } }
        public async Task<byte[]> ExportOeeReportAsync(List<OeeData> i, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportOeeReport", ResolveId(factoryId), i); } catch { return Array.Empty<byte>(); } }
        public async Task<byte[]> ExportManualConsumptionReportAsync(ManualConsumptionSummary s, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportManualConsumptionReport", ResolveId(factoryId), s); } catch { return Array.Empty<byte>(); } }
        public async Task<byte[]> ExportGeneralDetailedConsumptionReportAsync(GeneralConsumptionExportDto d, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportGeneralDetailedConsumptionReport", ResolveId(factoryId), d); } catch { return Array.Empty<byte>(); } }
        public async Task<byte[]> ExportActionLogsReportAsync(List<TekstilScada.Core.Models.ActionLogEntry> l, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportActionLogsReport", ResolveId(factoryId), l); } catch { return Array.Empty<byte>(); } }
        public async Task<byte[]> ExportProductionDetailFileAsync(int mId, string bId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return Array.Empty<byte>(); try { return await _hubConnection.InvokeAsync<byte[]>("ExportProductionDetailFile", ResolveId(factoryId), mId, bId); } catch { return Array.Empty<byte>(); } }

        public async Task<Dictionary<int, string>?> GetHmiRecipeNamesAsync(int mId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<Dictionary<int, string>>("GetHmiRecipeNames", ResolveId(factoryId), mId); } catch { return null; } }
        public async Task<ScadaRecipe?> GetHmiRecipePreviewAsync(int mId, string f, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return null; try { return await _hubConnection.InvokeAsync<ScadaRecipe>("GetHmiRecipePreview", ResolveId(factoryId), mId, f); } catch { return null; } }
        public async Task<bool> QueueSequentiallyNamedSendJobsAsync(List<int> r, List<int> m, int s, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return false; try { return await _hubConnection.InvokeAsync<bool>("QueueSequentiallyNamedSendJobs", ResolveId(factoryId), r, m, s); } catch { return false; } }
        public async Task<bool> QueueReceiveJobsAsync(List<string> f, int m, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return false; try { return await _hubConnection.InvokeAsync<bool>("QueueReceiveJobs", ResolveId(factoryId), f, m); } catch { return false; } }
        public async Task<List<TransferJob>> GetActiveFtpJobsAsync(int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return new List<TransferJob>(); try { return await _hubConnection.InvokeAsync<List<TransferJob>>("GetActiveFtpJobs", ResolveId(factoryId)); } catch { return new List<TransferJob>(); } }
        public async Task<string> GetGatewayIpAsync(int mId, int factoryId = 0) { if (_hubConnection?.State != HubConnectionState.Connected) return "localhost:5901"; try { return await _hubConnection.InvokeAsync<string>("GetGatewayIpForMachine", ResolveId(factoryId), mId); } catch { return "localhost:5901"; } }
        public async Task LogUserActionAsync(int uId, string t, string d, int factoryId = 0) { if (_hubConnection?.State == HubConnectionState.Connected) try { await _hubConnection.InvokeAsync("HandleLogAction", ResolveId(factoryId), new TekstilScada.Core.Models.ActionLogEntry { UserId = uId, ActionType = t, Details = d, Timestamp = DateTime.Now }); } catch { } }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null) { try { await _hubConnection.StopAsync(); await _hubConnection.DisposeAsync(); } catch { } _hubConnection = null; }
        }
        public void ExitFactory()
        {
            CurrentFactoryName = ""; _currentSelectedFactoryId = 0; MachineData.Clear(); MachineDetailsCache.Clear(); OnFactoryChanged?.Invoke();
        }
    }
}