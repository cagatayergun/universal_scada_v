// UI/Controls/KpiCard_Control.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Telemetry.UI.Controls
{
    public partial class KpiCard_Control : UserControl
    {
        public KpiCard_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Değerler hızlı akarken kart çizimlerinin titremesini (Flickering) tamamen engeller
            this.DoubleBuffered = true;
        }

        public void SetData(string title, string value, Color backgroundColor)
        {
            // Thread-safe arayüz koruması
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetData(title, value, backgroundColor)));
                return;
            }

            lblKpiTitle.Text = title;
            lblKpiValue.Text = value;
            this.BackColor = backgroundColor;

            // =========================================================================
            // UX MODERNİZASYON: AKILLI METİN KONTRAST ALGORİTMASI (W3C Standartı)
            // Gelen rengin ışık yoğunluğunu (Luminance) hesaplayarak metin rengini seçer.
            // Bu sayede açık renklerde yazı Siyah, koyu renklerde otomatik Beyaz olur.
            // =========================================================================
            try
            {
                double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;

                // Eğer arka plan açık bir renkse (Luminance > 0.5) yazıyı Koyu Füme yap, değilse Beyaz bırak
                Color textColor = luminance > 0.5 ? Color.FromArgb(33, 33, 33) : Color.White;

                lblKpiTitle.ForeColor = textColor;
                lblKpiValue.ForeColor = textColor;
            }
            catch (Exception)
            {
                // Hata durumunda güvenli varsayılan olarak beyaz metinde kal
                lblKpiTitle.ForeColor = Color.White;
                lblKpiValue.ForeColor = Color.White;
            }
        }
    }
}