// UI/LiveEventPopup_Form.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using Telemetry.Services;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm bileşenleri için

namespace Telemetry.UI
{
    // Form yerine MaterialForm'dan türetiyoruz
    public partial class LiveEventPopup_Form : MaterialForm
    {
        private const int MAX_EVENTS = 100; // Ekranda gösterilecek maksimum olay sayısı

        public LiveEventPopup_Form()
        {
            InitializeComponent();

            // =========================================================================
            // MATERIALSKIN ENTEGRASYONU VE PERFORMANS AYARLARI
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this); // Formu temalandırma motoruna bağla

            this.DoubleBuffered = true; // Formun kendi çizim kırpışmalarını engelle

            // SPEED OPTİMİZASYON: Satır eklendikçe ListView'de oluşan beyaz dalgalanma/titremeyi önler
            if (lstEvents != null)
            {
                EnableDoubleBuffer(lstEvents);
            }

            try
            {
                // Olay dinleyicisine abone ol
                LiveEventAggregator.Instance.OnEventPublished += OnNewEventPublished;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Event aboneliği hatası: {ex.Message}");
            }
        }

        private void OnNewEventPublished(LiveEvent liveEvent)
        {
            try
            {
                // GÜVENLİK KONTROLÜ: Form kapandıysa veya handle oluşmadıysa işlem yapma.
                if (this.IsDisposed || !this.IsHandleCreated) return;

                // Bu olay arka plan thread'inden gelebilir, bu yüzden Invoke kullanmak zorunludur.
                if (this.InvokeRequired)
                {
                    // Invoke işlemini de try-catch içinde yapmak, form kapanırken oluşan yarış durumlarını (race condition) engeller.
                    try
                    {
                        this.Invoke(new Action(() => AddEventToList(liveEvent)));
                    }
                    catch (ObjectDisposedException)
                    {
                        // Form invoke sırasında kapandıysa bu hatayı yutabiliriz.
                    }
                    return;
                }

                AddEventToList(liveEvent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnNewEventPublished hatası: {ex.Message}");
            }
        }

        private void AddEventToList(LiveEvent liveEvent)
        {
            try
            {
                // UI elemanına erişmeden önce tekrar kontrol (Thread güvenliği için)
                if (lstEvents.IsDisposed) return;

                // Performans için BeginUpdate kullanıyoruz (Arayüz kilitlenmesini önler)
                lstEvents.BeginUpdate();

                var item = new ListViewItem(liveEvent.Timestamp.ToString("HH:mm:ss"));
                item.SubItems.Add(liveEvent.Source ?? "-"); // Null check
                item.SubItems.Add(liveEvent.Message ?? "-");

                // =========================================================================
                // MODERNİZASYON: GOOGLE MATERIAL DARK THEME UYUMLU SATIR RENKLERİ
                // Eski çiğ renkler yerine Dark Mode arka planında gözü yormayan pastel tonlar seçildi.
                // =========================================================================
                switch (liveEvent.Type)
                {
                    case EventType.Alarm:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(183, 28, 28); // Material Red 900 (Koyu Kırmızı)
                        break;
                    case EventType.Process:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(0, 121, 107); // Material Teal 700 (Koyu Camgöbeği)
                        break;
                    case EventType.SystemSuccess:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(46, 125, 50); // Material Green 800 (Yumuşak Yeşil)
                        break;
                    case EventType.SystemWarning:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(230, 81, 0); // Material Orange 900 (Yumuşak Turuncu)
                        break;
                    default:
                        // Varsayılan satırlarda Dark Mode uyumu için yazı rengi açık gri/beyaz kalmalı
                        item.ForeColor = Color.FromArgb(220, 220, 220);
                        break;
                }

                // Yeni olayı listenin en üstüne ekle
                lstEvents.Items.Insert(0, item);

                // Eğer liste çok uzarsa, en eski olayı sil
                if (lstEvents.Items.Count > MAX_EVENTS)
                {
                    lstEvents.Items.RemoveAt(MAX_EVENTS);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Listeye ekleme hatası: {ex.Message}");
            }
            finally
            {
                // Hata olsa bile çizimi bitirmemiz lazım, yoksa liste donuk kalır.
                if (!lstEvents.IsDisposed)
                {
                    lstEvents.EndUpdate();
                }
            }
        }

        // Formu kapatmak yerine gizle
        private void LiveEventPopup_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Kullanıcı çarpıya bastıysa (UserClosing), formu kapatma sadece gizle.
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Form gizleme hatası: {ex.Message}");
            }
        }

        // Form tamamen kapatıldığında (Application Exit vb.) olay dinleyicisinden aboneliği kaldır
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (LiveEventAggregator.Instance != null)
                {
                    LiveEventAggregator.Instance.OnEventPublished -= OnNewEventPublished;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unsubscribe hatası: {ex.Message}");
            }
            finally
            {
                base.OnFormClosed(e);
            }
        }

        // SPEED OPTİMİZASYON: Kontrolün korumalı (protected) DoubleBuffered özelliğini aktif eden yardımcı metot
        private void EnableDoubleBuffer(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }
}