// Dosya: Universalscada.WebApp/Services/SignalRClientService.cs - YENİ DOSYA

using Microsoft.AspNetCore.SignalR.Client;
using Universalscada.Models;
using System;
using System.Threading.Tasks;

namespace Universalscada.WebApp.Services
{
    /// <summary>
    /// WebAPI'daki ScadaHub'a bağlantıyı yöneten ve gelen anlık veriyi
    /// tüm bileşenlere jenerik event ile yayan hizmet.
    /// </summary>
    public class SignalRClientService
    {
        private HubConnection _hubConnection;
        private readonly string _hubUrl;

        // Abone olunan event: FullMachineStatus tipinde jenerik veri yayınlar.
        public event Action<FullMachineStatus> OnReceiveUpdate;

        public SignalRClientService(string hubUrl)
        {
            _hubUrl = hubUrl;
        }

        /// <summary>
        /// SignalR bağlantısını başlatır.
        /// </summary>
        public async Task StartConnectionAsync()
        {
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                return;
            }

            // Hub bağlantısını oluştur
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect() // Otomatik yeniden bağlanmayı etkinleştir
                .Build();

            // WebAPI'daki SignalRBridgeService'ten gelen "ReceiveMachineUpdate" mesajına abone ol
            // Gelen veri, jenerik FullMachineStatus nesnesidir.
            _hubConnection.On<FullMachineStatus>("ReceiveMachineUpdate", (status) =>
            {
                // Abone olan tüm Blazor bileşenlerine veriyi ilet
                OnReceiveUpdate?.Invoke(status);
            });

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR bağlantısı başarılı: " + _hubUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR bağlantı hatası: {ex.Message}");
                // Yeniden deneme mekanizması eklenebilir.
            }
        }

        /// <summary>
        /// SignalR bağlantısını durdurur.
        /// </summary>
        public async Task StopConnectionAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
            }
        }
    }
}