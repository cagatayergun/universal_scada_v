// UI/SelectMachineForm.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Telemetry.Models;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm bileşenleri için

namespace Telemetry.UI
{
    // Form yerine MaterialForm sınıfından türetiyoruz
    public partial class SelectMachineForm : MaterialForm
    {
        public Machine SelectedMachine { get; private set; }

        public SelectMachineForm(List<Machine> machines)
        {
            InitializeComponent();

            // =========================================================================
            // MATERIALSKIN FORM ENTEGRASYONU VE PERFORMANS AYARLARI
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this); // Formu merkezi temaya kaydet

            this.DoubleBuffered = true; // Form yüklenirken veya ekranda taşınırken titremeyi engeller

            // DÜZELTME: Veri kaynağını atamadan önce DisplayMember ve ValueMember'ı temizleyelim.
            // Bu, eski ayarların önbellekte kalmasını engeller.
            lstMachines.DisplayMember = "";
            lstMachines.ValueMember = "";
            lstMachines.DataSource = null;

            // Veri kaynağını ve hangi alanların kullanılacağını yeniden ata.
            lstMachines.DataSource = machines;
            lstMachines.DisplayMember = "DisplayInfo";
            lstMachines.ValueMember = "Id";

            // Liste boş değilse, ilk elemanı varsayılan olarak seç
            if (lstMachines.Items.Count > 0)
            {
                lstMachines.SelectedIndex = 0;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (lstMachines.SelectedItem != null)
            {
                // 1. Seçimi yap
                SelectedMachine = lstMachines.SelectedItem as Machine;
                // 2. Formun sonucunu OK olarak ayarla
                this.DialogResult = DialogResult.OK;
                // 3. Formu kapat
                this.Close();
            }
            else
            {
                // Eğer listede hiç makine yoksa veya bir şekilde seçim yapılamadıysa
                MessageBox.Show("Please select a machine from the list.", "No Selection Made", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}