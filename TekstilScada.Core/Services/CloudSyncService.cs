using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
//using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Services; // PlcPollingService'in olduğu namespace

namespace TekstilScada.UI.Services
{
    /// <summary>
    /// Bu servis, Yerel PLC servisinden gelen verileri dinler ve Bulut API'ye gönderir.
    /// </summary>
    public class CloudSyncService : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly PlcPollingService _pollingService;
        public event Action<int, string, string> OnRemoteCommandReceived;
        // API Adresi (Geliştirme aşamasında localhost, canlıda gerçek domain olacak)
        // Eğer API ile WinForms aynı bilgisayardaysa localhost kalabilir.
        private const string ApiBaseUrl = "http://localhost:7039/scadaHub";

        // Veri gönderim sıklığını kontrol etmek için (Opsiyonel throttling)
        private DateTime _lastSentTime = DateTime.MinValue;
        private readonly int _sendIntervalMs = 500; // Yarım saniyede bir gönderim limiti

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public CloudSyncService(PlcPollingService pollingService, string authToken)
        {
            _pollingService = pollingService;

            // 1. SignalR Bağlantısını Yapılandır
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ApiBaseUrl, options =>
                {
                    // API'ye güvenli erişim için JWT Token'ı header'a ekliyoruz
                    options.AccessTokenProvider = () => Task.FromResult(authToken);
                })
                .WithAutomaticReconnect() // Bağlantı koparsa otomatik tekrar dene
                .Build();

            // 2. Bağlantı olaylarını (Loglama için) dinle
            _hubConnection.Reconnecting += error =>
            {
                // UI thread'inde olmadığımız için Console veya Log dosyasına yazıyoruz
                System.Diagnostics.Debug.WriteLine($"Bulut bağlantısı koptu, yeniden bağlanılıyor... Hata: {error?.Message}");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                System.Diagnostics.Debug.WriteLine($"Bulut bağlantısı tekrar sağlandı. ID: {connectionId}");
                return Task.CompletedTask;
            };
            _hubConnection.On<int, string, string>("ReceiveCommand", (machineId, command, parameters) =>
            {
                // Gelen emri konsola yaz (Debug için)
                System.Diagnostics.Debug.WriteLine($"Buluttan Emir Geldi: Makine:{machineId} Komut:{command}");

                // Olayı tetikle ve abone olanları (MainForm) haberdar et
                // UI thread sorunu yaşamamak için Invoke gerekebilir ama event fırlatmak güvenlidir.
                OnRemoteCommandReceived?.Invoke(machineId, command, parameters);
            });
            // 3. Yerel PLC Veri Akışına Abone Ol
            // PlcPollingService her veri okuduğunda bu metot çalışacak
            _pollingService.OnMachineDataRefreshed += OnLocalDataRefreshed;
        }

        public async Task StartAsync()
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                    System.Diagnostics.Debug.WriteLine("Bulut API'ye başarıyla bağlanıldı.");
                }
            }
            catch (Exception ex)
            {
                // Bağlantı hatası durumunda uygulamayı çökertme, sadece logla.
                System.Diagnostics.Debug.WriteLine($"Bulut bağlantı hatası: {ex.Message}");
                // İsterseniz burada bir Timer ile 10 saniye sonra tekrar deneme mantığı kurabilirsiniz.
            }
        }

        private async void OnLocalDataRefreshed(int machineId, FullMachineStatus status)
        {
            // Bağlantı yoksa veya veri boşsa işlem yapma
            if (_hubConnection.State != HubConnectionState.Connected || status == null)
                return;

            // (Opsiyonel) Çok sık veri gidiyorsa burada filtreleme yapılabilir
            // if ((DateTime.Now - _lastSentTime).TotalMilliseconds < _sendIntervalMs) return;

            try
            {
                // API'deki "BroadcastFromLocal" metodunu çağır ve veriyi gönder
                // Not: "BroadcastFromLocal" ismini API tarafındaki Hub'da da aynen kullanacağız.
                await _hubConnection.InvokeAsync("BroadcastFromLocal", status);

                _lastSentTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Veri gönderme hatası ({machineId}): {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            // Aboneliği iptal et
            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= OnLocalDataRefreshed;
            }

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

    }
}