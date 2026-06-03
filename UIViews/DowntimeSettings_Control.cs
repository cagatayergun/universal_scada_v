// UIViews/DowntimeSettings_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using Telemetry.Repositories;
using Telemetry.Services;
using Telemetry.Core;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UIViews
{
    public partial class DowntimeSettings_Control : UserControl
    {
        private EfficiencyRepository _efficiencyRepository;
        private PlcPollingService _plcPollingService;

        public DowntimeSettings_Control()
        {
            InitializeComponent();

            // UX OPTMİZASYONU: Ayarlar tab kontrolünün koyu/açık temasına lekesiz uyum sağlar
            this.BackColor = Color.Transparent;
            this.DoubleBuffered = true;

            // SPEED OPTİMİZASYON: Satır ekleme/silme süreçlerinde tablonun göz kırpmasını engeller
            EnableDoubleBuffer(dgvDowntime);
        }

        /// <summary>
        /// Called to initialize this control from the settings page.
        /// </summary>
        public void InitializeControl(EfficiencyRepository efficiencyRepo, PlcPollingService pollingService)
        {
            _efficiencyRepository = efficiencyRepo;
            _plcPollingService = pollingService;
            LoadData();
        }

        private void DowntimeSettings_Control_Load(object sender, EventArgs e)
        {
            // Kod arkasından güvenli başlatma mimarisi korundu.
        }

        private void LoadData()
        {
            if (_efficiencyRepository == null || this.IsDisposed) return;

            // Performans için arayüz çizim motorunu kilitle
            this.SuspendLayout();
            dgvDowntime.SuspendLayout();

            try
            {
                var definitions = _efficiencyRepository.GetDowntimeDefinitions();

                dgvDowntime.Rows.Clear();
                foreach (var def in definitions.OrderBy(x => x.Key))
                {
                    dgvDowntime.Rows.Add(def.Key, def.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Çizim kilitlerini kaldır ve toplu render et
                dgvDowntime.ResumeLayout(true);
                this.ResumeLayout(true);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvDowntime.Rows.Add();
            dgvDowntime.Rows[rowIndex].Cells[0].Value = 0; // Varsayılan PLC bit adresi
            dgvDowntime.Rows[rowIndex].Cells[1].Value = "New Downtime Reason";
            dgvDowntime.CurrentCell = dgvDowntime.Rows[rowIndex].Cells[1];
            dgvDowntime.BeginEdit(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDowntime.SelectedRows.Count > 0)
            {
                var confirm = MessageBox.Show("Are you sure you want to delete the selected reason?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    dgvDowntime.SuspendLayout();
                    try
                    {
                        foreach (DataGridViewRow row in dgvDowntime.SelectedRows)
                        {
                            if (!row.IsNewRow)
                            {
                                dgvDowntime.Rows.Remove(row);
                            }
                        }
                    }
                    finally
                    {
                        dgvDowntime.ResumeLayout(true);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;

            this.Cursor = Cursors.WaitCursor;
            this.SuspendLayout();

            try
            {
                var list = new List<dynamic>();
                var bitIndices = new HashSet<int>();

                // Veri doğrulama ve toplama döngüsü
                foreach (DataGridViewRow row in dgvDowntime.Rows)
                {
                    if (row.Cells[0].Value == null || row.Cells[1].Value == null) continue;

                    if (!int.TryParse(row.Cells[0].Value.ToString(), out int bitIndex))
                    {
                        MessageBox.Show("Bit address must be numeric!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (bitIndices.Contains(bitIndex))
                    {
                        MessageBox.Show($"Duplicate Bit Address detected: {bitIndex}. Please provide a unique definition for each bit.",
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    bitIndices.Add(bitIndex);
                    list.Add(new
                    {
                        BitIndex = bitIndex,
                        ReasonText = row.Cells[1].Value.ToString()
                    });
                }

                // 1. Veritabanına Asenkron Kayıt Döngüsü
                await _efficiencyRepository.SaveDowntimeDefinitionsAsync(list);

                // 2. Arka plan PLC polling servisini anında güncelle (Önbellek Tazeleme)
                if (_plcPollingService != null)
                {
                    _plcPollingService.LoadWaitingDefinitionsCache();
                }

                MessageBox.Show("Downtime reasons successfully saved and the system has been updated.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadData(); // Listeyi tazele
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during save: {ex.Message}", "Critical Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.ResumeLayout(true);
                this.Cursor = Cursors.Default;
            }
        }

        // SPEED OPTİMİZASYON: Tablonun kaydırma (scroll) ivmesini artıran yansıtma metodu
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