using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using TekstilScada.Core.Models; // Modellerin namespace'i
using TekstilScada.Models;
using TekstilScada.WebApp.Services;

namespace TekstilScada.WebApp.Services
{
    public class FactoryStateService : IHostedService, IDisposable
    {
        public ConcurrentDictionary<int, List<FullMachineStatus>> FactoryDataCache { get; private set; } = new();
        public event Action? OnChange;

        private Timer? _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        // Bağlantıyı sürekli açık tutmak için servisi sınıf seviyesinde tutacağız
        private ScadaDataService? _scadaService;
        private IServiceScope? _scope; // Scope'u da açık tutmalıyız

        public FactoryStateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 1. Servisi ve Bağlantıyı BAŞLANGIÇTA bir kere kur
            _scope = _scopeFactory.CreateScope();
            _scadaService = _scope.ServiceProvider.GetRequiredService<ScadaDataService>();

            try
            {
                // Bir kere login ol ve bağlan
                await _scadaService.InitializeForBackgroundServiceAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FactoryStateService] Başlangıç bağlantı hatası: {ex.Message}");
            }

            // 2. Timer'ı başlat (Sadece veri çekmek için)
            _timer = new Timer(FetchAllFactoryData, null, 0, 2000);
        }

        private async void FetchAllFactoryData(object? state)
        {
            if (_scadaService == null) return;

            try
            {
                // Bağlantı kopmuşsa tekrar bağlanmayı dene
                if (_scadaService.HubConnection == null ||
                    _scadaService.HubConnection.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Disconnected)
                {
                    await _scadaService.InitializeForBackgroundServiceAsync();
                }

                // 1. Fabrikaları çek
                var factories = await _scadaService.GetAllFactoriesAsync();

                // 2. Her fabrika için veriyi çek
                await Parallel.ForEachAsync(factories, async (factory, token) =>
                {
                    try
                    {
                        var machines = await _scadaService.GetLiveMachineStatusByFactoryId(factory.Id);

                        // ÖNEMLİ DÜZELTME: Sadece veri varsa güncelle!
                        // Bağlantı hatasında boş liste gelirse eski veriyi silme.
                        if (machines != null && machines.Count > 0)
                        {
                            FactoryDataCache.AddOrUpdate(factory.Id, machines, (key, oldValue) => machines);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fabrika {factory.Id} veri hatası: {ex.Message}");
                    }
                });

                // Arayüzü güncelle
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FactoryStateService Döngü Hatası: {ex.Message}");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            // Temizlik
            if (_scadaService != null) await _scadaService.DisposeAsync();
            _scope?.Dispose();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _scope?.Dispose();
        }
    }
}