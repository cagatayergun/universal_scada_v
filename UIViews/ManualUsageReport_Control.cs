// UI/Views/ManualUsageReport_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class ManualUsageReport_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProcessLogRepository _processLogRepository;
        private List<Machine> _selectedMachinesCache = new List<Machine>();

        public ManualUsageReport_Control()
        {
            // Dil değişim olayına kayıt
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // SPEED OPTİMİZASYON: Kontrol sekme geçişlerinde arayüzün titremesini engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimi üst ebeveyne devredildi

            // Hücre formatlama olayını (Birim dönüşümü için) bağlıyoruz
            dgvManualUsage.CellFormatting += dgvManualUsage_CellFormatting;
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
            btnGenerateReport.Text = Resources.Reports;
            btnExportToExcel.Text = Resources.ExportToExcel;
        }

        private List<Machine> GetSelectedMachines()
        {
            var selectedList = new List<Machine>();

            foreach (Control ctrl in flpMachineGroups.Controls)
            {
                if (ctrl is GroupBox grp)
                {
                    var chkList = grp.Controls.OfType<CheckedListBox>().FirstOrDefault();
                    if (chkList != null)
                    {
                        foreach (var item in chkList.CheckedItems)
                        {
                            if (item is Machine machine)
                            {
                                selectedList.Add(machine);
                            }
                        }
                    }
                }
            }
            return selectedList;
        }

        // =========================================================================
        // MODERNİZASYON & SPEED OPTİMİZASYON: DİNAMİK KONTROL ÜRETİM ADAPTÖRÜ
        // Yerleşim motoru donduruldu ve Dark Mode renk uyum blokları mühürlendi.
        // =========================================================================
        private void LoadMachineGroups()
        {
            if (_machineRepository == null || this.IsDisposed) return;

            // Performans için arayüz yerleşim hesaplamalarını askıya alıyoruz
            this.SuspendLayout();
            flpMachineGroups.SuspendLayout();

            try
            {
                flpMachineGroups.Controls.Clear();

                var allMachines = _machineRepository.GetAllEnabledMachines();
                if (allMachines == null || !allMachines.Any()) return;

                // Kurutma Makinesi tipindeki makineleri filtreliyoruz
                var groupedMachines = allMachines
                    .Where(m => m.MachineType != "Kurutma Makinesi")
                    .GroupBy(m => !string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType)
                    .OrderBy(g => g.Key);

                // Merkezi tema motorundan anlık Dark Mode kontrolü yapıyoruz
                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

                Color containerBg = isDark ? Color.FromArgb(44, 52, 64) : Color.FromArgb(241, 245, 249);
                Color textFg = isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(51, 65, 85);
                Color listBg = isDark ? Color.FromArgb(30, 41, 59) : Color.White;

                foreach (var group in groupedMachines)
                {
                    GroupBox grpBox = new GroupBox
                    {
                        Text = group.Key,
                        Width = 200,
                        Height = 150,
                        Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
                        Margin = new Padding(6),
                        BackColor = containerBg,
                        ForeColor = textFg
                    };

                    CheckedListBox chkList = new CheckedListBox
                    {
                        Dock = DockStyle.Fill,
                        CheckOnClick = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = listBg,
                        ForeColor = textFg,
                        Font = new Font("Segoe UI", 9, FontStyle.Regular)
                    };

                    foreach (var machine in group)
                    {
                        chkList.Items.Add(machine, false);
                    }

                    chkList.DisplayMember = "MachineName";

                    grpBox.Controls.Add(chkList);
                    flpMachineGroups.Controls.Add(grpBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine listesi yüklenirken hata oluştu: {ex.Message}", "Hata");
            }
            finally
            {
                // Toplu çizimi ekrana yansıtıp kilitleri açıyoruz
                flpMachineGroups.ResumeLayout(true);
                this.ResumeLayout(true);
            }
        }

        private void ManualUsageReport_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Today;
            dtpEndTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            LoadMachineGroups();
            EnableDoubleBuffer(dgvManualUsage); // Tablo kaydırma hızlandırıcısı açıldı
        }

        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;

            DateTime startTime = dtpStartTime.Value;
            DateTime endTime = dtpEndTime.Value;

            var selectedMachines = GetSelectedMachines();

            if (selectedMachines.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir makine seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnGenerateReport.Enabled = false;

                // SPEED OPTİMİZASYON: Tablo veri kaynağı yenilenirken arayüz yerleşim motorunu dondurur
                this.SuspendLayout();

                var reportData = new List<ManualConsumptionSummary>();

                // Ağır log tarama döngüsünü arka planda (Thread-pool) asenkron çalıştırıyoruz
                await Task.Run(() =>
                {
                    foreach (var machine in selectedMachines)
                    {
                        var summary = _processLogRepository.GetManualConsumptionSummary(machine.Id, machine.MachineName, startTime, endTime);
                        if (summary != null)
                        {
                            reportData.Add(summary);
                        }
                    }
                });

                foreach (var item in reportData)
                {
                    var machineInfo = selectedMachines.FirstOrDefault(m => m.MachineName == item.Makine);
                    if (machineInfo != null && machineInfo.MachineType == "Kurutma Makinesi")
                    {
                        item.ToplamSuTuketimi_Litre = 0;
                    }
                }

                dgvManualUsage.DataSource = null;
                if (reportData.Any())
                {
                    dgvManualUsage.DataSource = reportData;
                }

                CustomizeGridAppearance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata} {ex.Message}", $"{Resources.Error}");
            }
            finally
            {
                // Çizim kilidini çözüp toplu render ediyoruz
                this.ResumeLayout(true);

                this.Cursor = Cursors.Default;
                btnGenerateReport.Enabled = true;
            }
        }

        // =========================================================================
        // MODERNİZASYON: ANALİZ TABLOSU GÖRSEL MAT KONTRAST DÜZENLEMESİ
        // DataGrid renk ve çizgi matrisleri merkezi temayla tam eşitlendi.
        // =========================================================================
        private void CustomizeGridAppearance()
        {
            if (dgvManualUsage.DataSource == null) return;

            if (dgvManualUsage.Columns["OrtalamaSicaklik"] != null) dgvManualUsage.Columns["OrtalamaSicaklik"].Visible = false;
            if (dgvManualUsage.Columns["OrtalamaDevir"] != null) dgvManualUsage.Columns["OrtalamaDevir"].Visible = false;

            if (dgvManualUsage.Columns["Makine"] != null) dgvManualUsage.Columns["Makine"].HeaderText = "Machine Name";
            if (dgvManualUsage.Columns["RaporAraligi"] != null) dgvManualUsage.Columns["RaporAraligi"].HeaderText = "Report Interval";
            if (dgvManualUsage.Columns["ToplamManuelSure"] != null) dgvManualUsage.Columns["ToplamManuelSure"].HeaderText = "Total Manual Time";

            if (dgvManualUsage.Columns["ToplamSuTuketimi_Litre"] != null) dgvManualUsage.Columns["ToplamSuTuketimi_Litre"].HeaderText = "Total Water (m³)";
            if (dgvManualUsage.Columns["ToplamElektrikTuketimi_kW"] != null) dgvManualUsage.Columns["ToplamElektrikTuketimi_kW"].HeaderText = "Total Electricity (kWh)";
            if (dgvManualUsage.Columns["ToplamBuharTuketimi_kg"] != null) dgvManualUsage.Columns["ToplamBuharTuketimi_kg"].HeaderText = "Total Steam (kg)";
            if (dgvManualUsage.Columns["DurationMinutes"] != null) dgvManualUsage.Columns["DurationMinutes"].HeaderText = "Duration (Minutes)";

            dgvManualUsage.BorderStyle = BorderStyle.None;
            dgvManualUsage.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvManualUsage.EnableHeadersVisualStyles = false;
            
            dgvManualUsage.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Tablo Renk Uyumluluğu (Koyu/Açık Tema Koruyucu)
            dgvManualUsage.BackgroundColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            dgvManualUsage.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254);
            dgvManualUsage.DefaultCellStyle.SelectionForeColor = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(15, 23, 42);
            dgvManualUsage.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(38, 50, 68) : Color.FromArgb(248, 250, 252);

            dgvManualUsage.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
            dgvManualUsage.ColumnHeadersDefaultCellStyle.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            dgvManualUsage.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgvManualUsage.ColumnHeadersHeight = 45;
            dgvManualUsage.RowTemplate.Height = 36;

            dgvManualUsage.DefaultCellStyle.Format = "N3";
        }

        private void dgvManualUsage_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null || e.Value == DBNull.Value) return;

            string colName = dgvManualUsage.Columns[e.ColumnIndex].Name;

            try
            {
                double val = Convert.ToDouble(e.Value);

                if (colName == "ToplamSuTuketimi_Litre" || colName == "ToplamElektrikTuketimi_kW")
                {
                    double result = val / 1000.0;
                    e.Value = result.ToString("N3");
                    e.FormattingApplied = true;
                }
                else if (colName == "ToplamBuharTuketimi_kg")
                {
                    e.Value = val.ToString("N0");
                    e.FormattingApplied = true;
                }
            }
            catch
            {
                // Sayısal format hatası koruması
            }
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (dgvManualUsage.Rows.Count == 0) return;

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

        // =========================================================================
        // KUSURSUZ BELLEK TEMİZLİĞİ: STATİK EVENT ABONELİK BAĞLANTISI KOPARILDI
        // Kontrolün RAM'de asılı kalarak hafıza sızıntısı yapmasını kesin önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnHandleDestroyed(e);
        }

        // SPEED OPTİMİZASYON: DataGrid kaydırma (scroll) ivmesini artıran yansıtma metodu
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