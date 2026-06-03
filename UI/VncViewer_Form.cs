// UI/VncViewer_Form.cs
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using VncSharpCore; // RemoteDesktop sınıfı için
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm bileşenleri için

namespace Telemetry.UI
{
    // Form yerine MaterialForm sınıfından türetiyoruz
    public partial class VncViewer_Form : MaterialForm
    {
        private readonly string _address;
        private readonly string _password;
        private int _port = 5900; // Varsayılan port
        private bool _isClosingInitiated = false; // Form kapatma işleminin başlatılıp başlatılmadığını izle

        public VncViewer_Form(string address, string password)
        {
            InitializeComponent();

            // =========================================================================
            // MATERIALSKIN FORM ENTEGRASYONU VE CANLI AKIŞ PERFORMANS AYARLARI
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this); // Formu merkezi temaya bağla

            // CANLI YAYIN OPTİMİZASYONU: VNC üzerinden gelen pikseller ekrana basılırken 
            // arayüzün titremesini, yırtılmasını (tearing) ve donmasını tamamen engeller.
            this.DoubleBuffered = true;

            // Adres ve Port ayrıştırma işlemi
            if (address.Contains(":"))
            {
                var parts = address.Split(':');
                _address = parts[0];
                if (parts.Length > 1 && !int.TryParse(parts[1], out _port))
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Invalid port number detected: '{parts[1]}'. The default port (5900) will be used.");
                    MessageBox.Show("Invalid port number. The default port (5900) will be used.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _port = 5900;
                }
            }
            else
            {
                _address = address;
            }

            _password = password;

            // Olayları (events) bağlayalım
            remoteDesktop1.ConnectComplete += VncControl_ConnectComplete;
            remoteDesktop1.ConnectionLost += VncControl_ConnectionLost;
            remoteDesktop1.GetPassword = () => _password; // Şifre delegate'ini burada ayarla
        }

        private async void VncViewer_Form_Load(object sender, EventArgs e)
        {
            this.Text = $"{_address}:{_port} - Connecting...";

            // UX DÜZELTMESİ: VNC ekranı bağlanana kadar arka planda beyaz boşluk kalmaması için
            // tuval (canvas) rengini asenkron olarak koyu gri/siyah yapıyoruz.
            remoteDesktop1.BackColor = System.Drawing.Color.FromArgb(17, 24, 39);

            try
            {
                // Bağlantıyı arka planda thread kilitlemeden (Non-blocking) başlatıyoruz
                await Task.Run(() => remoteDesktop1.Connect(_address));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNC connection initialization error: {ex.Message}");
                MessageBox.Show($"Error initializing VNC connection: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                _isClosingInitiated = true;
                this.Close();
            }
        }

        private void VncControl_ConnectComplete(object sender, ConnectEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => VncControl_ConnectComplete(sender, e)));
                return;
            }
            this.Text = $"{_address}:{_port} - Connected: {e.DesktopName}";
        }

        private void VncControl_ConnectionLost(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => VncControl_ConnectionLost(sender, e)));
                return;
            }

            System.Diagnostics.Debug.WriteLine("VNC connection was interrupted or lost.");
            MessageBox.Show("VNC connection was interrupted or lost.", "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (!_isClosingInitiated)
            {
                _isClosingInitiated = true;
                this.Close();
            }
        }

        private void VncViewer_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isClosingInitiated && e.CloseReason != CloseReason.None)
            {
                return;
            }

            _isClosingInitiated = true;

            // GÜVENLİK OPTİMİZASYONU: VNC bağlantısı koparılırken formun tamamen bellekten 
            // silinmesini garantiye almak için olay aboneliklerini en başta temizliyoruz
            remoteDesktop1.ConnectComplete -= VncControl_ConnectComplete;
            remoteDesktop1.ConnectionLost -= VncControl_ConnectionLost;

            if (remoteDesktop1.IsConnected)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("VncViewer_Form_FormClosing: Disconnecting...");
                    remoteDesktop1.Disconnect();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"VncViewer_Form_FormClosing: Error while disconnecting: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("VncViewer_Form_FormClosing: The connection is already disconnected.");
            }
        }
    }
}