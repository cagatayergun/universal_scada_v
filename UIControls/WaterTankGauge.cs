// UIControls/WaterTankGauge.cs -> UIControls/ResourceGauge.cs (YENİ İSİM)
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Namespace'i kendi projenize göre düzenleyebilirsiniz.
namespace Universalscada.UI.Controls
{
    // COMMENT UPDATE: Su veya diğer akışkan seviyelerini göstermek yerine, 
    // genel olarak bir Kaynak Seviyesini/Miktarını görselleştiren bir kontrol haline getirildi.
    public partial class ResourceGauge : UserControl // Sınıf adı güncellendi
    {
        private int _value = 0;
        private int _maximum = 5000;
        // Başlık ve birimi genel terimlerle değiştirildi
        private string _title = "RESOURCE AMOUNT";
        private string _unit = "UNIT";
        private Color _resourceColor = Color.FromArgb(52, 152, 219); // Varsayılan mavi korundu

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(0, Math.Min(_maximum, value));
                this.Invalidate(); // Değer değiştiğinde kontrolü yeniden çizdir.
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

        // Yeni/Güncellenmiş özellikler:
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                this.Invalidate();
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                this.Invalidate();
            }
        }

        public Color ResourceColor
        {
            get => _resourceColor;
            set
            {
                _resourceColor = value;
                this.Invalidate();
            }
        }

        public ResourceGauge() // Sınıf adı güncellendi
        {
            // InitializeComponent(); // Designer kodu olmadan yorumda bırakıldı
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int panelWidth = this.Width;
            int panelHeight = this.Height;
            int padding = 10;
            // Alttaki metinler için 40px boşluk bırak
            RectangleF tankBounds = new RectangleF(padding, padding, panelWidth - 2 * padding, panelHeight - 2 * padding - 40);

            // 1. Tankın dış çerçevesini çiz
            using (Pen tankOutlinePen = new Pen(Color.FromArgb(100, 100, 100), 3))
            {
                g.DrawEllipse(tankOutlinePen, tankBounds);
            }

            if (_value > 0)
            {
                // 2. Kaynak yüksekliğini hesapla
                float doluYukseklik = tankBounds.Height * ((float)_value / _maximum);

                // 3. Kaynak için bir GraphicsPath oluştur
                using (GraphicsPath tankPath = new GraphicsPath())
                {
                    tankPath.AddEllipse(tankBounds);

                    // 4. Kaynak dikdörtgenini oluştur
                    RectangleF resourceRectangle = new RectangleF(
                        tankBounds.X,
                        tankBounds.Y + tankBounds.Height - doluYukseklik,
                        tankBounds.Width,
                        doluYukseklik
                    );

                    // 5. Elips ile kaynak dikdörtgeninin kesişimini al
                    Region resourceRegion = new Region(resourceRectangle);
                    resourceRegion.Intersect(tankPath);

                    // 6. Kesişim bölgesini dinamik renkle doldur
                    using (SolidBrush resourceBrush = new SolidBrush(_resourceColor)) // Renk dinamikleştirildi
                    {
                        g.FillRegion(resourceBrush, resourceRegion);
                    }
                    resourceRegion.Dispose();
                }
            }

            // 7. Metinleri çiz
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Değer Metni 
                RectangleF valueRect = new RectangleF(0, tankBounds.Bottom, panelWidth, 25);
                using (Font valueFont = new Font("Segoe UI", 12F, FontStyle.Bold))
                {
                    g.DrawString($"{_value} {_unit}", valueFont, Brushes.Black, valueRect, sf);
                }

                // Başlık Metni 
                RectangleF titleRect = new RectangleF(0, tankBounds.Bottom + 20, panelWidth, 20);
                using (Font titleFont = new Font("Segoe UI", 9F))
                {
                    g.DrawString(_title, titleFont, Brushes.Gray, titleRect, sf);
                }
            }
        }
    }
}