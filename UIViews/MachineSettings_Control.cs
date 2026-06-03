// UI/Views/MachineSettings_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.Services;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class MachineSettings_Control : UserControl
    {
        public event EventHandler MachineListChanged;
        private readonly UserRepository _userRepository;
        private readonly MachineRepository _repository;
        private List<Telemetry.Models.Machine> _machines;
        private Telemetry.Models.Machine _selectedMachine;
        private List<object> _machineTypeOptions;

        public MachineSettings_Control()
        {
            // Statik dil değişim olayına kayıt
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            _repository = new MachineRepository();
            _userRepository = new UserRepository();

            // SPEED OPTİMİZASYON: Listeler yüklenirken ve pencereler kaydırılırken (scroll) titremeyi engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimi üst ebeveyne devredildi
            EnableDoubleBuffer(dgvMachines);

            // =========================================================================
            // MODERNİZASYON: STANDART METİN KUTULARINI DARK MODE UYARLAMA ADAPTÖRÜ
            // Giriş ve parametre kutularını koyu tema arka planıyla kusursuz eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);    // Koyu grafit gri
            Color controlFg = Color.FromArgb(240, 240, 240); // Soft mat beyaz

            var textControls = new List<TextBox>
            {
                txtMachineId, txtMachineName, txtIpAddress, txtPort,
                txtVncAddress, txtFtpUsername, txtFtpPassword,
                txtMachineSubType, displaybox, textdisplayname
            };

            foreach (var txt in textControls)
            {
                if (txt != null)
                {
                    txt.BackColor = controlBg;
                    txt.ForeColor = controlFg;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
            }
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
            PopulateMachineTypeComboBox();
        }

        public void ApplyLocalization()
        {
            groupBox1.Text = Resources.makinebilgileri;
            label1.Text = Resources.MachineID;
            label2.Text = Resources.MachineName;
            label3.Text = Resources.ipadres;
            label5.Text = Resources.vncadres;
            chkIsEnabled.Text = Resources.izlemeaktif;
            label6.Text = Resources.makinegrup;
            label9.Text = Resources.makinetip;
            label7.Text = Resources.ftpuser;
            label8.Text = Resources.ftppass;
            btnDelete.Text = Resources.Delete;
            btnNew.Text = Resources.New;
            btnSave.Text = Resources.Save;
        }

        private void MachineSettings_Control_Load(object sender, EventArgs e)
        {
            PopulateMachineTypeComboBox();
            RefreshMachineList();
            ApplyLocalization();
        }

        private void PopulateMachineTypeComboBox()
        {
            _machineTypeOptions = new List<object>
            {
                new { Display = Resources.bymakinesi,       Value = "BYMakinesi" },
                new { Display = Resources.kurutmamakinesi,  Value = "Kurutma Makinesi" }
            };

            cmbMachineType.DataSource = null;
            cmbMachineType.DataSource = _machineTypeOptions;
            cmbMachineType.DisplayMember = "Display";
            cmbMachineType.ValueMember = "Value";
        }

        public void RefreshMachineList()
        {
            if (this.IsDisposed) return;

            // SPEED OPTİMİZASYON: Veriler grid üzerine bind edilirken ekranda dalgalanma oluşmasını engeller
            this.SuspendLayout();

            try
            {
                _machines = _repository.GetAllMachines();
                dgvMachines.DataSource = null;
                dgvMachines.DataSource = _machines;

                if (dgvMachines.Columns["Id"] != null) dgvMachines.Columns["Id"].Visible = false;
                if (dgvMachines.Columns["VncPassword"] != null) dgvMachines.Columns["VncPassword"].Visible = false;
                if (dgvMachines.Columns["FtpPassword"] != null) dgvMachines.Columns["FtpPassword"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.makineyüklemehatası} {ex.Message}", $"{Resources.DatabaseError}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void dgvMachines_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMachines.SelectedRows.Count > 0)
            {
                _selectedMachine = dgvMachines.SelectedRows[0].DataBoundItem as Telemetry.Models.Machine;
                if (_selectedMachine != null)
                {
                    PopulateFields(_selectedMachine);
                }
            }
        }

        private void PopulateFields(Telemetry.Models.Machine machine)
        {
            txtMachineId.Text = machine.MachineUserDefinedId;
            txtMachineName.Text = machine.MachineName;
            txtIpAddress.Text = machine.IpAddress;
            txtPort.Text = machine.Port.ToString();
            txtVncAddress.Text = machine.VncAddress;
            chkIsEnabled.Checked = machine.IsEnabled;
            cmbMachineType.SelectedValue = machine.MachineType;
            txtFtpUsername.Text = machine.FtpUsername;
            txtFtpPassword.Text = machine.FtpPassword;
            txtMachineSubType.Text = machine.MachineSubType;
            displaybox.Text = machine.DisplayOrder.ToString();
            textdisplayname.Text = string.IsNullOrEmpty(machine.MachineHall) ? "Empty" : machine.MachineHall;
        }

        private void ClearFields()
        {
            _selectedMachine = null;
            dgvMachines.ClearSelection();
            txtMachineId.Text = "";
            txtMachineName.Text = "";
            txtIpAddress.Text = "";
            txtPort.Text = "502";
            txtVncAddress.Text = "";
            chkIsEnabled.Checked = true;
            cmbMachineType.SelectedIndex = 0;
            txtFtpUsername.Text = "";
            txtFtpPassword.Text = "";
            txtMachineSubType.Text = "";
            displaybox.Text = "";
            textdisplayname.Text = "Empty";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            dgvMachines.SelectionChanged -= dgvMachines_SelectionChanged;
            ClearFields();
            dgvMachines.SelectionChanged += dgvMachines_SelectionChanged;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMachineId.Text) || string.IsNullOrWhiteSpace(txtIpAddress.Text) || string.IsNullOrWhiteSpace(txtMachineSubType.Text))
            {
                MessageBox.Show($"{Resources.makineidveipzorunlu}]", $"{Resources.EksikBilgi}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hallName = string.IsNullOrWhiteSpace(textdisplayname.Text) ? "Empty" : textdisplayname.Text;

            try
            {
                if (_selectedMachine == null) // Yeni Kayıt
                {
                    var newMachine = new Telemetry.Models.Machine
                    {
                        MachineUserDefinedId = txtMachineId.Text,
                        MachineName = txtMachineName.Text,
                        IpAddress = txtIpAddress.Text,
                        Port = int.Parse(txtPort.Text),
                        VncAddress = txtVncAddress.Text,
                        IsEnabled = chkIsEnabled.Checked,
                        MachineType = cmbMachineType.SelectedValue.ToString(),
                        FtpUsername = txtFtpUsername.Text,
                        FtpPassword = txtFtpPassword.Text,
                        MachineSubType = txtMachineSubType.Text,
                        DisplayOrder = string.IsNullOrWhiteSpace(displaybox.Text) ? 0 : int.Parse(displaybox.Text),
                        MachineHall = hallName
                    };
                    _repository.AddMachine(newMachine);
                    MessageBox.Show($"{Resources.yenimakinebasarili}", $"{Resources.Confirim}", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (CurrentUser.IsLoggedIn)
                    {
                        _userRepository.LogAction(CurrentUser.User.Id, "Machine Settings", $"'{newMachine.MachineName}' Added new machine called.");
                    }
                }
                else // Güncelleme
                {
                    _selectedMachine.MachineUserDefinedId = txtMachineId.Text;
                    _selectedMachine.MachineName = txtMachineName.Text;
                    _selectedMachine.IpAddress = txtIpAddress.Text;
                    _selectedMachine.Port = int.Parse(txtPort.Text);
                    _selectedMachine.VncAddress = txtVncAddress.Text;
                    _selectedMachine.IsEnabled = chkIsEnabled.Checked;
                    _selectedMachine.MachineType = cmbMachineType.SelectedValue.ToString();
                    _selectedMachine.FtpUsername = txtFtpUsername.Text;
                    _selectedMachine.FtpPassword = txtFtpPassword.Text;
                    _selectedMachine.MachineSubType = txtMachineSubType.Text;
                    _selectedMachine.DisplayOrder = string.IsNullOrWhiteSpace(displaybox.Text) ? 0 : int.Parse(displaybox.Text);
                    _selectedMachine.MachineHall = hallName;

                    _repository.UpdateMachine(_selectedMachine);

                    MessageBox.Show($"{Resources.makinebilgilerigüncellendi}", $"{Resources.Confirim}", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (CurrentUser.IsLoggedIn)
                    {
                        _userRepository.LogAction(CurrentUser.User.Id, "Machine Settings", $"The settings for the machine '{_selectedMachine.MachineName}' have been updated.");
                    }
                }

                dgvMachines.SelectionChanged -= dgvMachines_SelectionChanged;
                RefreshMachineList();
                ClearFields();
                dgvMachines.SelectionChanged += dgvMachines_SelectionChanged;
                MachineListChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.kayitsirasihatasi} {ex.Message}", $"{Resources.Error}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedMachine == null)
            {
                MessageBox.Show($"{Resources.lütfensilmekicinmakinesec}", $"{Resources.Warning}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"'{_selectedMachine.MachineName}' ${Resources.makinesilmeeminmisin}", $"{Resources.silmeonayı}", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _repository.DeleteMachine(_selectedMachine.Id);
                    RefreshMachineList();
                    ClearFields();
                    MachineListChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Resources.Silmesırasındahata} {ex.Message}", $"{Resources.Error}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =========================================================================
        // BELLEK SIZINTISI KORUMASI: STATİK EVENT BAĞLANTI TEMİZLİĞİ
        // Kontrolün RAM'de asılı kalarak şişme yapmasını kesin olarak engeller.
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
                System.Diagnostics.Debug.WriteLine($"DoubleBuffering error: {ex.Message}");
            }
        }
    }
}