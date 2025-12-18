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

                        // Token'ı Header'a ekle

                        options.AccessTokenProvider = () => Task.FromResult(_accessToken);

                    })

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

        public Task<List<Machine>?> GetMachinesAsync()

        {

            // Cache'deki makineleri listeye çevirip dön

            var list = MachineDetailsCache.Values.ToList();

            return Task.FromResult<List<Machine>?>(list);

        }



        // --- 4. DİĞER API METOTLARI (RAPORLAR VB.) ---

        // NOT: API'de Controller'lar silindiği için bu metotlar şu an 404 dönecektir.

        // İleride "Proxy" mantığı kurulana kadar bu şekilde kalabilirler.



        public async Task<Machine?> AddMachineAsync(Machine machine)

        {

            // Veritabanı WinForms'ta olduğu için API üzerinden ekleme şu an çalışmaz.

            // Bu metotları şimdilik pasif bırakıyoruz veya hata yönetimi ekliyoruz.

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/machines", machine);

                return await response.Content.ReadFromJsonAsync<Machine>();

            }

            catch { return null; }

        }



        public async Task<bool> UpdateMachineAsync(Machine machine)

        {

            try

            {

                var response = await _httpClient.PutAsJsonAsync($"api/machines/{machine.Id}", machine);

                return response.IsSuccessStatusCode;

            }

            catch { return false; }

        }



        public async Task<bool> DeleteMachineAsync(int machineId)

        {

            try

            {

                var response = await _httpClient.DeleteAsync($"api/machines/{machineId}");

                return response.IsSuccessStatusCode;

            }

            catch { return false; }

        }



        // ... Diğer Raporlama Metotları (Aynen Kalabilir, hata yakalama blokları var) ...



        public async Task<List<User>?> GetUsersAsync()

        {

            try { return await _httpClient.GetFromJsonAsync<List<User>>("api/users"); }

            catch (Exception ex) { Console.WriteLine($"Kullanıcılar alınamadı: {ex.Message}"); return null; }

        }



        public async Task<List<ScadaRecipe>?> GetRecipesAsync()

        {

            try { return await _httpClient.GetFromJsonAsync<List<ScadaRecipe>>("api/recipes"); }

            catch (Exception ex) { Console.WriteLine($"Reçete listesi alınamadı: {ex.Message}"); return null; }

        }



        public async Task<ScadaRecipe?> GetRecipeDetailsAsync(int recipeId)

        {

            try { return await _httpClient.GetFromJsonAsync<ScadaRecipe>($"api/recipes/{recipeId}"); }

            catch (Exception ex) { Console.WriteLine($"Reçete detayı alınamadı: {ex.Message}"); return null; }

        }



        public async Task<ScadaRecipe?> SaveRecipeAsync(ScadaRecipe recipe)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/recipes", recipe);

                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ScadaRecipe>() : null;

            }

            catch { return null; }

        }



        public async Task<bool> DeleteRecipeAsync(int recipeId)

        {

            try

            {

                var response = await _httpClient.DeleteAsync($"api/recipes/{recipeId}");

                return response.IsSuccessStatusCode;

            }

            catch { return false; }

        }



        // Web'den PLC'ye Reçete Gönderme (Komut Mantığına Dönüştürülecek)

        public async Task<bool> SendRecipeToPlcAsync(int recipeId, int machineId)

        {

            // İLERİDE: Bu metot API'de "SendCommandToLocal" tetikleyen bir endpoint'e gitmeli.

            // Şimdilik eski endpoint'e istek atıyor (404 dönebilir).

            try

            {

                var response = await _httpClient.PostAsync($"api/recipes/{recipeId}/send-to-plc/{machineId}", null);

                return response.IsSuccessStatusCode;

            }

            catch { return false; }

        }



        public async Task<ScadaRecipe?> ReadRecipeFromPlcAsync(int machineId)

        {

            try { return await _httpClient.GetFromJsonAsync<ScadaRecipe>($"api/recipes/read-from-plc/{machineId}"); }

            catch { return null; }

        }



        public async Task<List<ProductionReportItem>?> GetProductionReportAsync(ReportFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/production", filters);

                if (!response.IsSuccessStatusCode) return new List<ProductionReportItem>();

                return await response.Content.ReadFromJsonAsync<List<ProductionReportItem>>();

            }

            catch { return new List<ProductionReportItem>(); }

        }



        public async Task<List<string>?> GetHmiRecipesAsync(int machineId)

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<string>>($"api/ftp/list/{machineId}");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"HMI reçeteleri alınamadı: {ex.Message}");

                return new List<string> { $"Hata: {ex.Message}" };

            }

        }



        public async Task<List<AlarmReportItem>?> GetAlarmReportAsync(ReportFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/alarms", filters);

                if (!response.IsSuccessStatusCode) return new List<AlarmReportItem>();

                return await response.Content.ReadFromJsonAsync<List<AlarmReportItem>>();

            }

            catch { return new List<AlarmReportItem>(); }

        }



        public async Task<List<OeeData>?> GetOeeReportAsync(ReportFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/dashboard/oee-report", filters);

                if (!response.IsSuccessStatusCode) return new List<OeeData>();

                return await response.Content.ReadFromJsonAsync<List<OeeData>>();

            }

            catch { return new List<OeeData>(); }

        }



        public async Task<List<object>?> GetTrendDataAsync(ReportFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/trend", filters);

                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<List<object>>();

            }

            catch { return null; }

        }



        public async Task<List<ProductionReportItem>?> GetRecipeConsumptionHistoryAsync(int recipeId)

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<ProductionReportItem>>($"api/recipes/{recipeId}/usage-history");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Reçete kullanım geçmişi alınamadı: {ex.Message}");

                return null;

            }

        }



        public async Task<ManualConsumptionSummary?> GetManualConsumptionReportAsync(ReportFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/manual-consumption", filters);

                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ManualConsumptionSummary>();

            }

            catch { return null; }

        }



        public async Task<ConsumptionTotals?> GetConsumptionTotalsAsync(ReportFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/consumption-totals", filters);

                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ConsumptionTotals>();

            }

            catch { return null; }

        }



        public async Task<List<ProductionReportItem>?> GetGeneralDetailedConsumptionReportAsync(GeneralDetailedConsumptionFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/general-detailed", filters);

                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<List<ProductionReportItem>>();

            }

            catch { return null; }

        }



        public async Task<List<HourlyConsumptionData>?> GetHourlyConsumptionAsync()

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<HourlyConsumptionData>>("api/dashboard/hourly-consumption");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Saatlik tüketim verileri alınamadı: {ex.Message}");

                return null;

            }

        }



        public async Task<List<HourlyOeeData>?> GetHourlyOeeAsync()

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<HourlyOeeData>>("api/dashboard/hourly-oee");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Saatlik OEE verileri alınamadı: {ex.Message}");

                return null;

            }

        }



        public async Task<List<TopAlarmData>?> GetTopAlarmsAsync()

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<TopAlarmData>>("api/dashboard/top-alarms");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Popüler alarmlar alınamadı: {ex.Message}");

                return null;

            }

        }



        public async Task<Dictionary<int, string>?> GetHmiRecipeNamesAsync(int machineId)

        {

            try

            {

                var response = await _httpClient.GetFromJsonAsync<Dictionary<int, string>>($"api/ftp/hmi-recipe-names/{machineId}");

                return response;

            }

            catch (HttpRequestException ex)

            {

                Console.WriteLine($"API çağrısı sırasında hata: {ex.Message}");

                return null;

            }

        }



        public async Task<ScadaRecipe?> GetHmiRecipePreviewAsync(int machineId, string remoteFileName)

        {

            try

            {

                var response = await _httpClient.GetFromJsonAsync<ScadaRecipe>($"api/ftp/hmi-recipe-preview/{machineId}?fileName={remoteFileName}");

                return response;

            }

            catch (HttpRequestException ex)

            {

                Console.WriteLine($"API çağrısı sırasında hata: {ex.Message}");

                return null;

            }

        }



        public async Task<bool> QueueSequentiallyNamedSendJobsAsync(List<int> recipeIds, List<int> machineIds, int startNumber)

        {

            try

            {

                var payload = new

                {

                    RecipeIds = recipeIds,

                    MachineIds = machineIds,

                    StartNumber = startNumber

                };

                var response = await _httpClient.PostAsJsonAsync("api/ftp/queue-send-jobs", payload);

                return response.IsSuccessStatusCode;

            }

            catch (HttpRequestException ex)

            {

                Console.WriteLine($"API çağrısı sırasında hata: {ex.Message}");

                return false;

            }

        }



        public async Task<bool> QueueReceiveJobsAsync(List<string> fileNames, int machineId)

        {

            try

            {

                var payload = new

                {

                    FileNames = fileNames,

                    MachineId = machineId

                };

                var response = await _httpClient.PostAsJsonAsync("api/ftp/queue-receive-jobs", payload);

                return response.IsSuccessStatusCode;

            }

            catch (HttpRequestException ex)

            {

                Console.WriteLine($"API çağrısı sırasında hata: {ex.Message}");

                return false;

            }

        }



        public async Task<ProductionDetailDto?> GetProductionDetailAsync(int machineId, string batchId)

        {

            try

            {

                var url = $"api/reports/production-detail/{machineId}/{batchId}";

                return await _httpClient.GetFromJsonAsync<ProductionDetailDto>(url);

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Üretim detayı alınamadı: {ex.Message}");

                return null;

            }

        }



        public async Task<bool> ExportProductionDetailAsync(int machineId, string batchId)

        {

            try

            {

                var response = await _httpClient.GetAsync($"api/reports/export-production-detail/{machineId}/{batchId}");

                return response.IsSuccessStatusCode;

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Excel dışa aktarımı başarısız: {ex.Message}");

                return false;

            }

        }



        public async Task<string> GetLayoutJsonAsync(string machineSubType, int stepId)

        {

            try

            {

                var response = await _httpClient.GetAsync($"api/Json/config/layout/{machineSubType}/{stepId}");

                if (response.IsSuccessStatusCode)

                {

                    return await response.Content.ReadAsStringAsync();

                }

                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)

                {

                    return string.Empty;

                }

                else

                {

                    throw new Exception($"API call failed: {response.ReasonPhrase}");

                }

            }

            catch (Exception ex)

            {

                throw;

            }

        }



        public async Task<List<StepTypeDto>> GetStepTypesAsync()

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<StepTypeDto>>("api/Json/config/steptypes");

            }

            catch (Exception ex)

            {

                return new List<StepTypeDto>();

            }

        }



        public async Task<List<string>> GetMachineSubTypesAsync()

        {

            try

            {

                return await _httpClient.GetFromJsonAsync<List<string>>("api/Json/config/machinesubtypes");

            }

            catch (Exception ex)

            {

                return new List<string> { "DEFAULT" };

            }

        }



        public async Task<bool> SaveLayoutAsync(SaveLayoutRequest request)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/Json/config/savelayout", request);

                response.EnsureSuccessStatusCode();

                return true;

            }

            catch (Exception ex)

            {

                return false;

            }

        }



        public async Task<byte[]> ExportProductionReportAsync(List<ProductionReportItem> reportItems)

        {

            var json = JsonSerializer.Serialize(reportItems, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/reports/export/production", content);



            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Rapor dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<byte[]> ExportAlarmReportAsync(List<AlarmReportItem> reportItems)

        {

            var json = JsonSerializer.Serialize(reportItems, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/reports/export/alarms", content);



            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Alarm raporu dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<byte[]> ExportOeeReportAsync(List<OeeData> reportItems)

        {

            var json = JsonSerializer.Serialize(reportItems, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/reports/export/oee", content);



            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"OEE raporu dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<byte[]> ExportManualConsumptionReportAsync(ManualConsumptionSummary summary)

        {

            var json = JsonSerializer.Serialize(summary, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/reports/export/manual-consumption", content);



            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Manuel Tüketim raporu dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<byte[]> ExportGeneralDetailedConsumptionReportAsync(GeneralConsumptionExportDto exportData)

        {

            var json = JsonSerializer.Serialize(exportData, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/reports/export/general-detailed", content);



            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Genel Tüketim raporu dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<byte[]> ExportActionLogsReportAsync(List<TekstilScada.Core.Models.ActionLogEntry> logs)

        {

            var json = JsonSerializer.Serialize(logs, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/reports/export/action-logs", content);



            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Eylem Kayıtları raporu dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<byte[]> ExportProductionDetailFileAsync(int machineId, string batchId)

        {

            var response = await _httpClient.GetAsync($"api/reports/export/production-detail/{machineId}/{batchId}");

            if (!response.IsSuccessStatusCode)

            {

                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Üretim detayı dışa aktarma başarısız oldu: {response.ReasonPhrase}. Detay: {error}");

            }

            return await response.Content.ReadAsByteArrayAsync();

        }



        public async Task<List<AlarmDefinition>> GetAlarmsAsync()

        {

            try { return await _httpClient.GetFromJsonAsync<List<AlarmDefinition>>("api/alarms"); }

            catch { return new List<AlarmDefinition>(); }

        }



        public async Task AddAlarmAsync(AlarmDefinition alarm)

        {

            try { await _httpClient.PostAsJsonAsync("api/alarms", alarm); } catch { }

        }



        public async Task UpdateAlarmAsync(AlarmDefinition alarm)

        {

            try { await _httpClient.PutAsJsonAsync($"api/alarms/{alarm.Id}", alarm); } catch { }

        }



        public async Task DeleteAlarmAsync(int id)

        {

            try { await _httpClient.DeleteAsync($"api/alarms/{id}"); } catch { }

        }



        public async Task AddUserAsync(User user)

        {

            try { await _httpClient.PostAsJsonAsync("api/users", user); } catch { }

        }



        public async Task UpdateUserAsync(User user)

        {

            try { await _httpClient.PutAsJsonAsync($"api/users/{user.Id}", user); } catch { }

        }



        public async Task DeleteUserAsync(int id)

        {

            try { await _httpClient.DeleteAsync($"api/users/{id}"); } catch { }

        }



        public async Task<List<Role>> GetRolesAsync()

        {

            try { return await _httpClient.GetFromJsonAsync<List<Role>>("api/users/roles"); }

            catch { return new List<Role>(); }

        }



        public async Task AddUserAsync(UserViewModel userVm)

        {

            try { await _httpClient.PostAsJsonAsync("api/users", userVm); } catch { }

        }



        public async Task UpdateUserAsync(UserViewModel userVm)

        {

            try { await _httpClient.PutAsJsonAsync($"api/users/{userVm.Id}", userVm); } catch { }

        }



        public async Task<List<CostParameter>> GetCostsAsync()

        {

            try { return await _httpClient.GetFromJsonAsync<List<CostParameter>>("api/costs"); }

            catch { return new List<CostParameter>(); }

        }



        public async Task UpdateCostsAsync(List<CostParameter> costs)

        {

            try { await _httpClient.PostAsJsonAsync("api/costs", costs); } catch { }

        }



        public async Task<List<PlcOperator>> GetPlcOperatorsAsync()

        {

            try { return await _httpClient.GetFromJsonAsync<List<PlcOperator>>("api/plcoperators"); }

            catch { return new List<PlcOperator>(); }

        }



        public async Task SavePlcOperatorAsync(PlcOperator op)

        {

            try { await _httpClient.PostAsJsonAsync("api/plcoperators", op); } catch { }

        }



        public async Task AddDefaultPlcOperatorAsync()

        {

            try { await _httpClient.PostAsync("api/plcoperators/default", null); } catch { }

        }



        public async Task DeletePlcOperatorAsync(int id)

        {

            try { await _httpClient.DeleteAsync($"api/plcoperators/{id}"); } catch { }

        }



        public async Task<List<string>> GetMachineSubTypesAsyncDesign()

        {

            try { return await _httpClient.GetFromJsonAsync<List<string>>("api/recipeconfigurations/subtypes"); }

            catch { return new List<string>(); }

        }



        public async Task<List<StepTypeDtoDesign>> GetStepTypesAsyncDesign()

        {

            try { return await _httpClient.GetFromJsonAsync<List<StepTypeDtoDesign>>("api/recipeconfigurations/steptypes"); }

            catch { return new List<StepTypeDtoDesign>(); }

        }



        public async Task<List<ControlMetadata>> GetLayoutAsync(string subType, int stepTypeId)

        {

            try

            {

                var jsonString = await _httpClient.GetStringAsync($"api/recipeconfigurations/layout?subType={subType}&stepTypeId={stepTypeId}");

                if (string.IsNullOrEmpty(jsonString)) return new List<ControlMetadata>();



                var options = new System.Text.Json.JsonSerializerOptions

                {

                    PropertyNameCaseInsensitive = true

                };

                return System.Text.Json.JsonSerializer.Deserialize<List<ControlMetadata>>(jsonString, options);

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Tasarım yüklenirken hata: {ex.Message}");

                return new List<ControlMetadata>();

            }

        }



        public async Task SaveLayoutAsync(string subType, int stepTypeId, List<ControlMetadata> layout)

        {

            try { await _httpClient.PostAsJsonAsync($"api/recipeconfigurations/layout?subType={subType}&stepTypeId={stepTypeId}", layout); } catch { }

        }



        public async Task LogUserActionAsync(int userId, string actionType, string details)

        {

            try

            {

                var entry = new TekstilScada.Core.Models.ActionLogEntry

                {

                    UserId = userId,

                    ActionType = actionType,

                    Details = details,

                    Timestamp = DateTime.Now,

                    Username = ""

                };

                await _httpClient.PostAsJsonAsync("api/actionlogs", entry);

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Log gönderilemedi: {ex.Message}");

            }

        }



        public async Task<List<TekstilScada.Core.Models.ActionLogEntry>?> GetActionLogsAsync(ActionLogFilters filters)

        {

            try

            {

                var response = await _httpClient.PostAsJsonAsync("api/reports/action-logs", filters);

                if (!response.IsSuccessStatusCode) return new List<TekstilScada.Core.Models.ActionLogEntry>();

                return await response.Content.ReadFromJsonAsync<List<TekstilScada.Core.Models.ActionLogEntry>>();

            }

            catch { return new List<TekstilScada.Core.Models.ActionLogEntry>(); }

        }



        public async Task<List<TransferJob>> GetActiveFtpJobsAsync()

        {

            try

            {

                var jobs = await _httpClient.GetFromJsonAsync<List<TransferJob>>("api/ftp/active-jobs");

                return jobs ?? new List<TransferJob>();

            }

            catch (Exception ex)

            {

                Console.WriteLine($"FTP İşleri alınırken hata: {ex.Message}");

                return new List<TransferJob>();

            }

        }

        

    }

}