// UI/Views/ProductionReport_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Repositories;
using Telemetry.UI;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ
using static Telemetry.Repositories.ProductionRepository;

namespace Telemetry.UI.Views
{
    public partial class ProductionReport_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProductionRepository _productionRepository;
        private RecipeRepository _recipeRepository;
        private ProcessLogRepository _processLogRepo;
        private AlarmRepository _alarmRepo;

        public ProductionReport_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Kontrol yüklenirken arayüzün titremesini önler
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan rengini üst ebeveyn panel yönetir
        }

        public void InitializeControl(MachineRepository machineRepo, ProductionRepository productionRepo, RecipeRepository recipeRepo, ProcessLogRepository processLogRepo, AlarmRepository alarmRepo)
        {
            _machineRepository = machineRepo;
            _productionRepository = productionRepo;
            _recipeRepository = recipeRepo;
            _processLogRepo = processLogRepo;
            _alarmRepo = alarmRepo;
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

            // Tablo kaydırma (scroll) performansını maksimuma çıkaran donanımsal ivme
            EnableDoubleBuffer(dgvProductionReport);
        }

        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;

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

            // Sorgulama esnasında arayüz kilitleme ve yükleniyor hazırlığı
            this.Cursor = Cursors.WaitCursor;
            btnGenerateReport.Enabled = false;

            // SPEED OPTİMİZASYON: Tablo yenilenirken düzen motorunu askıya al
            this.SuspendLayout();

            try
            {
                // OPTİMİZASYON: Ağır log tarama sorgusu ve birim dönüşümleri tamamen asenkron arka plana taşındı
                var reportData = await Task.Run(() =>
                {
                    var data = _productionRepository.GetProductionReport(filters);
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            item.TotalWater = item.TotalWater / 1000.0;
                            item.TotalElectricity = item.TotalElectricity / 1000.0;
                            item.TotalSteam = item.TotalSteam;
                        }
                    }
                    return data;
                });

                dgvProductionReport.DataSource = null;
                if (reportData != null)
                {
                    dgvProductionReport.DataSource = reportData;
                }

                CustomizeGridHeaders();

                // İstenmeyen OEE ve detay sütunlarını gizleme bloğu
                try
                {
                    string[] hiddenColumns = {
                        "MachineAlarmDurationSeconds", "OperatorPauseDurationSeconds",
                        "TheoreticalCycleTimeSeconds", "GoodCount", "ScrapCount",
                        "TotalProductionCount", "DefectiveProductionCount", "TotalDownTimeSeconds"
                    };

                    foreach (string colName in hiddenColumns)
                    {
                        if (dgvProductionReport.Columns[colName] != null)
                            dgvProductionReport.Columns[colName].Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Column visibility error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Çizim kilidini çözüp tek frame'de ekrana yansıt
                this.ResumeLayout(true);

                this.Cursor = Cursors.Default;
                btnGenerateReport.Enabled = true;
            }
        }

        // =========================================================================
        // MODERNİZASYON: RAPOR TABLOSU GÖRSEL MAT KONTRAST DÜZENLEMESİ
        // DataGrid başlık, tarih formatı ve çizgileri merkezi şemaya tam eşitlendi.
        // =========================================================================
        private void CustomizeGridHeaders()
        {
            var grid = dgvProductionReport;
            if (grid.DataSource == null) return;

            // Genel Bilgiler Sütun Eşleşmeleri
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

            // Zamanlamalar Sütun Eşleşmeleri
            SetColumnHeader(grid, "StartTime", "Start Time");
            SetColumnHeader(grid, "BaslangicZamani", "Start Time");
            SetColumnHeader(grid, "Baslangic", "Start Time");
            SetColumnHeader(grid, "EndTime", "End Time");
            SetColumnHeader(grid, "BitisZamani", "End Time");
            SetColumnHeader(grid, "Bitis", "End Time");
            SetColumnHeader(grid, "Duration", "Duration");
            SetColumnHeader(grid, "Sure", "Duration");
            SetColumnHeader(grid, "TotalDuration", "Total Duration");

            // Tüketimler & Sayı Formatları (N3 / N0)
            if (grid.Columns.Contains("TotalWater"))
            {
                grid.Columns["TotalWater"].HeaderText = "Total Water (m³)";
                grid.Columns["TotalWater"].DefaultCellStyle.Format = "N3";
            }
            if (grid.Columns.Contains("TotalElectricity"))
            {
                grid.Columns["TotalElectricity"].HeaderText = "Total Electricity (kWh)";
                grid.Columns["TotalElectricity"].DefaultCellStyle.Format = "N3";
            }
            if (grid.Columns.Contains("TotalSteam"))
            {
                grid.Columns["TotalSteam"].HeaderText = "Total Steam (kg)";
                grid.Columns["TotalSteam"].DefaultCellStyle.Format = "N0";
            }

            SetColumnHeader(grid, "Cost", "Total Cost");
            SetColumnHeader(grid, "Maliyet", "Total Cost");

            // Tarih format standardizasyonu
            if (grid.Columns.Contains("StartTime")) grid.Columns["StartTime"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
            if (grid.Columns.Contains("EndTime")) grid.Columns["EndTime"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";

            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Tablo Renk Uyumluluğu (Koyu/Açık Tema Koruyucu)
            grid.BackgroundColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            grid.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(15, 23, 42);
            grid.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(38, 50, 68) : Color.FromArgb(248, 250, 252);

            grid.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            grid.ColumnHeadersHeight = 45;
            grid.RowTemplate.Height = 36;
        }

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
                    var detailForm = new ProductionDetail_Form(selectedReportItem, _recipeRepository, _processLogRepo, _alarmRepo);
                    detailForm.Show();
                }
            }
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (dgvProductionReport.Rows.Count == 0) return;
            ExcelExporter.ExportDataGridViewToExcel(dgvProductionReport);
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