// UI/Views/PlcOperatorSettings_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Repositories;
using Telemetry.Services;

namespace Telemetry.UI.Views
{
    public partial class PlcOperatorSettings_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private PlcOperatorRepository _plcOperatorRepository;
        private Dictionary<int, IPlcManager> _plcManagers;

        public PlcOperatorSettings_Control()
        {
            // Statik dil değişim olayına kayıt (Bellek sızıntısı koruması için OnHandleDestroyed içinde çıkarılacak)
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // UX OPTMİZASYONU: Ayarlar tab panelinin koyu/açık temasına pürüzsüz uyum sağlar
            this.BackColor = Color.Transparent;
            this.DoubleBuffered = true;

            _plcOperatorRepository = new PlcOperatorRepository();
            dgvOperators.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOperators.ScrollBars = ScrollBars.Both;

            // SPEED OPTİMİZASYON: Tablo kaydırma (scroll) ve düzenleme süreçlerinde göz kırpmayı engeller
            EnableDoubleBuffer(dgvOperators);

            // Olay Yöneticileri
            dgvOperators.CellEndEdit += DgvOperators_CellEndEdit;
            dgvOperators.CellValidating += DgvOperators_CellValidating;
            dgvOperators.DataError += DgvOperators_DataError;

            // =========================================================================
            // MODERNİZASYON: STANDART COMBOBOX ELEMANLARINI DARK MODE UYARLAMA ADAPTÖRÜ
            // Açılır kutuları koyu tema arka planıyla kusursuz eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);    // Koyu grafit gri
            Color controlFg = Color.FromArgb(240, 240, 240); // Soft mat beyaz

            if (cmbMachines != null) { cmbMachines.BackColor = controlBg; cmbMachines.ForeColor = controlFg; }
            if (cmbSlot != null) { cmbSlot.BackColor = controlBg; cmbSlot.ForeColor = controlFg; }
        }

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

            cmbSlot.Items.Clear();
            for (int i = 1; i <= 5; i++)
            {
                cmbSlot.Items.Add(i);
            }
            cmbSlot.SelectedIndex = 0;

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            if (this.IsDisposed) return;

            // SPEED OPTİMİZASYON: Veriler tabloya bind edilirken arayüzün parça parça çizilmesini önler
            this.SuspendLayout();

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
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void DgvOperators_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvOperators.Rows[e.RowIndex].DataBoundItem is PlcOperator editedOperator)
                {
                    _plcOperatorRepository.Update(editedOperator);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Değişiklik kaydedilirken hata oluştu: {ex.Message}", "Hata");
            }
        }

        private void dgvOperators_SelectionChanged(object sender, EventArgs e)
        {
            // İhtiyaç durumunda tasarımcı referansı için açık bırakılmıştır.
        }

        private void DgvOperators_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string headerText = dgvOperators.Columns[e.ColumnIndex].DataPropertyName;

            if (headerText == "Password" || headerText == "UserId")
            {
                string newValue = e.FormattedValue.ToString();

                if (string.IsNullOrEmpty(newValue)) return;

                if (!short.TryParse(newValue, out short result))
                {
                    e.Cancel = true;
                    dgvOperators.Rows[e.RowIndex].ErrorText = "Invalid Value! Enter only numbers (Max: 32767).";
                    MessageBox.Show($"'{headerText}' Only numbers can be entered in this field, and the value cannot exceed the limit of 32,767.", "Invalid Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (result < 0)
                {
                    e.Cancel = true;
                    dgvOperators.Rows[e.RowIndex].ErrorText = "Negative values cannot be entered.";
                    MessageBox.Show("Please enter 0 or a greater value.", "Invalid Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    dgvOperators.Rows[e.RowIndex].ErrorText = string.Empty;
                }
            }
        }

        private void DgvOperators_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (cmbMachines.SelectedItem is Machine selectedMachine)
            {
                if (_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
                {
                    int slotIndex = cmbSlot.SelectedIndex;
                    ReadOperatorFromPlc(plcManager, slotIndex);
                }
            }
        }

        private async void ReadOperatorFromPlc(IPlcManager plcManager, int slotIndex)
        {
            if (this.IsDisposed) return;

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
                    int slotIndex = cmbSlot.SelectedIndex;

                    selectedOperator.SlotIndex = slotIndex;
                    SendOperatorToPlc(plcManager, selectedOperator);
                }
            }
        }

        private async void SendOperatorToPlc(IPlcManager plcManager, PlcOperator plcOperator)
        {
            if (this.IsDisposed) return;

            this.Cursor = Cursors.WaitCursor;
            var result = await plcManager.WritePlcOperatorAsync(plcOperator);
            this.Cursor = Cursors.Default;

            if (result.IsSuccess)
            {
                MessageBox.Show($"Operator '{plcOperator.Name}' was successfully written to slot {plcOperator.SlotIndex + 1} of the selected machine.", "Success");
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
                _plcOperatorRepository.AddDefaultOperator();
                RefreshGrid();
                MessageBox.Show("A new blank operator template has been added successfully. Click on it to edit and save.", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding a new operator: {ex.Message}", "Error");
            }
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            // İhtiyaç halinde yerelleştirme fonksiyonu bağlanabilir
        }

        // =========================================================================
        // KUSURSUZ BELLEK TEMİZLİĞİ: STATİK EVENT ABONELİK BAĞLANTISI KOPARILDI
        // Kontrolün RAM'de asılı kalarak hafıza sızıntısı yapmasını kesin önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnHandleDestroyed(e);
        }

        // SPEED OPTİMİZASYON: DataGrid akıcılığını sağlayan yansıtma (Reflection) metodu
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
    }
}