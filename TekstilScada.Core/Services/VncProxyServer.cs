using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TekstilScada.Services
{
    public class VncProxyServer
    {
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        // WebApp bu porta bağlanacak (Örn: ws://localhost:5901/vnc/)
        private readonly int _localPort;

        public VncProxyServer(int port = 5901)
        {
            _localPort = port;
        }

        public void Start()
        {
            if (_listener != null && _listener.IsListening) return;

            _listener = new HttpListener();
            // Prefix: http://*:5901/vnc/ -> Tüm IP'lerden gelen istekleri dinle
            // Yönetici izni gerekebilir (Visual Studio'yu yönetici olarak çalıştırın)
            _listener.Prefixes.Add($"http://*:{_localPort}/vnc/");

            try
            {
                _listener.Start();
                _cts = new CancellationTokenSource();
                _listenTask = Task.Run(() => AcceptConnections(_cts.Token));
                System.Diagnostics.Debug.WriteLine($"[VNC Proxy] WinForms Sunucusu Başladı: Port {_localPort}");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VNC Proxy] Başlatma Hatası: {ex.Message}");
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
        }

        private async Task AcceptConnections(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    // İstemci bağlantısı bekle
                    var context = await _listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        // Arka planda işle (Bloklamadan)
                        _ = ProcessWebSocketRequest(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (HttpListenerException) { break; } // Listener durdurulduğunda
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[VNC Proxy] Bağlantı Hatası: {ex.Message}"); }
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocket webSocket = null;
            try
            {
                var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                webSocket = wsContext.WebSocket;

                // URL'den IP ve Port parametrelerini al
                // Örnek: ws://localhost:5901/vnc/?target=192.168.1.50:5900
                string target = context.Request.QueryString["target"];

                if (string.IsNullOrEmpty(target))
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Hedef IP belirtilmedi", CancellationToken.None);
                    return;
                }

                string ip = target.Split(':')[0];
                int port = target.Contains(":") ? int.Parse(target.Split(':')[1]) : 5900;

                // Controller'daki mantığın aynısı:
                await ProxyTraffic(webSocket, ip, port);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VNC Proxy] İşlem Hatası: {ex.Message}");
            }
            finally
            {
                if (webSocket != null) webSocket.Dispose();
            }
        }

        private async Task ProxyTraffic(WebSocket webSocket, string ip, int port)
        {
            using var tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(ip, port);
                using var tcpStream = tcpClient.GetStream();
                using var cts = new CancellationTokenSource();

                var bufferSize = 4096;

                // GÖREV 1: WebSocket -> TCP (Tarayıcıdan PLC'ye)
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
                    catch { }
                    finally { cts.Cancel(); }
                });

                // GÖREV 2: TCP -> WebSocket (PLC'den Tarayıcıya)
                var receiving = Task.Run(async () =>
                {
                    var buffer = new byte[bufferSize];
                    bool isFirstPacket = true;
                    try
                    {
                        while (webSocket.State == WebSocketState.Open && tcpClient.Connected && !cts.Token.IsCancellationRequested)
                        {
                            int bytesRead = await tcpStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                            if (bytesRead == 0) break;

                            // --- PROTOKOL YAMASI (Controller'daki gibi) ---
                            if (isFirstPacket)
                            {
                                string header = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                if (header.Contains("RFB 003.005"))
                                {
                                    var newHeader = "RFB 003.003\n";
                                    var newBytes = Encoding.ASCII.GetBytes(newHeader);
                                    await webSocket.SendAsync(new ArraySegment<byte>(newBytes), WebSocketMessageType.Binary, true, cts.Token);
                                    isFirstPacket = false;
                                    continue;
                                }
                                isFirstPacket = false;
                            }
                            // ----------------------------------------------

                            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, true, cts.Token);
                        }
                    }
                    catch { }
                    finally { cts.Cancel(); }
                });

                await Task.WhenAny(sending, receiving);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VNC Proxy] Bağlantı sonlandı ({ip}): {ex.Message}");
            }
        }
    }
}