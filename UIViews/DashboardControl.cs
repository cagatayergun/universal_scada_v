// UI/Views/DashboardControl.cs
using System;
using System.Collections.Concurrent; // Added for live data cache
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.Core.Repositories;
using Universalscada.Models;
using Universalscada.Properties;
using Universalscada.Repositories;
// using Universalscada.Services; // REMOVED
using Universalscada.UI.Controls;

namespace Universalscada.UI.Views
{
    // Renamed from GenelBakis_Control for global sector appeal
    public partial class DashboardControl : UserControl // CLASS NAME CHANGED
    {
        // === REMOVED ===
        // private PlcPollingService _pollingService;

        // === NEW ===
        // This UserControl's own live data cache
        private readonly ConcurrentDictionary<int, FullMachineStatus> _machineDataCache = new ConcurrentDictionary<int, FullMachineStatus>();

        private MachineRepository _machineRepository;
        private DashboardRepository _dashboardRepository;
        private AlarmRepository _alarmRepository;
        private ProcessLogRepository _logRepository;
        private ProductionRepository _productionRepository;
        private Dictionary<int, bool> _previousBatchStatuses;
        private readonly Dictionary<int, DashboardMachineCard_Control> _machineCards = new Dictionary<int, DashboardMachineCard_Control>();
        private System.Windows.Forms.Timer _uiUpdateTimer;

        // ... (KPI cards and Color list remain the same) ...
        private KpiCard_Control _kpiTotalMachines;
        private KpiCard_Control _kpiRunningMachines;
        private KpiCard_Control _kpiAlarmMachines;
        private KpiCard_Control _kpiIdleMachines;
        private Random _random = new Random();
        private readonly List<Color> _darkColors = new List<Color>
        {
            Color.FromArgb(44, 62, 80), Color.FromArgb(46, 204, 113),
            Color.FromArgb(231, 76, 60), Color.FromArgb(155, 89, 182),
            Color.FromArgb(52, 152, 219), Color.FromArgb(241, 196, 15),
            Color.FromArgb(22, 160, 133), Color.FromArgb(192, 57, 43),
            Color.FromArgb(41, 128, 185), Color.FromArgb(243, 156, 18),
            Color.FromArgb(211, 84, 0), Color.FromArgb(127, 140, 141),
            Color.FromArgb(52, 73, 94), Color.FromArgb(249, 105, 14),
            Color.FromArgb(189, 195, 199), Color.FromArgb(149, 165, 166),
            Color.FromArgb(236, 240, 241), Color.FromArgb(101, 159, 105),
            Color.FromArgb(10, 61, 98), Color.FromArgb(119, 177, 169)
        };
        private int _colorIndex = 0;


        public DashboardControl() // CONSTRUCTOR NAME CHANGED
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            typeof(FlowLayoutPanel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, flpTopKpis, new object[] { true });

            ApplyLocalization();
        }

        // === MODIFIED: InitializeControl - PlcPollingService parameter removed
        public void InitializeControl(MachineRepository machineRepo, DashboardRepository dashboardRepo, AlarmRepository alarmRepo, ProcessLogRepository logRepo, ProductionRepository productionRepo)
        {
            // _pollingService = pollingService; // REMOVED
            _machineRepository = machineRepo;
            _dashboardRepository = dashboardRepo;
            _alarmRepository = alarmRepo;
            _logRepository = logRepo;
            _productionRepository = productionRepo;
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        private void DashboardControl_Load(object sender, EventArgs e) // LOAD METHOD NAME CHANGED
        {
            if (this.DesignMode) return;

            InitializeKpiCards();

            // === MODIFIED ===
            _previousBatchStatuses = new Dictionary<int, bool>();

            BuildMachineCards();

            // _pollingService.OnMachineDataRefreshed += PollingService_OnMachineDataRefreshed; // REMOVED

            _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            _uiUpdateTimer.Tick += (s, a) => RefreshDashboard();
            _uiUpdateTimer.Start();

            RefreshDashboard();
        }

        // ... (InitializeKpiCards method remains the same) ...
        private void InitializeKpiCards()
        {
            _kpiTotalMachines = new KpiCard_Control();
            _kpiRunningMachines = new KpiCard_Control();
            _kpiAlarmMachines = new KpiCard_Control();
            _kpiIdleMachines = new KpiCard_Control();
            flpTopKpis.Controls.Add(_kpiTotalMachines);
            flpTopKpis.Controls.Add(_kpiRunningMachines);
            flpTopKpis.Controls.Add(_kpiAlarmMachines);
            flpTopKpis.Controls.Add(_kpiIdleMachines);
        }

        private void BuildMachineCards()
        {
            flpMachineGroups.SuspendLayout();

            var allMachines = _machineRepository.GetAllEnabledMachines();
            _machineCards.Clear();
            flpMachineGroups.Controls.Clear();

            // === MODIFIED ===
            // Use _machineDataCache directly
            var machineCache = _machineDataCache;

            // ... (Sorting logic remains the same) ...
            var sortedMachines = allMachines
                .OrderByDescending(m =>
                {
                    if (machineCache.TryGetValue(m.Id, out var status))
                    {
                        return status.IsInRecipeMode;
                    }
                    return false;
                })
                .ThenByDescending(m =>
                {
                    if (machineCache.TryGetValue(m.Id, out var status) && status.IsInRecipeMode)
                    {
                        var batchTimes = _productionRepository.GetBatchTimestamps(status.BatchNumarasi, m.Id);
                        return batchTimes.StartTime ?? DateTime.MinValue;
                    }
                    return DateTime.MinValue;
                });

            // UPDATED: Resources.diger -> Resources.OtherMachineType
            var groupedMachines = sortedMachines
                .GroupBy(m => m.MachineSubType ?? $"{Resources.OtherMachineType}");
            _colorIndex = 0;
            foreach (var group in groupedMachines)
            {
                // ... (GroupBox and innerPanel creation codes remain the same) ...
                var groupPanel = new GroupBox
                {
                    Text = group.Key,
                    Width = flpMachineGroups.Width - 25,
                    AutoSize = true,
                    Font = new Font("Times New Roman", 13F, FontStyle.Bold),
                    ForeColor = Color.White,
                };

                Color groupColor = _darkColors[_colorIndex % _darkColors.Count];
                groupPanel.BackColor = groupColor;

                var innerPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Padding = new Padding(5),
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoScroll = true
                };

                foreach (var machine in group)
                {
                    var card = new DashboardMachineCard_Control(machine);
                    _machineCards.Add(machine.Id, card);
                    innerPanel.Controls.Add(card);
                }
                groupPanel.Controls.Add(innerPanel);
                flpMachineGroups.Controls.Add(groupPanel);
                _colorIndex++;
            }
            flpMachineGroups.ResumeLayout();
        }

        private void RefreshDashboard()
        {
            if (this.IsDisposed) return;

            // === MODIFIED ===
            // Use _machineDataCache directly
            var currentBatchStatuses = _machineDataCache
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsInRecipeMode);

            if (!_previousBatchStatuses.SequenceEqual(currentBatchStatuses))
            {
                BuildMachineCards();
                _previousBatchStatuses = currentBatchStatuses;
            }

            UpdateKpiCards();
            UpdateSidebarCharts();
        }

        // === MODIFIED: Public method replaces PollingService event handler
        // Public method to receive push data (e.g., from SignalR)
        public void UpdateMachineStatus(FullMachineStatus status)
        {
            if (status == null) return;

            // 1. Store the incoming data in the local cache
            _machineDataCache[status.MachineId] = status;

            // 2. Find and update the relevant card
            if (_machineCards.TryGetValue(status.MachineId, out var cardToUpdate))
            {
                List<ProcessDataPoint> trendData;

                if (!string.IsNullOrEmpty(status.BatchNumarasi))
                {
                    trendData = _logRepository.GetLogsForBatch(status.MachineId, status.BatchNumarasi, DateTime.Now.AddMinutes(-15), DateTime.Now);
                }
                else
                {
                    trendData = _logRepository.GetManualLogs(status.MachineId, DateTime.Now.AddMinutes(-15), DateTime.Now);
                }

                // Update the DashboardMachineCard_Control on the UI thread
                if (cardToUpdate.InvokeRequired)
                {
                    cardToUpdate.Invoke(new Action(() => cardToUpdate.UpdateData(status, trendData)));
                }
                else
                {
                    cardToUpdate.UpdateData(status, trendData);
                }
            }
        }

        // === UPDATED: UpdateKpiCards ===
        // Uses new resource keys
        private void UpdateKpiCards()
        {
            // Get data from local cache
            var allStatuses = _machineDataCache.Values;

            int totalMachines = allStatuses.Count;
            int runningMachines = allStatuses.Count(s => s.IsInRecipeMode && !s.HasActiveAlarm);
            int alarmMachines = allStatuses.Count(s => s.HasActiveAlarm);
            int idleMachines = totalMachines - runningMachines - alarmMachines;

            // Update KPI cards with new localized text keys
            _kpiTotalMachines.SetData($"{Resources.TotalMachinesCountLabel}", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
            _kpiRunningMachines.SetData($"{Resources.ActiveProductionKpi}", runningMachines.ToString(), Color.FromArgb(46, 204, 113));
            _kpiAlarmMachines.SetData($"{Resources.AlarmStatusKpi}", alarmMachines.ToString(), Color.FromArgb(231, 76, 60));
            _kpiIdleMachines.SetData($"{Resources.IdleWaitingKpi}", idleMachines.ToString(), Color.FromArgb(243, 156, 18));
        }

        // ... (UpdateSidebarCharts method remains the same) ...
        private void UpdateSidebarCharts()
        {
            var hourlyElecData = _dashboardRepository.GetHourlyFactoryConsumption(DateTime.Today);
            formsPlotHourly.Plot.Clear();
            if (hourlyElecData.Rows.Count > 0)
            {
                double[] hours = hourlyElecData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                double[] consumption = hourlyElecData.AsEnumerable().Select(row => row.IsNull("ToplamElektrik") ? 0.0 : Convert.ToDouble(row["ToplamElektrik"])).ToArray();
                var barPlot = formsPlotHourly.Plot.Add.Scatter(hours, consumption);
                barPlot.Color = ScottPlot.Colors.SteelBlue;
            }
            formsPlotHourly.Plot.Axes.AutoScale();
            formsPlotHourly.Refresh();

            var hourlyWaterData = _dashboardRepository.GetHourlyFactoryConsumption(DateTime.Today);
            formsPlotHourlyWater.Plot.Clear();
            if (hourlyWaterData.Rows.Count > 0)
            {
                double[] hours = hourlyWaterData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                double[] consumption = hourlyWaterData.AsEnumerable().Select(row => row.IsNull("ToplamSu") ? 0.0 : Convert.ToDouble(row["ToplamSu"])).ToArray();
                var barPlot = formsPlotHourlyWater.Plot.Add.Scatter(hours, consumption);
                barPlot.Color = ScottPlot.Colors.CornflowerBlue;
            }
            formsPlotHourlyWater.Plot.Axes.AutoScale();
            formsPlotHourlyWater.Refresh();

            var hourlySteamData = _dashboardRepository.GetHourlyFactoryConsumption(DateTime.Today);
            formsPlotHourlySteam.Plot.Clear();
            if (hourlySteamData.Rows.Count > 0)
            {
                double[] hours = hourlySteamData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                double[] consumption = hourlySteamData.AsEnumerable().Select(row => row.IsNull("ToplamBuhar") ? 0.0 : Convert.ToDouble(row["ToplamBuhar"])).ToArray();
                var barPlot = formsPlotHourlySteam.Plot.Add.Scatter(hours, consumption);
                barPlot.Color = ScottPlot.Colors.DimGray;
            }
            formsPlotHourlySteam.Plot.Axes.AutoScale();
            formsPlotHourlySteam.Refresh();

            var topAlarms = _alarmRepository.GetTopAlarmsByFrequency(DateTime.Now.AddDays(-1), DateTime.Now);
            formsPlotTopAlarms.Plot.Clear();
            if (topAlarms.Any())
            {
                double[] counts = topAlarms.Select(a => (double)a.Count).ToArray();
                var labels = topAlarms.Select(a => a.AlarmText).ToArray();
                var barPlot = formsPlotTopAlarms.Plot.Add.Bars(counts);
                barPlot.Color = ScottPlot.Colors.OrangeRed;
                var ticks = Enumerable.Range(0, labels.Length).Select(i => new ScottPlot.Tick(i, labels[i])).ToArray();
                formsPlotTopAlarms.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            }
            formsPlotTopAlarms.Plot.Axes.AutoScale();
            formsPlotTopAlarms.Refresh();

            var hourlyOeeData = _dashboardRepository.GetHourlyAverageOee(DateTime.Today);
            formsPlotHourlyOee.Plot.Clear();
            if (hourlyOeeData.Rows.Count > 0)
            {
                double[] hours = hourlyOeeData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                double[] oeeValues = hourlyOeeData.AsEnumerable().Select(row => row.IsNull("AverageOEE") ? 0.0 : Convert.ToDouble(row["AverageOEE"])).ToArray();
                var linePlot = formsPlotHourlyOee.Plot.Add.Scatter(hours, oeeValues);
                linePlot.Color = ScottPlot.Colors.Orange;
                linePlot.LineStyle.Width = 2;
                linePlot.MarkerStyle.Shape = ScottPlot.MarkerShape.FilledCircle;
                linePlot.MarkerStyle.Size = 5;
                formsPlotHourlyOee.Plot.Axes.Bottom.Label.Text = "Saat";
            }
            formsPlotHourlyOee.Plot.Axes.AutoScale();
            formsPlotHourlyOee.Refresh();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // === REMOVED ===
            // if (_pollingService != null)
            // {
            //     _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
            // }

            _uiUpdateTimer?.Stop();
            _uiUpdateTimer?.Dispose();
            base.OnHandleDestroyed(e);
        }

        // === UPDATED: ApplyLocalization ===
        // Uses new resource keys
        public void ApplyLocalization()
        {
            gbHourlyConsumption.Text = Resources.HourlyElectricityConsumptionChartTitle;
            gbTopAlarms.Text = Resources.TopAlarmsLast24HoursTitle;
            gbHourlyConsumptionWater.Text = Resources.AverageWaterConsumptionTitle;
            gbHourlyConsumptionSteam.Text = Resources.AverageSteamConsumptionTitle;
            gbHourlyOee.Text = "24 Hourly OEE"; // Consider adding this to the resource file for full localization
        }
    }
}