using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using TekstilScada.WebApp.Services;

namespace TekstilScada.WebApp.Services
{
    public class DashboardCacheWarmerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DashboardCacheWarmerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            // Uygulama ilk ayağa kalktığında grafikler boş kalmasın diye hemen bir kere çalıştır
            await WarmUpDashboardCacheAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await WarmUpDashboardCacheAsync();
            }
        }

        private async Task WarmUpDashboardCacheAsync()
        {
            try
            {
                // 🚨 KRİTİK DÜZELTME 1: IAsyncDisposable hatasını engellemek için 'CreateAsyncScope' ve 'await using' yapısına geçildi.
                await using var scope = _scopeFactory.CreateAsyncScope();
                var scadaService = scope.ServiceProvider.GetRequiredService<ScadaDataService>();

                await scadaService.InitializeForBackgroundServiceAsync();
                var factories = await scadaService.GetMyFactoriesAsync();

                foreach (var factory in factories)
                {
                    // Her fabrikayı kendi içinde try-catch'e alıyoruz ki bir fabrika offline ise diğerleri iptal olmasın
                    try
                    {
                        Console.WriteLine($"[Cache Warmer] Fabrika ID: {factory.Id} ({factory.FactoryName}) için grafikler peşin hesaplanıyor...");

                        // 🚨 KRİTİK DÜZELTME 2: Merkez Hub'ın boş veri ( [] ) döndürmemesi için bağlantıyı o fabrikaya resmi olarak abone ediyoruz (WinForms simülasyonu)
                        var onlineFactoryIds = await scadaService.GetOnlineFactoryIdsAsync();
                        if (onlineFactoryIds != null && onlineFactoryIds.Contains(factory.Id))
                        {
                            await scadaService.SelectFactoryAndSubscribeAsync(factory.Id, factory.FactoryName);

                            // Bilgileri veritabanından tazeleyip RAM'e yazmaya zorla (forceRefresh: true)
                            await scadaService.GetHourlyConsumptionAsync(factory.Id, forceRefresh: true);
                            await scadaService.GetHourlyOeeAsync(factory.Id, forceRefresh: true);
                            await scadaService.GetTopAlarmsAsync(factory.Id, forceRefresh: true);
                        }
                        else
                        {
                            Console.WriteLine($"[Cache Warmer] Uyarı: Fabrika ID {factory.Id} anlık olarak offline, grafik hesabı pas geçildi.");
                        }
                    }
                    catch (Exception fEx)
                    {
                        Console.WriteLine($"[Cache Warmer] Fabrika {factory.Id} işlenirken alt hata: {fEx.Message}");
                    }
                }

                Console.WriteLine("[Cache Warmer] Tüm aktif fabrikaların dashboard grafikleri başarıyla RAM'e peşin yazıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cache Warmer] Genel önbellek ısıtma hatası: {ex.Message}");
            }
        }
    }
}