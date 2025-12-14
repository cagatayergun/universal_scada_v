// UI/Views/Prosesİzleme_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing; // Color kullanımı için eklendi
using System.Linq;    // Count(linq) kullanımı için eklendi
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Properties; // Resources kullanımı için
using TekstilScada.Services;
using TekstilScada.UI.Controls;

namespace TekstilScada.UI.Views
{
    public partial class Prosesİzleme_Control : UserControl
    {
        public event EventHandler<int> MachineDetailsRequested;
        public event EventHandler<int> MachineVncRequested;

        private PlcPollingService _pollingService;
        private readonly Dictionary<int, MachineCard_Control> _machineCards = new Dictionary<int, MachineCard_Control>();

        // --- YENİ EKLENEN KPI KARTLARI ---
        private KpiCard_Control _kpiTotalMachines;
        private KpiCard_Control _kpiOfflineMachines;
        private KpiCard_Control _kpiRunningMachines;
        private KpiCard_Control _kpiAlarmMachines;
        private KpiCard_Control _kpiManualMachines;
        private KpiCard_Control _kpiIdleMachines;

        public Prosesİzleme_Control()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void InitializeView(List<Machine> machines, PlcPollingService service)
        {
            ClearView();
            _pollingService = service;

            _pollingService.OnMachineDataRefreshed += PollingService_OnMachineDataRefreshed;
            _pollingService.OnMachineConnectionStateChanged += PollingService_OnMachineConnectionStateChanged;

            // --- KPI KARTLARINI OLUŞTUR ---
            InitializeKpiCards();

            int displayCounter = 1;
            foreach (var machine in machines)
            {
                var card = new MachineCard_Control(machine.Id, machine.MachineUserDefinedId, machine.MachineName, displayCounter++);
                card.DetailsRequested += Card_DetailsRequested;
                card.VncRequested += Card_VncRequested;
                _machineCards.Add(machine.Id, card);
                flowLayoutPanelMachines.Controls.Add(card);
            }

            // İlk açılışta KPI verilerini güncelle
            UpdateKpiCards();
        }

        // --- YENİ KPI OLUŞTURMA METODU ---
        private void InitializeKpiCards()
        {
            // Panelde zaten varsa tekrar ekleme
            if (_kpiTotalMachines != null && flpTopKpis.Controls.Contains(_kpiTotalMachines)) return;

            // Kartları oluştur
            _kpiTotalMachines = new KpiCard_Control();
            _kpiOfflineMachines = new KpiCard_Control();
            _kpiRunningMachines = new KpiCard_Control();
            _kpiAlarmMachines = new KpiCard_Control();
            _kpiManualMachines = new KpiCard_Control();
            _kpiIdleMachines = new KpiCard_Control();

            // Panele ekle
            flpTopKpis.Controls.Clear();
            flpTopKpis.Controls.Add(_kpiTotalMachines);
            flpTopKpis.Controls.Add(_kpiOfflineMachines);
            flpTopKpis.Controls.Add(_kpiRunningMachines);
            flpTopKpis.Controls.Add(_kpiAlarmMachines);
            flpTopKpis.Controls.Add(_kpiManualMachines);
            flpTopKpis.Controls.Add(_kpiIdleMachines);
        }

        // --- YENİ KPI GÜNCELLEME METODU ---
        private void UpdateKpiCards()
        {
            if (_pollingService == null || _kpiTotalMachines == null) return;

            // UI thread güvenliği
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(UpdateKpiCards));
                return;
            }

            // Cache'den tüm makine durumlarını al
            var allStatuses = _pollingService.MachineDataCache.Values;

            // İstatistikleri hesapla
            int totalMachines = allStatuses.Count;
            int offlineMachines = allStatuses.Count(s => s.ConnectionState != ConnectionStatus.Connected);
            int runningMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.IsInRecipeMode && !s.HasActiveAlarm);
            int alarmMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.HasActiveAlarm);
            int manualMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && s.manuel_status && !s.IsInRecipeMode && !s.HasActiveAlarm);
            int idleMachines = allStatuses.Count(s => s.ConnectionState == ConnectionStatus.Connected && !s.manuel_status && !s.IsInRecipeMode && !s.HasActiveAlarm);

            // Kartlara verileri bas (Renkler GenelBakis ile aynı)
            _kpiTotalMachines.SetData(Resources.AllMachines ?? "Total", totalMachines.ToString(), Color.FromArgb(41, 128, 185));
            _kpiOfflineMachines.SetData("Offline Status", offlineMachines.ToString(), Color.FromArgb(149, 165, 166));
            _kpiRunningMachines.SetData(Resources.aktifüretim ?? "Running", runningMachines.ToString(), Color.FromArgb(46, 204, 113));
            _kpiAlarmMachines.SetData(Resources.alarmdurum ?? "Alarm", alarmMachines.ToString(), Color.FromArgb(231, 76, 60));
            _kpiManualMachines.SetData("Manuel Mode", manualMachines.ToString(), Color.FromArgb(155, 89, 182));
            _kpiIdleMachines.SetData(Resources.bosbekleyen ?? "Idle", idleMachines.ToString(), Color.FromArgb(243, 156, 18));
        }

        private void Card_DetailsRequested(object sender, EventArgs e)
        {
            var card = sender as MachineCard_Control;
            if (card != null)
            {
                MachineDetailsRequested?.Invoke(this, card.MachineId);
            }
        }

        private void Card_VncRequested(object sender, EventArgs e)
        {
            var card = sender as MachineCard_Control;
            if (card != null)
            {
                MachineVncRequested?.Invoke(this, card.MachineId);
            }
        }

        private void ClearView()
        {
            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
                _pollingService.OnMachineConnectionStateChanged -= PollingService_OnMachineConnectionStateChanged;
            }

            foreach (var card in _machineCards.Values)
            {
                card.Dispose();
            }
            _machineCards.Clear();
            flowLayoutPanelMachines.Controls.Clear();

            // KPI panelini de temizle (Opsiyonel, yeniden oluşturulacaksa gerekli değil ama temizlik iyidir)
            flpTopKpis.Controls.Clear();
            _kpiTotalMachines = null; // Referansları düşür
        }

        private void PollingService_OnMachineConnectionStateChanged(int machineId, FullMachineStatus status)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    // KPI kartlarını güncelle
                    UpdateKpiCards();

                    if (_machineCards.TryGetValue(machineId, out var card) && !card.IsDisposed)
                    {
                        card.UpdateView(status);
                    }
                }));
            }
        }

        private void PollingService_OnMachineDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    // KPI kartlarını güncelle
                    UpdateKpiCards();

                    if (_machineCards.TryGetValue(machineId, out var card) && !card.IsDisposed)
                    {
                        card.UpdateView(status);
                    }
                }));
            }
        }

        private void flpTopKpis_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}