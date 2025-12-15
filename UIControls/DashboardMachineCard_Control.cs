using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Threading.Tasks; // EKLENDİ
using System.Text.Json;       // EKLENDİ
using static TekstilScada.Repositories.ProcessLogRepository;

namespace TekstilScada.UI.Controls
{
    public partial class DashboardMachineCard_Control : UserControl
    {
        private readonly Machine _machine;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();
        private List<PointF> _sparklinePoints = new List<PointF>();
        private readonly Color _colorAlarm = Color.FromArgb(231, 76, 60);
        private readonly Color _colorRunning = Color.FromArgb(46, 204, 113);
        private readonly Color _colorIdle = Color.FromArgb(243, 156, 18);
        private readonly Color _colorStopped = Color.SlateGray;
        private int _lastValidProgress = 0;

        public DashboardMachineCard_Control(Machine machine)
        {
            InitializeComponent();
            _machine = machine;
            lblMachineName.Text = _machine.MachineName;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // Renk ayarları
            lblMachineName.ForeColor = Color.Black;
            lblRecipeName.ForeColor = Color.Black;
            lblBatchId.ForeColor = Color.Black;
            lblTemperature.ForeColor = Color.Red;
            gaugeRpm.ForeColor = Color.Black;
            lblPercentage.ForeColor = Color.Black;
            lblHumidity.ForeColor = Color.Blue;
            lblhumudity.ForeColor = Color.Black;
            label2.ForeColor = Color.Black;
            SetRpmGaugeLimitAsync();
        }
        private async void SetRpmGaugeLimitAsync()
        {
            try
            {
                // 1. Veritabanından adım tiplerini çek
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int rpmStepTypeId = -1;

                // 2. "Sıkma" (Squeezing) adımının ID'sini bul
                foreach (System.Data.DataRow row in stepTypesTable.Rows)
                {
                    string stepName = row["StepName"].ToString();
                    if (stepName.Contains("Sıkma") || stepName.Contains("Squeezing"))
                    {
                        rpmStepTypeId = Convert.ToInt32(row["Id"]);
                        break;
                    }
                }

                // Eğer Sıkma adımı bulunduysa
                if (rpmStepTypeId != -1)
                {
                    // 3. KRİTİK NOKTA: Bu kartın ait olduğu makinenin alt tipini kullanıyoruz
                    // _machine.MachineSubType -> Örn: "Boyama", "Yıkama"
                    string layoutJson = await Task.Run(() =>
                        _configRepo.GetLayoutJson(_machine.MachineSubType, rpmStepTypeId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = System.Text.Json.JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                        // 4. Tasarım içindeki RPM kontrolünü bul
                        var rpmControl = controls.FirstOrDefault(c =>
                            c.Maximum > 50 &&
                            (
                                (c.Name != null && (c.Name.IndexOf("numSikmaDevri", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                    c.Name.IndexOf("Rpm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                    c.Name.IndexOf("Squeezing Speed", StringComparison.OrdinalIgnoreCase) >= 0)) ||
                                (c.Text != null && c.Text.IndexOf("Devir", StringComparison.OrdinalIgnoreCase) >= 0)
                            )
                        );

                        if (rpmControl != null)
                        {
                            // 5. Değeri ata (1.33 katı ile)
                            int newMax = (int)(rpmControl.Maximum * 1.33m);

                            if (gaugeRpm.InvokeRequired)
                            {
                                gaugeRpm.Invoke(new Action(() => gaugeRpm.Maximum = newMax));
                            }
                            else
                            {
                                gaugeRpm.Maximum = newMax;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Dashboard'da çok kart olduğu için hata patlatmayalım, loglayalım
                System.Diagnostics.Debug.WriteLine($"RPM limiti ayarlanamadı ({_machine.MachineName}): {ex.Message}");
            }
        }
        public void UpdateData(FullMachineStatus status, List<ProcessDataPoint> trendData)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateData(status, trendData)));
                return;
            }

            lblRecipeName.Text = $"Recipe: {status.RecipeName ?? "-"}";
            lblBatchId.Text = $"Party: {status.BatchNumarasi ?? "-"}";
           
            
            gaugeRpm.Value = status.AnlikDevirRpm;
            gaugeRpm.Text = status.AnlikDevirRpm.ToString();

            // --- YENİ: Kurutma Makinesi Kontrolü ---
            bool isDrying = _machine.MachineType == "Kurutma Makinesi";
            if (!isDrying)
            {
                lblTemperature.Text = $"{status.AnlikSicaklik / 10.0m}°C";
            }
            else
            {
                lblTemperature.Text = $"{status.AnlikSicaklik / 100.0m}°C";
            }
                // Kurutma makinesi ise barı gizle, nemi göster
                progressBar.Visible = !isDrying;
            lblPercentage.Visible = !isDrying;
            lblProcessing.Visible = !isDrying;
            lblHumidity.Visible = isDrying;
            lblhumudity.Visible = isDrying;
            if (isDrying)
            {
                // Not: Modelde Nem alanını ekleyince burayı status.AnlikNem yaparsınız.
                // Şimdilik mevcut yapıyı koruyoruz.
                lblHumidity.Text = $"{status.AnlikSuSeviyesi} %";
            }
            // ---------------------------------------

            if (status.HasActiveAlarm)
            {
                if (progressBar.Value > 0) _lastValidProgress = progressBar.Value;
                progressBar.Value = _lastValidProgress;
                lblPercentage.Text = $"{_lastValidProgress} %";

                pnlStatusIndicator.BackColor = _colorAlarm;
                lblStatus.Text = $"ALARM #{status.ActiveAlarmNumber}";
                lblStatus.ForeColor = _colorAlarm;
            }
            else
            {
                _lastValidProgress = Math.Max(0, Math.Min(100, (int)status.ProsesYuzdesi));
                progressBar.Value = _lastValidProgress;
                lblPercentage.Text = $"{_lastValidProgress} %";
                if (status.manuel_status)
                {
                    pnlStatusIndicator.BackColor = _colorRunning;
                    lblStatus.Text = $"Working - Manuel";
                    lblStatus.ForeColor = _colorRunning;
                }
                else
                {
                    if (status.IsInRecipeMode)
                {
                    pnlStatusIndicator.BackColor = _colorRunning;
                    lblStatus.Text = $"Working - Step {status.AktifAdimNo}";
                    lblStatus.ForeColor = _colorRunning;
                }
                else
                {
                    pnlStatusIndicator.BackColor = _colorStopped;
                    lblStatus.Text = "Stops";
                    lblStatus.ForeColor = _colorStopped;
                }
                }
            }


            
        }

      
    }
}