// UI/Controls/DashboardMachineCard_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Repositories;
using static Telemetry.Repositories.ProcessLogRepository;

namespace Telemetry.UI.Controls
{
    public partial class DashboardMachineCard_Control : UserControl
    {
        private readonly Machine _machine;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();

        private readonly Color _colorAlarm = Color.FromArgb(239, 83, 80);
        private readonly Color _colorRunning = Color.FromArgb(102, 187, 106);
        private readonly Color _colorIdle = Color.FromArgb(255, 167, 38);
        private readonly Color _colorStopped = Color.FromArgb(144, 164, 174);
        private int _lastValidProgress = 0;

        public DashboardMachineCard_Control(Machine machine)
        {
            InitializeComponent();
            _machine = machine;
            lblMachineName.Text = _machine.MachineName;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // =========================================================================
            // GÜVENLİK YAMASI: CIRCULAR PROGRESS BAR ÇÖKME ENGELLEYİCİ
            // Ebeveynin arka plan rengini zorla kopyalayarak şeffaflık (Transparent) krizini çözer
            // =========================================================================
            Color solidBgColor = Color.FromArgb(44, 52, 64);
            this.materialCard1.BackColor = solidBgColor;
            this.gaugeRpm.BackColor = solidBgColor;

            Color textLight = Color.FromArgb(240, 240, 240);
            Color textMuted = Color.FromArgb(176, 190, 197);

            lblMachineName.ForeColor = textLight;
            lblRecipeName.ForeColor = textMuted;
            lblBatchId.ForeColor = textMuted;
            lblPercentage.ForeColor = textLight;
            lblhumudity.ForeColor = textMuted;
            label2.ForeColor = textMuted;
            gaugeRpm.ForeColor = textLight;

            lblTemperature.ForeColor = Color.FromArgb(255, 110, 110);
            lblHumidity.ForeColor = Color.FromArgb(100, 181, 246);

            SetRpmGaugeLimitAsync();
        }

        private async void SetRpmGaugeLimitAsync()
        {
            try
            {
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int rpmStepTypeId = -1;

                foreach (System.Data.DataRow row in stepTypesTable.Rows)
                {
                    string stepName = row["StepName"].ToString();
                    if (stepName.Contains("Sıkma") || stepName.Contains("Squeezing"))
                    {
                        rpmStepTypeId = Convert.ToInt32(row["Id"]);
                        break;
                    }
                }

                if (rpmStepTypeId != -1)
                {
                    string layoutJson = await Task.Run(() =>
                        _configRepo.GetLayoutJson(_machine.MachineSubType, rpmStepTypeId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = System.Text.Json.JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

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
                            int newMax = (int)(rpmControl.Maximum);

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

            this.SuspendLayout();

            lblRecipeName.Text = $"Recipe: {status.RecipeName ?? "-"}";
            lblBatchId.Text = $"Party: {status.BatchNumarasi ?? "-"}";

            try
            {
                gaugeRpm.Value = status.AnlikDevirRpm;
                gaugeRpm.Text = status.AnlikDevirRpm.ToString();
            }
            catch (Exception ex) { }

            bool isDrying = _machine.MachineType == "Kurutma Makinesi";
            if (!isDrying)
            {
                lblTemperature.Text = $"{status.AnlikSicaklik / 10.0m:F1}°C";
            }
            else
            {
                lblTemperature.Text = $"{status.AnlikSicaklik / 100.0m:F1}°C";
            }

            if (progressBar.Visible == isDrying)
            {
                progressBar.Visible = !isDrying;
                lblPercentage.Visible = !isDrying;
                lblProcessing.Visible = !isDrying;
                lblHumidity.Visible = isDrying;
                lblhumudity.Visible = isDrying;
            }

            if (isDrying)
            {
                lblHumidity.Text = $"{status.AnlikSuSeviyesi} %";
            }

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

            this.ResumeLayout(true);
        }
    }
}