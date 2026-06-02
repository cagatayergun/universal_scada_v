using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Services;
using Telemetry.UI.Controls;

namespace Telemetry.UI.Views
{
    public partial class Prosesİzleme_Control : UserControl
    {
        public event EventHandler<int> MachineDetailsRequested;
        public event EventHandler<int> MachineVncRequested;

        private PlcPollingService _pollingService;
        private readonly Dictionary<int, MachineCard_Control> _machineCards = new Dictionary<int, MachineCard_Control>();

        // OPTİMİZASYON: Tek bir merkezi UI Timer hem kartları hem KPI'ları throttle (frenleme) mantığıyla günceller
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
            // Çizim performansını artırmak ve anlık kırpışmaları (flickering) önlemek için DoubleBuffered açıyoruz
            this.DoubleBuffered = true;
        }

        public void InitializeView(List<Machine> machines, PlcPollingService service)
        {
            ClearView();
            _pollingService = service;

            // KRİTİK KRİTER: Event abonelikleri tamamen KAlDIRILDI. 
            // Saniyede 400 makineden gelen push trafiği UI thread'i kilitlemeyecektir.

            // KPI Kartlarını oluştur
            InitializeKpiCards();
            var sortedMachines = machines.OrderBy(m => m.DisplayOrder).ToList();

            int displayCounter = 1;

            // Performans için paneli askıya al (Arayüz çizimini dondur)
            flowLayoutPanelMachines.SuspendLayout();

            foreach (var machine in sortedMachines)
            {
                var card = new MachineCard_Control(machine.Id, machine.MachineUserDefinedId, machine.MachineName, displayCounter++, machine.MachineType);

                card.DetailsRequested += Card_DetailsRequested;
                card.VncRequested += Card_VncRequested;
                _machineCards.Add(machine.Id, card);
                flowLayoutPanelMachines.Controls.Add(card);
            }

            // Çizimi tek seferde serbest bırak
            flowLayoutPanelMachines.ResumeLayout();

            // İlk açılışta verileri bir kez manuel güncelle
            UpdateKpiCards();
            UpdateAllMachineCards();

            // --- OPTİMİZASYON AYARI: Throttled Pull UI Timer ---
            if (_uiRefreshTimer == null)
            {
                _uiRefreshTimer = new System.Windows.Forms.Timer();
                _uiRefreshTimer.Interval = 300; // Saniyede ~3 kez çalışır (İnsan gözü için tamamen akıcı ve gecikmesizdir)
                _uiRefreshTimer.Tick += UiRefreshTimer_Tick;
            }
            _uiRefreshTimer.Start();
        }

        private void UiRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_pollingService == null || this.IsDisposed || !this.IsHandleCreated) return;

            // 1. ADIM: Tüm kartları BATCH (Toplu) olarak güncelle (Invoke GEREKMEZ, zaten UI thread'deyiz)
            UpdateAllMachineCards();

            // 2. ADIM: Ağır LINQ sorguları barındıran KPI kartlarını saniyede sadece 1 kez güncelle (300ms * 3 = ~900ms)
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
                // UI kilitlenmesini önlemek için doğrudan hafızadaki thread-safe Cache yapısından okuyoruz
                var cache = _pollingService.MachineDataCache;
                if (cache == null || cache.IsEmpty) return;

                foreach (var kvp in _machineCards)
                {
                    int machineId = kvp.Key;
                    var card = kvp.Value;

                    if (!card.IsDisposed && cache.TryGetValue(machineId, out var status))
                    {
                        // Kartın text ve grafik arayüzünü doğrudan RAM verisiyle sarsıntısız güncelle
                        card.UpdateView(status);
                    }
                }
            }
            catch (Exception)
            {
                // Döngü içi anlık hataların UI akışını bozmasını engelle
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

        private void UpdateKpiCards()
        {
            if (_pollingService == null || _kpiTotalMachines == null || this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                var allStatuses = _pollingService.MachineDataCache.Values;

                // İstatistikleri tek bir döngüde veya optimize edilmiş LINQ ile hesapla
                int totalMachines = allStatuses.Count;
                int offlineMachines = allStatuses.Count(s => s.ConnectionState != ConnectionStatus.Connected);
                int runningMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.IsInRecipeMode && !s.HasActiveAlarm);
                int alarmMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.HasActiveAlarm);
                int manualMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.manuel_status && !s.IsInRecipeMode && !s.HasActiveAlarm);
                int idleMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && !s.manuel_status && !s.IsInRecipeMode && !s.HasActiveAlarm);

                _kpiTotalMachines.SetData(Resources.AllMachines ?? "Total", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
                _kpiOfflineMachines.SetData("Offline Status", offlineMachines.ToString(), Color.FromArgb(149, 165, 166));
                _kpiRunningMachines.SetData(Resources.aktifüretim ?? "Running", runningMachines.ToString(), Color.FromArgb(46, 204, 113));
                _kpiAlarmMachines.SetData(Resources.alarmdurum ?? "Alarm", alarmMachines.ToString(), Color.FromArgb(231, 76, 60));
                _kpiManualMachines.SetData("Manuel Mode", manualMachines.ToString(), Color.FromArgb(155, 89, 182));
                _kpiIdleMachines.SetData(Resources.bosbekleyen ?? "Idle", idleMachines.ToString(), Color.FromArgb(243, 156, 18));
            }
            catch (Exception)
            {
                // UI çökmemesi için anlık istisna koruması
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
            // Önce Timer'ı durdur ve temizle
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

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Nesne yok edilirken hafıza sızıntılarını (Memory Leak) önlemek için timer'ı temizle
            _uiRefreshTimer?.Stop();
            _uiRefreshTimer?.Dispose();
            _uiRefreshTimer = null;

            base.OnHandleDestroyed(e);
        }

        private void flpTopKpis_Paint(object sender, PaintEventArgs e) { }
    }
}