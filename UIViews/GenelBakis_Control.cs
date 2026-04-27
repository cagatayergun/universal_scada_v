// UI/Views/GenelBakis_Control.cs
using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks; // Task için gerekli
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.UI.Controls;
using TekstilScada.UIControls;
using static TekstilScada.Repositories.ProcessLogRepository;

namespace TekstilScada.UI.Views
{
    public partial class GenelBakis_Control : UserControl
    {
        private PlcPollingService _pollingService;
        private UtilityPollingService _utilityPollingService; // YENİ: Servis Tanımı
        private MachineRepository _machineRepository;
        private DashboardRepository _dashboardRepository;
        private AlarmRepository _alarmRepository;
        private ProcessLogRepository _logRepository;
        private ProductionRepository _productionRepository;
        private UtilityRepository _utilityRepository;
        private Dictionary<int, IPlcManager> _plcManagers;
        private Dictionary<int, bool> _previousBatchStatuses;
        private readonly Dictionary<int, DashboardMachineCard_Control> _machineCards = new Dictionary<int, DashboardMachineCard_Control>();
        private System.Windows.Forms.Timer _uiUpdateTimer;

        // Utility (Enerji) Takibi İçin
        private Dictionary<int, DateTime> _utilityLastSeen = new Dictionary<int, DateTime>(); // Son veri zamanı
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

        // InitializeControl GÜNCELLENDİ: utilityService parametresi eklendi
        public void InitializeControl(
            PlcPollingService pollingService,
            MachineRepository machineRepo,
            Dictionary<int, IPlcManager> plcManagers,
            DashboardRepository dashboardRepo,
            AlarmRepository alarmRepo,
            ProcessLogRepository logRepo,
            ProductionRepository productionRepo,
            UtilityRepository utilityrepo,
            UtilityPollingService utilityService) // <--- YENİ PARAMETRE
        {
            _pollingService = pollingService;
            _machineRepository = machineRepo;
            _plcManagers = plcManagers;
            _dashboardRepository = dashboardRepo;
            _alarmRepository = alarmRepo;
            _logRepository = logRepo;
            _productionRepository = productionRepo;
            _utilityRepository = utilityrepo;
            _utilityPollingService = utilityService; // <--- ATAMA YAPILDI

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
                BuildUtilityStrip(); // Enerji kartlarını oluştur

                // Olay Abonelikleri
                _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
                _pollingService.OnMachineDataRefreshed += PollingService_OnMachineDataRefreshed;

                // YENİ: Enerji Servisi Olay Aboneliği
                if (_utilityPollingService != null)
                {
                    _utilityPollingService.OnUtilityDataRefreshed -= UtilityService_OnDataRefreshed;
                    _utilityPollingService.OnUtilityDataRefreshed += UtilityService_OnDataRefreshed;
                }

                if (_uiUpdateTimer == null)
                {
                    _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 2000 };
                    _uiUpdateTimer.Tick += (s, a) => RefreshDashboard();
                }
                _uiUpdateTimer.Start();

                RefreshDashboard();

                _isDashboardSetup = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard kurulum hatası: {ex.Message}");
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
            flpUtilityStrip.Visible=false;
            // Hatları çek (Repository'de GetUtilityLines var)
            var lines = _utilityRepository.GetUtilityLines();

            foreach (var line in lines)
            {
                var card = new UtilityDashboardCard_Control();

                // Başlangıçta kartı oluşturuyoruz, henüz veri yok
                var initialData = new UtilityDashboardDto { LineName = line.LineName };
                card.SetData(initialData);
                card.SetConnectionStatus(false); // Başlangıçta kopuk görünsün, veri gelince yeşil olacak

                _utilityCards.Add(line.Id, card);
                flpUtilityStrip.Controls.Add(card);
            }

            flpUtilityStrip.ResumeLayout();
        }

        private void BuildMachineCards()
        {
            if (_machineRepository == null || _pollingService == null || _alarmRepository == null) return;

            // --- YENİ EKLENEN: Alarmları Veritabanından Bir Kez Çekiyoruz ---
            // AlarmRepository içindeki GetAllAlarmDefinitions() metodunu kullanarak verileri çekip Sözlüğe (Dictionary) çeviriyoruz.
            var allAlarmDefs = _alarmRepository.GetAllAlarmDefinitions();
            var alarmDict = allAlarmDefs.ToDictionary(a => a.AlarmNumber, a => a.AlarmText);
            // -----------------------------------------------------------------

            flpMachineGroups.SuspendLayout();
            _machineCards.Clear();
            flpMachineGroups.Controls.Clear();

            var allMachines = _machineRepository.GetAllEnabledMachines();
            var machineCache = _pollingService.MachineDataCache;

            var groupedMachines = allMachines
                 .GroupBy(m => m.MachineSubType ?? "Other")
                 .OrderBy(g =>
                 {
                     var type = g.FirstOrDefault()?.MachineType?.ToString() ?? "";
                     if (type.Contains("BYMakinesi")) return 1;
                     if (type.Contains("Kurutma Makinesi")) return 2;
                     return 3;
                 })
                 .ThenBy(g => g.Key);

            _colorIndex = 0;

            foreach (var group in groupedMachines)
            {
                var groupPanel = new GroupBox
                {
                    Text = group.Key,
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

                    // --- YENİ EKLENEN: Veritabanından çektiğimiz alarmları karta aktarıyoruz ---
                    card.AlarmDefinitions = alarmDict;
                    // ---------------------------------------------------------------------------

                    // Kart oluşturulduktan sonra PLC yöneticilerini ve diğer servisleri karta iletiyoruz.
                    // GenelBakis_Control içinde bulunmayan servisler (RecipeRepository, FtpTransfer, UserRepository) için şimdilik null geçiyoruz.
                    card.InitializeControl(null, _machineRepository, _plcManagers, _pollingService, null, null);

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

            var currentBatchStatuses = _pollingService.MachineDataCache
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsInRecipeMode);

            if (_previousBatchStatuses != null && !_previousBatchStatuses.SequenceEqual(currentBatchStatuses))
            {
                _previousBatchStatuses = currentBatchStatuses;
            }

            UpdateKpiCards();

            // YENİ: Enerji Bağlantı Kontrolü (Timeout)
            CheckUtilityConnections();

            UpdateSidebarCharts();
        }

        // YENİ: Canlı Veri Geldiğinde Çalışacak Metot
        private void UtilityService_OnDataRefreshed(List<UtilityLog> logs)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            // UI Thread'e geçiş
            this.BeginInvoke(new Action(() =>
            {
                foreach (var log in logs)
                {
                    if (_utilityCards.TryGetValue(log.LineId, out var card))
                    {
                        // 1. "Görüldü" zamanını güncelle
                        _utilityLastSeen[log.LineId] = DateTime.Now;

                        // 2. Kartı "BAĞLI" moduna geçir (Yeşil ışık)
                        card.SetConnectionStatus(true);

                        // 3. Verileri DTO'ya çevirip karta bas
                        var dto = new UtilityDashboardDto
                        {
                            LineName = "", // İsim zaten kartta var, değiştirmeye gerek yok
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

        // YENİ: Bağlantı Kopma Kontrolü (25 saniye kuralı)
        private void CheckUtilityConnections()
        {
            var now = DateTime.Now;
            foreach (var kvp in _utilityCards)
            {
                int lineId = kvp.Key;
                var card = kvp.Value;

                // Eğer son veri üzerinden 25 saniye geçtiyse "Bağlantı Yok" yap
                if (!_utilityLastSeen.ContainsKey(lineId) || (now - _utilityLastSeen[lineId]).TotalSeconds > 25)
                {
                    card.SetConnectionStatus(false);
                }
            }
        }

        private void PollingService_OnMachineDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                if (_machineCards.TryGetValue(machineId, out var cardToUpdate))
                {
                    cardToUpdate.UpdateData(status, new List<ProcessDataPoint>());

                    if (cardToUpdate.Parent is FlowLayoutPanel parentPanel)
                    {
                        SortMachinesInPanel(parentPanel);
                    }
                }
            }));
        }

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

        private void UpdateUtilityKpiData()
        {
            // Bu metot artık kullanılmıyor, canlı veri UtilityService_OnDataRefreshed üzerinden geliyor.
            // Ancak geçmiş verileri veya toplamları göstermek isterseniz burada tutabilirsiniz.
            // Şimdilik boş bırakıyorum veya silebilirsiniz.
        }

        private void UpdateKpiCards()
        {
            if (_pollingService == null || _kpiTotalMachines == null) return;

            var allStatuses = _pollingService.MachineDataCache.Values;

            int totalMachines = allStatuses.Count;
            int offlineMachines = allStatuses.Count(s => s.ConnectionState != ConnectionStatus.Connected);
            int runningMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.IsInRecipeMode && !s.HasActiveAlarm);
            int alarmMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.HasActiveAlarm);
            int manualMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.manuel_status && !s.IsInRecipeMode && !s.HasActiveAlarm);
            int idleMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && !s.manuel_status && !s.IsInRecipeMode && !s.HasActiveAlarm);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateKpiCards));
                return;
            }

            _kpiTotalMachines.SetData($"{Resources.AllMachines}", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
            _kpiOfflineMachines.SetData("Offline Status", offlineMachines.ToString(), Color.FromArgb(149, 165, 166));
            _kpiRunningMachines.SetData($"{Resources.aktifüretim}", runningMachines.ToString(), Color.FromArgb(46, 204, 113));
            _kpiAlarmMachines.SetData($"{Resources.alarmdurum}", alarmMachines.ToString(), Color.FromArgb(231, 76, 60));
            _kpiManualMachines.SetData("Manuel Mode", manualMachines.ToString(), Color.FromArgb(155, 89, 182));
            _kpiIdleMachines.SetData($"{Resources.bosbekleyen}", idleMachines.ToString(), Color.FromArgb(243, 156, 18));
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

                    return new
                    {
                        ElecData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        WaterData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        SteamData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        TopAlarms = _alarmRepository.GetTopAlarmsByFrequency(now.AddDays(-1), now),
                        OeeData = _dashboardRepository.GetHourlyAverageOee(today)
                    };
                });

             //   formsPlotHourly.Plot.Clear();
                if (result.ElecData != null && result.ElecData.Rows.Count > 0)
                {
                    double[] hours = result.ElecData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.ElecData.AsEnumerable().Select(row => row.IsNull("ToplamElektrik") ? 0.0 : Convert.ToDouble(row["ToplamElektrik"]) / 1000.0).ToArray();

                //    var barPlot = formsPlotHourly.Plot.Add.Scatter(hours, consumption);
                 //   barPlot.Color = ScottPlot.Colors.SteelBlue;
                 //   barPlot.MarkerSize = 0;
                 //   formsPlotHourly.Plot.Axes.Left.Label.Text = "kWh";
                }
               // formsPlotHourly.Plot.Axes.AutoScale();
               // formsPlotHourly.Refresh();

              //  formsPlotHourlyWater.Plot.Clear();
                if (result.WaterData != null && result.WaterData.Rows.Count > 0)
                {
                    double[] hours = result.WaterData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.WaterData.AsEnumerable().Select(row => row.IsNull("ToplamSu") ? 0.0 : Convert.ToDouble(row["ToplamSu"]) / 1000.0).ToArray();

              //      var barPlot = formsPlotHourlyWater.Plot.Add.Scatter(hours, consumption);
                //    barPlot.Color = ScottPlot.Colors.CornflowerBlue;
                //    barPlot.MarkerSize = 0;
               //     formsPlotHourlyWater.Plot.Axes.Left.Label.Text = "m³";
                }
               // formsPlotHourlyWater.Plot.Axes.AutoScale();
               // formsPlotHourlyWater.Refresh();

              //  formsPlotHourlySteam.Plot.Clear();
                if (result.SteamData != null && result.SteamData.Rows.Count > 0)
                {
                    double[] hours = result.SteamData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.SteamData.AsEnumerable().Select(row => row.IsNull("ToplamBuhar") ? 0.0 : Convert.ToDouble(row["ToplamBuhar"]) / 1000.0).ToArray();

               //     var barPlot = formsPlotHourlySteam.Plot.Add.Scatter(hours, consumption);
              //      barPlot.Color = ScottPlot.Colors.DimGray;
              //      barPlot.MarkerSize = 0;
              //      formsPlotHourlySteam.Plot.Axes.Left.Label.Text = "m³";
                }
               // formsPlotHourlySteam.Plot.Axes.AutoScale();
                //formsPlotHourlySteam.Refresh();

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
                System.Diagnostics.Debug.WriteLine($"Grafik güncelleme hatası: {ex.Message}");
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
            }

            // YENİ: Enerji Servisi Aboneliğini Kaldır
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
            //gbHourlyConsumption.Text = "Hourly Electricity (kWh)";
            gbTopAlarms.Text = Resources.ensikalarm;
           // gbHourlyConsumptionWater.Text = "Hourly Water (m³)";
           // gbHourlyConsumptionSteam.Text = "Hourly Steam (m³)";
            gbHourlyOee.Text = "24 Hourly OEE";
        }
    }
}