using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms; // Timer için gerekli
using TekstilScada.Core.Services; // SignalRGatewayService
using TekstilScada.Services;
using VncSharpCore; // VncSharpCore.dll
using System.Reflection;
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
                _vncEngine.Connect(plcIp, 0, false);

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
        public void SendClick(int x, int y)
        {
            if (_vncEngine == null || !_vncEngine.IsConnected) return;

            try
            {
                // 1. ADIM: Gizli protokol nesnesini bulmaya çalış (İsim 'vnc' veya 'rfb' olabilir)
                object protocolObject = null;
                Type engineType = typeof(RemoteDesktop);

                // Önce 'vnc' ismini dene (VncSharp standart ismi)
                FieldInfo field = engineType.GetField("vnc", BindingFlags.Instance | BindingFlags.NonPublic);

                // Bulamazsa 'rfb' ismini dene
                if (field == null)
                    field = engineType.GetField("rfb", BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null)
                {
                    protocolObject = field.GetValue(_vncEngine);
                }

                if (protocolObject != null)
                {
                    // 2. ADIM: WritePointerEvent metodunu bul
                    MethodInfo writeMethod = protocolObject.GetType().GetMethod("WritePointerEvent");

                    if (writeMethod != null)
                    {
                        // 3. ADIM: Parametre sayısına göre çağırma (Overload kontrolü)
                        ParameterInfo[] parameters = writeMethod.GetParameters();

                        // Durum A: Metot 3 parametre alıyorsa -> (int buttonMask, int x, int y)
                        if (parameters.Length == 3)
                        {
                            // Tıklama: Bas (1) ve Bırak (0)
                            writeMethod.Invoke(protocolObject, new object[] { 1, x, y });
                            writeMethod.Invoke(protocolObject, new object[] { 0, x, y });
                            Console.WriteLine($"[VNC] Tıklandı (3 param): {x},{y}");
                        }
                        // Durum B: Metot 2 parametre alıyorsa -> (byte buttonMask, Point p)
                        else if (parameters.Length == 2)
                        {
                            writeMethod.Invoke(protocolObject, new object[] { (byte)1, new Point(x, y) });
                            writeMethod.Invoke(protocolObject, new object[] { (byte)0, new Point(x, y) });
                            Console.WriteLine($"[VNC] Tıklandı (2 param): {x},{y}");
                        }
                    }
                    else
                    {
                        // Metot bulunamadıysa logla
                        Console.WriteLine("[HATA] 'WritePointerEvent' metodu bulunamadı!");
                    }
                }
                else
                {
                    Console.WriteLine("[HATA] Protokol nesnesi ('vnc' veya 'rfb') bulunamadı!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HATA] Tıklama işlemi başarısız: {ex.Message}");
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