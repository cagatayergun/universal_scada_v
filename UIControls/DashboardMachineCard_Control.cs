using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Threading.Tasks;
using System.Text.Json;
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
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int rpmStepTypeId = -1;

                foreach (System.Data.DataRow row in stepTypesTable.Rows)
                {
                    string stepName = row["StepName"].ToString();
                    if (stepName.Contains("Sıkma") || stepName.Contains("Extraction"))
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
                                                    c.Name.IndexOf("Extraction Speed", StringComparison.OrdinalIgnoreCase) >= 0)) ||
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

            lblRecipeName.Text = $"Recipe: {status.RecipeName ?? "-"}";
            lblBatchId.Text = $"Party: {status.BatchNumarasi ?? "-"}";

            try
            {
                gaugeRpm.Value = status.AnlikDevirRpm;
                gaugeRpm.Text = status.AnlikDevirRpm.ToString();
            }
            catch (Exception ex) { }

            // --- Process Status (Word 24 / ControlWord Çözümleme) ---
            // Modelinizdeki ControlWord alanına göre aktarım yapın (örn: status.AktifAdimControlWord)
            UpdateProcessStatusFromWord(status.AktifAdimTipiWordu);

            // --- Kurutma Makinesi Kontrolü ---
            bool isDrying = _machine.MachineType == "Kurutma Makinesi";
            if (!isDrying)
            {
                lblTemperature.Text = $"{status.AnlikSicaklik / 10.0m}°C";
            }
            else
            {
                lblTemperature.Text = $"{status.AnlikSicaklik / 100.0m:F1}°C";
            }

            progressBar.Visible = !isDrying;
            lblPercentage.Visible = !isDrying;
            lblProcessing.Visible = !isDrying;
            lblHumidity.Visible = isDrying;
            lblhumudity.Visible = isDrying;
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
        }

        /// <summary>
        /// PLC'den gelen Word 24 (ControlWord) değerinin son 4 bitini (Bit 12-15) çözer ve txtProcessStatus kutucuğuna yazar.
        /// </summary>
        public void UpdateProcessStatusFromWord(int controlWord24)
        {
            byte statusValue = (byte)((controlWord24 >> 12) & 0x0F);
            txtProcessStatus.Text = GetProcessStatusText(statusValue);
        }

        private string GetProcessStatusText(byte statusValue)
        {
            return statusValue switch
            {
                1 => "ALLOVER SPRAY",
                2 => "BIO POLISH",
                3 => "BLEACH",
                4 => "BRIGHTNER",
                5 => "DESIZE",
                6 => "DRY",
                7 => "NEUTRAL",
                8 => "RINSE",
                9 => "SCRAP (NORMAL)",
                10 => "SOFTNER",
                11 => "STONE WASH",
                12 => "TINT",
                _ => "NONE"
            };
        }
    }
}