// UI/Views/Prosesİzleme_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Universalscada.Core.Models;
using Universalscada.Core.Repositories; // IMetaDataRepository için gerekli
using Universalscada.Models;
using Universalscada.UI.Controls;

namespace Universalscada.UI.Views
{
    public partial class Prosesİzleme_Control : UserControl
    {
        // === MEVCUT EVENT'ler: Hata CS1061'i çözer ===
        public event EventHandler<int> MachineDetailsRequested;
        public event EventHandler<int> MachineVncRequested;

        private readonly IMetaDataRepository _metaDataRepository; // DI ile alınan yeni bağımlılık
        private readonly IMachineRepository _machineRepository;

        private readonly Dictionary<int, MachineCard_Control> _machineCards = new Dictionary<int, MachineCard_Control>();

        // === CONSTRUCTOR: Hata CS7036'yı çözer ===
        public Prosesİzleme_Control(IMetaDataRepository metaDataRepository, IMachineRepository machineRepository)
        {
            // InitializeComponent, tasarımcı dosyasında tanımlı olduğu için burada kalmalıdır (CS0103 tasarım anında oluşur).
            InitializeComponent();
            this.DoubleBuffered = true;
            _metaDataRepository = metaDataRepository;
            _machineRepository = machineRepository;
        }

        // === InitializeView: Hata CS1061'i çözmek için yeniden eklendi/güncellendi ===
        // Bu metot, MainForm tarafından makine listesini başlatmak için çağrılır.
        public void InitializeView(List<Machine> machines)
        {
            // Not: Dinamik UI kodlarını (LoadDynamicProcessView) bu metot içinde veya
            // ayrı bir metotta çağırarak makine ID'sine göre ekranı şekillendireceksiniz.

            ClearView();

            foreach (var machine in machines)
            {
                var card = new MachineCard_Control(machine.Id, machine.MachineUserDefinedId, machine.MachineName, machine.Id);
                card.DetailsRequested += Card_DetailsRequested;
                card.VncRequested += Card_VncRequested;
                _machineCards.Add(machine.Id, card);
                // flowLayoutPanelMachines, Designer dosyasında tanımlı olmalıdır.
                flowLayoutPanelMachines.Controls.Add(card);
            }
        }

        // ... Card_DetailsRequested, Card_VncRequested, ClearView metotları ...

        private void ClearView()
        {
            foreach (var card in _machineCards.Values)
            {
                card.Dispose();
            }
            _machineCards.Clear();
            // flowLayoutPanelMachines.Controls.Clear();
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


        // === YENİ METOD: Hata CS1061'i çözer ===
        // MainForm'dan gelen SignalR verilerini (FullMachineStatus) almak için kullanılır.
        public void UpdateMachineStatus(FullMachineStatus status)
        {
            if (_machineCards.TryGetValue(status.MachineId, out var card))
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() => {
                        if (!card.IsDisposed)
                        {
                            card.UpdateView(status); // MachineCard_Control'ün içindeki güncelleme metodunu çağırır.
                        }
                    }));
                }
            }
        }
    }
}