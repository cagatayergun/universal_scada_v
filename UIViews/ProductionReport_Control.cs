// UI/Views/ProductionReport_Control.cs
using System;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.UI;
using static TekstilScada.Repositories.ProductionRepository;

namespace TekstilScada.UI.Views
{
    public partial class ProductionReport_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProductionRepository _productionRepository;
        private RecipeRepository _recipeRepository; // YENİ
        private ProcessLogRepository _processLogRepo; // HATA GİDERİLDİ: Alan eklendi
        private AlarmRepository _alarmRepo; // HATA GİDERİLDİ: Alan eklendi
        public ProductionReport_Control()
        {
            InitializeComponent();
        }

        public void InitializeControl(MachineRepository machineRepo, ProductionRepository productionRepo, RecipeRepository recipeRepo, ProcessLogRepository processLogRepo , AlarmRepository alarmRepo)
        {
            _machineRepository = machineRepo;
            _productionRepository = productionRepo;
            _recipeRepository = recipeRepo; // YENİ
            _processLogRepo = processLogRepo; // Gelen parametre alana atandı
            _alarmRepo = alarmRepo;           // Gelen parametre alana atandı
        }

        private void ProductionReport_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Today.AddDays(-7);
            dtpEndTime.Value = DateTime.Now;

            var machines = _machineRepository.GetAllMachines();
            machines.Insert(0, new Machine { Id = -1, MachineName = "All Machines", MachineUserDefinedId = "" });
            cmbMachines.DataSource = machines;
            cmbMachines.DisplayMember = "DisplayInfo";
            cmbMachines.ValueMember = "Id";
        }

        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            var filters = new ReportFilters
            {
                StartTime = dtpStartTime.Value,
                EndTime = dtpEndTime.Value,
                MachineId = (int)cmbMachines.SelectedValue == -1 ? (int?)null : (int)cmbMachines.SelectedValue,
                BatchNo = txtBatchNo.Text,
                RecipeName = txtRecipeName.Text,
                SiparisNo = txtOrderNo.Text,
                MusteriNo = txtCustomerNo.Text,
                OperatorName = txtOperator.Text
            };

            try
            {
                this.Cursor = Cursors.WaitCursor;
                var reportData = _productionRepository.GetProductionReport(filters);
                foreach (var item in reportData)
                {
                    item.TotalWater = item.TotalWater / 1000.0;
                    item.TotalElectricity = item.TotalElectricity / 1000.0;
                    item.TotalSteam = item.TotalSteam / 1000.0;
                }
                dgvProductionReport.DataSource = null;
                dgvProductionReport.DataSource = reportData;
                CustomizeGridHeaders();
                // --- YENİ EKLENECEK KOD BAŞLANGICI ---
                try
                {
                    // İstenmeyen OEE ve detay sütunlarını gizle
                    if (dgvProductionReport.Columns["MachineAlarmDurationSeconds"] != null)
                        dgvProductionReport.Columns["MachineAlarmDurationSeconds"].Visible = false;

                    if (dgvProductionReport.Columns["OperatorPauseDurationSeconds"] != null)
                        dgvProductionReport.Columns["OperatorPauseDurationSeconds"].Visible = false;

                    if (dgvProductionReport.Columns["TheoreticalCycleTimeSeconds"] != null)
                        dgvProductionReport.Columns["TheoreticalCycleTimeSeconds"].Visible = false;

                    if (dgvProductionReport.Columns["GoodCount"] != null)
                        dgvProductionReport.Columns["GoodCount"].Visible = false;

                    if (dgvProductionReport.Columns["ScrapCount"] != null)
                        dgvProductionReport.Columns["ScrapCount"].Visible = false;

                    if (dgvProductionReport.Columns["TotalProductionCount"] != null)
                        dgvProductionReport.Columns["TotalProductionCount"].Visible = false;

                    if (dgvProductionReport.Columns["DefectiveProductionCount"] != null)
                        dgvProductionReport.Columns["DefectiveProductionCount"].Visible = false;

                    if (dgvProductionReport.Columns["TotalDownTimeSeconds"] != null)
                        dgvProductionReport.Columns["TotalDownTimeSeconds"].Visible = false;
                }
                catch (Exception ex)
                {
                    // Kolon gizleme sırasında hata olursa kullanıcıyı bilgilendir
                    MessageBox.Show($"An error occurred while setting up report columns: {ex.Message}", "Warning");
                }
                // --- YENİ EKLENECEK KOD BİTİŞİ ---
            }

            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the report: {ex.Message}", "Error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void CustomizeGridHeaders()
        {
            // DataGridView isminiz muhtemelen 'dgvReport' veya 'dgvProduction'. 
            // Lütfen kendi kodunuzdaki isme göre burayı güncelleyin.
            var grid = dgvProductionReport; // <-- BURAYI KONTROL EDİN

            if (grid.DataSource == null) return;

            // --- GENEL BİLGİLER ---
            SetColumnHeader(grid, "MachineId", "Machine ID");
            SetColumnHeader(grid, "MachineName", "Machine Name");
            SetColumnHeader(grid, "MakineAdi", "Machine Name");

            SetColumnHeader(grid, "BatchNumarasi", "Batch No");
            SetColumnHeader(grid, "BatchNo", "Batch No");
            SetColumnHeader(grid, "PartiNo", "Batch No");

            SetColumnHeader(grid, "RecipeName", "Recipe Name");
            SetColumnHeader(grid, "ReceteAdi", "Recipe Name");

            SetColumnHeader(grid, "OrderNo", "Order No");
            SetColumnHeader(grid, "SiparisNo", "Order No");

            SetColumnHeader(grid, "Customer", "Customer");
            SetColumnHeader(grid, "MusteriNo", "Customer");

            SetColumnHeader(grid, "OperatorName", "Operator Name");
            SetColumnHeader(grid, "Operator", "Operator Name");

            // --- ZAMANLAMALAR ---
            SetColumnHeader(grid, "StartTime", "Start Time");
            SetColumnHeader(grid, "BaslangicZamani", "Start Time");
            SetColumnHeader(grid, "Baslangic", "Start Time");

            SetColumnHeader(grid, "EndTime", "End Time");
            SetColumnHeader(grid, "BitisZamani", "End Time");
            SetColumnHeader(grid, "Bitis", "End Time");

            SetColumnHeader(grid, "Duration", "Duration");
            SetColumnHeader(grid, "Sure", "Duration");
            SetColumnHeader(grid, "TotalDuration", "Total Duration");

            // --- TÜKETİMLER ---
            if (grid.Columns.Contains("TotalWater"))
            {
                grid.Columns["TotalWater"].HeaderText = "Total Water (m³)";
                grid.Columns["TotalWater"].DefaultCellStyle.Format = "N2"; // 0.00 formatı
            }

            // ELEKTRİK
            if (grid.Columns.Contains("TotalElectricity"))
            {
                grid.Columns["TotalElectricity"].HeaderText = "Total Electricity (kWh)";
                grid.Columns["TotalElectricity"].DefaultCellStyle.Format = "N2"; // 0.00 formatı
            }

            // BUHAR
            if (grid.Columns.Contains("TotalSteam"))
            {
                grid.Columns["TotalSteam"].HeaderText = "Total Steam (m³)";
                grid.Columns["TotalSteam"].DefaultCellStyle.Format = "N2"; // 0.00 formatı
            }

            SetColumnHeader(grid, "Cost", "Total Cost");
            SetColumnHeader(grid, "Maliyet", "Total Cost");

            // --- GÖRÜNÜM AYARLARI ---
            // Tarih formatlarını düzeltelim
            if (grid.Columns.Contains("StartTime")) grid.Columns["StartTime"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
            if (grid.Columns.Contains("EndTime")) grid.Columns["EndTime"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";

            
       
        }

        // Yardımcı Metot: Kolon varsa başlığını değiştirir
        private void SetColumnHeader(DataGridView grid, string dbColumnName, string newHeaderName)
        {
            if (grid.Columns.Contains(dbColumnName))
            {
                grid.Columns[dbColumnName].HeaderText = newHeaderName;
            }
        }

        
        private void dgvProductionReport_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedReportItem = dgvProductionReport.Rows[e.RowIndex].DataBoundItem as ProductionReportItem;
                if (selectedReportItem != null)
                {
                    // GÜNCELLENDİ: Detail form'a recipeRepository de gönderiliyor
                    var detailForm = new ProductionDetail_Form(selectedReportItem, _recipeRepository, _processLogRepo, _alarmRepo);
                    detailForm.Show();
                }
            }
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            ExcelExporter.ExportDataGridViewToExcel(dgvProductionReport);
        }
    }
}