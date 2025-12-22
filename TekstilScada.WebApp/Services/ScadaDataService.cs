// Dosya: TekstilScada.WebApp/Services/ScadaDataService.cs (GÜNCELLENMİŞ VERSİYON)



using Microsoft.AspNetCore.SignalR.Client;

using System;

using System.Collections.Concurrent;

using System.Net.Http.Json;

using System.Text;

using System.Text.Json;

using System.Threading.Tasks;

using TekstilScada.Models;

using TekstilScada.Repositories;

using TekstilScada.Services;

using Microsoft.Extensions.DependencyInjection; // <--- AddJsonProtocol için BU ŞART
using System.Text.Json.Serialization;

// --- DTO Sınıfları (Global) ---



public class TrendDataPoint

{

    public DateTime Timestamp { get; set; }

    public double Temperature { get; set; }

    public double Rpm { get; set; }

    public double WaterLevel { get; set; }

}



public class ProductionStepDetailDto : TekstilScada.Models.ProductionStepDetail

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

    public TekstilScada.Models.ProductionReportItem Header { get; set; } = new();

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



public class ActionLogFilters

{

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Username { get; set; }

    public string? Details { get; set; }

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



public class ReportFilters1

{

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? MachineId { get; set; }

}



public class JsReadyTrendDataPoint

{

    public DateTime Timestamp { get; set; }

    public double TimestampOADate { get; set; }

    public double Temperature { get; set; }

    public double Rpm { get; set; }

    public double WaterLevel { get; set; }

}



public class StepTypeDto

{

    public int Id { get; set; }

    public string Name { get; set; }

}



public class SaveLayoutRequest

{

    public string LayoutName { get; set; }

    public string MachineSubType { get; set; }

    public int StepTypeId { get; set; }

    public string LayoutJson { get; set; }

}



public class GeneralConsumptionExportDto

{

    public List<ProductionReportItem>? Items { get; set; }

    public string? ConsumptionType { get; set; }

}



namespace TekstilScada.WebApp.Services

{

    public class ScadaDataService : IAsyncDisposable

    {

        private HubConnection? _hubConnection;

        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _serializerOptions;



        // Canlı Veriler (Anlık Durumlar)

        public ConcurrentDictionary<int, FullMachineStatus> MachineData { get; private set; } = new();



        // Makine Sabit Bilgileri (Grup ismi, Tipi vb.)

        public ConcurrentDictionary<int, Machine> MachineDetailsCache { get; private set; } = new();



        // Token'ı hafızada tutuyoruz

        private string _accessToken = string.Empty;



        public event Action? OnDataUpdated;

        public event Action<TransferJob> OnFtpProgressReceived;



        public ScadaDataService(HttpClient httpClient)

        {

            _httpClient = httpClient;

            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        }



        // --- 1. BAŞLATMA VE BAĞLANTI (KRİTİK BÖLÜM) ---



        public async Task InitializeAsync()

        {

            // A. Önce Token Al (Login Ol)

            if (string.IsNullOrEmpty(_accessToken))

            {

                await LoginAndGetTokenAsync();

            }



            // B. SignalR Bağlantısını Kur

            if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
            {
                var hubUrl = new Uri(_httpClient.BaseAddress!, "/scadaHub");

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_accessToken);
                    })
                    // --- EKLENECEK KISIM BAŞLANGIÇ ---
                    .AddJsonProtocol(options =>
                    {
                        options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                        options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                    })
                    // --- EKLENECEK KISIM BİTİŞ ---
                    .WithAutomaticReconnect()
                    .Build();



                // C. Olayları Tanımla



                // FTP İlerlemesi

                _hubConnection.On<TransferJob>("ReceiveFtpProgress", (job) =>

                {

                    if (job != null) OnFtpProgressReceived?.Invoke(job);

                });



                // Canlı Veri Akışı ve DİNAMİK KEŞİF

                _hubConnection.On<FullMachineStatus>("ReceiveMachineUpdate", (status) =>

                {

                    if (status != null)

                    {

                        // 1. Canlı veriyi güncelle

                        MachineData[status.MachineId] = status;



                        // 2. Makine listesinde yoksa EKLE (Dynamic Discovery)

                        // Windows Forms'tan veri geldiği anda makineyi "var" sayıyoruz.

                        if (!MachineDetailsCache.ContainsKey(status.MachineId))

                        {

                            var discoveredMachine = new Machine

                            {

                                Id = status.MachineId,

                                MachineName = status.MachineName,

                                // WinForms'tan gelen 'MakineTipi'ni kullan, yoksa varsayılan ata

                                MachineSubType = string.IsNullOrEmpty(status.MakineTipi) ? "Standart" : status.MakineTipi

                            };

                            MachineDetailsCache.TryAdd(status.MachineId, discoveredMachine);

                            Console.WriteLine($"[ScadaService] Yeni Makine Keşfedildi: {status.MachineName}");

                        }



                        OnDataUpdated?.Invoke();

                    }

                });



                try

                {

                    await _hubConnection.StartAsync();

                    Console.WriteLine("[ScadaService] SignalR Bağlantısı Başarılı.");

                }

                catch (Exception ex)

                {

                    _hubConnection = null;

                    Console.WriteLine($"[ScadaService] SignalR bağlantı hatası: {ex.Message}");

                }

            }

        }



        // --- 2. YENİ LOGIN METODU ---

        private async Task LoginAndGetTokenAsync()

        {

            try

            {

                // API'deki AuthController'a sabit admin bilgileriyle istek atıyoruz (Köprü Modu)

                var loginData = new { Username = "admin", Password = "1234" };

                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginData);



                if (response.IsSuccessStatusCode)

                {

                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    _accessToken = result?.Token ?? string.Empty;

                }

                else

                {

                    Console.WriteLine("[ScadaService] API Login Başarısız! Token alınamadı.");

                }

            }

            catch (Exception ex)

            {

                Console.WriteLine($"[ScadaService] Login Hatası: {ex.Message}");

            }

        }



        // Token Yanıt Modeli

        private class LoginResponse { public string Token { get; set; } }



        public async ValueTask DisposeAsync()

        {

            var hub = _hubConnection;

            _hubConnection = null;



            if (hub is not null)

            {

                try

                {

                    await hub.StopAsync();

                    await hub.DisposeAsync();

                }

                catch { }

            }

        }



        // --- 3. REVİZE EDİLEN METOTLAR ---



        // Artık veritabanından çekmiyoruz, Cache'deki listeyi dönüyoruz.
        // --- KULLANICI YÖNETİMİ (SignalR) ---

        public async Task<List<User>?> GetUsersAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<User>();

            try
            {
                return await _hubConnection.InvokeAsync<List<User>>("GetUsers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcılar alınamadı: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<Role>();

            try
            {
                return await _hubConnection.InvokeAsync<List<Role>>("GetRoles");
            }
            catch { return new List<Role>(); }
        }

        // UserViewModel alan Add metodu (WebApp genellikle bunu kullanır)
        public async Task AddUserAsync(UserViewModel userVm)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                await _hubConnection.InvokeAsync<bool>("AddUser", userVm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı eklenemedi: {ex.Message}");
            }
        }

        // UserViewModel alan Update metodu
        public async Task UpdateUserAsync(UserViewModel userVm)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                await _hubConnection.InvokeAsync<bool>("UpdateUser", userVm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı güncellenemedi: {ex.Message}");
            }
        }

        // Eski User alan metodları (Eğer kullanılmıyorsa silebilirsiniz, uyumluluk için tutuyorsanız yönlendirin)
        public async Task AddUserAsync(User user)
        {
            // Bu metod UserViewModel bekleyen Hub metoduna uygun değil. 
            // Eğer kullanıyorsanız, User -> UserViewModel dönüşümü yapıp göndermelisiniz veya Hub'a overload eklemelisiniz.
            // Şimdilik boş bırakıyorum veya log basabilirsiniz.
            Console.WriteLine("Uyarı: User nesnesi ile ekleme desteklenmiyor, UserViewModel kullanın.");
        }

        public async Task UpdateUserAsync(User user)
        {
            Console.WriteLine("Uyarı: User nesnesi ile güncelleme desteklenmiyor, UserViewModel kullanın.");
        }

        public async Task DeleteUserAsync(int id)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                await _hubConnection.InvokeAsync<bool>("DeleteUser", id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı silinemedi: {ex.Message}");
            }
        }
        public async Task<List<Machine>?> GetMachinesAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
            {
                // Bağlantı yoksa hafızadaki cache'i dön (Offline destek)
                return MachineDetailsCache.Values.ToList();
            }

            try
            {
                // Hub üzerinden veritabanındaki tüm makineleri çek
                var machines = await _hubConnection.InvokeAsync<List<Machine>>("GetAllMachines");

                // Cache'i güncelle (Opsiyonel ama iyi pratik)
                foreach (var m in machines)
                {
                    if (!MachineDetailsCache.ContainsKey(m.Id))
                    {
                        MachineDetailsCache.TryAdd(m.Id, m);
                    }
                    // Veya MachineDetailsCache tamamen yenilenebilir
                }

                return machines;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Makine listesi alınamadı: {ex.Message}");
                return MachineDetailsCache.Values.ToList();
            }
        }

        public async Task<Machine?> AddMachineAsync(Machine machine)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;

            try
            {
                return await _hubConnection.InvokeAsync<Machine>("AddMachine", machine);
            }
            catch { return null; }
        }

        public async Task<bool> UpdateMachineAsync(Machine machine)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return false;

            try
            {
                return await _hubConnection.InvokeAsync<bool>("UpdateMachine", machine);
            }
            catch { return false; }
        }

        public async Task<bool> DeleteMachineAsync(int machineId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return false;

            try
            {
                return await _hubConnection.InvokeAsync<bool>("DeleteMachine", machineId);
            }
            catch { return false; }
        }

        // Özel bir durum için tekil statüs çekmek gerekirse
        public async Task<FullMachineStatus?> GetMachineStatusAsync(int id)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;

            try
            {
                return await _hubConnection.InvokeAsync<FullMachineStatus>("GetMachineStatus", id);
            }
            catch { return null; }
        }



        // ... Diğer Raporlama Metotları (Aynen Kalabilir, hata yakalama blokları var) ...



       


        public async Task<List<ScadaRecipe>?> GetRecipesAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<ScadaRecipe>();

            try
            {
                return await _hubConnection.InvokeAsync<List<ScadaRecipe>>("GetRecipes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reçete listesi alınamadı: {ex.Message}");
                return new List<ScadaRecipe>(); // null yerine boş liste dönmek daha güvenli olabilir
            }
        }

        public async Task<ScadaRecipe?> GetRecipeDetailsAsync(int recipeId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;

            try
            {
                return await _hubConnection.InvokeAsync<ScadaRecipe>("GetRecipeDetails", recipeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reçete detayı alınamadı: {ex.Message}");
                return null;
            }
        }

        public async Task<ScadaRecipe?> SaveRecipeAsync(ScadaRecipe recipe)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;

            try
            {
                return await _hubConnection.InvokeAsync<ScadaRecipe>("SaveRecipe", recipe);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reçete kaydedilemedi: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteRecipeAsync(int recipeId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return false;

            try
            {
                return await _hubConnection.InvokeAsync<bool>("DeleteRecipe", recipeId);
            }
            catch { return false; }
        }

        public async Task<bool> SendRecipeToPlcAsync(int recipeId, int machineId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return false;

            try
            {
                return await _hubConnection.InvokeAsync<bool>("SendRecipeToPlc", recipeId, machineId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC'ye gönderme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<ScadaRecipe?> ReadRecipeFromPlcAsync(int machineId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;

            try
            {
                return await _hubConnection.InvokeAsync<ScadaRecipe>("ReadRecipeFromPlc", machineId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC'den okuma hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ProductionReportItem>?> GetRecipeConsumptionHistoryAsync(int recipeId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<ProductionReportItem>();

            try
            {
                return await _hubConnection.InvokeAsync<List<ProductionReportItem>>("GetRecipeConsumptionHistory", recipeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reçete geçmişi alınamadı: {ex.Message}");
                return new List<ProductionReportItem>();
            }
        }



        public async Task<List<ProductionReportItem>?> GetProductionReportAsync(ReportFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<ProductionReportItem>();

            try { return await _hubConnection.InvokeAsync<List<ProductionReportItem>>("GetProductionReport", filters); }
            catch { return new List<ProductionReportItem>(); }
        }

        // GetAlarmReportAsync (Zaten yapılmıştı, kontrol edin)

        public async Task<List<object>?> GetTrendDataAsync(ReportFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;
            try
            {
                // Tip uyuşmazlığı olmaması için object olarak alıyoruz veya dynamic
                var result = await _hubConnection.InvokeAsync<List<object>>("GetTrendData", filters);
                return result;
            }
            catch { return null; }
        }

        public async Task<ManualConsumptionSummary?> GetManualConsumptionReportAsync(ReportFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;
            try { return await _hubConnection.InvokeAsync<ManualConsumptionSummary>("GetManualConsumptionReport", filters); }
            catch { return null; }
        }

        public async Task<ConsumptionTotals?> GetConsumptionTotalsAsync(ReportFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;
            try { return await _hubConnection.InvokeAsync<ConsumptionTotals>("GetConsumptionTotals", filters); }
            catch { return null; }
        }

        public async Task<List<ProductionReportItem>?> GetGeneralDetailedConsumptionReportAsync(GeneralDetailedConsumptionFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;
            try { return await _hubConnection.InvokeAsync<List<ProductionReportItem>>("GetGeneralDetailedConsumptionReport", filters); }
            catch { return null; }
        }

        public async Task<List<TekstilScada.Core.Models.ActionLogEntry>?> GetActionLogsAsync(ActionLogFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return new List<TekstilScada.Core.Models.ActionLogEntry>();
            try { return await _hubConnection.InvokeAsync<List<TekstilScada.Core.Models.ActionLogEntry>>("GetActionLogs", filters); }
            catch { return new List<TekstilScada.Core.Models.ActionLogEntry>(); }
        }

        public async Task<ProductionDetailDto?> GetProductionDetailAsync(int machineId, string batchId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return null;
            try { return await _hubConnection.InvokeAsync<ProductionDetailDto>("GetProductionDetail", machineId, batchId); }
            catch { return null; }
        }

        // --- EXPORT METOTLARI (Byte[] Döner) ---

        public async Task<byte[]> ExportProductionReportAsync(List<ProductionReportItem> reportItems)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            // Hub: ExportProductionReport
            return await _hubConnection.InvokeAsync<byte[]>("ExportProductionReport", reportItems);
        }

        public async Task<byte[]> ExportAlarmReportAsync(List<AlarmReportItem> reportItems)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            return await _hubConnection.InvokeAsync<byte[]>("ExportAlarmReport", reportItems);
        }

        public async Task<byte[]> ExportOeeReportAsync(List<OeeData> reportItems)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            return await _hubConnection.InvokeAsync<byte[]>("ExportOeeReport", reportItems);
        }

        public async Task<byte[]> ExportManualConsumptionReportAsync(ManualConsumptionSummary summary)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            return await _hubConnection.InvokeAsync<byte[]>("ExportManualConsumptionReport", summary);
        }

        public async Task<byte[]> ExportGeneralDetailedConsumptionReportAsync(GeneralConsumptionExportDto exportData)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            return await _hubConnection.InvokeAsync<byte[]>("ExportGeneralDetailedConsumptionReport", exportData);
        }

        public async Task<byte[]> ExportActionLogsReportAsync(List<TekstilScada.Core.Models.ActionLogEntry> logs)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            return await _hubConnection.InvokeAsync<byte[]>("ExportActionLogsReport", logs);
        }

        public async Task<byte[]> ExportProductionDetailFileAsync(int machineId, string batchId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return Array.Empty<byte>();
            return await _hubConnection.InvokeAsync<byte[]>("ExportProductionDetailFile", machineId, batchId);
        }



       



        public async Task<List<AlarmReportItem>> GetAlarmReportAsync(ReportFilters filters)
        {
            // Hub bağlantısı kontrolü
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Hub bağlantısı yok, alarm raporu çekilemedi.");
                return new List<AlarmReportItem>();
            }

            try
            {
                // HTTP Post yerine SignalR Invoke
                // "GetAlarmReport" -> Hub üzerindeki metodun adı
                return await _hubConnection.InvokeAsync<List<AlarmReportItem>>("GetAlarmReport", filters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Alarm Raporu Hatası: {ex.Message}");
                return new List<AlarmReportItem>();
            }
        }



        public async Task<List<OeeData>?> GetOeeReportAsync(ReportFilters filters)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<OeeData>();

            try
            {
                // "GetOeeReport" Hub metodu çağrılıyor
                return await _hubConnection.InvokeAsync<List<OeeData>>("GetOeeReport", filters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OEE verisi alınamadı: {ex.Message}");
                return new List<OeeData>();
            }
        }



        



        



        



        



        



        public async Task<List<HourlyConsumptionData>?> GetHourlyConsumptionAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<HourlyConsumptionData>();

            try
            {
                // "GetHourlyConsumption" Hub metodu çağrılıyor
                return await _hubConnection.InvokeAsync<List<HourlyConsumptionData>>("GetHourlyConsumption");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Saatlik tüketim verileri alınamadı: {ex.Message}");
                return null;
            }
        }



        public async Task<List<HourlyOeeData>?> GetHourlyOeeAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<HourlyOeeData>();

            try
            {
                // "GetHourlyOee" Hub metodu çağrılıyor
                return await _hubConnection.InvokeAsync<List<HourlyOeeData>>("GetHourlyOee");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Saatlik OEE verileri alınamadı: {ex.Message}");
                return null;
            }
        }



        public async Task<List<TopAlarmData>?> GetTopAlarmsAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<TopAlarmData>();

            try
            {
                // "GetTopAlarms" Hub metodu çağrılıyor
                return await _hubConnection.InvokeAsync<List<TopAlarmData>>("GetTopAlarms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Popüler alarmlar alınamadı: {ex.Message}");
                return null;
            }
        }



        public async Task<Dictionary<int, string>?> GetHmiRecipeNamesAsync(int machineId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return null;

            try
            {
                return await _hubConnection.InvokeAsync<Dictionary<int, string>>("GetHmiRecipeNames", machineId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HMI İsimleri Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<ScadaRecipe?> GetHmiRecipePreviewAsync(int machineId, string remoteFileName)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return null;

            try
            {
                return await _hubConnection.InvokeAsync<ScadaRecipe>("GetHmiRecipePreview", machineId, remoteFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Önizleme Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> QueueSequentiallyNamedSendJobsAsync(List<int> recipeIds, List<int> machineIds, int startNumber)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return false;

            try
            {
                // Hub metoduna parametreleri sırasıyla gönderiyoruz
                return await _hubConnection.InvokeAsync<bool>("QueueSequentiallyNamedSendJobs", recipeIds, machineIds, startNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gönderim Kuyruğu Hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> QueueReceiveJobsAsync(List<string> fileNames, int machineId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return false;

            try
            {
                return await _hubConnection.InvokeAsync<bool>("QueueReceiveJobs", fileNames, machineId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alma Kuyruğu Hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TransferJob>> GetActiveFtpJobsAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<TransferJob>();

            try
            {
                return await _hubConnection.InvokeAsync<List<TransferJob>>("GetActiveJobs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aktif İşler Hatası: {ex.Message}");
                return new List<TransferJob>();
            }
        }



        



       



        public async Task<string> GetLayoutJsonAsync(string machineSubType, int stepId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return string.Empty;

            try
            {
                // Hub: GetStepLayout
                var result = await _hubConnection.InvokeAsync<string>("GetStepLayout", machineSubType, stepId);
                return result ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Layout yüklenemedi: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<List<StepTypeDto>> GetStepTypesAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<StepTypeDto>();

            try
            {
                // Hub: GetStepTypes
                return await _hubConnection.InvokeAsync<List<StepTypeDto>>("GetStepTypes");
            }
            catch
            {
                return new List<StepTypeDto>();
            }
        }

        public async Task<List<string>> GetMachineSubTypesAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<string> { "DEFAULT" };

            try
            {
                // Hub: GetMachineSubTypes
                return await _hubConnection.InvokeAsync<List<string>>("GetMachineSubTypes");
            }
            catch
            {
                return new List<string> { "DEFAULT" };
            }
        }

        public async Task<bool> SaveLayoutAsync(SaveLayoutRequest request)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return false;

            try
            {
                // Hub: SaveLayout
                return await _hubConnection.InvokeAsync<bool>("SaveLayout", request);
            }
            catch
            {
                return false;
            }
        }



        



        



        



        



        



        



        



        public async Task<List<AlarmDefinition>> GetAlarmsAsync()
        {
            // Bağlantı kontrolü
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<AlarmDefinition>();

            try
            {
                // Hub üzerindeki "GetAlarms" metodunu çağırıp sonucu alıyoruz
                return await _hubConnection.InvokeAsync<List<AlarmDefinition>>("GetAlarms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alarm listesi alınamadı: {ex.Message}");
                return new List<AlarmDefinition>();
            }
        }

        public async Task AddAlarmAsync(AlarmDefinition alarm)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                await _hubConnection.InvokeAsync("AddAlarm", alarm);
            }
            catch (Exception ex) { Console.WriteLine($"Alarm ekleme hatası: {ex.Message}"); }
        }

        public async Task UpdateAlarmAsync(AlarmDefinition alarm)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                await _hubConnection.InvokeAsync("UpdateAlarm", alarm);
            }
            catch (Exception ex) { Console.WriteLine($"Alarm güncelleme hatası: {ex.Message}"); }
        }

        public async Task DeleteAlarmAsync(int id)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                await _hubConnection.InvokeAsync("DeleteAlarm", id);
            }
            catch (Exception ex) { Console.WriteLine($"Alarm silme hatası: {ex.Message}"); }
        }



























        public async Task<List<CostParameter>> GetCostsAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<CostParameter>();

            try
            {
                // Hub: GetCosts
                return await _hubConnection.InvokeAsync<List<CostParameter>>("GetCosts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Maliyet parametreleri alınamadı: {ex.Message}");
                return new List<CostParameter>();
            }
        }

        public async Task UpdateCostsAsync(List<CostParameter> costs)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                // Hub: UpdateCosts
                await _hubConnection.InvokeAsync("UpdateCosts", costs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Maliyetler güncellenemedi: {ex.Message}");
            }
        }



        public async Task<List<PlcOperator>> GetPlcOperatorsAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<PlcOperator>();

            try
            {
                // Hub: GetPlcOperators
                return await _hubConnection.InvokeAsync<List<PlcOperator>>("GetPlcOperators");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operatör listesi alınamadı: {ex.Message}");
                return new List<PlcOperator>();
            }
        }

        public async Task SavePlcOperatorAsync(PlcOperator op)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                // Hub: SavePlcOperator
                await _hubConnection.InvokeAsync("SavePlcOperator", op);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operatör kaydedilemedi: {ex.Message}");
            }
        }

        public async Task AddDefaultPlcOperatorAsync()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                // Hub: AddDefaultPlcOperator
                await _hubConnection.InvokeAsync("AddDefaultPlcOperator");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Varsayılan operatör eklenemedi: {ex.Message}");
            }
        }

        public async Task DeletePlcOperatorAsync(int id)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                // Hub: DeletePlcOperator
                await _hubConnection.InvokeAsync("DeletePlcOperator", id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operatör silinemedi: {ex.Message}");
            }
        }



        public async Task<List<string>> GetMachineSubTypesAsyncDesign()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<string>();

            try
            {
                // Hub: GetMachineSubTypesDesign
                return await _hubConnection.InvokeAsync<List<string>>("GetMachineSubTypesDesign");
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<StepTypeDtoDesign>> GetStepTypesAsyncDesign()
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<StepTypeDtoDesign>();

            try
            {
                // Hub: GetStepTypesDesign
                return await _hubConnection.InvokeAsync<List<StepTypeDtoDesign>>("GetStepTypesDesign");
            }
            catch
            {
                return new List<StepTypeDtoDesign>();
            }
        }

        public async Task<List<ControlMetadata>> GetLayoutAsync(string subType, int stepTypeId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
                return new List<ControlMetadata>();

            try
            {
                // Hub: GetLayoutDesign (Nesne listesi döner)
                return await _hubConnection.InvokeAsync<List<ControlMetadata>>("GetLayoutDesign", subType, stepTypeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tasarım yüklenirken hata: {ex.Message}");
                return new List<ControlMetadata>();
            }
        }

        // Not: Bu metod imzasını (string subType, int stepTypeId, List<ControlMetadata> layout) parametre olarak alacak şekilde değiştirdik.
        // ScadaHub tarafında tek bir DTO yerine ayrı parametreler tanımladığımız için InvokeAsync'e sırayla geçiyoruz.
        public async Task SaveLayoutAsync(string subType, int stepTypeId, List<ControlMetadata> layout)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected) return;

            try
            {
                // Hub: SaveLayoutDesign
                await _hubConnection.InvokeAsync("SaveLayoutDesign", subType, stepTypeId, layout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tasarım kaydedilemedi: {ex.Message}");
            }
        }



        public async Task LogUserActionAsync(int userId, string actionType, string details)
        {
            // Hub bağlantısı var mı kontrol et
            if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    var entry = new TekstilScada.Core.Models.ActionLogEntry
                    {
                        UserId = userId,
                        ActionType = actionType,
                        Details = details,
                        Timestamp = DateTime.Now,
                        Username = "" // Gerekirse doldurulabilir
                    };

                    // Controller yerine Hub üzerindeki metoda çağrı yapıyoruz ("LogAction")
                    await _hubConnection.InvokeAsync("LogAction", entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Log gönderme hatası: {ex.Message}");
                }
            }
            else
            {
                // Bağlantı yoksa yapılacaklar (örn: offline kuyruğuna atma veya konsola yazma)
                Console.WriteLine("Hub bağlantısı yok, log gönderilemedi.");
            }
        }



        



     

        

    }

}