using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Net.WebSockets;
using TekstilScada.Repositories;

namespace TekstilScada.WebAPI.Controllers
{
    public class VncProxyController : ControllerBase
    {
        private readonly MachineRepository _machineRepository;

        public VncProxyController(MachineRepository machineRepository)
        {
            _machineRepository = machineRepository;
        }

        [Route("api/vnc/connect/{machineId}")]
        public async Task Get(int machineId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                // 1. Makine bilgilerini al
                var machine = _machineRepository.GetAllMachines().FirstOrDefault(m => m.Id == machineId);

                if (machine == null || string.IsNullOrEmpty(machine.VncAddress)) // VncAddress IP olarak kullanılıyor
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Makine VNC adresi yok", CancellationToken.None);
                    return;
                }

                // IP ve Port ayrıştırma (Örn: 192.168.1.10:5900 veya sadece IP)
                string ip = machine.VncAddress.Split(':')[0];
                int port = 5900; // Varsayılan VNC portu

                try
                {
                    // 2. PLC'ye (VNC Server) TCP Bağlantısı aç
                    using var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(ip, port);
                    using var tcpStream = tcpClient.GetStream();

                    // 3. İki yönlü veri akışını başlat (Bridge)
                    var bufferSize = 1024 * 4;
                    var sending = Task.Run(async () =>
                    {
                        var buffer = new byte[bufferSize];
                        while (webSocket.State == WebSocketState.Open)
                        {
                            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Close) break;

                            await tcpStream.WriteAsync(buffer, 0, result.Count);
                        }
                    });

                    var receiving = Task.Run(async () =>
                    {
                        var buffer = new byte[bufferSize];
                        while (tcpClient.Connected && webSocket.State == WebSocketState.Open)
                        {
                            int bytesRead = await tcpStream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;

                            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, true, CancellationToken.None);
                        }
                    });

                    await Task.WhenAny(sending, receiving);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"VNC Proxy Hatası: {ex.Message}");
                }
                finally
                {
                    if (webSocket.State == WebSocketState.Open)
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}