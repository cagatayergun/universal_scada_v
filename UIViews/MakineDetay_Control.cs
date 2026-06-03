// UI/Views/MakineDetay_Control.cs
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.Services;
using MaterialSkin;
using MaterialSkin.Controls;

// =========================================================================
// ÇÖZÜM: CS0104 & CS1503 BELİRSİZ REFERANS VE METOT UYUŞMAZLIK KORUMASI
// Dosya genelinde bare yazılan tipler açıkça System.Drawing kütüphanesine mühürlendi.
// =========================================================================
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;

namespace Telemetry.UI.Views
{
    public partial class MakineDetay_Control : UserControl
    {
        public event EventHandler BackRequested;

        private PlcPollingService _pollingService;
        private ProcessLogRepository _logRepository;
        private AlarmRepository _alarmRepository;
        private RecipeRepository _recipeRepository;
        private ProductionRepository _productionRepository;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();
        private Machine _machine;

        // Plot nesneleri
        private ScottPlot.Plottables.Scatter _tempScatter;
        private ScottPlot.Plottables.Scatter _rpmScatter;
        private ScottPlot.Plottables.Scatter _waterScatter;

        private List<string> _currentlyDisplayedAlarms = new List<string>();
        private System.Windows.Forms.Timer _uiUpdateTimer;
        private string _lastLoadedBatchIdForChart = null;
        private bool _isSyncing = false;

        public MakineDetay_Control()
        {
            InitializeComponent();
            btnGeri.Click += (sender, args) => BackRequested?.Invoke(this, EventArgs.Empty);

            // SPEED OPTİMİZASYON: Kontrol geçişlerinde ve grafik kaydırmalarında titremeyi sıfırlar
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;

            this.progressTemp.Paint += new System.Windows.Forms.PaintEventHandler(this.progressTemp_Paint);
            this.humuditybar.Paint += new System.Windows.Forms.PaintEventHandler(this.humuditybar_Paint);

            // Titremeyi önlemek için dikey barların donanımsal çift tamponlamasını açıyoruz
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, progressTemp, new object[] { true });

            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, humuditybar, new object[] { true });

            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            // Grafik Eksenlerini Bağlama (Zaman Senkronizasyonu)
            formsPlotTemp.Plot.RenderManager.AxisLimitsChanged += (s, e) => SyncAxes(formsPlotTemp);
            formsPlotRpm.Plot.RenderManager.AxisLimitsChanged += (s, e) => SyncAxes(formsPlotRpm);
            formsPlotWater.Plot.RenderManager.AxisLimitsChanged += (s, e) => SyncAxes(formsPlotWater);
        }

        private void SyncAxes(ScottPlot.WinForms.FormsPlot sourcePlot)
        {
            if (_isSyncing) return;
            _isSyncing = true;

            var limits = sourcePlot.Plot.Axes.GetLimits();

            if (sourcePlot != formsPlotTemp)
            {
                formsPlotTemp.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                formsPlotTemp.Refresh();
            }
            if (sourcePlot != formsPlotRpm)
            {
                formsPlotRpm.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                formsPlotRpm.Refresh();
            }
            if (sourcePlot != formsPlotWater)
            {
                formsPlotWater.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                formsPlotWater.Refresh();
            }

            _isSyncing = false;
        }

        private void CleanupPreviousSession()
        {
            if (_uiUpdateTimer != null)
            {
                _uiUpdateTimer.Stop();
                _uiUpdateTimer.Tick -= UpdateLiveGauges_Tick;
                _uiUpdateTimer.Dispose();
                _uiUpdateTimer = null;
            }

            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= OnDataRefreshed;
                _pollingService.OnMachineConnectionStateChanged -= OnConnectionStateChanged;
                _pollingService.OnActiveAlarmStateChanged -= OnAlarmStateChanged;
            }

            this.VisibleChanged -= MakineDetay_Control_VisibleChanged;

            _tempScatter = null;
            _rpmScatter = null;
            _waterScatter = null;
            _lastLoadedBatchIdForChart = null;
            _currentlyDisplayedAlarms.Clear();

            formsPlotTemp.Plot.Clear();
            formsPlotRpm.Plot.Clear();
            formsPlotWater.Plot.Clear();

            formsPlotTemp.Refresh();
            formsPlotRpm.Refresh();
            formsPlotWater.Refresh();

            lblMakineAdi.Text = "---";
            lblReceteAdi.Text = "---";
            dgvAdimlar.DataSource = null;
        }

        public void InitializeControl(Machine machine, PlcPollingService service, ProcessLogRepository logRepo, AlarmRepository alarmRepo, RecipeRepository recipeRepo, ProductionRepository productionRepo)
        {
            CleanupPreviousSession();

            _machine = machine;
            _pollingService = service;
            _logRepository = logRepo;
            _alarmRepository = alarmRepo;
            _recipeRepository = recipeRepo;
            _productionRepository = productionRepo;

            _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _uiUpdateTimer.Tick += UpdateLiveGauges_Tick;
            _uiUpdateTimer.Start();

            _pollingService.OnMachineDataRefreshed += OnDataRefreshed;
            _pollingService.OnMachineConnectionStateChanged += OnConnectionStateChanged;
            _pollingService.OnActiveAlarmStateChanged += OnAlarmStateChanged;

            this.VisibleChanged += MakineDetay_Control_VisibleChanged;

            LoadInitialData();
            ConfigureStepsGridAppearance();
        }

        private void UpdateLiveGauges_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
                UpdateLiveGauges();
        }

        private void LoadInitialData()
        {
            bool isDrying = _machine.MachineType == "Kurutma Makinesi";

            waterTankGauge1.Visible = !isDrying;
            humuditypanel.Visible = isDrying;

            SetWaterGaugeLimitAsync();
            SetRpmGaugeLimitAsync();

            if (_pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status))
            {
                UpdateUI(status);
                UpdateAlarmList();
                LoadRecipeStepsFromPlcAsync();
            }
        }

        private void ClearAllFieldsWithMessage(string message)
        {
            ClearBatchSpecificFieldsWithMessage(message);
            lblReceteAdi.Text = "---";
            lblOperator.Text = "---";
            lblMusteriNo.Text = "---";
            lblBatchNo.Text = "---";
            lblSiparisNo.Text = "---";
            lblCalisanAdim.Text = "---";
        }

        private async void SetRpmGaugeLimitAsync()
        {
            try
            {
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int rpmStepTypeId = -1;

                foreach (DataRow row in stepTypesTable.Rows)
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
                    string layoutJson = await Task.Run(() => _configRepo.GetLayoutJson(_machine.MachineSubType, rpmStepTypeId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                        var rpmControl = controls.FirstOrDefault(c =>
                         (c.Name != null && (c.Name.IndexOf("numSikmaDevri", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Rpm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Squeezing Speed", StringComparison.OrdinalIgnoreCase) >= 0)) ||
                         (c.Text != null && c.Text.IndexOf("Devir", StringComparison.OrdinalIgnoreCase) >= 0)
                        );

                        if (rpmControl != null)
                        {
                            int newMax = (int)(rpmControl.Maximum);
                            this.SafeInvoke(() => gaugeRpm.Maximum = newMax);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RPM limiti ayarlanamadı: {ex.Message}");
            }
        }

        private async void SetWaterGaugeLimitAsync()
        {
            try
            {
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int waterStepTypeId = -1;

                foreach (DataRow row in stepTypesTable.Rows)
                {
                    string stepName = row["StepName"].ToString();
                    if (stepName.Contains("Su Alma") || stepName.Contains("Water Intake"))
                    {
                        waterStepTypeId = Convert.ToInt32(row["Id"]);
                        break;
                    }
                }

                if (waterStepTypeId != -1)
                {
                    string layoutJson = await Task.Run(() => _configRepo.GetLayoutJson(_machine.MachineSubType, waterStepTypeId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                        var waterControl = controls.FirstOrDefault(c =>
                         (c.Name != null && (c.Name.IndexOf("numLitre", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Su", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0))
                        );

                        if (waterControl != null)
                        {
                            int maxVal = (int)waterControl.Maximum;
                            this.SafeInvoke(() => waterTankGauge1.Maximum = maxVal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Su seviyesi limiti ayarlanamadı: {ex.Message}");
            }
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            btnGeri.Text = Resources.geri;
            label1.Text = Resources.makinebilgileri;
            label2.Text = Resources.RecipeName;
            label3.Text = Resources.Operator;
            label4.Text = Resources.CustomerNo;
            label5.Text = Resources.BatchNo;
            label6.Text = Resources.OrderNo;
            lblTempTitle.Text = Resources.Temperature;
            lstAlarmlar.Text = Resources.baglantibekleniyro;
        }

        private void OnConnectionStateChanged(int machineId, FullMachineStatus status)
        {
            if (machineId == _machine.Id && this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() => UpdateUI(status)));
            }
        }

        private void OnDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (machineId == _machine.Id && this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() => UpdateUI(status)));
            }
        }

        private void MakineDetay_Control_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (_uiUpdateTimer != null && !_uiUpdateTimer.Enabled)
                    _uiUpdateTimer.Start();

                if (_machine != null && _pollingService != null)
                {
                    _lastLoadedBatchIdForChart = null;
                    if (_pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status))
                    {
                        UpdateUI(status);
                        UpdateAlarmList();
                    }
                }
            }
            else
            {
                if (_uiUpdateTimer != null)
                    _uiUpdateTimer.Stop();
            }
        }

        private void UpdateLiveGauges()
        {
            if (_machine != null && _pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status))
            {
                SafeInvoke(() =>
                {
                    gaugeRpm.Value = status.AnlikDevirRpm;
                    gaugeRpm.Text = status.AnlikDevirRpm.ToString();

                    bool isDrying = _machine.MachineType == "Kurutma Makinesi";

                    if (!isDrying)
                    {
                        decimal anlikSicaklikDecimal = status.AnlikSicaklik / 10.0m;
                        progressTemp.Tag = anlikSicaklikDecimal;
                        lblTempValue.Text = $"{anlikSicaklikDecimal:F1} °C";
                    }
                    else
                    {
                        decimal anlikSicaklikDecimal = status.AnlikSicaklik / 100.0m;
                        progressTemp.Tag = anlikSicaklikDecimal;
                        lblTempValue.Text = $"{anlikSicaklikDecimal:F1} °C";
                    }

                    decimal AnlikSuSeviyesi = status.AnlikSuSeviyesi;
                    humuditybar.Tag = AnlikSuSeviyesi;
                    humuditytxt.Text = $"{AnlikSuSeviyesi} Rh";

                    humuditytxt.ForeColor = Color.FromArgb(33, 150, 243);
                    lblTempValue.ForeColor = Color.FromArgb(239, 83, 80);

                    progressTemp.Invalidate();
                    progressTemp.Update();
                    humuditybar.Invalidate();
                    humuditybar.Update();

                    waterTankGauge1.Value = status.AnlikSuSeviyesi;

                    if (_tempScatter != null)
                    {
                        var limits = formsPlotTemp.Plot.Axes.GetLimits();
                        double span = limits.Right - limits.Left;

                        double newMaxX;
                        double newMinX;

                        if (string.IsNullOrEmpty(_lastLoadedBatchIdForChart))
                        {
                            newMaxX = DateTime.Now.ToOADate();
                            newMinX = newMaxX - span;
                        }
                        else
                        {
                            if (status.BatchNumarasi == _lastLoadedBatchIdForChart)
                            {
                                newMaxX = DateTime.Now.ToOADate();
                                newMinX = newMaxX - span;
                            }
                            else
                            {
                                return;
                            }
                        }

                        formsPlotTemp.Plot.Axes.SetLimitsX(newMinX, newMaxX);
                        formsPlotRpm.Plot.Axes.SetLimitsX(newMinX, newMaxX);
                        formsPlotWater.Plot.Axes.SetLimitsX(newMinX, newMaxX);

                        formsPlotTemp.Refresh();
                        formsPlotRpm.Refresh();
                        formsPlotWater.Refresh();
                    }
                });
            }
        }

        private void UpdateUI(FullMachineStatus status)
        {
            lblMakineAdi.Text = status.MachineName;
            lblOperator.Text = string.IsNullOrEmpty(status.OperatorIsmi) ? "---" : status.OperatorIsmi;
            lblReceteAdi.Text = string.IsNullOrEmpty(status.RecipeName) ? "---" : status.RecipeName;
            lblMusteriNo.Text = string.IsNullOrEmpty(status.MusteriNumarasi) ? "---" : status.MusteriNumarasi;
            lblBatchNo.Text = string.IsNullOrEmpty(status.BatchNumarasi) ? "---" : status.BatchNumarasi;
            lblSiparisNo.Text = string.IsNullOrEmpty(status.SiparisNumarasi) ? "---" : status.SiparisNumarasi;
            lblCalisanAdim.Text = $"#{status.AktifAdimNo} - {status.AktifAdimAdi}";

            if (status.ConnectionState != ConnectionStatus.Connected)
            {
                ClearAllFieldsWithMessage($"{Resources.baglantibekleniyro}");
                return;
            }

            if (!string.IsNullOrEmpty(status.BatchNumarasi))
            {
                bool isNewBatch = status.BatchNumarasi != _lastLoadedBatchIdForChart;
                _lastLoadedBatchIdForChart = status.BatchNumarasi;

                if (isNewBatch)
                {
                    var alarms = _alarmRepository.GetAlarmDetailsForBatch(status.BatchNumarasi, _machine.Id);
                    var alarmStrings = alarms.Any() ? alarms.Select(a => a.AlarmDescription).ToList() : new List<string> { $"{Resources.bupartiicinalarmyok}" };

                    _currentlyDisplayedAlarms = alarmStrings;
                    lstAlarmlar.DataSource = _currentlyDisplayedAlarms;

                    formsPlotTemp.Plot.Clear();
                    formsPlotRpm.Plot.Clear();
                    formsPlotWater.Plot.Clear();

                    _tempScatter = null;
                    _rpmScatter = null;
                    _waterScatter = null;
                }

                LoadTimelineChartForBatch(status.BatchNumarasi);
            }
            else
            {
                if (_lastLoadedBatchIdForChart != null)
                {
                    LoadDataForLive(status);
                    UpdateAlarmList();
                }
                _lastLoadedBatchIdForChart = null;
                LoadDataForLive(status);
            }

            HighlightCurrentStep(status.AktifAdimNo);
        }

        private void LoadDataForBatch(FullMachineStatus status)
        {
            _lastLoadedBatchIdForChart = status.BatchNumarasi;
            var alarms = _alarmRepository.GetAlarmDetailsForBatch(status.BatchNumarasi, _machine.Id);
            var alarmStrings = alarms.Any() ? alarms.Select(a => a.AlarmDescription).ToList() : new List<string> { $"{Resources.bupartiicinalarmyok}" };

            _currentlyDisplayedAlarms = alarmStrings;
            lstAlarmlar.DataSource = _currentlyDisplayedAlarms;
            LoadTimelineChartForBatch(status.BatchNumarasi);
        }

        private void LoadDataForLive(FullMachineStatus status)
        {
            LoadTimelineChartForLive();
        }

        private async void LoadRecipeStepsFromPlcAsync()
        {
            dgvAdimlar.DataSource = new List<object> { new { Adım = "...", Açıklama = $"{Resources.receteplcdenokunuyor}" } };

            if (_pollingService.GetPlcManagers().TryGetValue(_machine.Id, out var plcManager))
            {
                var result = await plcManager.ReadRecipeFromPlcAsync();
                if (result.IsSuccess)
                {
                    var steps = new List<ScadaRecipeStep>();
                    var rawData = result.Content;

                    if (_machine.MachineType == $"{Resources.kurutmamakinesi}")
                    {
                        var step = new ScadaRecipeStep { StepNumber = 1 };
                        Array.Copy(rawData, 0, step.StepDataWords, 0, Math.Min(rawData.Length, 6));
                        steps.Add(step);
                    }
                    else
                    {
                        for (int i = 0; i < 98; i++)
                        {
                            var step = new ScadaRecipeStep { StepNumber = i + 1 };
                            int offset = i * 25;
                            if (offset + 25 <= rawData.Length)
                            {
                                Array.Copy(rawData, offset, step.StepDataWords, 0, 25);
                                steps.Add(step);
                            }
                        }
                    }
                    dgvAdimlar.DataSource = steps.Select(s => new { Adım = s.StepNumber, Açıklama = GetStepTypeName(s) }).ToList();
                }
                else
                {
                    dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"{Resources.plcdenreceteokunmadı} {result.Message}" } };
                }
            }
            else
            {
                dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"{Resources.makinebaglantısıbulunamadı}" } };
            }
        }

        // =========================================================================
        // DÜZELTME: CS0664 FLOAT VERİ TİPİ DÖNÜŞÜMÜ (1.5 -> 1.5f YAPILDI)
        // =========================================================================
        private void LoadTimelineChartForBatch(string batchId)
        {
            SafeInvoke(() =>
            {
                var (startTime, endTime) = _productionRepository.GetBatchTimestamps(batchId, _machine.Id);

                if (!startTime.HasValue)
                {
                    formsPlotTemp.Plot.Title($"{Resources.partibaslangıczamanıkayip}");
                    formsPlotTemp.Refresh();
                    return;
                }

                DateTime effectiveEndTime = endTime ?? DateTime.Now;
                var dataPoints = _logRepository.GetLogsForDateRange(_machine.Id, startTime.Value, effectiveEndTime);

                if (!dataPoints.Any())
                {
                    formsPlotTemp.Plot.Title($"{Resources.bupartihenüzkaydedilmemis}");
                    formsPlotTemp.Refresh();
                    return;
                }

                var xs = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                bool isDrying = _machine.MachineType == "Kurutma Makinesi";
                double[] ysTemp = isDrying ? dataPoints.Select(p => (double)p.Temperature / 100.0).ToArray() : dataPoints.Select(p => (double)p.Temperature / 10.0).ToArray();

                var ysRpm = dataPoints.Select(p => (double)p.Rpm).ToArray();
                var ysWater = dataPoints.Select(p => (double)p.WaterLevel).ToArray();

                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

                if (_tempScatter == null || !formsPlotTemp.Plot.GetPlottables().Contains(_tempScatter))
                {
                    formsPlotTemp.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotTemp, isDark);
                    formsPlotTemp.Plot.Title($"{_machine.MachineName} - {Resources.proseszamancizgisi} ({batchId})");

                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(xs, ysTemp);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _tempScatter.MarkerSize = 0;
                    formsPlotTemp.Plot.Axes.DateTimeTicksBottom();
                }
                else
                {
                    formsPlotTemp.Plot.Remove(_tempScatter);
                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(xs, ysTemp);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _tempScatter.MarkerSize = 0;
                }

                if (_rpmScatter == null || !formsPlotRpm.Plot.GetPlottables().Contains(_rpmScatter))
                {
                    formsPlotRpm.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotRpm, isDark);
                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(xs, ysRpm);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _rpmScatter.MarkerSize = 0;
                    formsPlotRpm.Plot.Axes.DateTimeTicksBottom();
                }
                else
                {
                    formsPlotRpm.Plot.Remove(_rpmScatter);
                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(xs, ysRpm);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _rpmScatter.MarkerSize = 0;
                }

                if (_waterScatter == null || !formsPlotWater.Plot.GetPlottables().Contains(_waterScatter))
                {
                    formsPlotWater.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotWater, isDark);
                    _waterScatter = formsPlotWater.Plot.Add.Scatter(xs, ysWater);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _waterScatter.MarkerSize = 0;
                    formsPlotWater.Plot.Axes.DateTimeTicksBottom();
                }
                else
                {
                    formsPlotWater.Plot.Remove(_waterScatter);
                    _waterScatter = formsPlotWater.Plot.Add.Scatter(xs, ysWater);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _waterScatter.MarkerSize = 0;
                }

                formsPlotTemp.Plot.Axes.AutoScaleY();
                formsPlotRpm.Plot.Axes.AutoScaleY();
                formsPlotWater.Plot.Axes.AutoScaleY();

                formsPlotTemp.Refresh();
                formsPlotRpm.Refresh();
                formsPlotWater.Refresh();
            });
        }

        // =========================================================================
        // DÜZELTME: CS0664 FLOAT VERİ TİPİ DÖNÜŞÜMÜ (1.5 -> 1.5f YAPILDI)
        // =========================================================================
        private void LoadTimelineChartForLive()
        {
            int timeWindowMinutes = 360;

            SafeInvoke(() =>
            {
                DateTime endTime = DateTime.Now;
                DateTime startTime = endTime.AddMinutes(-timeWindowMinutes);

                var dataPoints = _logRepository.GetManualLogs(_machine.Id, startTime, endTime);

                if (!dataPoints.Any())
                {
                    formsPlotTemp.Plot.Title($"{Resources.canlidata} (Veri Yok)");
                    formsPlotTemp.Refresh();
                    formsPlotRpm.Refresh();
                    formsPlotWater.Refresh();
                    return;
                }

                double[] timeData = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                bool isDrying = _machine.MachineType == "Kurutma Makinesi";
                double[] tempData = isDrying ? dataPoints.Select(p => (double)p.Temperature / 100.0).ToArray() : dataPoints.Select(p => (double)p.Temperature / 10.0).ToArray();

                double[] rpmData = dataPoints.Select(p => (double)p.Rpm).ToArray();
                double[] waterLevelData = dataPoints.Select(p => (double)p.WaterLevel).ToArray();

                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

                if (_tempScatter == null)
                {
                    formsPlotTemp.Plot.Clear(); formsPlotRpm.Plot.Clear(); formsPlotWater.Plot.Clear();

                    ApplyScottPlotTheme(formsPlotTemp, isDark);
                    ApplyScottPlotTheme(formsPlotRpm, isDark);
                    ApplyScottPlotTheme(formsPlotWater, isDark);

                    formsPlotTemp.Plot.Axes.DateTimeTicksBottom();
                    formsPlotRpm.Plot.Axes.DateTimeTicksBottom();
                    formsPlotWater.Plot.Axes.DateTimeTicksBottom();

                    formsPlotTemp.Plot.Title($"{_machine.MachineName} - {Resources.canliprosesdata}");

                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(timeData, tempData);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _tempScatter.MarkerSize = 0;
                    formsPlotTemp.Plot.Axes.Left.Label.Text = Resources.Temperature;
                    formsPlotTemp.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Red;

                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(timeData, rpmData);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _rpmScatter.MarkerSize = 0;
                    formsPlotRpm.Plot.Axes.Left.Label.Text = Resources.devir;
                    formsPlotRpm.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Green;

                    _waterScatter = formsPlotWater.Plot.Add.Scatter(timeData, waterLevelData);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _waterScatter.MarkerSize = 0;

                    formsPlotWater.Plot.Axes.Left.Label.Text = isDrying ? "Humidity (Rh)" : Resources.suseviyesi;
                    formsPlotWater.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Blue;

                    double startZoomOA = endTime.AddMinutes(-5).ToOADate();
                    double endOA = endTime.ToOADate();

                    formsPlotTemp.Plot.Axes.SetLimitsX(startZoomOA, endOA);
                    formsPlotRpm.Plot.Axes.SetLimitsX(startZoomOA, endOA);
                    formsPlotWater.Plot.Axes.SetLimitsX(startZoomOA, endOA);
                }
                else
                {
                    formsPlotTemp.Plot.Remove(_tempScatter);
                    formsPlotRpm.Plot.Remove(_rpmScatter);
                    formsPlotWater.Plot.Remove(_waterScatter);

                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(timeData, tempData);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _tempScatter.MarkerSize = 0;

                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(timeData, rpmData);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _rpmScatter.MarkerSize = 0;

                    _waterScatter = formsPlotWater.Plot.Add.Scatter(timeData, waterLevelData);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1.5f; // Sonek hatası giderildi
                    _waterScatter.MarkerSize = 0;
                }

                formsPlotTemp.Plot.Axes.AutoScaleY();
                formsPlotRpm.Plot.Axes.AutoScaleY();
                formsPlotWater.Plot.Axes.AutoScaleY();

                formsPlotTemp.Refresh();
                formsPlotRpm.Refresh();
                formsPlotWater.Refresh();
            });
        }

        private void ApplyScottPlotTheme(ScottPlot.WinForms.FormsPlot formsPlot, bool isDark)
        {
            formsPlot.Plot.FigureBackground.Color = ScottPlot.Colors.Transparent;
            formsPlot.Plot.DataBackground.Color = ScottPlot.Colors.Transparent;

            var axisColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(148, 163, 184)) : ScottPlot.Color.FromColor(Color.FromArgb(71, 85, 105));
            var gridColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(51, 65, 85)) : ScottPlot.Color.FromColor(Color.FromArgb(241, 245, 249));

            formsPlot.Plot.Axes.Color(axisColor);
            formsPlot.Plot.Grid.MajorLineColor = gridColor;

            formsPlot.Plot.Axes.Left.Label.ForeColor = axisColor;
            formsPlot.Plot.Axes.Bottom.Label.ForeColor = axisColor;
        }

        private void ClearBatchSpecificFieldsWithMessage(string message)
        {
            lstAlarmlar.DataSource = new List<string> { message };
            dgvAdimlar.DataSource = null;
            formsPlotTemp.Plot.Clear(); formsPlotTemp.Plot.Title(message); formsPlotTemp.Refresh();
            formsPlotRpm.Plot.Clear(); formsPlotRpm.Refresh();
            formsPlotWater.Plot.Clear(); formsPlotWater.Refresh();
        }

        private void HighlightCurrentStep(int currentStepNumber)
        {
            if (dgvAdimlar.DataSource == null) return;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            Color activeBg = isDark ? Color.FromArgb(27, 94, 32) : Color.FromArgb(200, 230, 201);
            Color activeFg = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(27, 94, 32);
            Color normalBg = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            Color normalFg = isDark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);

            foreach (DataGridViewRow row in dgvAdimlar.Rows)
            {
                if (row.Cells["Adım"] != null && row.Cells["Adım"].Value != null)
                {
                    if (int.TryParse(row.Cells["Adım"].Value.ToString(), out int stepValue))
                    {
                        if (stepValue == currentStepNumber)
                        {
                            row.DefaultCellStyle.BackColor = activeBg;
                            row.DefaultCellStyle.ForeColor = activeFg;
                            row.DefaultCellStyle.Font = new Font(dgvAdimlar.Font, FontStyle.Bold);
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = normalBg;
                            row.DefaultCellStyle.ForeColor = normalFg;
                            row.DefaultCellStyle.Font = new Font(dgvAdimlar.Font, FontStyle.Regular);
                        }
                    }
                }
            }
        }

        private void ConfigureStepsGridAppearance()
        {
            dgvAdimlar.BorderStyle = BorderStyle.None;
            dgvAdimlar.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvAdimlar.EnableHeadersVisualStyles = false;
            dgvAdimlar.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            dgvAdimlar.BackgroundColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            dgvAdimlar.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
            dgvAdimlar.ColumnHeadersDefaultCellStyle.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            dgvAdimlar.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvAdimlar.RowTemplate.Height = 32;
        }

        private string GetStepTypeName(ScadaRecipeStep step)
        {
            var stepTypes = new List<string>();
            short controlWord = step.StepDataWords[24];
            if ((controlWord & 1) != 0) stepTypes.Add($"{Resources.sualma}");
            if ((controlWord & 2) != 0) stepTypes.Add($"{Resources.isitma}");
            if ((controlWord & 4) != 0) stepTypes.Add($"{Resources.calisma}");
            if ((controlWord & 8) != 0) stepTypes.Add($"{Resources.dozaj}");
            if ((controlWord & 16) != 0) stepTypes.Add($"{Resources.bosaltma}");
            if ((controlWord & 32) != 0) stepTypes.Add($"{Resources.sikma}");
            if ((controlWord & 64) != 0) stepTypes.Add("Humidity Working");
            if ((controlWord & 128) != 0) stepTypes.Add("Timed Working");
            if ((controlWord & 256) != 0) stepTypes.Add("Humidity/Timed Working");
            if ((controlWord & 512) != 0) stepTypes.Add("Cooling");
            return string.Join(" + ", stepTypes);
        }

        private void progressTemp_Paint(object sender, PaintEventArgs e)
        {
            Control barControl = sender as Control;
            if (barControl == null) return;

            float currentValue = 0;
            if (barControl.Tag != null)
            {
                try { currentValue = Convert.ToSingle(barControl.Tag); } catch { }
            }

            float maximumValue = 100f;
            currentValue = Math.Max(0, Math.Min(maximumValue, currentValue));

            int w = barControl.Width;
            int h = barControl.Height;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color panelBg = isDark ? Color.FromArgb(44, 52, 64) : Color.FromArgb(241, 245, 249);
            Color borderCol = isDark ? Color.FromArgb(71, 85, 104) : Color.LightGray;

            using (SolidBrush bgBrush = new SolidBrush(panelBg))
            {
                e.Graphics.FillRectangle(bgBrush, 0, 0, w, h);
            }

            float ratio = currentValue / maximumValue;
            int fillHeight = (int)(h * ratio);
            int yPos = h - fillHeight;

            Rectangle filledRect = new Rectangle(0, yPos, w, fillHeight);

            using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Red))
            {
                e.Graphics.FillRectangle(brush, filledRect);
            }

            using (Pen borderPen = new Pen(borderCol, 1))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, w - 1, h - 1);
            }
        }

        private void humuditybar_Paint(object sender, PaintEventArgs e)
        {
            Control barControl = sender as Control;
            if (barControl == null) return;

            float currentValue = 0;
            if (barControl.Tag != null)
            {
                try { currentValue = Convert.ToSingle(barControl.Tag); } catch { }
            }

            float maximumValue = 100f;
            currentValue = Math.Max(0, Math.Min(maximumValue, currentValue));

            int w = barControl.Width;
            int h = barControl.Height;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color panelBg = isDark ? Color.FromArgb(44, 52, 64) : Color.FromArgb(241, 245, 249);
            Color borderCol = isDark ? Color.FromArgb(71, 85, 104) : Color.LightGray;

            using (SolidBrush bgBrush = new SolidBrush(panelBg))
            {
                e.Graphics.FillRectangle(bgBrush, 0, 0, w, h);
            }

            float ratio = currentValue / maximumValue;
            int fillHeight = (int)(h * ratio);
            int yPos = h - fillHeight;

            Rectangle filledRect = new Rectangle(0, yPos, w, fillHeight);

            using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Blue))
            {
                e.Graphics.FillRectangle(brush, filledRect);
            }

            using (Pen borderPen = new Pen(borderCol, 1))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, w - 1, h - 1);
            }
        }

        private void lblMakineAdi_Click(object sender, EventArgs e) { }

        private void SafeInvoke(Action action)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                try { this.BeginInvoke(action); } catch (Exception) { }
            }
        }

        private void OnAlarmStateChanged(int machineId, FullMachineStatus status)
        {
            if (machineId == _machine.Id && this.IsHandleCreated && !this.IsDisposed)
                this.BeginInvoke(new Action(UpdateAlarmList));
        }

        private void UpdateAlarmList()
        {
            if (string.IsNullOrEmpty(_lastLoadedBatchIdForChart))
            {
                var activeAlarms = _pollingService.GetActiveAlarmsForMachine(_machine.Id);
                List<string> newAlarmList;

                if (activeAlarms.Any())
                    newAlarmList = activeAlarms.Select(a => $"#{a.AlarmNumber}: {a.AlarmText}").ToList();
                else
                    newAlarmList = new List<string> { $"{Resources.aktifalarmyok}" };

                if (!_currentlyDisplayedAlarms.SequenceEqual(newAlarmList))
                {
                    _currentlyDisplayedAlarms = newAlarmList;
                    lstAlarmlar.DataSource = _currentlyDisplayedAlarms;
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;

            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= OnDataRefreshed;
                _pollingService.OnMachineConnectionStateChanged -= OnConnectionStateChanged;
                _pollingService.OnActiveAlarmStateChanged -= OnAlarmStateChanged;
            }

            if (_uiUpdateTimer != null)
            {
                _uiUpdateTimer.Stop();
                _uiUpdateTimer.Dispose();
            }

            base.OnHandleDestroyed(e);
        }

        private void pnlAlarmsAndSteps_Paint(object sender, PaintEventArgs e) { }
    }
}