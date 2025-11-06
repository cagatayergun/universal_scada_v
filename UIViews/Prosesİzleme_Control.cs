// UI/Views/Prosesİzleme_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Universalscada.Models;
// using Universalscada.Services; // KALDIRILDI
using Universalscada.UI.Controls;

namespace Universalscada.UI.Views
{
    public partial class Prosesİzleme_Control : UserControl
    {
        public event EventHandler<int> MachineDetailsRequested;
        public event EventHandler<int> MachineVncRequested;

        // === KALDIRILDI ===
        // private PlcPollingService _pollingService;

        // Makine kartlarını ID ile hızlıca bulmak için
        private readonly Dictionary<int, MachineCard_Control> _machineCards = new Dictionary<int, MachineCard_Control>();

        public Prosesİzleme_Control()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        // === DEĞİŞTİ: InitializeView ===
        // PlcPollingService parametresi kaldırıldı
        public void InitializeView(List<Machine> machines)
        {
            ClearView(); // Önceki kartları ve event'leri temizle

            // _pollingService = service; // KALDIRILDI

            // === KALDIRILDI ===
            // _pollingService.OnMachineDataRefreshed += PollingService_OnMachineDataRefreshed;
            // _pollingService.OnMachineConnectionStateChanged += PollingService_OnMachineConnectionStateChanged;

            foreach (var machine in machines)
            {
                // MachineCard_Control'ün de _pollingService bağımlılığı olMAMAlı.
                var card = new MachineCard_Control(machine.Id, machine.MachineUserDefinedId, machine.MachineName, machine.Id);
                card.DetailsRequested += Card_DetailsRequested;
                card.VncRequested += Card_VncRequested;
                _machineCards.Add(machine.Id, card);
                flowLayoutPanelMachines.Controls.Add(card);
            }
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
            // === KALDIRILDI ===
            // if (_pollingService != null)
            // {
            //     _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
            //     _pollingService.OnMachineConnectionStateChanged -= PollingService_OnMachineConnectionStateChanged;
            // }

            foreach (var card in _machineCards.Values)
            {
                card.Dispose();
            }
            _machineCards.Clear();
            flowLayoutPanelMachines.Controls.Clear();
        }

        // === KALDIRILDI ===
        // Bu event handler'lar artık PlcPollingService'e bağlı olmadığı için kaldırıldı.
        // private void PollingService_OnMachineConnectionStateChanged(int machineId, FullMachineStatus status)
        // { ... }
        // private void PollingService_OnMachineDataRefreshed(int machineId, FullMachineStatus status)
        // { ... }


        // === YENİ METOD ===
        // MainForm'dan gelen SignalR verilerini almak için eklendi.
        // Bu metod, eski PollingService event handler'larının görevini üstlenir.
        public void UpdateMachineStatus(FullMachineStatus status)
        {
            if (_machineCards.TryGetValue(status.MachineId, out var card))
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    // BeginInvoke, UI thread'inde güvenli güncelleme yapar
                    this.BeginInvoke(new Action(() => {
                        if (!card.IsDisposed)
                        {
                            card.UpdateView(status); // MachineCard_Control'deki güncelleme metodu
                        }
                    }));
                }
            }
        }
    }
}