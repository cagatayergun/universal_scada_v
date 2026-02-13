using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TekstilScada.Core.Services; // SignalRGatewayService namespace'iniz
using TekstilScada.Services;
using VncSharpCore; // VncSharp kütüphanesi

namespace TekstilScada
{
    public class VncProxyServer
    {
        // Temel Bileşenler
        private RemoteDesktop _vncEngine;
        private SignalRGatewayService _gateway;
        private int _currentMachineId;
        private string _password;

        // Zamanlayıcılar
        private System.Windows.Forms.Timer _broadcastTimer; // Görüntü gönderme zamanlayıcısı
        private System.Windows.Forms.Timer _watchdogTimer;  // Otomatik kapanma zamanlayıcısı (Heartbeat takibi)

        // Son Aktivite Zamanı (Kapanma kontrolü için)
        private DateTime _lastActivityTime;
        private const int TIMEOUT_SECONDS = 15; // 15 saniye ses çıkmazsa kapat

        public VncProxyServer(SignalRGatewayService gateway)
        {
            _gateway = gateway;

            // VNC Motorunu Başlat
            _vncEngine = new RemoteDesktop();
            _vncEngine.VncPort = 5900;

            // Olaylar
            _vncEngine.ConnectComplete += (s, e) => Console.WriteLine($"[VNC] {_currentMachineId} Bağlandı.");
            _vncEngine.ConnectionLost += (s, e) => Console.WriteLine($"[VNC] {_currentMachineId} Bağlantı Koptu.");

            // 1. Timer: Görüntü Yayını (500ms)
            _broadcastTimer = new System.Windows.Forms.Timer();
            _broadcastTimer.Interval = 500;
            _broadcastTimer.Tick += BroadcastTimer_Tick;

            // 2. Timer: Watchdog / Zaman Aşımı (1000ms)
            _watchdogTimer = new System.Windows.Forms.Timer();
            _watchdogTimer.Interval = 1000;
            _watchdogTimer.Tick += WatchdogTimer_Tick;
        }

        // --- BAŞLATMA VE DURDURMA ---

        public void StartStream(int machineId, string plcIp, string password)
        {
            // Önceki yayın varsa temizle
            StopStream();

            _currentMachineId = machineId;
            _password = password;

            try
            {
                // Aktivite zamanını sıfırla (Şu an başlattık)
                UpdateActivity();

                // VNC Bağlantısını Kur (ViewOnly = false yani kontrol edilebilir)
                // Not: Bazı VncSharp sürümlerinde GetPasswordHandler gerekebilir, 
                // ancak genelde Connect içinde veya otomatik halledilir.
                _vncEngine.GetPassword = () => _password;
                _vncEngine.Connect(plcIp, 0, false);

                // Zamanlayıcıları Başlat
                _broadcastTimer.Start();
                _watchdogTimer.Start();

                Console.WriteLine($"[VNC] Yayın başlatıldı: {plcIp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNC HATA] Başlatılamadı: {ex.Message}");
            }
        }

        public void StopStream()
        {
            // Zamanlayıcıları Durdur
            _broadcastTimer.Stop();
            _watchdogTimer.Stop();

            // VNC Bağlantısını Kes
            if (_vncEngine != null && _vncEngine.IsConnected)
            {
                try
                {
                    _vncEngine.Disconnect();
                }
                catch { }
            }
            Console.WriteLine("[VNC] Yayın durduruldu.");
        }

        // --- WATCHDOG / OTOMATİK KAPANMA MANTIĞI ---

        // Web tarafından her Click veya Heartbeat geldiğinde çağrılır
        public void UpdateActivity()
        {
            _lastActivityTime = DateTime.Now;
        }

        // Web'den gelen sadece "Ben Buradayım" sinyalini işler
        public void ProcessHeartbeat()
        {
            UpdateActivity();
            // Console.WriteLine("[VNC] Heartbeat alındı."); // Log kirliliği yapmaması için kapalı
        }

        // Her saniye çalışır, zaman aşımını kontrol eder
        private void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            var elapsed = (DateTime.Now - _lastActivityTime).TotalSeconds;

            if (elapsed > TIMEOUT_SECONDS)
            {
                Console.WriteLine($"[VNC WATCHDOG] {TIMEOUT_SECONDS} saniyedir işlem yok. Otomatik kapatılıyor...");
                StopStream();
            }
        }

        // --- GÖRÜNTÜ YAYINI ---

        private async void BroadcastTimer_Tick(object sender, EventArgs e)
        {
            if (!_vncEngine.IsConnected || _gateway == null) return;

            try
            {
                // Ekran görüntüsünü al (Thread-safe erişim gerekebilir, VncSharp genelde Desktop property'si sunar)
                Bitmap screen = _vncEngine.Desktop as Bitmap;

                // Bazen Desktop null gelebilir veya cast edilemeyebilir
                if (screen == null && _vncEngine.Desktop != null)
                {
                    try { screen = new Bitmap(_vncEngine.Desktop); } catch { }
                }

                if (screen != null)
                {
                    // Resmi Base64'e çevir
                    string base64Image = ImageToBase64(screen);

                    // Gateway üzerinden API'ye (Hub'a) gönder
                    await _gateway.SendScreenImageAsync(_currentMachineId, base64Image);
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda yayını kesme, bir sonraki kareyi dene
                Console.WriteLine($"[VNC YAYIN HATA] {ex.Message}");
            }
        }

        private string ImageToBase64(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // JPEG Sıkıştırma (Performans için Kalite %50)
                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

                // Bitmap kullanımı sırasında kilitlenme olmaması için lock
                lock (bmp)
                {
                    // Clone alarak asıl bitmap'i kitlemeden işleyelim
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

        // --- TIKLAMA GÖNDERME (REFLECTION İLE) ---

        public void SendClick(int x, int y)
        {
            // Önce aktiviteyi güncelle (Bu bir yaşam belirtisidir)
            UpdateActivity();

            if (_vncEngine == null || !_vncEngine.IsConnected) return;

            try
            {
                // VncSharpCore içindeki 'WritePointerEvent' metodu genelde private/protected veya
                // bir alt nesne (rfb/vnc protocol object) içindedir. Reflection ile erişiyoruz.

                // 1. Protokol nesnesini bul
                object protocolObject = null;
                Type engineType = typeof(RemoteDesktop);

                // 'vnc' veya 'rfb' field'ını ara
                FieldInfo field = engineType.GetField("vnc", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null) field = engineType.GetField("rfb", BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null)
                {
                    protocolObject = field.GetValue(_vncEngine);
                }

                if (protocolObject != null)
                {
                    // 2. Metodu bul
                    MethodInfo writeMethod = protocolObject.GetType().GetMethod("WritePointerEvent");

                    if (writeMethod != null)
                    {
                        ParameterInfo[] parameters = writeMethod.GetParameters();

                        // 3. Parametre sayısına göre çağır
                        // Sol Tık Bas (1) -> Sol Tık Bırak (0)

                        if (parameters.Length == 3) // (mask, x, y)
                        {
                            writeMethod.Invoke(protocolObject, new object[] { 1, x, y });
                            writeMethod.Invoke(protocolObject, new object[] { 0, x, y });
                        }
                        else if (parameters.Length == 2) // (mask, Point)
                        {
                            writeMethod.Invoke(protocolObject, new object[] { (byte)1, new Point(x, y) });
                            writeMethod.Invoke(protocolObject, new object[] { (byte)0, new Point(x, y) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNC CLICK HATA] {ex.Message}");
            }
        }
    }
}