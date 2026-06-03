// UI/Views/Prosesİzleme_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Services;
using Telemetry.UI.Controls;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class Prosesİzleme_Control : UserControl
    {
        public event EventHandler<int> MachineDetailsRequested;
        public event EventHandler<int> MachineVncRequested;

        private PlcPollingService _pollingService;
        private readonly Dictionary<int, MachineCard_Control> _machineCards = new Dictionary<int, MachineCard_Control>();

        // Merkezi UI Timer frenleme (throttle) düzeneği korundu
        private System.Windows.Forms.Timer _uiRefreshTimer;
        private int _kpiTickCounter = 0;

        // KPI Kartları
        private KpiCard_Control _kpiTotalMachines;
        private KpiCard_Control _kpiOfflineMachines;
        private KpiCard_Control _kpiRunningMachines;
        private KpiCard_Control _kpiAlarmMachines;
        private KpiCard_Control _kpiManualMachines;
        private KpiCard_Control _kpiIdleMachines;

        public Prosesİzleme_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Genel arayüzün ve sekmelerin geçişlerde titremesini engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimini üst ebeveyn forma devret

            // SPEED OPTİMİZASYON: Kart matrisinin kaydırma (scroll) ivmesini en üst seviyeye çıkartır
            EnableDoubleBuffer(flowLayoutPanelMachines);
            EnableDoubleBuffer(flpTopKpis);
        }

        public void InitializeView(List<Machine> machines, PlcPollingService service)
        {
            ClearView();
            _pollingService = service;

            // KPI Kartlarını oluştur
            InitializeKpiCards();
            var sortedMachines = machines.OrderBy(m => m.DisplayOrder).ToList();

            int displayCounter = 1;

            // Performans için paneli askıya al (Arayüz çizim hesaplamalarını dondur)
            flowLayoutPanelMachines.SuspendLayout();

            foreach (var machine in sortedMachines)
            {
                var card = new MachineCard_Control(machine.Id, machine.MachineUserDefinedId, machine.MachineName, displayCounter++, machine.MachineType);

                card.DetailsRequested += Card_DetailsRequested;
                card.VncRequested += Card_VncRequested;
                _machineCards.Add(machine.Id, card);
                flowLayoutPanelMachines.Controls.Add(card);
            }

            // Çizimi tek bir karede (frame) serbest bırak
            flowLayoutPanelMachines.ResumeLayout(true);

            // İlk açılış RAM verileriyle hızlı dolum
            UpdateKpiCards();
            UpdateAllMachineCards();

            // Throttled Pull UI Timer Başlatma
            if (_uiRefreshTimer == null)
            {
                _uiRefreshTimer = new System.Windows.Forms.Timer();
                _uiRefreshTimer.Interval = 300; // Saniyede ~3 kez çalışarak gözü yormayan akıcılık sunar
                _uiRefreshTimer.Tick += UiRefreshTimer_Tick;
            }
            _uiRefreshTimer.Start();
        }

        private void UiRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_pollingService == null || this.IsDisposed || !this.IsHandleCreated) return;

            // 1. ADIM: Tüm kartları Invoke maliyeti olmadan doğrudan UI thread üzerinde toplu güncelle
            UpdateAllMachineCards();

            // 2. ADIM: Frenleme (Throttle) mantığıyla saniyede 1 kez KPI hesaplama tetiği (~900ms)
            _kpiTickCounter++;
            if (_kpiTickCounter >= 3)
            {
                _kpiTickCounter = 0;
                UpdateKpiCards();
            }
        }

        private void UpdateAllMachineCards()
        {
            try
            {
                var cache = _pollingService.MachineDataCache;
                if (cache == null || cache.IsEmpty) return;

                foreach (var kvp in _machineCards)
                {
                    int machineId = kvp.Key;
                    var card = kvp.Value;

                    if (!card.IsDisposed && cache.TryGetValue(machineId, out var status))
                    {
                        // Kartın text ve grafik motorunu doğrudan RAM verisiyle sarsıntısız güncelle
                        card.UpdateView(status);
                    }
                }
            }
            catch (Exception)
            {
                // UI akış işleyiş koruması
            }
        }

        private void InitializeKpiCards()
        {
            if (_kpiTotalMachines != null && flpTopKpis.Controls.Contains(_kpiTotalMachines)) return;

            _kpiTotalMachines = new KpiCard_Control();
            _kpiOfflineMachines = new KpiCard_Control();
            _kpiRunningMachines = new KpiCard_Control();
            _kpiAlarmMachines = new KpiCard_Control();
            _kpiManualMachines = new KpiCard_Control();
            _kpiIdleMachines = new KpiCard_Control();

            flpTopKpis.Controls.Clear();
            flpTopKpis.Controls.Add(_kpiTotalMachines);
            flpTopKpis.Controls.Add(_kpiOfflineMachines);
            flpTopKpis.Controls.Add(_kpiRunningMachines);
            flpTopKpis.Controls.Add(_kpiAlarmMachines);
            flpTopKpis.Controls.Add(_kpiManualMachines);
            flpTopKpis.Controls.Add(_kpiIdleMachines);
        }

        // =========================================================================
        // SPEED OPTİMİZASYON: TEK GEÇİŞLİ (O(n)) KPI İSTATİSTİK MOTORU
        // Önceden RAM cache listesini 5 defa tarayan yapı tek döngüye indirgenmiştir.
        // =========================================================================
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

                // İşlemci dostu ham döngü taraması
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

                _kpiTotalMachines.SetData(Resources.AllMachines ?? "Total", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
                _kpiOfflineMachines.SetData("Offline Status", offlineMachines.ToString(), Color.FromArgb(144, 164, 174));
                _kpiRunningMachines.SetData(Resources.aktifüretim ?? "Running", runningMachines.ToString(), Color.FromArgb(76, 175, 80));
                _kpiAlarmMachines.SetData(Resources.alarmdurum ?? "Alarm", alarmMachines.ToString(), Color.FromArgb(211, 47, 47));
                _kpiManualMachines.SetData("Manuel Mode", manualMachines.ToString(), Color.FromArgb(142, 68, 173));
                _kpiIdleMachines.SetData(Resources.bosbekleyen ?? "Idle", idleMachines.ToString(), Color.FromArgb(230, 126, 34));
            }
            catch (Exception)
            {
                // UI çökme koruması
            }
        }

        private void Card_DetailsRequested(object sender, EventArgs e)
        {
            if (sender is MachineCard_Control card)
            {
                MachineDetailsRequested?.Invoke(this, card.MachineId);
            }
        }

        private void Card_VncRequested(object sender, EventArgs e)
        {
            if (sender is MachineCard_Control card)
            {
                MachineVncRequested?.Invoke(this, card.MachineId);
            }
        }

        private void ClearView()
        {
            _uiRefreshTimer?.Stop();
            _kpiTickCounter = 0;

            foreach (var card in _machineCards.Values)
            {
                card.Dispose();
            }
            _machineCards.Clear();
            flowLayoutPanelMachines.Controls.Clear();

            flpTopKpis.Controls.Clear();
            _kpiTotalMachines = null;
        }

        // =========================================================================
        // MEMORY LEAK (BELLEK SIZINTISI) KORUMASI
        // Kontrolün RAM'de asılı kalmasını ve arka plan sızıntılarını kesin önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_uiRefreshTimer != null)
            {
                _uiRefreshTimer.Stop();
                _uiRefreshTimer.Dispose();
                _uiRefreshTimer = null;
            }

            base.OnHandleDestroyed(e);
        }

        // SPEED OPTİMİZASYON: Kaydırma (scroll) akıcılığını sağlayan yansıtma (Reflection) metodu
        private void EnableDoubleBuffer(Control control)
        {
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, control, new object[] { true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DoubleBuffering could not be enabled: {ex.Message}");
            }
        }

        private void flpTopKpis_Paint(object sender, PaintEventArgs e) { }
    }
}