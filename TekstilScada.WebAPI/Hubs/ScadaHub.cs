using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using TekstilScada.Models;
using System.Threading.Tasks;
using System;

namespace TekstilScada.WebAPI.Hubs
{
    // [Authorize] attribute'u sayesinde sadece Token almış güvenli istemciler bağlanabilir.
    //[Authorize]
    public class ScadaHub : Hub
    {
        // --- 1. Windows Forms'un Çağıracağı Metotlar ---

        /// <summary>
        /// Windows Forms uygulaması (Gateway), yerel PLC'den okuduğu veriyi bu metoda gönderir.
        /// Sunucu bu veriyi alır ve bağlı olan tüm Web Tarayıcılarına (Dashboard) iletir.
        /// </summary>
        public async Task BroadcastFromLocal(FullMachineStatus status)
        {
            if (status == null) return;

            // Loglama (Opsiyonel): Verinin geldiğini sunucu konsolunda görmek için
            // Console.WriteLine($"Gelen Veri: {status.MachineName} - {DateTime.Now}");

            // "Others" kullanarak, gönderen kişi (WinForms) hariç diğer herkese (Web Clientlara) gönder.
            // Web tarafındaki JavaScript/Blazor, "ReceiveMachineUpdate" olayını dinlemelidir.
            await Clients.Others.SendAsync("ReceiveMachineUpdate", status);
        }

        // --- 2. Web Uygulamasının (Tarayıcının) Çağıracağı Metotlar ---

        /// <summary>
        /// Web arayüzünden bir operatör "Makineyi Durdur" butonuna bastığında bu metot çağrılır.
        /// Sunucu bu komutu alır ve Windows Forms uygulamasına iletir.
        /// </summary>
        public async Task SendCommandToLocal(int machineId, string command, string parameters)
        {
            // Bu komutu tüm bağlı istemcilere (WinForms dahil) gönderir.
            // Ancak Windows Forms tarafında "ReceiveCommand" dinleyicisi sadece kendi ID'sine aitse işlem yapar.
            // Daha güvenli olması için WinForms'un ConnectionId'si saklanıp sadece ona da gönderilebilir (İleri Seviye).
            await Clients.All.SendAsync("ReceiveCommand", machineId, command, parameters);
        }

        // --- 3. Bağlantı Yönetimi ---

        public override async Task OnConnectedAsync()
        {
            // Bağlanan kişinin kim olduğunu loglayabiliriz (User.Identity.Name)
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}