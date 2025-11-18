// Dosya: Universalscada.WebAPI/Services/SignalRBridgeService.cs - GÜNCEL VERSİYON

using Microsoft.AspNetCore.SignalR;
using Universalscada.Models;
using Universalscada.Core.Services; // LiveEventAggregator için
using Universalscada.WebAPI.Hubs;

namespace Universalscada.WebAPI.Services
{
    /// <summary>
    /// LiveEventAggregator'dan gelen olayları dinler ve SignalR Hub'ına iletir.
    /// PLC Polling Service ile UI arasında gevşek bağlı (decoupled) köprü görevi görür.
    /// </summary>
    public class SignalRBridgeService : IDisposable
    {
        private readonly IHubContext<ScadaHub> _hubContext;
        private readonly LiveEventAggregator _eventAggregator;
        private IDisposable _subscription; // Aboneliği tutmak için

        public SignalRBridgeService(IHubContext<ScadaHub> hubContext, LiveEventAggregator eventAggregator)
        {
            _hubContext = hubContext;
            _eventAggregator = eventAggregator;

            // LiveEventAggregator'a abone oluyoruz.
            // Bu sayede PollingService'e doğrudan bağımlı kalmıyoruz.
            _subscription = _eventAggregator.Subscribe(OnMachineDataRefreshedHandler);
        }

        // Olay tetiklendiğinde çalışacak asenkron metot.
        private async Task OnMachineDataRefreshedHandler(FullMachineStatus status)
        {
            // Gelen jenerik FullMachineStatus verisini tüm bağlı web tarayıcılarına yayınla.
            // Tüm sektörler için tek ve jenerik bir veri yapısı kullanılır.
            await _hubContext.Clients.All.SendAsync("ReceiveMachineUpdate", status);
        }

        public void Dispose()
        {
            // Uygulama kapanırken aboneliği sonlandır
            _subscription?.Dispose();
        }
    }
}