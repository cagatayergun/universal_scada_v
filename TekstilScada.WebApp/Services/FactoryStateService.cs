using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.WebApp.Services;

namespace TekstilScada.WebApp.Services
{
    public class FactoryStateService : IHostedService, IDisposable
    {
        public ConcurrentDictionary<int, List<FullMachineStatus>> FactoryDataCache { get; private set; } = new();

        // Arayüzün dinlediği olay
        public event Action? OnChange;

        private readonly IServiceScopeFactory _scopeFactory;
        private ScadaDataService? _scadaService;
        private IServiceScope? _scope;

        // Timerlar
        private Timer? _syncTimer;      // Veri eksikse tamamlamak için (30 sn)
        private Timer? _uiRefreshTimer; // Arayüzü kasmadan yenilemek için (1 sn)

        // Performans Bayrağı: Eğer veri değişmediyse arayüzü boşuna yenilemeyelim
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
                // 1. ÖNCE BAĞLANTIYI KUR
                Console.WriteLine("[FactoryStateService] Servis başlatılıyor...");
                await _scadaService.InitializeForBackgroundServiceAsync();

                // 2. SONRA ABONE OL
                if (_scadaService.HubConnection != null)
                {
                    _scadaService.SubscribeToLiveUpdates(OnMachineUpdateReceived);
                    Console.WriteLine("[FactoryStateService] ✅ Canlı veri akışına abone olundu.");
                }

                // 3. İLK VERİLERİ ÇEK
                await RefreshAllData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FactoryStateService] Başlangıç Hatası: {ex.Message}");
            }

            // --- ZAMANLAYICILAR ---

            // A. UI Timer: Saniyede 1 kere çalışır, arayüzü rahatlatır (Takılmayı çözen kısım burası)
            _uiRefreshTimer = new Timer(UiRefreshTick, null, 1000, 1000);

            // B. Sync Timer: 30 saniyede bir toplu kontrol yapar
            _syncTimer = new Timer(SyncTimerCallback, null, 30000, 30000);
        }

        // --- CANLI VERİ BURAYA GELİR (Saniyede yüzlerce kez tetiklenebilir) ---
        private void OnMachineUpdateReceived(FullMachineStatus status)
        {
            if (status == null) return;

            // 1. Veriyi RAM'e (Cache) İşle (Bu işlem çok hızlıdır, arayüzü yormaz)
            bool found = false;
            foreach (var factoryId in FactoryDataCache.Keys)
            {
                if (FactoryDataCache.TryGetValue(factoryId, out var machines))
                {
                    var existingMachine = machines.FirstOrDefault(m => m.MachineId == status.MachineId);

                    if (existingMachine != null)
                    {
                        // Varsa güncelle
                        int index = machines.IndexOf(existingMachine);
                        machines[index] = status;
                        found = true;
                    }
                    else
                    {
                        // Yoksa ekle
                        machines.Add(status);
                        found = true;
                    }

                    if (found) break;
                }
            }

            // 2. DİKKAT: Burada OnChange?.Invoke() ÇAĞIRMIYORUZ!
            // Sadece "Veri değişti, haberin olsun" bayrağını kaldırıyoruz.
            // Arayüzü _uiRefreshTimer güncelleyecek.
            if (found)
            {
                _hasPendingChanges = true;
            }
        }

        // --- ARAYÜZ YENİLEME TİMER'I (Saniyede 1 Kere) ---
        private void UiRefreshTick(object? state)
        {
            // Eğer yeni veri geldiyse arayüzü tetikle
            if (_hasPendingChanges)
            {
                _hasPendingChanges = false; // Bayrağı indir
                OnChange?.Invoke(); // Şimdi arayüzü yenile (Saniyede max 1 kez)
            }
        }

        private async void SyncTimerCallback(object? state)
        {
            await RefreshAllData();
        }

        private async Task RefreshAllData()
        {
            if (_scadaService == null || _scadaService.HubConnection?.State != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
                return;

            try
            {
                var factories = await _scadaService.GetAllFactoriesAsync();

                if (factories == null || factories.Count == 0) return;

                var factoryIds = factories.Select(f => f.Id).ToList();
                await _scadaService.HubConnection.InvokeAsync("SubscribeToFactories", factoryIds);

                await Parallel.ForEachAsync(factories, async (factory, token) =>
                {
                    var machines = await _scadaService.GetLiveMachineStatusByFactoryId(factory.Id);
                    var safeList = machines ?? new List<FullMachineStatus>();

                    FactoryDataCache.AddOrUpdate(factory.Id, safeList, (key, old) =>
                    {
                        if (safeList.Count == 0 && old.Count > 0) return old;
                        return safeList;
                    });
                });

                // Toplu güncellemede hemen yansısın (Zaten 30 saniyede bir oluyor)
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