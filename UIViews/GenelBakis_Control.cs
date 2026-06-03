// UI/Views/GenelBakis_Control.cs
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices; // Windows DWM API için eklendi
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.Services;
using Telemetry.UI.Controls;
using Telemetry.UIControls;
using static Telemetry.Repositories.ProcessLogRepository;

namespace Telemetry.UI.Views
{
    public partial class GenelBakis_Control : UserControl
    {
        // =========================================================================
        // KÜRESEL ÇÖZÜM: WINFORMS SCROLLBARLARINI KARANLIK MODA ZORLAYAN WINDOWS API
        // =========================================================================
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private PlcPollingService _pollingService;
        private UtilityPollingService _utilityPollingService;
        private MachineRepository _machineRepository;
        private DashboardRepository _dashboardRepository;
        private AlarmRepository _alarmRepository;
        private ProcessLogRepository _logRepository;
        private ProductionRepository _productionRepository;
        private UtilityRepository _utilityRepository;

        private Dictionary<int, bool> _previousBatchStatuses;
        private readonly Dictionary<int, DashboardMachineCard_Control> _machineCards = new Dictionary<int, DashboardMachineCard_Control>();

        private System.Windows.Forms.Timer _uiUpdateTimer;
        private System.Windows.Forms.Timer _chartUpdateTimer;

        private Dictionary<int, DateTime> _utilityLastSeen = new Dictionary<int, DateTime>();
        private Dictionary<int, UtilityDashboardCard_Control> _utilityCards = new Dictionary<int, UtilityDashboardCard_Control>();

        private bool _isDashboardSetup = false;

        private KpiCard_Control _kpiTotalMachines;
        private KpiCard_Control _kpiOfflineMachines;
        private KpiCard_Control _kpiRunningMachines;
        private KpiCard_Control _kpiAlarmMachines;
        private KpiCard_Control _kpiManualMachines;
        private KpiCard_Control _kpiIdleMachines;

        public GenelBakis_Control()
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            InitializeComponent();

            this.BackColor = Color.Transparent;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            try
            {
                typeof(FlowLayoutPanel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, flpTopKpis, new object[] { true });
            }
            catch { }

            ApplyLocalization();
        }

        // =========================================================================
        // SCROLLBAR TEMALANDIRICI TETİKLEYİCİ
        // =========================================================================
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK)
            {
                ApplyDarkScrollbars(flpMachineGroups);
                ApplyDarkScrollbars(flpUtilityStrip);
                ApplyDarkScrollbars(this);
            }
        }

        private void ApplyDarkScrollbars(Control control)
        {
            if (control != null && control.IsHandleCreated && Environment.OSVersion.Version.Major >= 10)
            {
                int useImmersiveDarkMode = 1;
                // Windows 11 ve güncel Windows 10 için karanlık kaydırma çubuğu komutu
                DwmSetWindowAttribute(control.Handle, 20, ref useImmersiveDarkMode, sizeof(int));
                // Eski Windows 10 sürümleri için geri uyumluluk komutu
                DwmSetWindowAttribute(control.Handle, 19, ref useImmersiveDarkMode, sizeof(int));
            }
        }

        public void InitializeControl(
            PlcPollingService pollingService,
            MachineRepository machineRepo,
            DashboardRepository dashboardRepo,
            AlarmRepository alarmRepo,
            ProcessLogRepository logRepo,
            ProductionRepository productionRepo,
            UtilityRepository utilityrepo,
            UtilityPollingService utilityService)
        {
            _pollingService = pollingService;
            _machineRepository = machineRepo;
            _dashboardRepository = dashboardRepo;
            _alarmRepository = alarmRepo;
            _logRepository = logRepo;
            _productionRepository = productionRepo;
            _utilityRepository = utilityrepo;
            _utilityPollingService = utilityService;

            if (this.IsHandleCreated && !_isDashboardSetup)
            {
                SetupDashboard();
            }
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        private void GenelBakis_Control_Load(object sender, EventArgs e)
        {
            if (this.DesignMode) return;

            if (_pollingService == null || _machineRepository == null || _dashboardRepository == null)
                return;

            if (!_isDashboardSetup)
            {
                SetupDashboard();
            }
        }

        private void SetupDashboard()
        {
            try
            {
                this.SuspendLayout();

                InitializeKpiCards();

                if (flpMachineGroups != null)
                {
                    flpMachineGroups.FlowDirection = FlowDirection.TopDown; // Gruplar DİKEY dizilir
                    flpMachineGroups.WrapContents = false;
                    flpMachineGroups.AutoScroll = true; // Ana panelin DİKEY scrollu açık

                    flpMachineGroups.Resize += (s, ev) => AdjustMachineGroupWidths();
                }

                var cache = _pollingService?.MachineDataCache;
                if (cache != null)
                {
                    _previousBatchStatuses = cache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.IsInRecipeMode ?? false);
                }
                else
                {
                    _previousBatchStatuses = new Dictionary<int, bool>();
                }

                BuildMachineCards();
                BuildUtilityStrip();

                if (_utilityPollingService != null)
                {
                    _utilityPollingService.OnUtilityDataRefreshed -= UtilityService_OnDataRefreshed;
                    _utilityPollingService.OnUtilityDataRefreshed += UtilityService_OnDataRefreshed;
                }

                if (_uiUpdateTimer == null)
                {
                    _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                    _uiUpdateTimer.Tick += (s, a) => {
                        UpdateLiveMachineCardsAndSort();
                        UpdateKpiCards();
                        CheckUtilityConnections();
                    };
                }
                _uiUpdateTimer.Start();

                if (_chartUpdateTimer == null)
                {
                    _chartUpdateTimer = new System.Windows.Forms.Timer { Interval = 60000 };
                    _chartUpdateTimer.Tick += (s, a) => UpdateSidebarCharts();
                }
                _chartUpdateTimer.Start();

                UpdateLiveMachineCardsAndSort();
                UpdateKpiCards();
                UpdateSidebarCharts();

                _isDashboardSetup = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gösterge paneli (Dashboard) yüklenirken bir veri eksikliği oluştu:\n\n{ex.Message}\n\nİz: {ex.StackTrace}", "Kurulum Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void AdjustMachineGroupWidths()
        {
            if (flpMachineGroups == null) return;
            int safeWidth = flpMachineGroups.ClientSize.Width > 200 ? (flpMachineGroups.ClientSize.Width - 25) : 750;

            foreach (Control ctrl in flpMachineGroups.Controls)
            {
                if (ctrl is GroupBox gb)
                {
                    gb.Width = safeWidth;
                }
            }
        }

        private void UpdateLiveMachineCardsAndSort()
        {
            if (_pollingService == null || this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                var cache = _pollingService.MachineDataCache;
                if (cache == null || cache.IsEmpty) return;

                foreach (var kvp in _machineCards)
                {
                    int machineId = kvp.Key;
                    var card = kvp.Value;

                    if (card != null && !card.IsDisposed && cache.TryGetValue(machineId, out var status))
                    {
                        card.UpdateData(status, new List<ProcessDataPoint>());
                    }
                }

                if (flpMachineGroups != null)
                {
                    foreach (System.Windows.Forms.Control control in flpMachineGroups.Controls)
                    {
                        if (control is GroupBox gb && gb.Controls.Count > 0 && gb.Controls[0] is FlowLayoutPanel innerPanel)
                        {
                            SortMachinesInPanel(innerPanel);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void InitializeKpiCards()
        {
            if (flpTopKpis == null) return;

            if (_kpiTotalMachines == null)
            {
                _kpiTotalMachines = new KpiCard_Control();
                _kpiOfflineMachines = new KpiCard_Control();
                _kpiRunningMachines = new KpiCard_Control();
                _kpiAlarmMachines = new KpiCard_Control();
                _kpiManualMachines = new KpiCard_Control();
                _kpiIdleMachines = new KpiCard_Control();
            }

            if (!flpTopKpis.Controls.Contains(_kpiTotalMachines))
            {
                flpTopKpis.Controls.Clear();
                flpTopKpis.Controls.Add(_kpiTotalMachines);
                flpTopKpis.Controls.Add(_kpiOfflineMachines);
                flpTopKpis.Controls.Add(_kpiRunningMachines);
                flpTopKpis.Controls.Add(_kpiAlarmMachines);
                flpTopKpis.Controls.Add(_kpiManualMachines);
                flpTopKpis.Controls.Add(_kpiIdleMachines);
            }
        }

        private void BuildUtilityStrip()
        {
            if (_utilityRepository == null || flpUtilityStrip == null) return;

            flpUtilityStrip.SuspendLayout();
            flpUtilityStrip.Controls.Clear();
            _utilityCards.Clear();
            flpUtilityStrip.Visible = false;

            var lines = _utilityRepository.GetUtilityLines() ?? new List<UtilityLine>();

            foreach (var line in lines)
            {
                if (line == null) continue;

                var card = new UtilityDashboardCard_Control();
                var initialData = new UtilityDashboardDto { LineName = line.LineName };
                card.SetData(initialData);
                card.SetConnectionStatus(false);

                _utilityCards.Add(line.Id, card);
                flpUtilityStrip.Controls.Add(card);
            }

            flpUtilityStrip.ResumeLayout();
        }

        // =========================================================================
        // ÇÖZÜM: YATAY KAYDIRMA (HORIZONTAL SCROLLING) VE DİNAMİK SCROLLBAR TEMALANDIRMA
        // =========================================================================
        private void BuildMachineCards()
        {
            if (_machineRepository == null || flpMachineGroups == null) return;

            flpMachineGroups.SuspendLayout();
            _machineCards.Clear();
            flpMachineGroups.Controls.Clear();

            var allMachines = _machineRepository.GetAllEnabledMachines() ?? new List<Machine>();
            var machineCache = _pollingService?.MachineDataCache;

            var groupedMachines = allMachines
                 .Where(m => m != null)
                 .GroupBy(m => string.IsNullOrWhiteSpace(m.MachineHall) ? "Tümü" : m.MachineHall)
                 .OrderBy(g => g.Key);

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color gbFg = isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(51, 65, 85);

            int safeWidth = flpMachineGroups.ClientSize.Width > 200 ? (flpMachineGroups.ClientSize.Width - 25) : 750;

            foreach (var group in groupedMachines)
            {
                var groupPanel = new GroupBox
                {
                    Text = $"{group.Key} ",
                    Width = safeWidth,
                    Height = 275, // YATAY KAYDIRMA ÇUBUĞUNA YER AÇMAK İÇİN YÜKSEKLİK SABİTLENDİ
                    Font = new System.Drawing.Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                    ForeColor = gbFg,
                    BackColor = Color.Transparent,
                    Padding = new Padding(6, 15, 6, 6)
                };

                var innerPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false, // KARTLAR ASLA ALT SATIRA GEÇMEZ, YAN YANA DİZİLİR
                    AutoScroll = true,    // SIĞMAYAN KARTLAR İÇİN YATAY KAYDIRMA ÇUBUĞU AÇILIR
                    BackColor = Color.Transparent
                };

                // Dinamik olarak oluşturulan iç panelin scrollbar'ını karanlık moda boya
                innerPanel.HandleCreated += (s, ev) =>
                {
                    if (MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK)
                    {
                        ApplyDarkScrollbars((Control)s);
                    }
                };

                var sortedMachines = group.OrderBy(m =>
                {
                    if (machineCache != null && machineCache.TryGetValue(m.Id, out var status))
                        return GetSortPriority(status);
                    return 100;
                }).ToList();

                foreach (var machine in sortedMachines)
                {
                    var card = new DashboardMachineCard_Control(machine);
                    card.Tag = machine.Id;
                    card.Margin = new Padding(8);

                    _machineCards[machine.Id] = card;
                    innerPanel.Controls.Add(card);
                }

                groupPanel.Controls.Add(innerPanel);
                flpMachineGroups.Controls.Add(groupPanel);
            }

            flpMachineGroups.ResumeLayout();
        }

        private void UtilityService_OnDataRefreshed(List<UtilityLog> logs)
        {
            if (this.IsDisposed || !this.IsHandleCreated || logs == null) return;

            this.BeginInvoke(new Action(() =>
            {
                foreach (var log in logs)
                {
                    if (log != null && _utilityCards.TryGetValue(log.LineId, out var card))
                    {
                        _utilityLastSeen[log.LineId] = DateTime.Now;
                        card.SetConnectionStatus(true);

                        var dto = new UtilityDashboardDto
                        {
                            LineName = "",
                            DailyElecUsage = log.ElecCounter,
                            DailyWaterUsage = log.WaterCounter,
                            DailySteamUsage = log.SteamCounter,
                            DailyAirUsage = log.AirCounter
                        };
                        card.SetData(dto);
                    }
                }
            }));
        }

        private void CheckUtilityConnections()
        {
            var now = DateTime.Now;
            foreach (var kvp in _utilityCards)
            {
                int lineId = kvp.Key;
                var card = kvp.Value;

                if (!_utilityLastSeen.ContainsKey(lineId) || (now - _utilityLastSeen[lineId]).TotalSeconds > 25)
                {
                    card.SetConnectionStatus(false);
                }
            }
        }

        private void SortMachinesInPanel(FlowLayoutPanel panel)
        {
            if (panel == null || _pollingService == null) return;

            var cards = panel.Controls.OfType<DashboardMachineCard_Control>().ToList();

            var sortedCards = cards.OrderBy(c =>
            {
                if (c.Tag is int mId && _pollingService.MachineDataCache != null && _pollingService.MachineDataCache.TryGetValue(mId, out var status))
                {
                    return GetSortPriority(status);
                }
                return 100;
            }).ToList();

            for (int i = 0; i < sortedCards.Count; i++)
            {
                if (panel.Controls.GetChildIndex(sortedCards[i]) != i)
                {
                    panel.Controls.SetChildIndex(sortedCards[i], i);
                }
            }
        }

        private int GetSortPriority(FullMachineStatus status)
        {
            if (status == null) return 100;
            if (status.ConnectionState == ConnectionStatus.Connected && (status.IsInRecipeMode || status.manuel_status)) return 1;
            if (status.ConnectionState == ConnectionStatus.Connected && status.HasActiveAlarm) return 2;
            if (status.ConnectionState == ConnectionStatus.Connected) return 3;
            return 4;
        }

        private void UpdateKpiCards()
        {
            if (_pollingService == null || _kpiTotalMachines == null || this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                var cache = _pollingService.MachineDataCache;
                if (cache == null) return;

                var allStatuses = cache.Values;
                if (allStatuses == null || !allStatuses.Any()) return;

                int totalMachines = 0, offlineMachines = 0, runningMachines = 0, alarmMachines = 0, manualMachines = 0, idleMachines = 0;

                foreach (var s in allStatuses)
                {
                    if (s == null) continue;

                    totalMachines++;
                    if (s.ConnectionState != ConnectionStatus.Connected) offlineMachines++;
                    else
                    {
                        if (s.HasActiveAlarm) alarmMachines++;
                        else if (s.IsInRecipeMode) runningMachines++;
                        else if (s.manuel_status) manualMachines++;
                        else idleMachines++;
                    }
                }

                _kpiTotalMachines.SetData($"{Resources.AllMachines}", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
                _kpiOfflineMachines.SetData("Offline Status", offlineMachines.ToString(), Color.FromArgb(144, 164, 174));
                _kpiRunningMachines.SetData($"{Resources.aktifüretim}", runningMachines.ToString(), Color.FromArgb(76, 175, 80));
                _kpiAlarmMachines.SetData($"{Resources.alarmdurum}", alarmMachines.ToString(), Color.FromArgb(211, 47, 47));
                _kpiManualMachines.SetData("Manuel Mode", manualMachines.ToString(), Color.FromArgb(142, 68, 173));
                _kpiIdleMachines.SetData($"{Resources.bosbekleyen}", idleMachines.ToString(), Color.FromArgb(230, 126, 34));
            }
            catch (Exception) { }
        }

        private async void UpdateSidebarCharts()
        {
            if (_dashboardRepository == null || _alarmRepository == null || this.IsDisposed) return;

            try
            {
                var result = await Task.Run(() =>
                {
                    var today = DateTime.Today;
                    var now = DateTime.Now;
                    return new
                    {
                        ConsumptionData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        TopAlarms = _alarmRepository.GetTopAlarmsByFrequency(now.AddDays(-1), now),
                        OeeData = _dashboardRepository.GetHourlyAverageOee(today)
                    };
                });

                if (this.IsDisposed || !this.IsHandleCreated) return;

                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

                if (formsPlotHourly != null)
                {
                    formsPlotHourly.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotHourly, isDark);
                    if (result.ConsumptionData != null && result.ConsumptionData.Rows.Count > 0)
                    {
                        double[] hours = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                        double[] consumption = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("ToplamElektrik") ? 0.0 : Convert.ToDouble(row["ToplamElektrik"]) / 1000.0).ToArray();

                        var barPlot = formsPlotHourly.Plot.Add.Scatter(hours, consumption);
                        barPlot.Color = ScottPlot.Colors.SteelBlue;
                        barPlot.MarkerSize = 0;
                        formsPlotHourly.Plot.Axes.Left.Label.Text = "kWh";
                    }
                    formsPlotHourly.Plot.Axes.AutoScale();
                    formsPlotHourly.Refresh();
                }

                if (formsPlotHourlyWater != null)
                {
                    formsPlotHourlyWater.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotHourlyWater, isDark);
                    if (result.ConsumptionData != null && result.ConsumptionData.Rows.Count > 0)
                    {
                        double[] hours = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                        double[] consumption = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("ToplamSu") ? 0.0 : Convert.ToDouble(row["ToplamSu"]) / 1000.0).ToArray();

                        var barPlot = formsPlotHourlyWater.Plot.Add.Scatter(hours, consumption);
                        barPlot.Color = ScottPlot.Colors.CornflowerBlue;
                        barPlot.MarkerSize = 0;
                        formsPlotHourlyWater.Plot.Axes.Left.Label.Text = "m³";
                    }
                    formsPlotHourlyWater.Plot.Axes.AutoScale();
                    formsPlotHourlyWater.Refresh();
                }

                if (formsPlotHourlySteam != null)
                {
                    formsPlotHourlySteam.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotHourlySteam, isDark);
                    if (result.ConsumptionData != null && result.ConsumptionData.Rows.Count > 0)
                    {
                        double[] hours = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                        double[] consumption = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("ToplamBuhar") ? 0.0 : Convert.ToDouble(row["ToplamBuhar"]) / 1000.0).ToArray();

                        var barPlot = formsPlotHourlySteam.Plot.Add.Scatter(hours, consumption);
                        barPlot.Color = ScottPlot.Colors.LightSlateGray;
                        barPlot.MarkerSize = 0;
                        formsPlotHourlySteam.Plot.Axes.Left.Label.Text = "m³";
                    }
                    formsPlotHourlySteam.Plot.Axes.AutoScale();
                    formsPlotHourlySteam.Refresh();
                }

                if (formsPlotTopAlarms != null)
                {
                    formsPlotTopAlarms.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotTopAlarms, isDark);
                    if (result.TopAlarms != null && result.TopAlarms.Any())
                    {
                        double[] counts = result.TopAlarms.Select(a => (double)a.Count).ToArray();
                        var labels = result.TopAlarms.Select(a => a.AlarmText).ToArray();
                        var barPlot = formsPlotTopAlarms.Plot.Add.Bars(counts);
                        barPlot.Color = ScottPlot.Colors.Crimson;

                        var ticks = Enumerable.Range(0, labels.Length).Select(i => new ScottPlot.Tick(i, labels[i])).ToArray();
                        formsPlotTopAlarms.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                        formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Rotation = -90;
                        formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.LowerRight;
                        formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.FontSize = 11;
                        formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Bold = true;
                        formsPlotTopAlarms.Plot.Axes.Bottom.MinimumSize = 160;
                    }
                    formsPlotTopAlarms.Plot.Axes.AutoScale();
                    formsPlotTopAlarms.Refresh();
                }

                if (formsPlotHourlyOee != null)
                {
                    formsPlotHourlyOee.Plot.Clear();
                    ApplyScottPlotTheme(formsPlotHourlyOee, isDark);
                    if (result.OeeData != null && result.OeeData.Rows.Count > 0)
                    {
                        double[] hours = result.OeeData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                        double[] oeeValues = result.OeeData.AsEnumerable().Select(row => row.IsNull("AverageOEE") ? 0.0 : Convert.ToDouble(row["AverageOEE"])).ToArray();

                        var linePlot = formsPlotHourlyOee.Plot.Add.Scatter(hours, oeeValues);
                        linePlot.Color = ScottPlot.Colors.Gold;
                        linePlot.LineStyle.Width = 2;
                        linePlot.MarkerStyle.Size = 0;
                        formsPlotHourlyOee.Plot.Axes.Bottom.Label.Text = "Saat";
                    }
                    formsPlotHourlyOee.Plot.Axes.AutoScale();
                    formsPlotHourlyOee.Refresh();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"CRITICAL REFRESH ERROR: {ex}");
            }
        }

        private void ApplyScottPlotTheme(ScottPlot.WinForms.FormsPlot formsPlot, bool isDark)
        {
            if (formsPlot == null || formsPlot.IsDisposed) return;

            formsPlot.Plot.FigureBackground.Color = ScottPlot.Colors.Transparent;
            formsPlot.Plot.DataBackground.Color = ScottPlot.Colors.Transparent;

            var axisColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(148, 163, 184)) : ScottPlot.Color.FromColor(Color.FromArgb(71, 85, 105));
            var gridColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(51, 65, 85)) : ScottPlot.Color.FromColor(Color.FromArgb(241, 245, 249));

            formsPlot.Plot.Axes.Color(axisColor);
            formsPlot.Plot.Grid.MajorLineColor = gridColor;

            formsPlot.Plot.Axes.Left.Label.ForeColor = axisColor;
            formsPlot.Plot.Axes.Bottom.Label.ForeColor = axisColor;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;

            if (_utilityPollingService != null)
            {
                _utilityPollingService.OnUtilityDataRefreshed -= UtilityService_OnDataRefreshed;
            }

            _uiUpdateTimer?.Stop();
            _uiUpdateTimer?.Dispose();

            _chartUpdateTimer?.Stop();
            _chartUpdateTimer?.Dispose();

            base.OnHandleDestroyed(e);
        }

        public void ApplyLocalization()
        {
            gbHourlyConsumption.Text = "Hourly Electricity (kWh)";
            gbTopAlarms.Text = Resources.ensikalarm;
            gbHourlyConsumptionWater.Text = "Hourly Water (m³)";
            gbHourlyConsumptionSteam.Text = "Hourly Steam (m³)";
            gbHourlyOee.Text = "24 Hourly OEE";
        }
    }
}