using System;
using System.Drawing;
using System.Windows.Forms;
using Telemetry.Models;

namespace Telemetry.UI.Controls
{
    public partial class AlarmBanner_Control : UserControl
    {
        public AlarmBanner_Control()
        {
            InitializeComponent();
            this.Visible = false; // Başlangıçta gizli

            // HIZ OPTİMİZASYONU: Alarm geçişlerinde ve yanıp sönmelerde arayüzün titremesini (Flickering) tamamen engeller
            this.DoubleBuffered = true;

            // Label'a yapılan tıklamaların, banner'ın kendi Click olayını tetiklemesini sağla.
            lblAlarmText.Click += (sender, e) => this.OnClick(e);
        }

        public void ShowAlarm(string machineName, AlarmDefinition alarmDef)
        {
            // Thread-safe kontrolü
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowAlarm(machineName, alarmDef)));
                return;
            }

            if (alarmDef == null)
            {
                HideBanner();
                return;
            }

            lblAlarmText.Text = $"[{machineName}] - ALARM #{alarmDef.AlarmNumber}: {alarmDef.AlarmText}";

            // =========================================================================
            // MODERNİZASYON: GOOGLE MATERIAL DESIGN ALARM & KONTRAST PALETİ
            // =========================================================================
            switch (alarmDef.Severity)
            {
                case 4: // Kritik Alarm
                    this.BackColor = Color.FromArgb(183, 28, 28); // Material Red 900 (Koyu Kırmızı)
                    lblAlarmText.ForeColor = Color.White;
                    break;

                case 3: // Yüksek Önemli Alarm
                    this.BackColor = Color.FromArgb(229, 57, 53); // Material Red 600 (Canlı Kırmızı)
                    lblAlarmText.ForeColor = Color.White;
                    break;

                case 2: // Orta Önemli Alarm
                    this.BackColor = Color.FromArgb(239, 108, 0); // Material Orange 800 (Turuncu)
                    lblAlarmText.ForeColor = Color.White;
                    break;

                default: // Düşük Önemli Alarm (veya Tanımsız)
                    this.BackColor = Color.FromArgb(255, 202, 40); // Material Amber 500 (Tatlı Sarı)
                    // UX DÜZELTMESİ: Sarı arka planda beyaz yazı okunamaz. Okunabilirlik için yazı rengi Koyu Gri yapıldı.
                    lblAlarmText.ForeColor = Color.FromArgb(33, 33, 33);
                    break;
            }

            if (!this.Visible)
            {
                this.Visible = true;
            }
        }

        public void HideBanner()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HideBanner()));
                return;
            }

            if (this.Visible)
            {
                this.Visible = false;
            }
        }
    }
}