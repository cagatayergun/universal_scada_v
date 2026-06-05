using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TekstilScada.Models;
using TekstilScada.WebAPI.Hubs;

namespace TekstilScada.WebAPI.Services
{
    public class ApiDashboardCacheWarmerService : BackgroundService
    {
        private readonly IHubContext<ScadaHub> _hubContext;
        private readonly IMemoryCache _memoryCache;

        public ApiDashboardCacheWarmerService(IHubContext<ScadaHub> hubContext, IMemoryCache memoryCache)
        {
            _hubContext = hubContext;
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 5 dakikalık periyot tanımlıyoruz
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            // API ilk ayağa kalktığında grafikler boş kalmasın diye HEMEN tetikle (Cache Warming)
            await WarmUpApiCacheAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await WarmUpApiCacheAsync();
            }
        }

        private async Task WarmUpApiCacheAsync()
        {
            try
            {
                // Hub katmanına o an canlı bağlı olan tüm aktif Fabrika ID'lerini çekiyoruz
                var onlineFactoryIds = ScadaHub.GetOnlineFactoryIdsStatic();

                if (onlineFactoryIds == null || !onlineFactoryIds.Any())
                {
                    Console.WriteLine("[API Cache Warmer] Aktif canlı bağlantısı olan fabrika yok, işlem pas geçildi.");
                    return;
                }

                foreach (var factoryId in onlineFactoryIds)
                {
                    try
                    {
                        Console.WriteLine($"[API Cache Warmer] Fabrika ID: {factoryId} için ağır dashboard verileri RAM'e peşin yazılıyor...");

                        // 1. Tüketim Grafiğini Gateway'den çek ve API RAM'ine yaz
                        var consumptionData = await ScadaHub.InvokeOnGatewayFromBackground<List<HourlyConsumptionData>>(
                            _hubContext, factoryId, "GetHourlyFactoryConsumption");
                        if (consumptionData != null && consumptionData.Any())
                        {
                            _memoryCache.Set($"HourlyConsumption_Factory_{factoryId}", consumptionData, TimeSpan.FromMinutes(6));
                        }

                        // 2. OEE Grafiğini Gateway'den çek ve API RAM'ine yaz
                        var oeeData = await ScadaHub.InvokeOnGatewayFromBackground<List<HourlyOeeData>>(
                            _hubContext, factoryId, "GetHourlyAverageOee");
                        if (oeeData != null && oeeData.Any())
                        {
                            _memoryCache.Set($"HourlyOee_Factory_{factoryId}", oeeData, TimeSpan.FromMinutes(6));
                        }

                        // 3. En Çok Tetiklenen Alarmları Gateway'den çek ve API RAM'ine yaz
                        var topAlarmsData = await ScadaHub.InvokeOnGatewayFromBackground<List<TopAlarmData>>(
                            _hubContext, factoryId, "GetTopAlarmsByFrequency");
                        if (topAlarmsData != null && topAlarmsData.Any())
                        {
                            _memoryCache.Set($"TopAlarms_Factory_{factoryId}", topAlarmsData, TimeSpan.FromMinutes(6));
                        }
                    }
                    catch (Exception fEx)
                    {
                        Console.WriteLine($"[API Cache Warmer] Fabrika ID {factoryId} işlenirken alt hata: {fEx.Message}");
                    }
                }

                Console.WriteLine("[API Cache Warmer] Tüm canlı fabrikaların dashboard grafikleri API RAM belleğine peşin yazıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API Cache Warmer] Genel önbellek ısıtma hatası: {ex.Message}");
            }
        }
    }
}