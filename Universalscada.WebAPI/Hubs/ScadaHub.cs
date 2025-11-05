// Dosya: Universalscada.WebAPI/Hubs/ScadaHub.cs
using Microsoft.AspNetCore.SignalR;

namespace Universalscada.WebAPI.Hubs
{
    // Bu Hub, sunucudan istemcilere anlık veri göndermek için kullanılacak.
    public class ScadaHub : Hub
    {
        // İstemciler (web tarayıcıları) bu Hub'a bağlanacak.
        // Sunucu, bu Hub üzerinden onlara mesaj gönderecek.
        // Şimdilik içi boş kalabilir.
    }
}