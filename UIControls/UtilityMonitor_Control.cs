using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;

namespace TekstilScada.UIControls
{
    public partial class UtilityMonitor_Control : UserControl
    {
        private UtilityPollingService _service;
        private UtilityRepository _repository;

        // Hangi hat ID'si hangi UI bileşenlerini (Label'ları) tutuyor?
        private Dictionary<int, UtilityCardComponents> _uiMap = new Dictionary<int, UtilityCardComponents>();

        // Kart üzerindeki güncellenecek nesnelerin referanslarını tutan sınıf
        private class UtilityCardComponents
        {
            public Label LblStatus { get; set; }
            public Panel PnlStatusColor { get; set; }
            public Label LblLastUpdate { get; set; }

            public Label LblAirVal { get; set; }
            public Label LblElecVal { get; set; }
            public Label LblSteamVal { get; set; }
         //   public Label LblAirVal { get; set; }
        }

        public UtilityMonitor_Control()
        {
            InitializeComponent();
        }

        // Ana formdan çağrılacak başlatma metodu
        public void InitializeControl(UtilityPollingService service, UtilityRepository repo)
        {
            _service = service;
            _repository = repo;

            // Event'e abone ol (Canlı veri akışı için)
            if (_service != null)
            {
                _service.OnUtilityDataRefreshed += Service_OnUtilityDataRefreshed;
            }

            // İlk açılışta kartları oluştur
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            if (_repository == null) return;

            // UI thread güvenliği
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshLayout));
                return;
            }

            flowLayoutPanelCards.SuspendLayout();
            flowLayoutPanelCards.Controls.Clear();
            _uiMap.Clear();

            var lines = _repository.GetUtilityLines();

            foreach (var line in lines)
            {
                CreateCard(line);
            }

            flowLayoutPanelCards.ResumeLayout();
        }

        private void CreateCard(UtilityLine line)
        {
            // --- KART GÖVDESİ ---
            Panel card = new Panel
            {
                Size = new Size(260, 200),
                BackColor = Color.White,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // --- BAŞLIK KISMI ---
            Panel pnlTitle = new Panel { Dock = DockStyle.Top, Height = 35, BackColor = Color.SteelBlue };
            Label lblTitle = new Label
            {
                Text = line.LineName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Durum İkonu (Sağ üst köşe)
            Panel pnlStatus = new Panel
            {
                Size = new Size(15, 15),
                BackColor = Color.Gray, // Başlangıçta gri (veri gelince yeşil olacak)
                Location = new Point(235, 10)
            };
            // Yuvarlak yapmak için Region ayarı (Basit kare kalsın performans için veya Region eklenebilir)

            pnlTitle.Controls.Add(pnlStatus); // Önce ekle ki label üstüne binmesin
            pnlTitle.Controls.Add(lblTitle);
            card.Controls.Add(pnlTitle);

            // --- DEĞERLER KISMI ---
            // Dinamik olarak sadece aktif (Enabled) olan sensörleri ekleyeceğiz.
            int currentY = 45;
            var comps = new UtilityCardComponents();
            comps.PnlStatusColor = pnlStatus;

            // 1. Su Sayacı
            if (line.AirEnabled)
            {
                comps.LblAirVal = AddRow(card, "Su Tüketimi:", "m³", Color.DarkBlue, ref currentY);
            }
            // 2. Elektrik Sayacı
            if (line.ElecEnabled)
            {
                comps.LblElecVal = AddRow(card, "Elektrik:", "kWh", Color.Red, ref currentY);
            }
            // 3. Buhar Sayacı
            if (line.SteamEnabled)
            {
                comps.LblSteamVal = AddRow(card, "Buhar:", "kg", Color.DarkOrange, ref currentY);
            }
            // 4. Hava Sayacı
            if (line.AirEnabled)
            {
                comps.LblAirVal = AddRow(card, "Hava:", "m³", Color.Teal, ref currentY);
            }

            // --- ALT BİLGİ (Zaman) ---
            Label lblTime = new Label
            {
                Text = "--:--:--",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(10, 175)
            };
            comps.LblLastUpdate = lblTime;
            card.Controls.Add(lblTime);

            // Dictionary'e kaydet
            _uiMap[line.Id] = comps;
            flowLayoutPanelCards.Controls.Add(card);
        }

        private Label AddRow(Panel parent, string title, string unit, Color valColor, ref int yPos)
        {
            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            Label lblVal = new Label
            {
                Text = "0.00",
                Location = new Point(110, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = valColor
            };

            Label lblUnit = new Label
            {
                Text = unit,
                Location = new Point(200, yPos + 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            parent.Controls.Add(lblTitle);
            parent.Controls.Add(lblVal);
            parent.Controls.Add(lblUnit);

            yPos += 25; // Bir sonraki satır için aşağı kaydır
            return lblVal;
        }

        // --- VERİ GÜNCELLEME ---
        private void Service_OnUtilityDataRefreshed(List<UtilityLog> logs)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            // UI Thread'e geçiş
            this.Invoke(new Action(() =>
            {
                foreach (var log in logs)
                {
                    if (_uiMap.TryGetValue(log.LineId, out var ui))
                    {
                        // Değerleri güncelle
                        if (ui.LblAirVal != null) ui.LblAirVal.Text = log.AirCounter.ToString("N2");
                        if (ui.LblElecVal != null) ui.LblElecVal.Text = log.ElecCounter.ToString("N2");
                        if (ui.LblSteamVal != null) ui.LblSteamVal.Text = log.SteamCounter.ToString("N2");
                        if (ui.LblAirVal != null) ui.LblAirVal.Text = log.AirCounter.ToString("N2");

                        // Zaman ve Durum
                        ui.LblLastUpdate.Text = log.LogTime.ToString("HH:mm:ss");
                        ui.PnlStatusColor.BackColor = Color.LimeGreen; // Veri geldiyse yeşil yap
                    }
                    else
                    {
                        // Eğer listede olmayan bir ID geldiyse (yeni eklenmiş olabilir), arayüzü yenile
                        // Ancak her saniye yenilememesi için bir flag kontrolü yapılabilir.
                        // Şimdilik basit bırakıyoruz.
                    }
                }
            }));
        }

        private void DetachEvents()
        {
            if (_service != null)
            {
                _service.OnUtilityDataRefreshed -= Service_OnUtilityDataRefreshed;
            }
        }
    }
}