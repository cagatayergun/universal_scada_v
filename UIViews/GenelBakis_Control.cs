// UI/Views/GenelBakis_Control.cs
using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        // Utility (Enerji) Takibi İçin
        private Dictionary<int, DateTime> _utilityLastSeen = new Dictionary<int, DateTime>();
        private Dictionary<int, UtilityDashboardCard_Control> _utilityCards = new Dictionary<int, UtilityDashboardCard_Control>();

        private bool _isDashboardSetup = false;

        // KPI Kartları
        private KpiCard_Control _kpiTotalMachines;
        private KpiCard_Control _kpiOfflineMachines;
        private KpiCard_Control _kpiRunningMachines;
        private KpiCard_Control _kpiAlarmMachines;
        private KpiCard_Control _kpiManualMachines;
        private KpiCard_Control _kpiIdleMachines;

        private readonly List<Color> _darkColors = new List<Color>
        {
            Color.FromArgb(44, 62, 80), Color.FromArgb(46, 204, 113), Color.FromArgb(231, 76, 60),
            Color.FromArgb(155, 89, 182), Color.FromArgb(52, 152, 219), Color.FromArgb(241, 196, 15),
            Color.FromArgb(22, 160, 133), Color.FromArgb(192, 57, 43), Color.FromArgb(41, 128, 185),
            Color.FromArgb(243, 156, 18), Color.FromArgb(211, 84, 0), Color.FromArgb(127, 140, 141),
            Color.FromArgb(52, 73, 94), Color.FromArgb(249, 105, 14), Color.FromArgb(189, 195, 199),
            Color.FromArgb(149, 165, 166), Color.FromArgb(236, 240, 241), Color.FromArgb(101, 159, 105),
            Color.FromArgb(10, 61, 98), Color.FromArgb(119, 177, 169)
        };
        private int _colorIndex = 0;

        public GenelBakis_Control()
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            InitializeComponent();

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
                InitializeKpiCards();

                // Ana Panel Ayarları (Dikey Sıralama)
                flpMachineGroups.FlowDirection = FlowDirection.TopDown;
                flpMachineGroups.WrapContents = false;
                flpMachineGroups.AutoScroll = true;

                _previousBatchStatuses = _pollingService.MachineDataCache
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsInRecipeMode);

                BuildMachineCards();
                BuildUtilityStrip();

                // KRİTİK PERFORMANS REVİZYONU: Anlık veri yenileme event aboneliği tamamen kaldırıldı.
                // Saniyede 400 makineden gelen veri seli arayüzü kilitlemeyecektir.

                // Enerji Servisi Olay Aboneliği (Geniş periyotlu olduğu için kalabilir)
                if (_utilityPollingService != null)
                {
                    _utilityPollingService.OnUtilityDataRefreshed -= UtilityService_OnDataRefreshed;
                    _utilityPollingService.OnUtilityDataRefreshed += UtilityService_OnDataRefreshed;
                }

                if (_uiUpdateTimer == null)
                {
                    // OPTİMİZASYON: 1000ms (1 saniye) aralıklarla uyanan merkezi PULL timer'ı.
                    _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                    _uiUpdateTimer.Tick += (s, a) => {
                        UpdateLiveMachineCardsAndSort(); // Kartları toplu ve sarsıntısız hafızadan güncelle
                        UpdateKpiCards();                // KPI kartlarını tek döngüde hesaplayıp güncelle
                        CheckUtilityConnections();       // Enerji cihaz bağlantılarını denetle
                    };
                }
                _uiUpdateTimer.Start();

                // GRAFİKLER İÇİN BAĞIMSIZ ULTRA YAVAŞ TIMER (60 saniyede bir çizim yükü getirir)
                System.Windows.Forms.Timer chartUpdateTimer = new System.Windows.Forms.Timer { Interval = 60000 };
                chartUpdateTimer.Tick += (s, a) => UpdateSidebarCharts();
                chartUpdateTimer.Start();

                // İlk açılışta bir kez el ile tetikleyin (RAM verileriyle hızlı dolum)
                UpdateLiveMachineCardsAndSort();
                UpdateKpiCards();
                UpdateSidebarCharts();

                _isDashboardSetup = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard kurulum hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Saniyede 1 kez uyanarak 400 makinenin grafik ve metin verilerini doğrudan RAM'den çeker ve panelleri sıralar.
        /// </summary>
        private void UpdateLiveMachineCardsAndSort()
        {
            if (_pollingService == null || this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                var cache = _pollingService.MachineDataCache;
                if (cache == null || cache.IsEmpty) return;

                // 1. ADIM: Tüm kartları Invoke maliyeti olmadan doğrudan güncelle
                foreach (var kvp in _machineCards)
                {
                    int machineId = kvp.Key;
                    var card = kvp.Value;

                    if (!card.IsDisposed && cache.TryGetValue(machineId, out var status))
                    {
                        card.UpdateData(status, new List<ProcessDataPoint>());
                    }
                }

                // 2. ADIM: Her hol panelindeki kartları sıralama önceliğine göre yerleştir (Görünür performansı korur)
                // DÜZELTME: Belirsizliği gidermek için System.Windows.Forms.Control olarak açıkça belirttik
                foreach (System.Windows.Forms.Control control in flpMachineGroups.Controls)
                {
                    if (control is GroupBox gb && gb.Controls.Count > 0 && gb.Controls[0] is FlowLayoutPanel innerPanel)
                    {
                        SortMachinesInPanel(innerPanel);
                    }
                }
            }
            catch (Exception)
            {
                // UI akışının kopmaması için
            }
        }

        private void InitializeKpiCards()
        {
            if (_kpiTotalMachines != null && flpTopKpis.Controls.Contains(_kpiTotalMachines)) return;

            if (_kpiTotalMachines == null)
            {
                _kpiTotalMachines = new KpiCard_Control();
                _kpiOfflineMachines = new KpiCard_Control();
                _kpiRunningMachines = new KpiCard_Control();
                _kpiAlarmMachines = new KpiCard_Control();
                _kpiManualMachines = new KpiCard_Control();
                _kpiIdleMachines = new KpiCard_Control();
            }

            flpTopKpis.Controls.Clear();
            flpTopKpis.Controls.Add(_kpiTotalMachines);
            flpTopKpis.Controls.Add(_kpiOfflineMachines);
            flpTopKpis.Controls.Add(_kpiRunningMachines);
            flpTopKpis.Controls.Add(_kpiAlarmMachines);
            flpTopKpis.Controls.Add(_kpiManualMachines);
            flpTopKpis.Controls.Add(_kpiIdleMachines);
        }

        private void BuildUtilityStrip()
        {
            if (_utilityRepository == null) return;

            flpUtilityStrip.SuspendLayout();
            flpUtilityStrip.Controls.Clear();
            _utilityCards.Clear();
            flpUtilityStrip.Visible = false;

            var lines = _utilityRepository.GetUtilityLines();

            foreach (var line in lines)
            {
                var card = new UtilityDashboardCard_Control();
                var initialData = new UtilityDashboardDto { LineName = line.LineName };
                card.SetData(initialData);
                card.SetConnectionStatus(false);

                _utilityCards.Add(line.Id, card);
                flpUtilityStrip.Controls.Add(card);
            }

            flpUtilityStrip.ResumeLayout();
        }

        private void BuildMachineCards()
        {
            if (_machineRepository == null || _pollingService == null) return;

            flpMachineGroups.SuspendLayout();
            _machineCards.Clear();
            flpMachineGroups.Controls.Clear();

            var allMachines = _machineRepository.GetAllEnabledMachines();
            var machineCache = _pollingService.MachineDataCache;

            var groupedMachines = allMachines
                 .GroupBy(m => string.IsNullOrWhiteSpace(m.MachineHall) ? "Empty" : m.MachineHall)
                 .OrderBy(g => g.Key);

            _colorIndex = 0;

            foreach (var group in groupedMachines)
            {
                var groupPanel = new GroupBox
                {
                    Text = $"{group.Key} ",
                    Width = flpMachineGroups.ClientSize.Width * 2 - 50,
                    Height = 265,
                    Font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = _darkColors[_colorIndex % _darkColors.Count],
                    Padding = new Padding(5, 5, 5, 5)
                };

                var innerPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoScroll = true,
                    BackColor = Color.WhiteSmoke
                };

                var sortedMachines = group.OrderBy(m =>
                {
                    if (machineCache.TryGetValue(m.Id, out var status))
                        return GetSortPriority(status);
                    return 100;
                }).ToList();

                foreach (var machine in sortedMachines)
                {
                    var card = new DashboardMachineCard_Control(machine);
                    card.Tag = machine.Id;
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
            if (!_isDashboardSetup || _pollingService == null) return;

            UpdateLiveMachineCardsAndSort();
            UpdateKpiCards();
            CheckUtilityConnections();
            UpdateSidebarCharts();
        }

        private void UtilityService_OnDataRefreshed(List<UtilityLog> logs)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                foreach (var log in logs)
                {
                    if (_utilityCards.TryGetValue(log.LineId, out var card))
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

        // DİKKAT: PollingService_OnMachineDataRefreshed metodu artık event dinlemediğimiz için gövdesi boş bırakılmıştır, uyumluluk için silinmemiştir.
        private void PollingService_OnMachineDataRefreshed(int machineId, FullMachineStatus status) { }

        private void SortMachinesInPanel(FlowLayoutPanel panel)
        {
            var cards = panel.Controls.OfType<DashboardMachineCard_Control>().ToList();

            var sortedCards = cards.OrderBy(c =>
            {
                if (c.Tag is int mId && _pollingService.MachineDataCache.TryGetValue(mId, out var status))
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

        private void UpdateUtilityKpiData() { }

        /// <summary>
        /// 400 makine listesini tek bir döngüde (O(n)) tarayarak KPI verilerini ultra hızlı hesaplar.
        /// </summary>
        private void UpdateKpiCards()
        {
            if (_pollingService == null || _kpiTotalMachines == null || this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                var allStatuses = _pollingService.MachineDataCache.Values;
                if (allStatuses == null || !allStatuses.Any()) return;

                int totalMachines = 0;
                int offlineMachines = 0;
                int runningMachines = 0;
                int alarmMachines = 0;
                int manualMachines = 0;
                int idleMachines = 0;

                // OPTİMİZASYON: LINQ sorguları yerine tek bir ham döngü (Tek geçiş)
                foreach (var s in allStatuses)
                {
                    totalMachines++;
                    if (s.ConnectionState != ConnectionStatus.Connected)
                    {
                        offlineMachines++;
                    }
                    else
                    {
                        if (s.HasActiveAlarm)
                        {
                            alarmMachines++;
                        }
                        else if (s.IsInRecipeMode)
                        {
                            runningMachines++;
                        }
                        else if (s.manuel_status)
                        {
                            manualMachines++;
                        }
                        else
                        {
                            idleMachines++;
                        }
                    }
                }

                _kpiTotalMachines.SetData($"{Resources.AllMachines}", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
                _kpiOfflineMachines.SetData("Offline Status", offlineMachines.ToString(), Color.FromArgb(149, 165, 166));
                _kpiRunningMachines.SetData($"{Resources.aktifüretim}", runningMachines.ToString(), Color.FromArgb(46, 204, 113));
                _kpiAlarmMachines.SetData($"{Resources.alarmdurum}", alarmMachines.ToString(), Color.FromArgb(231, 76, 60));
                _kpiManualMachines.SetData("Manuel Mode", manualMachines.ToString(), Color.FromArgb(155, 89, 182));
                _kpiIdleMachines.SetData($"{Resources.bosbekleyen}", idleMachines.ToString(), Color.FromArgb(243, 156, 18));
            }
            catch (Exception)
            {
                // UI koruması
            }
        }

        private async void UpdateSidebarCharts()
        {
            if (_dashboardRepository == null || _alarmRepository == null) return;

            try
            {
                var result = await Task.Run(() =>
                {
                    var today = DateTime.Today;
                    var now = DateTime.Now;

                    var singleConsumptionTable = _dashboardRepository.GetHourlyFactoryConsumption(today);

                    return new
                    {
                        ConsumptionData = singleConsumptionTable,
                        TopAlarms = _alarmRepository.GetTopAlarmsByFrequency(now.AddDays(-1), now),
                        OeeData = _dashboardRepository.GetHourlyAverageOee(today)
                    };
                });

                if (this.IsDisposed || !this.IsHandleCreated) return;

                // 1. ELEKTRİK GRAFİĞİ
                formsPlotHourly.Plot.Clear();
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

                // 2. SU GRAFİĞİ
                formsPlotHourlyWater.Plot.Clear();
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

                // 3. BUHAR GRAFİĞİ
                formsPlotHourlySteam.Plot.Clear();
                if (result.ConsumptionData != null && result.ConsumptionData.Rows.Count > 0)
                {
                    double[] hours = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.ConsumptionData.AsEnumerable().Select(row => row.IsNull("ToplamBuhar") ? 0.0 : Convert.ToDouble(row["ToplamBuhar"]) / 1000.0).ToArray();

                    var barPlot = formsPlotHourlySteam.Plot.Add.Scatter(hours, consumption);
                    barPlot.Color = ScottPlot.Colors.DimGray;
                    barPlot.MarkerSize = 0;
                    formsPlotHourlySteam.Plot.Axes.Left.Label.Text = "m³";
                }
                formsPlotHourlySteam.Plot.Axes.AutoScale();
                formsPlotHourlySteam.Refresh();

                // 4. ALARM GRAFİĞİ
                formsPlotTopAlarms.Plot.Clear();
                if (result.TopAlarms != null && result.TopAlarms.Any())
                {
                    double[] counts = result.TopAlarms.Select(a => (double)a.Count).ToArray();
                    var labels = result.TopAlarms.Select(a => a.AlarmText).ToArray();
                    var barPlot = formsPlotTopAlarms.Plot.Add.Bars(counts);
                    barPlot.Color = ScottPlot.Colors.OrangeRed;

                    var ticks = Enumerable.Range(0, labels.Length).Select(i => new ScottPlot.Tick(i, labels[i])).ToArray();
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Rotation = -90;
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.LowerRight;
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.FontSize = 12;
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Bold = true;
                    formsPlotTopAlarms.Plot.Axes.Bottom.MinimumSize = 160;
                }
                formsPlotTopAlarms.Plot.Axes.AutoScale();
                formsPlotTopAlarms.Refresh();

                // 5. OEE GRAFİĞİ
                formsPlotHourlyOee.Plot.Clear();
                if (result.OeeData != null && result.OeeData.Rows.Count > 0)
                {
                    double[] hours = result.OeeData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] oeeValues = result.OeeData.AsEnumerable().Select(row => row.IsNull("AverageOEE") ? 0.0 : Convert.ToDouble(row["AverageOEE"])).ToArray();

                    var linePlot = formsPlotHourlyOee.Plot.Add.Scatter(hours, oeeValues);
                    linePlot.Color = ScottPlot.Colors.Orange;
                    linePlot.LineStyle.Width = 2;
                    linePlot.MarkerStyle.Shape = ScottPlot.MarkerShape.FilledCircle;
                    linePlot.MarkerStyle.Size = 0;
                    formsPlotHourlyOee.Plot.Axes.Bottom.Label.Text = "Saat";
                }
                formsPlotHourlyOee.Plot.Axes.AutoScale();
                formsPlotHourlyOee.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"CRITICAL REFRESH ERROR: {ex}");
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_utilityPollingService != null)
            {
                _utilityPollingService.OnUtilityDataRefreshed -= UtilityService_OnDataRefreshed;
            }

            _uiUpdateTimer?.Stop();
            _uiUpdateTimer?.Dispose();
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