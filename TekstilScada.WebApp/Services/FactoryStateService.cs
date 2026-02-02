using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.WebApp.Services;

namespace TekstilScada.WebApp.Services
{
    public class FactoryStateService : IHostedService, IDisposable
    {
        // Anahtar: FactoryId, Değer: O fabrikanın makineleri
        public ConcurrentDictionary<int, List<FullMachineStatus>> FactoryDataCache { get; private set; } = new();

        public event Action? OnChange;

        private readonly IServiceScopeFactory _scopeFactory;
        private ScadaDataService? _scadaService;
        private IServiceScope? _scope;
        private Timer? _uiRefreshTimer;
        private Timer? _syncTimer;
        private volatile bool _hasPendingChanges = false;

        public FactoryStateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scope = _scopeFactory.CreateScope();
            _scadaService = _scope.ServiceProvider.GetRequiredService<ScadaDataService>();

            try
            {
                Console.WriteLine("[FactoryStateService] Servis başlatılıyor...");
                await _scadaService.InitializeForBackgroundServiceAsync();

                if (_scadaService.HubConnection != null)
                {
                    // DÜZELTME: Handler artık (factoryId, status) alıyor
                    _scadaService.SubscribeToLiveUpdates(OnMachineUpdateReceived);
                    Console.WriteLine("[FactoryStateService] ✅ Canlı veri akışına abone olundu.");
                }

                await RefreshAllData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FactoryStateService] Başlangıç Hatası: {ex.Message}");
            }

            _uiRefreshTimer = new Timer(UiRefreshTick, null, 1000, 1000);
            _syncTimer = new Timer(SyncTimerCallback, null, 30000, 30000);
        }

        // --- KRİTİK DÜZELTME BURADA ---
        // Core modelinde değişiklik yapmadan, FactoryId'yi parametre olarak aldık.
        private void OnMachineUpdateReceived(int factoryId, FullMachineStatus status)
        {
            if (status == null) return;

            // 1. Sadece ilgili fabrikanın listesini çek
            // (Dictionary'de yoksa otomatik oluşturur)
            var machines = FactoryDataCache.GetOrAdd(factoryId, new List<FullMachineStatus>());

            lock (machines) // Listeye aynı anda erişimi koru
            {
                var existingMachine = machines.FirstOrDefault(m => m.MachineId == status.MachineId);

                if (existingMachine != null)
                {
                    // Güncelle
                    int index = machines.IndexOf(existingMachine);
                    machines[index] = status;
                }
                else
                {
                    // Ekle
                    machines.Add(status);
                }
            }

            _hasPendingChanges = true;
        }

        private void UiRefreshTick(object? state)
        {
            if (_hasPendingChanges)
            {
                _hasPendingChanges = false;
                OnChange?.Invoke();
            }
        }

        private async void SyncTimerCallback(object? state) => await RefreshAllData();

        private async Task RefreshAllData()
        {
            if (_scadaService == null || _scadaService.HubConnection?.State != HubConnectionState.Connected)
                return;

            try
            {
                // NOT: Burada ScadaDataService içindeki GetAllFactories metodunu çağırıyorsunuz.
                // Bu metodun çalıştığını varsayıyorum.
                var factories = await _scadaService.GetAllFactoriesAsync();
                if (factories == null || factories.Count == 0) return;

                var factoryIds = factories.Select(f => f.Id).ToList();

                // Abonelikleri yenile
                await _scadaService.HubConnection.InvokeAsync("SubscribeToFactories", factoryIds);

                await Parallel.ForEachAsync(factories, async (factory, token) =>
                {
                    // DÜZELTME: Hub'daki yeni metoda FactoryId göndererek çekiyoruz.
                    // (Hub'da GetLiveMachineStatusByFactoryId metodunun güncel halini kullanır)
                    var machines = await _scadaService.HubConnection.InvokeAsync<List<FullMachineStatus>>("GetLiveMachineStatusByFactoryId", factory.Id);

                    var safeList = machines ?? new List<FullMachineStatus>();

                    FactoryDataCache.AddOrUpdate(factory.Id, safeList, (key, old) => safeList);
                });

                _hasPendingChanges = false;
                OnChange?.Invoke();
            }
            catch { }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _uiRefreshTimer?.Change(Timeout.Infinite, 0);
            _syncTimer?.Change(Timeout.Infinite, 0);
            if (_scadaService != null) await _scadaService.DisposeAsync();
            _scope?.Dispose();
        }

        public void Dispose()
        {
            _uiRefreshTimer?.Dispose();
            _syncTimer?.Dispose();
            _scope?.Dispose();
        }
    }
}