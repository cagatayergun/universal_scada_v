// UI/Views/GenelBakis_Control.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.UI.Controls;
using static TekstilScada.Repositories.ProcessLogRepository;

namespace TekstilScada.UI.Views
{
    public partial class GenelBakis_Control : UserControl
    {
        private PlcPollingService _pollingService;
        private MachineRepository _machineRepository;
        private DashboardRepository _dashboardRepository;
        private AlarmRepository _alarmRepository;
        private ProcessLogRepository _logRepository;
        private ProductionRepository _productionRepository;

        private Dictionary<int, bool> _previousBatchStatuses;
        private readonly Dictionary<int, DashboardMachineCard_Control> _machineCards = new Dictionary<int, DashboardMachineCard_Control>();
        private System.Windows.Forms.Timer _uiUpdateTimer;

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

        public void InitializeControl(PlcPollingService pollingService, MachineRepository machineRepo, DashboardRepository dashboardRepo, AlarmRepository alarmRepo, ProcessLogRepository logRepo, ProductionRepository productionRepo)
        {
            _pollingService = pollingService;
            _machineRepository = machineRepo;
            _dashboardRepository = dashboardRepo;
            _alarmRepository = alarmRepo;
            _logRepository = logRepo;
            _productionRepository = productionRepo;

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

                _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
                _pollingService.OnMachineDataRefreshed += PollingService_OnMachineDataRefreshed;

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

        private void BuildMachineCards()
        {
            if (_machineRepository == null || _pollingService == null) return;

            // Paneli dondur (Performans için)
            flpMachineGroups.SuspendLayout();

            _machineCards.Clear();
            flpMachineGroups.Controls.Clear();

            var allMachines = _machineRepository.GetAllEnabledMachines();
            var machineCache = _pollingService.MachineDataCache;

            // 1. Makineleri Alt Tipe Göre Grupla
            var groupedMachines = allMachines
                .GroupBy(m => m.MachineSubType ?? "Other") // Alt tip yoksa "Diğer"
                .OrderBy(g =>
                {
                    // Sıralama Mantığı: BY -> Kurutma -> Diğerleri
                    string key = g.Key?.ToUpper() ?? "";

                    if (key.Contains("BYMakinesi")) return 1;       // En üstte
                    if (key.Contains("Kurutma Makinesi")) return 2;  // İkinci sırada
                    return 3;                               // Diğerleri
                })
                .ThenBy(g => g.Key); // Diğerleri kendi içinde alfabetik sıralansın

            _colorIndex = 0;

            foreach (var group in groupedMachines)
            {
                // 2. Her Grup İçin Başlık (GroupBox) Oluştur
                var groupPanel = new GroupBox
                {
                    Text = group.Key,
                    Width = flpMachineGroups.ClientSize.Width*2 - 50, // Scroll bar payı
                    Height = 265, // Kart yüksekliğine göre ayarlanmalı (Kartlar ~280px ise)
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = _darkColors[_colorIndex % _darkColors.Count],
                    Padding = new Padding(5, 5, 5, 5) // Başlık için üst boşluk
                };

                // 3. Grup İçin Yatay Scroll Paneli (Inner FlowLayoutPanel)
                var innerPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false, // Tek satırda devam etsin
                    AutoScroll = true,    // Yatay scroll çıksın
                    BackColor = Color.WhiteSmoke
                };

                // Makineleri öncelik sırasına göre panele ekle
                // İlk açılışta da sıralı gelmesi için burada sıralama yapıyoruz
                var sortedMachines = group.OrderBy(m =>
                {
                    if (machineCache.TryGetValue(m.Id, out var status))
                        return GetSortPriority(status);
                    return 100;
                }).ToList();

                foreach (var machine in sortedMachines)
                {
                    var card = new DashboardMachineCard_Control(machine);

                    // ÖNEMLİ: Sıralama algoritmasının çalışabilmesi için ID'yi Tag'e atıyoruz
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

            // Reçete modu değişikliklerini kontrol et (Gerekirse tüm yapıyı yeniden kurmak için)
            var currentBatchStatuses = _pollingService.MachineDataCache
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsInRecipeMode);

            // Eğer makine eklenip çıkarılmadıysa BuildMachineCards çağırmaya gerek yok,
            // sıralamayı OnMachineDataRefreshed içinde hallediyoruz. 
            // Ancak yeni bir reçete başlangıcında kart yapısı değişiyorsa burası kalabilir.
            if (_previousBatchStatuses != null && !_previousBatchStatuses.SequenceEqual(currentBatchStatuses))
            {
                // BuildMachineCards(); // Sıralamayı dinamik yaptığımız için full rebuild gerekmeyebilir.
                _previousBatchStatuses = currentBatchStatuses;
            }

            UpdateKpiCards();
            UpdateSidebarCharts();
        }

        private void PollingService_OnMachineDataRefreshed(int machineId, FullMachineStatus status)
        {
            // Form veya kontrol kapandıysa işlem yapma
            if (this.IsDisposed || !this.IsHandleCreated) return;

            // Veritabanı işlemi olmadığı için Task.Run'a gerek yoktur.
            // Ancak Polling servisi arka planda çalıştığı için UI elemanlarına erişmek üzere
            // işlemi BeginInvoke ile ana iş parçacığına (UI Thread) gönderiyoruz.
            this.BeginInvoke(new Action(() =>
            {
                if (_machineCards.TryGetValue(machineId, out var cardToUpdate))
                {
                    // Trend verisi (grafik) istenmediği için boş bir liste gönderiyoruz.
                    // Bu sayede veritabanı yorulmaz ve UI anlık olarak güncellenir.
                    cardToUpdate.UpdateData(status, new List<ProcessDataPoint>());

                    // Sıralama işlemi
                    if (cardToUpdate.Parent is FlowLayoutPanel parentPanel)
                    {
                        SortMachinesInPanel(parentPanel);
                    }
                }
            }));
        }

        // --- YENİ: PANEL İÇİ SIRALAMA MANTIĞI ---
        private void SortMachinesInPanel(FlowLayoutPanel panel)
        {
            // Paneldeki kartları al
            var cards = panel.Controls.OfType<DashboardMachineCard_Control>().ToList();

            // Puanlamaya göre sırala
            var sortedCards = cards.OrderBy(c =>
            {
                if (c.Tag is int mId && _pollingService.MachineDataCache.TryGetValue(mId, out var status))
                {
                    return GetSortPriority(status);
                }
                return 100; // Bilinmeyen durum en sona
            }).ToList();

            // UI'daki sırasını güncelle (SetChildIndex ile)
            for (int i = 0; i < sortedCards.Count; i++)
            {
                if (panel.Controls.GetChildIndex(sortedCards[i]) != i)
                {
                    panel.Controls.SetChildIndex(sortedCards[i], i);
                }
            }
        }

        // --- YENİ: SIRALAMA PUANLAMA MANTIĞI ---
        private int GetSortPriority(FullMachineStatus status)
        {
            if (status == null) return 100;

            // 1. Öncelik: Üretim (Reçete veya Manuel Mod) -> EN BAŞA
            if (status.ConnectionState == ConnectionStatus.Connected && (status.IsInRecipeMode || status.manuel_status))
                return 1;

            // 2. Öncelik: Alarm Durumu -> İKİNCİ
            if (status.ConnectionState == ConnectionStatus.Connected && status.HasActiveAlarm)
                return 2;

            // 3. Öncelik: Bekleme (Boşta) -> ÜÇÜNCÜ
            if (status.ConnectionState == ConnectionStatus.Connected)
                return 3;

            // 4. Öncelik: Bağlantı Yok -> EN SONA
            return 4;
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
                // 1. Veritabanı işlemlerini arka plana atıyoruz (UI Donmaz)
                var result = await Task.Run(() =>
                {
                    var today = DateTime.Today;
                    var now = DateTime.Now;

                    // Tüm verileri arka planda çekip bir nesne olarak döndürüyoruz
                    return new
                    {
                        ElecData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        WaterData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        SteamData = _dashboardRepository.GetHourlyFactoryConsumption(today),
                        TopAlarms = _alarmRepository.GetTopAlarmsByFrequency(now.AddDays(-1), now),
                        OeeData = _dashboardRepository.GetHourlyAverageOee(today)
                    };
                });

                // 2. Grafikleri güncelleme (Burası UI Thread'de çalışır)

                // --- Elektrik ---
                formsPlotHourly.Plot.Clear();
                if (result.ElecData != null && result.ElecData.Rows.Count > 0)
                {
                    double[] hours = result.ElecData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.ElecData.AsEnumerable().Select(row => row.IsNull("ToplamElektrik") ? 0.0 : Convert.ToDouble(row["ToplamElektrik"]) / 1000.0).ToArray();

                    var barPlot = formsPlotHourly.Plot.Add.Scatter(hours, consumption);
                    barPlot.Color = ScottPlot.Colors.SteelBlue;
                    barPlot.MarkerSize = 0;
                    formsPlotHourly.Plot.Axes.Left.Label.Text = "kWh";
                }
                formsPlotHourly.Plot.Axes.AutoScale();
                formsPlotHourly.Refresh();

                // --- Su ---
                formsPlotHourlyWater.Plot.Clear();
                if (result.WaterData != null && result.WaterData.Rows.Count > 0)
                {
                    double[] hours = result.WaterData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.WaterData.AsEnumerable().Select(row => row.IsNull("ToplamSu") ? 0.0 : Convert.ToDouble(row["ToplamSu"]) / 1000.0).ToArray();

                    var barPlot = formsPlotHourlyWater.Plot.Add.Scatter(hours, consumption);
                    barPlot.Color = ScottPlot.Colors.CornflowerBlue;
                    barPlot.MarkerSize = 0;
                    formsPlotHourlyWater.Plot.Axes.Left.Label.Text = "m³";
                }
                formsPlotHourlyWater.Plot.Axes.AutoScale();
                formsPlotHourlyWater.Refresh();

                // --- Buhar ---
                formsPlotHourlySteam.Plot.Clear();
                if (result.SteamData != null && result.SteamData.Rows.Count > 0)
                {
                    double[] hours = result.SteamData.AsEnumerable().Select(row => row.IsNull("Saat") ? 0.0 : Convert.ToDouble(row["Saat"])).ToArray();
                    double[] consumption = result.SteamData.AsEnumerable().Select(row => row.IsNull("ToplamBuhar") ? 0.0 : Convert.ToDouble(row["ToplamBuhar"]) / 1000.0).ToArray();

                    var barPlot = formsPlotHourlySteam.Plot.Add.Scatter(hours, consumption);
                    barPlot.Color = ScottPlot.Colors.DimGray;
                    barPlot.MarkerSize = 0;
                    formsPlotHourlySteam.Plot.Axes.Left.Label.Text = "m³";
                }
                formsPlotHourlySteam.Plot.Axes.AutoScale();
                formsPlotHourlySteam.Refresh();

                // --- Alarmlar ---
                formsPlotTopAlarms.Plot.Clear();
                if (result.TopAlarms != null && result.TopAlarms.Any())
                {
                    double[] counts = result.TopAlarms.Select(a => (double)a.Count).ToArray();
                    var labels = result.TopAlarms.Select(a => a.AlarmText).ToArray();
                    var barPlot = formsPlotTopAlarms.Plot.Add.Bars(counts);
                    barPlot.Color = ScottPlot.Colors.OrangeRed;

                    var ticks = Enumerable.Range(0, labels.Length).Select(i => new ScottPlot.Tick(i, labels[i])).ToArray();
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                    formsPlotTopAlarms.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                }
                formsPlotTopAlarms.Plot.Axes.AutoScale();
                formsPlotTopAlarms.Refresh();

                // --- OEE ---
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