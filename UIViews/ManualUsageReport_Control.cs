// UI/Views/ManualUsageReport_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;

namespace TekstilScada.UI.Views
{
    public partial class ManualUsageReport_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProcessLogRepository _processLogRepository;

        public ManualUsageReport_Control()
        {
            InitializeComponent();
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            // Hücre formatlama olayını (Birim dönüşümü için) bağlıyoruz
            dgvManualUsage.CellFormatting += DgvManualUsage_CellFormatting;
        }

        public void InitializeControl(MachineRepository machineRepo, ProcessLogRepository processLogRepo)
        {
            _machineRepository = machineRepo;
            _processLogRepository = processLogRepo;
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            label1.Text = Resources.DateRange;
            label3.Text = Resources.Machine;
            btnGenerateReport.Text = Resources.Reports;
            btnExportToExcel.Text = Resources.ExportToExcel;
        }

        private void ManualUsageReport_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Today;
            dtpEndTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            var machines = _machineRepository.GetAllMachines();
            cmbMachines.DataSource = machines;
            cmbMachines.DisplayMember = "DisplayInfo";
            cmbMachines.ValueMember = "Id";
        }

        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            DateTime startTime = dtpStartTime.Value;
            DateTime endTime = dtpEndTime.Value;

            var selectedMachine = cmbMachines.SelectedItem as Machine;

            if (selectedMachine == null) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                var summary = _processLogRepository.GetManualConsumptionSummary(selectedMachine.Id, selectedMachine.MachineName, startTime, endTime);

                var reportData = new List<ManualConsumptionSummary>();
                if (summary != null)
                {
                    reportData.Add(summary);
                }

                dgvManualUsage.DataSource = null;
                dgvManualUsage.DataSource = reportData;

                // Tablo görünümünü ve başlıkları ayarla
                CustomizeGridAppearance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata} {ex.Message}", $"{Resources.Error}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void CustomizeGridAppearance()
        {
            if (dgvManualUsage.DataSource == null) return;

            // 1. İstenmeyen kolonları gizle
            if (dgvManualUsage.Columns["OrtalamaSicaklik"] != null)
                dgvManualUsage.Columns["OrtalamaSicaklik"].Visible = false;

            if (dgvManualUsage.Columns["OrtalamaDevir"] != null)
                dgvManualUsage.Columns["OrtalamaDevir"].Visible = false;

            // 2. Başlıkları İngilizce yap ve Birimleri Ekle
            if (dgvManualUsage.Columns["MachineName"] != null)
                dgvManualUsage.Columns["MachineName"].HeaderText = "Machine Name";
            
            if (dgvManualUsage.Columns["ToplamSuTuketimi_Litre"] != null)
                dgvManualUsage.Columns["ToplamSuTuketimi_Litre"].HeaderText = "Total Water (m³)";

            if (dgvManualUsage.Columns["ToplamElektrikTuketimi_kW"] != null)
                dgvManualUsage.Columns["ToplamElektrikTuketimi_kW"].HeaderText = "Total Electricity (kWh)";

            if (dgvManualUsage.Columns["ToplamBuharTuketimi_kg"] != null)
                dgvManualUsage.Columns["ToplamBuharTuketimi_kg"].HeaderText = "Total Steam (m³)";

            if (dgvManualUsage.Columns["DurationMinutes"] != null)
                dgvManualUsage.Columns["DurationMinutes"].HeaderText = "Duration (Minutes)";

            // 3. Sayı formatı (Virgülden sonra 2 basamak)
            dgvManualUsage.DefaultCellStyle.Format = "N2";
        }

        // Verileri dönüştürmek için (Litre -> m³, Watt -> kW) kullanılan olay
        // Verileri dönüştürmek için (Litre -> m³, Watt -> kW) kullanılan olay
        private void DgvManualUsage_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Satır indeksi geçersizse veya değer boşsa işlem yapma
            if (e.RowIndex < 0 || e.Value == null || e.Value == DBNull.Value) return;

            string colName = dgvManualUsage.Columns[e.ColumnIndex].Name;

            // Su, Elektrik ve Buhar kolonlarını yakala
            if (colName == "ToplamSuTuketimi_Litre" ||
                colName == "ToplamElektrikTuketimi_kW" ||
                colName == "ToplamBuharTuketimi_kg")
            {
                try
                {
                    // Değeri double'a çevir
                    double val = Convert.ToDouble(e.Value);

                    // 1000'e böl
                    double result = val / 1000.0;

                    // Sonucu virgülden sonra 2 basamaklı String olarak ata
                    // Bu sayede "N2" formatı ile çakışmaz ve kesin görünür.
                    e.Value = result.ToString("N2");

                    // Formatlamanın tamamlandığını bildir (Grid tekrar formatlamaya çalışmasın)
                    e.FormattingApplied = true;
                }
                catch
                {
                    // Eğer sayısal bir değer değilse (örn. hata metni varsa) dokunma
                }
            }
        }

        private async void btnExportToExcel_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ExcelExporter.ExportDataGridViewToExcel(dgvManualUsage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata} {ex.Message}", $"{Resources.Error}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}