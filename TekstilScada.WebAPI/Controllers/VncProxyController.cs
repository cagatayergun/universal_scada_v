using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
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

                if (machine == null || string.IsNullOrEmpty(machine.VncAddress))
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Makine VNC adresi yok", CancellationToken.None);
                    return;
                }

                string ip = machine.VncAddress.Split(':')[0];
                int port = 5900;
                // Eğer adreste port varsa onu kullan (Örn: 192.168.1.10:5901)
                if (machine.VncAddress.Contains(":"))
                {
                    if (int.TryParse(machine.VncAddress.Split(':')[1], out int p)) port = p;
                }

                using var tcpClient = new TcpClient();

                try
                {
                    // 2. PLC'ye Bağlan
                    await tcpClient.ConnectAsync(ip, port);
                    using var tcpStream = tcpClient.GetStream();
                    using var cts = new CancellationTokenSource();

                    // 3. Veri Akışı (WebSocket <-> TCP)
                    var bufferSize = 1024 * 4;

                    // GÖREV 1: WebSocket'ten oku -> TCP'ye yaz (İstemci -> PLC)
                    var sending = Task.Run(async () =>
                    {
                        var buffer = new byte[bufferSize];
                        try
                        {
                            while (webSocket.State == WebSocketState.Open && tcpClient.Connected && !cts.Token.IsCancellationRequested)
                            {
                                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                                if (result.MessageType == WebSocketMessageType.Close) break;

                                await tcpStream.WriteAsync(buffer, 0, result.Count, cts.Token);
                            }
                        }
                        catch (Exception) { /* Bağlantı koptuğunda hata fırlatabilir, yutuyoruz */ }
                        finally { cts.Cancel(); } // Diğer döngüyü de durdur
                    });

                    // GÖREV 2: TCP'den oku -> WebSocket'e yaz (PLC -> İstemci)
                    var receiving = Task.Run(async () =>
                    {
                        var buffer = new byte[bufferSize];
                        bool isFirstPacket = true; // İlk paketi kontrol etmek için bayrak

                        try
                        {
                            while (webSocket.State == WebSocketState.Open && tcpClient.Connected && !cts.Token.IsCancellationRequested)
                            {
                                int bytesRead = await tcpStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                                if (bytesRead == 0) break;

                                // --- YAMA BAŞLANGICI ---
                                // Sorun: PLC "RFB 003.005" gönderiyor, noVNC bunu reddediyor.
                                // Çözüm: İlk pakette bu sürümü görürsek "RFB 003.003" (standart) olarak değiştiriyoruz.
                                if (isFirstPacket)
                                {
                                    string header = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                    // Sadece versiyon paketiyse (genellikle 12 byte: RFB 003.005\n)
                                    if (header.Contains("RFB 003.005"))
                                    {
                                        Console.WriteLine($"[VNC Proxy] Eski sürüm (003.005) tespit edildi. 003.003 olarak düzeltiliyor...");

                                        // 3.5 yerine 3.3 gönderiyoruz. 3.3 en uyumlu sürümdür.
                                        var newHeader = "RFB 003.003\n";
                                        var newBytes = Encoding.ASCII.GetBytes(newHeader);

                                        // Web socket'e düzeltilmiş veriyi gönder
                                        await webSocket.SendAsync(new ArraySegment<byte>(newBytes), WebSocketMessageType.Binary, true, cts.Token);

                                        isFirstPacket = false;
                                        continue; // Döngüye devam et, orijinal buffer'ı gönderme
                                    }
                                    isFirstPacket = false;
                                }
                                // --- YAMA BİTİŞİ ---

                                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, true, cts.Token);
                            }
                        }
                        catch (Exception) { /* Bağlantı koptu */ }
                        finally { cts.Cancel(); }
                    });

                    // Herhangi biri bitene kadar bekle
                    await Task.WhenAny(sending, receiving);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"VNC Proxy Hatası ({ip}): {ex.Message}");
                }
                finally
                {
                    // Temiz kapanış
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