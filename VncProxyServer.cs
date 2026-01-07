using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms; // Timer için gerekli
using TekstilScada.Core.Services; // SignalRGatewayService
using TekstilScada.Services;
using VncSharpCore; // VncSharpCore.dll

namespace TekstilScada
{
    public class VncProxyServer
    {
        private RemoteDesktop _vncEngine;
        private SignalRGatewayService _gateway;
        private int _currentMachineId;

        // CS0104 Çözümü: Timer'ın tam yolu belirtildi
        private System.Windows.Forms.Timer _broadcastTimer;
        private string _password;

        public VncProxyServer(SignalRGatewayService gateway)
        {
            _gateway = gateway;

            _vncEngine = new RemoteDesktop();
            _vncEngine.VncPort = 5900;

            // CS0246 Çözümü: GetPasswordHandler bazen tanınmayabiliyor.
            // Bu blok hata verirse VncSharp sürümünüz farklıdır.
            // Şifreyi Connect metodunda halledeceğimiz için burayı boş bırakabiliriz veya
            // try-catch ile sarabiliriz.
            try
            {
                // Eğer bu satır hala hata veriyorsa tamamen silebilirsiniz.
                // _vncEngine.GetPassword = new GetPasswordHandler(() => _password);
            }
            catch { }

            // Bağlantı durumlarını konsola yaz
            _vncEngine.ConnectComplete += (s, e) => Console.WriteLine("VNC: Bağlandı.");
            _vncEngine.ConnectionLost += (s, e) => Console.WriteLine("VNC: Bağlantı Koptu.");

            // Timer Ayarla (500ms)
            _broadcastTimer = new System.Windows.Forms.Timer();
            _broadcastTimer.Interval = 500;
            _broadcastTimer.Tick += BroadcastTimer_Tick;
        }

        public void StartStream(int machineId, string plcIp, string password)
        {
            StopStream();

            _currentMachineId = machineId;
            _password = password;

            try
            {
                // VncSharp Başlat
                // Connect(IP, Display, ViewOnly)
                // ViewOnly parametresi yoksa: _vncEngine.Connect(plcIp, 0); yazın.
                _vncEngine.Connect(plcIp, 0, true);

                // Yayını Başlat
                _broadcastTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNC Başlatma Hatası: {ex.Message}");
            }
        }

        public void StopStream()
        {
            _broadcastTimer.Stop();
            if (_vncEngine != null && _vncEngine.IsConnected)
            {
                _vncEngine.Disconnect();
            }
        }

        private async void BroadcastTimer_Tick(object sender, EventArgs e)
        {
            if (!_vncEngine.IsConnected || _gateway == null) return;

            try
            {
                // Güvenli dönüşüm
                Bitmap screen = _vncEngine.Desktop as Bitmap;

                if (screen == null && _vncEngine.Desktop != null)
                {
                    screen = new Bitmap(_vncEngine.Desktop);
                }

                if (screen != null)
                {
                    string base64Image = ImageToBase64(screen);
                    await _gateway.SendScreenImageAsync(_currentMachineId, base64Image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Resim Hatası: {ex.Message}");
            }
        }

        private string ImageToBase64(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

                lock (bmp)
                {
                    using (var clone = new Bitmap(bmp))
                    {
                        clone.Save(ms, jpgEncoder, encoderParams);
                    }
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == format.Guid) return codec;
            }
            return null;
        }
    }
}