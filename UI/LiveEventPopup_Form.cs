// UI/LiveEventPopup_Form.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using TekstilScada.Services;

namespace TekstilScada.UI
{
    public partial class LiveEventPopup_Form : Form
    {
        private const int MAX_EVENTS = 100; // Ekranda gösterilecek maksimum olay sayısı

        public LiveEventPopup_Form()
        {
            InitializeComponent();

            try
            {
                // Olay dinleyicisine abone ol
                LiveEventAggregator.Instance.OnEventPublished += OnNewEventPublished;
            }
            catch (Exception ex)
            {
                //($"Event aboneliği hatası: {ex.Message}");
            }
        }

        private void OnNewEventPublished(LiveEvent liveEvent)
        {
            try
            {
                // 1. GÜVENLİK KONTROLÜ: Form kapandıysa veya handle oluşmadıysa işlem yapma.
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
                //($"OnNewEventPublished hatası: {ex.Message}");
            }
        }

        private void AddEventToList(LiveEvent liveEvent)
        {
            try
            {
                // UI elemanına erişmeden önce tekrar kontrol (Thread güvenliği için)
                if (lstEvents.IsDisposed) return;

                // Performans için BeginUpdate kullanıyoruz (Titreşimi önler)
                lstEvents.BeginUpdate();

                var item = new ListViewItem(liveEvent.Timestamp.ToString("HH:mm:ss"));
                item.SubItems.Add(liveEvent.Source ?? "-"); // Null check eklendi
                item.SubItems.Add(liveEvent.Message ?? "-");

                // Olay türüne göre renklendir
                switch (liveEvent.Type)
                {
                    case EventType.Alarm:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(192, 57, 43); // Kırmızı
                        break;
                    case EventType.Process:
                        item.ForeColor = Color.Black;
                        item.BackColor = Color.FromArgb(178, 235, 242); // Açık Mavi
                        break;
                    case EventType.SystemSuccess:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(39, 174, 96); // Yeşil
                        break;
                    case EventType.SystemWarning:
                        item.ForeColor = Color.White;
                        item.BackColor = Color.FromArgb(243, 156, 18); // Turuncu
                        break;
                    default:
                        item.ForeColor = Color.Black;
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
                //($"Listeye ekleme hatası: {ex.Message}");
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

        // GÜNCELLENDİ: Formu kapatmak yerine gizle
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
                //($"Form gizleme hatası: {ex.Message}");
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
                //($"Unsubscribe hatası: {ex.Message}");
            }
            finally
            {
                base.OnFormClosed(e);
            }
        }
    }
}