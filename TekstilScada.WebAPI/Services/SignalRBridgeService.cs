using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TekstilScada.Models;
using TekstilScada.Services;
using TekstilScada.WebAPI.Hubs;

namespace TekstilScada.WebAPI.Services
{
    /// <summary>
    /// PlcPollingService'den gelen olayları dinler, tamponlar ve belirli aralıklarla SignalR Hub'ına iletir.
    /// </summary>
    public class SignalRBridgeService : IDisposable
    {
        private readonly IHubContext<ScadaHub> _hubContext;
        private readonly PlcPollingService _pollingService;

        // Throttling için gerekli yapılar
        private readonly ConcurrentDictionary<int, FullMachineStatus> _bufferedData;
        private readonly System.Threading.Timer _broadcastTimer;
        private readonly int _broadcastIntervalMs = 1000; // 1 saniyede bir güncelleme (Tarayıcı için ideal)
        private volatile bool _hasNewData = false;

        public SignalRBridgeService(IHubContext<ScadaHub> hubContext, PlcPollingService pollingService)
        {
            _hubContext = hubContext;
            _pollingService = pollingService;

            _bufferedData = new ConcurrentDictionary<int, FullMachineStatus>();

            // Zamanlayıcıyı başlat (1 saniyede bir çalışır)
            _broadcastTimer = new System.Threading.Timer(BroadcastBufferedData, null, _broadcastIntervalMs, _broadcastIntervalMs);

            // Polling servisinden gelen veri yenileme olayına abone oluyoruz.
            _pollingService.OnMachineDataRefreshed += OnMachineDataRefreshedHandler;
        }

        // Olay tetiklendiğinde çalışacak metot (Artık direkt göndermiyor, havuza atıyor)
        private void OnMachineDataRefreshedHandler(int machineId, FullMachineStatus status)
        {
            // Gelen veriyi havuza ekle veya varsa güncelle (Sadece en son veri önemlidir)
            _bufferedData[machineId] = status;
            _hasNewData = true;
        }

        // Zamanlayıcı tetiklendiğinde çalışır (Verileri toplu gönderir)
        private async void BroadcastBufferedData(object state)
        {
            if (!_hasNewData || _bufferedData.IsEmpty) return;

            try
            {
                // Havuzdaki tüm verilerin bir kopyasını al
                var currentDataBatch = _bufferedData.Values.ToList();

                // Bayrağı sıfırla (Yeni veri gelene kadar tekrar gönderme yapma)
                _hasNewData = false;

                // İsteğe bağlı: Gönderildikten sonra havuzu temizlemek isterseniz:
                // _bufferedData.Clear(); 
                // Ancak SCADA sistemlerinde genellikle son durumu tutmak ve sadece değişenleri göndermek 
                // daha karmaşık olduğundan, her seferinde güncel snapshot'ı göndermek daha güvenilirdir.

                // YÖNTEM 1: Her makineyi tek tek ama hızlı döngüde gönder (Mevcut İstemci Yapısını Bozmaz)
                foreach (var status in currentDataBatch)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMachineUpdate", status);
                }

                // YÖNTEM 2 (ÖNERİLEN - İLERİ SEVİYE): 
                // Eğer istemci tarafını (ScadaDataService.cs) "ReceiveBatchUpdate" metodunu dinleyecek şekilde güncellerseniz,
                // tüm listeyi tek seferde göndererek performansı daha da artırabilirsiniz.
                // await _hubContext.Clients.All.SendAsync("ReceiveBatchUpdate", currentDataBatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Broadcast Hatası: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _broadcastTimer?.Dispose();
            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= OnMachineDataRefreshedHandler;
            }
        }
    }
}