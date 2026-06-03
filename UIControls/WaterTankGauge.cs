// WaterTankGauge.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Telemetry.UI.Controls
{
    public partial class WaterTankGauge : UserControl
    {
        private int _value = 0;
        private int _maximum = 5000;
        private string _title = "AMOUNT OF WATER";
        private string _unit = "L";

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(0, Math.Min(_maximum, value));
                this.Invalidate(); // Değer değiştiğinde kontrolü asenkron olarak yeniden çizdir.
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                this.Invalidate();
            }
        }

        public WaterTankGauge()
        {
            InitializeComponent();

            // UX OPTMİZASYONU: Kartın arkasında beyaz kare kutular kalmaması için şeffaflığı açıyoruz
            this.BackColor = Color.Transparent;

            // Daha akıcı çizimler ve sıfır titreme (flickering) için donanımsal DoubleBuffering'i etkinleştir.
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Eğrisel kenarları pürüzsüzleştir (Anti-Aliasing)

            int panelWidth = this.Width;
            int panelHeight = this.Height;
            int padding = 10;

            // Alttaki metinler için 40px boşluk bırak
            RectangleF tankBounds = new RectangleF(padding, padding, panelWidth - 2 * padding, panelHeight - 2 * padding - 40);

            // 1. Tankın dış çerçevesini çiz (Koyu Mode uyumlu Slate Gray)
            using (Pen tankOutlinePen = new Pen(Color.FromArgb(74, 85, 104), 3))
            {
                g.DrawEllipse(tankOutlinePen, tankBounds);
            }

            if (_value > 0)
            {
                // 2. Suyun yüksekliğini hesapla
                float doluYukseklik = tankBounds.Height * ((float)_value / _maximum);

                // 3. Su için bir GraphicsPath oluştur
                using (GraphicsPath tankPath = new GraphicsPath())
                {
                    tankPath.AddEllipse(tankBounds);

                    // 4. Suyu temsil eden dikdörtgeni oluştur
                    RectangleF suDikdortgeni = new RectangleF(
                        tankBounds.X,
                        tankBounds.Y + tankBounds.Height - doluYukseklik,
                        tankBounds.Width,
                        doluYukseklik
                    );

                    // 5. Elips ile su dikdörtgeninin kesişimini al
                    using (Region suBolgesi = new Region(suDikdortgeni))
                    {
                        suBolgesi.Intersect(tankPath);

                        // 6. Kesişim bölgesini yumuşak mavi renkle doldur (Material Blue)
                        using (SolidBrush suBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
                        {
                            g.FillRegion(suBrush, suBolgesi);
                        }
                    }
                }
            }

            // 7. Metinleri çiz
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // =========================================================================
                // MODERNİZASYON: KOYU MOD YAZI VE BELLEK OPTİMİZASYONU
                // Brushes.Black yerine koyu zeminde parlayan soft beyaz fırçalar kullanıldı.
                // =========================================================================

                // Değer Metni (Örn: "1234 L")
                RectangleF valueRect = new RectangleF(0, tankBounds.Bottom + 4, panelWidth, 25);
                using (SolidBrush valueBrush = new SolidBrush(Color.FromArgb(240, 240, 240))) // Parlak Açık Gri
                using (Font valueFont = new Font("Segoe UI Semibold", 12F, FontStyle.Bold))
                {
                    g.DrawString($"{_value} {_unit}", valueFont, valueBrush, valueRect, sf);
                }

                // Başlık Metni (Örn: "SU MİKTARI")
                RectangleF titleRect = new RectangleF(0, tankBounds.Bottom + 24, panelWidth, 20);
                using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(144, 164, 174))) // Soluk Mat Mavi-Gri
                using (Font titleFont = new Font("Segoe UI", 8.5F, FontStyle.Regular))
                {
                    g.DrawString(_title, titleFont, titleBrush, titleRect, sf);
                }
            }
        }
    }
}