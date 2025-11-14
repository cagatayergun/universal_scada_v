// Dosya: Universalscada.WebApp/Services/ScadaDataService.cs - GÜNCEL VERSİYON
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
// Universalscada.Models'tan temel modelleri (Machine, ScadaRecipe vb.) kullanır
using Universalscada.Models;

namespace Universalscada.WebApp.Services
{
    public class ScadaDataService
    {
        private readonly HttpClient _httpClient;

        public ScadaDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // =========================================================================
        // 1. MAKİNE YÖNETİMİ (Jenerik Machine Modeli)
        // =========================================================================

        /// <summary>
        /// Tüm makinelerin jenerik listesini çeker. (Machine.Sector ve Machine.ConfigurationJson dahil)
        /// </summary>
        public async Task<List<Machine>> GetMachinesAsync()
        {
            // WebAPI endpoint: /api/Machines
            var response = await _httpClient.GetAsync("api/Machines");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Machine>>();
        }

        /// <summary>
        /// Makineye jenerik PLC komutu gönderir (Örn: Alarm Onaylama).
        /// </summary>
        public async Task<bool> AcknowledgeAlarmAsync(int machineId)
        {
            // WebAPI endpoint: /api/Machines/{id}/acknowledgeAlarm
            var response = await _httpClient.PostAsync($"api/Machines/{machineId}/acknowledgeAlarm", null);
            return response.IsSuccessStatusCode;
        }

        // =========================================================================
        // 2. REÇETE VE ANALİZ (Dinamik Hesaplama DTO'ları)
        // =========================================================================

        /// <summary>
        /// Belirli bir reçeteyi ve WebAPI'da dinamik olarak hesaplanmış teorik süresini çeker.
        /// </summary>
        public async Task<RecipeDetailDto> GetRecipeDetailsAsync(int recipeId)
        {
            // WebAPI endpoint: /api/Recipes/{id}
            var response = await _httpClient.GetAsync($"api/Recipes/{recipeId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RecipeDetailDto>();
        }

        // =========================================================================
        // 3. DASHBOARD VE RAPORLAR (Jenerik KPI ve Tüketim Modelleri)
        // =========================================================================

        /// <summary>
        /// Jenerik KPI (Key Performance Indicator) listesini çeker.
        /// Her KPI, Key, Display, Value ve Unit alanlarını içerir.
        /// </summary>
        public async Task<List<KpiData>> GetDashboardKpisAsync()
        {
            // WebAPI endpoint: /api/Dashboard/kpis
            var response = await _httpClient.GetAsync("api/Dashboard/kpis");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<KpiData>>();
        }

        /// <summary>
        /// Dinamik Maliyet ve Tüketim özetini çeker (ConsumptionMetrics jenerik key/value içerir).
        /// </summary>
        public async Task<ConsumptionReportDto> GetConsumptionSummaryAsync(string batchId)
        {
            // WebAPI endpoint: /api/Reports/consumptionSummary?batchId={batchId}
            var response = await _httpClient.GetAsync($"api/Reports/consumptionSummary?batchId={batchId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ConsumptionReportDto>();
        }
    }
}
// NOT: WebAPI'da tanımlanan RecipeDetailDto, KpiData, ConsumptionReportDto modellerinin
// bu projede de tanımlı (veya referans alınmış) olması gerekir.