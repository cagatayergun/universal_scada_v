using System;
using System.Drawing;
using System.Windows.Forms;
using TekstilScada.Models;

namespace TekstilScada.UIControls
{
    public partial class UtilityDashboardCard_Control : UserControl
    {
        private bool _isConnected = false;

        public UtilityDashboardCard_Control()
        {
            InitializeComponent();

            // Başlangıçta kopuk (Gri) olarak başlasın
            SetConnectionStatus(false);
        }

        // Bağlantı durumunu güncelleyen metot
        public void SetConnectionStatus(bool isConnected)
        {
            _isConnected = isConnected;

            if (isConnected)
            {
                pnlStatusIndicator.BackColor = Color.LimeGreen; // Yeşil Işık
                pnlHeader.BackColor = Color.WhiteSmoke;
                this.Enabled = true;
            }
            else
            {
                pnlStatusIndicator.BackColor = Color.Gray; // Sönük Işık
                pnlHeader.BackColor = Color.LightGray;

                // Bağlı değilse değerleri sıfırla
                ResetValues();
            }
        }

        public void SetData(UtilityDashboardDto data)
        {
            if (data == null) return;

            lblLineName.Text = data.LineName;

            // Eğer bağlıysa değerleri yaz, değilse 0 kalır (SetConnectionStatus yönetecek)
            if (_isConnected)
            {
                lblElecVal.Text = $"{data.DailyElecUsage:N1}\nkWh";
                lblWaterVal.Text = $"{data.DailyWaterUsage:N1}\nm³";
                lblSteamVal.Text = $"{data.DailySteamUsage:N1}\nkg";
                lblAirVal.Text = $"{data.DailyAirUsage:N1}\nm³";
            }
        }

        private void ResetValues()
        {
            lblElecVal.Text = "0.0\nkWh";
            lblWaterVal.Text = "0.0\nm³";
            lblSteamVal.Text = "0.0\nkg";
            lblAirVal.Text = "0.0\nm³";
        }
    }
}