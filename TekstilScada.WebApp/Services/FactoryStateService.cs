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

        // --- KONFIGURASYON ---
        // Pull (Tam Sorgu) süresini 5 dakikaya çektik.
        // Çünkü asıl veriyi artık SignalR "Push" (Canlı Yayın) ile alıyoruz.
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _uiRefreshInterval = TimeSpan.FromSeconds(1);

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
                    // 1. PUSH ABONELİĞİ: Canlı veri geldiğinde bu metot tetiklenir.
                    // (factoryId, status) parametrelerini alacak şekilde ayarlandı.
                    _scadaService.SubscribeToLiveUpdates(OnMachineUpdateReceived);

                    // 2. RECONNECTION: Bağlantı kopup gelirse tam senkronizasyon yap.
                    _scadaService.HubConnection.Reconnected += async (s) =>
                    {
                        Console.WriteLine("[FactoryStateService] Bağlantı tazelendi, veriler senkronize ediliyor...");
                        await RefreshAllData();
                    };

                    Console.WriteLine("[FactoryStateService] ✅ Canlı veri akışına abone olundu.");
                }

                // İlk açılışta bir kez tam veri çek
                await RefreshAllData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FactoryStateService] Başlangıç Hatası: {ex.Message}");
            }

            // UI Throttling Timer (1 sn)
            _uiRefreshTimer = new Timer(UiRefreshTick, null, _uiRefreshInterval, _uiRefreshInterval);

            // Sync Timer (5 dk) - Sadece veri bütünlüğünü sağlamak için (Backup)
            _syncTimer = new Timer(SyncTimerCallback, null, _syncInterval, _syncInterval);
        }

        // --- PUSH EVENT HANDLER (CANLI VERİ) ---
        private void OnMachineUpdateReceived(int factoryId, FullMachineStatus status)
        {
            if (status == null) return;

            // Cache'ten ilgili fabrikanın listesini al veya oluştur
            var machines = FactoryDataCache.GetOrAdd(factoryId, new List<FullMachineStatus>());

            lock (machines) // Thread-Safety için kilitleme
            {
                var existingMachine = machines.FirstOrDefault(m => m.MachineId == status.MachineId);

                if (existingMachine != null)
                {
                    // Mevcut makineyi güncelle (Listeden silip eklemek yerine özelliklerini güncellemek daha hafif olabilir ama bu yöntem güvenli)
                    int index = machines.IndexOf(existingMachine);
                    machines[index] = status;
                }
                else
                {
                    // Yeni makine ekle
                    machines.Add(status);
                }
            }

            // UI tarafına "Veri değişti" sinyali için bayrağı kaldır
            _hasPendingChanges = true;
        }

        // --- UI REFRESH (THROTTLING) ---
        private void UiRefreshTick(object? state)
        {
            // Eğer değişiklik varsa ve UI henüz haber almadıysa tetikle
            if (_hasPendingChanges)
            {
                _hasPendingChanges = false;
                OnChange?.Invoke();
            }
        }

        // --- SYNC (PULL) ---
        private async void SyncTimerCallback(object? state) => await RefreshAllData();

        private async Task RefreshAllData()
        {
            if (_scadaService == null || _scadaService.HubConnection?.State != HubConnectionState.Connected)
                return;

            try
            {
                // Tüm fabrikaların listesini al
                var factories = await _scadaService.GetAllFactoriesAsync();
                if (factories == null || factories.Count == 0) return;

                var factoryIds = factories.Select(f => f.Id).ToList();

                // Hub'a "Ben bu fabrikaların verilerini dinlemek istiyorum" de.
                // Bu sayede Hub bize Push bildirimi göndermeye başlar.
                await _scadaService.HubConnection.InvokeAsync("SubscribeToFactories", factoryIds);

                // Paralel olarak tüm fabrikaların ANLIK durumunu çek (Snapshot)
                // Bu işlem artık sadece 5 dakikada bir veya ilk açılışta yapılır.
                await Parallel.ForEachAsync(factories, async (factory, token) =>
                {
                    try
                    {
                        var machines = await _scadaService.HubConnection.InvokeAsync<List<FullMachineStatus>>("GetLiveMachineStatusByFactoryId", factory.Id);
                        var safeList = machines ?? new List<FullMachineStatus>();
                        FactoryDataCache.AddOrUpdate(factory.Id, safeList, (key, old) => safeList);
                    }
                    catch
                    {
                        // Tek bir fabrikadaki hata diğerlerini etkilemesin
                    }
                });

                _hasPendingChanges = true;
                // UiRefreshTick zaten bunu yakalayıp UI'ı güncelleyecek
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FactoryStateService] Refresh Hatası: {ex.Message}");
            }
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