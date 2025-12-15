using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;

namespace TekstilScada.UI.Views
{
    public partial class PlcOperatorSettings_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private PlcOperatorRepository _plcOperatorRepository;
        // DEĞİŞİKLİK: LsPlcManager -> IPlcManager
        private Dictionary<int, IPlcManager> _plcManagers;

        public PlcOperatorSettings_Control()
        {
            InitializeComponent();
            _plcOperatorRepository = new PlcOperatorRepository();

            // --- EKLEMELER: Olay Yöneticileri ---

            // 1. Otomatik Kayıt: Hücre düzenleme bittiğinde tetiklenir
            dgvOperators.CellEndEdit += DgvOperators_CellEndEdit;

            // 2. Doğrulama: Kullanıcı hücreden çıkarken değerleri kontrol eder (Harf, Sembol, Sınır)
            dgvOperators.CellValidating += DgvOperators_CellValidating;

            // 3. Hata Yönetimi: Grid üzerindeki format hatalarının uygulamayı çökertmesini engeller
            dgvOperators.DataError += DgvOperators_DataError;
        }

        // DEĞİŞİKLİK: LsPlcManager -> IPlcManager
        public void InitializeControl(MachineRepository machineRepo, Dictionary<int, IPlcManager> plcManagers)
        {
            _machineRepository = machineRepo;
            _plcManagers = plcManagers;
        }

        private void PlcOperatorSettings_Control_Load(object sender, EventArgs e)
        {
            var machines = _machineRepository.GetAllEnabledMachines();
            cmbMachines.DataSource = machines;
            cmbMachines.DisplayMember = "DisplayInfo";
            cmbMachines.ValueMember = "Id";

            for (int i = 1; i <= 5; i++)
            {
                cmbSlot.Items.Add(i);
            }
            cmbSlot.SelectedIndex = 0;

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            try
            {
                dgvOperators.DataSource = null;
                dgvOperators.DataSource = _plcOperatorRepository.GetAll();
                if (dgvOperators.Columns["SlotIndex"] != null) dgvOperators.Columns["SlotIndex"].HeaderText = "DB ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator templates: {ex.Message}", "Error");
            }
        }

        // --- YENİ EKLENEN OLAY YÖNETİCİLERİ ---

        private void DgvOperators_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Düzenlenen satırdaki nesneyi al
                if (dgvOperators.Rows[e.RowIndex].DataBoundItem is PlcOperator editedOperator)
                {
                    // Repository'deki yeni Update metodunu çağırarak anlık kayıt yap
                    _plcOperatorRepository.Update(editedOperator);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Değişiklik kaydedilirken hata oluştu: {ex.Message}", "Hata");
            }
        }

        private void DgvOperators_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Hangi kolonda işlem yapıldığını kontrol et
            string headerText = dgvOperators.Columns[e.ColumnIndex].DataPropertyName;

            // Sadece 'Password' ve 'UserId' alanlarını kontrol et
            if (headerText == "Password" || headerText == "UserId")
            {
                string newValue = e.FormattedValue.ToString();

                // Boş ise izin ver (isteğe bağlı)
                if (string.IsNullOrEmpty(newValue)) return;

                // 1. Sayısal Kontrol ve Word (short) Sınır Kontrolü
                // short.TryParse: Hem harf/sembol olup olmadığını, hem de 32767 sınırını kontrol eder.
                if (!short.TryParse(newValue, out short result))
                {
                    e.Cancel = true; // Geçersizse hücreden çıkmayı engelle
                    dgvOperators.Rows[e.RowIndex].ErrorText = "Invalid Value! Enter only numbers (Max: 32767).";
                    MessageBox.Show($"'{headerText}' Only numbers can be entered in this field, and the value cannot exceed the limit of 32,767.", "Invalid Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (result < 0)
                {
                    // Negatif sayı kontrolü
                    e.Cancel = true;
                    dgvOperators.Rows[e.RowIndex].ErrorText = "Negative values ​​cannot be entered.";
                    MessageBox.Show("Please enter 0 or a greater value.", "Invalid Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    // Hata yoksa satırdaki hata ikonunu temizle
                    dgvOperators.Rows[e.RowIndex].ErrorText = string.Empty;
                }
            }
        }

        private void DgvOperators_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Olası format hatalarında çökmesini engelle
            e.Cancel = true;
        }

        // ------------------------------------------

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (cmbMachines.SelectedItem is Machine selectedMachine)
            {
                if (_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
                {
                    int slotIndex = cmbSlot.SelectedIndex; // 0-4
                    ReadOperatorFromPlc(plcManager, slotIndex);
                }
            }
        }

        // DEĞİŞİKLİK: LsPlcManager -> IPlcManager
        private async void ReadOperatorFromPlc(IPlcManager plcManager, int slotIndex)
        {
            this.Cursor = Cursors.WaitCursor;
            var result = await plcManager.ReadSinglePlcOperatorAsync(slotIndex);
            this.Cursor = Cursors.Default;

            if (result.IsSuccess)
            {
                var opFromPlc = result.Content;
                _plcOperatorRepository.SaveOrUpdate(opFromPlc);
                RefreshGrid();
                MessageBox.Show($"The operator information at {slotIndex + 1} on the machine was read and added/updated to the list.", "Success");
            }
            else
            {
                MessageBox.Show($"Error reading operator: {result.Message}", "Error");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (dgvOperators.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an operator from the list to send to PLC.", "Warning");
                return;
            }
            if (cmbMachines.SelectedItem is Machine selectedMachine)
            {
                if (_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
                {
                    var selectedOperator = dgvOperators.SelectedRows[0].DataBoundItem as PlcOperator;
                    int slotIndex = cmbSlot.SelectedIndex; // 0-4

                    // GÜNCELLENDİ: SlotIndex'i gönderim için değiştiriyoruz.
                    // Veritabanı bütünlüğünün bozulmaması için işlem sonrasında liste yenilenecek.
                    selectedOperator.SlotIndex = slotIndex;

                    SendOperatorToPlc(plcManager, selectedOperator);
                }
            }
        }

        // DEĞİŞİKLİK: LsPlcManager -> IPlcManager
        private async void SendOperatorToPlc(IPlcManager plcManager, PlcOperator plcOperator)
        {
            this.Cursor = Cursors.WaitCursor;
            var result = await plcManager.WritePlcOperatorAsync(plcOperator);
            this.Cursor = Cursors.Default;

            if (result.IsSuccess)
            {
                MessageBox.Show($"Operator '{plcOperator.Name}' was successfully written to slot {plcOperator.SlotIndex + 1} of the selected machine.", "Success");

                // GÜNCELLENDİ: SlotIndex değişikliğinin grid üzerindeki etkisini düzeltmek için yenile
                RefreshGrid();
            }
            else
            {
                MessageBox.Show($"Error sending operator: {result.Message}", "Error");
                RefreshGrid();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOperators.SelectedRows.Count > 0)
            {
                var selectedOperator = dgvOperators.SelectedRows[0].DataBoundItem as PlcOperator;
                var result = MessageBox.Show($"'{selectedOperator.Name}' Are you sure you want to delete the template?", "Confirm", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    _plcOperatorRepository.Delete(selectedOperator.SlotIndex);
                    RefreshGrid();
                }
            }
        }

        private void ekle_Click(object sender, EventArgs e)
        {
            try
            {
                // Yeni bir boş operatör şablonu oluşturup veritabanına ekle
                _plcOperatorRepository.AddDefaultOperator();

                // Tabloyu yenile
                RefreshGrid();

                MessageBox.Show("A new blank operator template has been added successfully. Click on it to edit and save.", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding a new operator: {ex.Message}", "Error");
            }
        }
    }
}