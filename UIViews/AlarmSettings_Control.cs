// UI/Views/AlarmSettings_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;

namespace Telemetry.UI.Views
{
    public partial class AlarmSettings_Control : UserControl
    {
        private readonly AlarmRepository _repository;
        private List<AlarmDefinition> _definitions;
        private AlarmDefinition _selectedDefinition;

        public AlarmSettings_Control()
        {
            // Dil değişim olayına kayıt
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            _repository = new AlarmRepository();

            // SPEED OPTİMİZASYON: Listeler yüklenirken ve pencereler kaydırılırken (scroll) titremeyi engeller
            this.DoubleBuffered = true;
            EnableDoubleBuffer(dgvAlarms);

            // =========================================================================
            // MODERNİZASYON: STANDART KONTROLLERİ DARK MODE UYARLAMA ADAPTÖRÜ
            // Sayı ve düz metin kutularını koyu tema arka planıyla kusursuz eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);    // Koyu grafit gri
            Color controlFg = Color.FromArgb(240, 240, 240); // Soft mat beyaz

            var numControls = new List<NumericUpDown> { numAlarmNo, numSeverity };
            foreach (var num in numControls)
            {
                if (num != null)
                {
                    num.BackColor = controlBg;
                    num.ForeColor = controlFg;
                    num.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            // Standart metin kutularının renk adaptasyonu
            if (txtAlarmText != null) { txtAlarmText.BackColor = controlBg; txtAlarmText.ForeColor = controlFg; txtAlarmText.BorderStyle = BorderStyle.FixedSingle; }
            if (txtCategory != null) { txtCategory.BackColor = controlBg; txtCategory.ForeColor = controlFg; txtCategory.BorderStyle = BorderStyle.FixedSingle; }

            // Koyu Mod Metin Kontrastı: Sabit başlık yazıları açık gri tonlara çekildi
            Color labelColor = Color.FromArgb(176, 190, 197);
            if (label2 != null) label2.ForeColor = labelColor;
            if (label3 != null) label3.ForeColor = labelColor;
            if (label4 != null) label4.ForeColor = labelColor;

            // --- GÜVENLİ YERLEŞİM (LAYOUT FIX) ---
            this.Controls.Remove(dgvAlarms);
            this.Controls.Remove(groupBox1);

            Panel pnlGridContainer = new Panel();
            pnlGridContainer.Dock = DockStyle.Fill;
            pnlGridContainer.Controls.Add(dgvAlarms);

            dgvAlarms.Dock = DockStyle.Fill;
            dgvAlarms.ScrollBars = ScrollBars.Both;

            this.Controls.Add(pnlGridContainer);
            this.Controls.Add(groupBox1);

            this.Dock = DockStyle.Fill;
            // ----------------------------------------------------

            ApplyLocalization();
        }

        private void AlarmSettings_Control_Load(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            groupBox1.Text = Resources.AlarmDetails;
            label2.Text = Resources.AlarmText;
            label3.Text = Resources.Severity;
            label4.Text = Resources.Category;
            btnNew.Text = Resources.New;
            btnDelete.Text = Resources.Delete;
            btnSave.Text = Resources.Save;
        }

        private void RefreshList()
        {
            if (this.IsDisposed) return;

            // SPEED OPTİMİZASYON: Veriler grid üzerine bind edilirken ekranda dalgalanma oluşmasını engeller
            this.SuspendLayout();

            try
            {
                _definitions = _repository.GetAllAlarmDefinitions();
                dgvAlarms.DataSource = null;
                dgvAlarms.DataSource = _definitions;
                if (dgvAlarms.Columns["Id"] != null) dgvAlarms.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.alarmraporerror} {ex.Message}", $"{Resources.DatabaseError}");
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void dgvAlarms_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAlarms.SelectedRows.Count > 0)
            {
                _selectedDefinition = dgvAlarms.SelectedRows[0].DataBoundItem as AlarmDefinition;
                if (_selectedDefinition != null)
                {
                    PopulateFields(_selectedDefinition);
                }
            }
        }

        private void PopulateFields(AlarmDefinition def)
        {
            numAlarmNo.Value = def.AlarmNumber;
            txtAlarmText.Text = def.AlarmText;
            numSeverity.Value = def.Severity;
            txtCategory.Text = def.Category;
        }

        private void ClearFields()
        {
            _selectedDefinition = null;
            dgvAlarms.ClearSelection();
            numAlarmNo.Value = 0;
            txtAlarmText.Text = "";
            numSeverity.Value = 1;
            txtCategory.Text = "";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAlarmText.Text) || numAlarmNo.Value == 0)
            {
                MessageBox.Show($"{Resources.alarmnozorunlu}", $"{Resources.EksikBilgi}");
                return;
            }

            try
            {
                if (_selectedDefinition == null) // Yeni Kayıt
                {
                    var newDef = new AlarmDefinition
                    {
                        AlarmNumber = (int)numAlarmNo.Value,
                        AlarmText = txtAlarmText.Text,
                        Severity = (int)numSeverity.Value,
                        Category = txtCategory.Text
                    };
                    _repository.AddAlarmDefinition(newDef);
                }
                else // Güncelleme
                {
                    _selectedDefinition.AlarmNumber = (int)numAlarmNo.Value;
                    _selectedDefinition.AlarmText = txtAlarmText.Text;
                    _selectedDefinition.Severity = (int)numSeverity.Value;
                    _selectedDefinition.Category = txtCategory.Text;
                    _repository.UpdateAlarmDefinition(_selectedDefinition);
                }
                RefreshList();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.Kayıt_sırasında_hata_} {ex.Message}", $"{Resources.Error}");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedDefinition == null)
            {
                MessageBox.Show($"{Resources.lütfendeleteuyarı}", $"{Resources.Warning}");
                return;
            }
            var result = MessageBox.Show($"'{_selectedDefinition.AlarmText}' {Resources.alarmtanımısil}", $"{Resources.Confirim}", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _repository.DeleteAlarmDefinition(_selectedDefinition.Id);
                    RefreshList();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Resources.Silmesırasındahata} {ex.Message}", $"{Resources.Error}");
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