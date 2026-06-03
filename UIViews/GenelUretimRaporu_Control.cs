// UIViews/GenelUretimRaporu_Control.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class GenelUretimRaporu_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProductionRepository _productionRepository;
        private DataTable _reportData;

        public GenelUretimRaporu_Control()
        {
            // Statik dil değişim olayına kayıt
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // SPEED OPTİMİZASYON: Sekme geçişlerinde ve filtre yüklemelerinde arayüzün titremesini engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimini üst ebeveyne devret
        }

        public void InitializeControl(MachineRepository machineRepo, ProductionRepository productionRepo)
        {
            _machineRepository = machineRepo;
            _productionRepository = productionRepo;
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            groupBox1.Text = Resources.tüketimtipi;
            btnRaporOlustur.Text = Resources.Reports;

            if (btnExportToExcel != null)
                btnExportToExcel.Text = Resources.ExportToExcel ?? "Export to Excel";

            radioElektrik.Text = Resources.elk;
            radioBuhar.Text = Resources.buhar;
            radioSu.Text = Resources.su;
        }

        private void GenelUretimRaporu_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Today;
            dtpEndTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            // Makineleri dinamik olarak yükle
            LoadMachineGroups();

            // Tablo görünüm ve hız ayarları
            dgvReport.Dock = DockStyle.Fill;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Tablo kaydırma ivmesini artıran donanımsal çift tamponlama tetikleyicisi
            EnableDoubleBuffer(dgvReport);
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

                var groupedMachines = allMachines
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

        private async void btnRaporOlustur_Click(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;

            var selectedMachineObjects = GetSelectedMachines();
            var selectedMachineNames = selectedMachineObjects.Select(m => m.MachineName).ToList();

            if (!selectedMachineNames.Any())
            {
                MessageBox.Show($"{Resources.lütfenbirmakinesec}", $"{Resources.Warning}");
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnRaporOlustur.Enabled = false;

                // SPEED OPTİMİZASYON: Tablo veri kaynağı yenilenirken arayüz yerleşim motorunu dondurur
                this.SuspendLayout();

                // Ağır veritabanı log taramasını arka planda (Thread-pool) asenkron çalıştırıyoruz
                await Task.Run(() =>
                {
                    _reportData = _productionRepository.GetGeneralProductionReport(dtpStartTime.Value, dtpEndTime.Value, selectedMachineNames);

                    // --- KURUTMA MAKİNESİ İÇİN SU TÜKETİMİNİ SIFIRLA ---
                    if (_reportData != null && _reportData.Columns.Contains("MachineName") && _reportData.Columns.Contains("TotalWater"))
                    {
                        var dryingMachineNames = selectedMachineObjects
                            .Where(m => (m.MachineType != null && m.MachineType.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                        (m.MachineSubType != null && m.MachineSubType.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                        m.MachineName.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0)
                            .Select(m => m.MachineName)
                            .ToHashSet();

                        foreach (DataRow row in _reportData.Rows)
                        {
                            string machineName = row["MachineName"].ToString();
                            if (dryingMachineNames.Contains(machineName) || machineName.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                row["TotalWater"] = 0m;
                            }
                        }
                    }

                    // Birimleri Dönüştür (Litre -> m3, Watt -> kW)
                    ConvertUnits(_reportData);
                });

                dgvReport.DataSource = null;
                if (_reportData != null)
                {
                    dgvReport.DataSource = _reportData;
                }

                ConfigureGridAppearance();
                FilterGridByConsumptionType();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata}: {ex.Message}", $"{Resources.Error}");
            }
            finally
            {
                // Çizim kilidini çözüp toplu render ediyoruz
                this.ResumeLayout(true);

                this.Cursor = Cursors.Default;
                btnRaporOlustur.Enabled = true;
            }
        }

        private void ConvertUnits(DataTable table)
        {
            if (table == null || table.Rows.Count == 0) return;

            void ConvertColumnToDecimalAndDivide(string columnName, decimal divisor)
            {
                if (!table.Columns.Contains(columnName)) return;

                string tempColName = columnName + "_Temp";
                if (!table.Columns.Contains(tempColName))
                {
                    table.Columns.Add(tempColName, typeof(decimal));
                }

                foreach (DataRow row in table.Rows)
                {
                    if (row[columnName] != DBNull.Value)
                    {
                        try
                        {
                            decimal val = Convert.ToDecimal(row[columnName]);
                            row[tempColName] = val / divisor;
                        }
                        catch
                        {
                            row[tempColName] = 0m;
                        }
                    }
                    else
                    {
                        row[tempColName] = 0m;
                    }
                }

                int ordinalIndex = table.Columns[columnName].Ordinal;
                table.Columns.Remove(columnName);
                table.Columns[tempColName].ColumnName = columnName;
                table.Columns[columnName].SetOrdinal(ordinalIndex);
            }

            ConvertColumnToDecimalAndDivide("TotalWater", 1000m);       // Litre -> m3
            ConvertColumnToDecimalAndDivide("TotalElectricity", 1000m); // Watt -> kWh
            ConvertColumnToDecimalAndDivide("TotalSteam", 1);           // Skala korundu
        }

        // =========================================================================
        // MODERNİZASYON: RAPOR TABLOSU GÖRSEL MAT KONTRAST DÜZENLEMESİ
        // DataGrid renk ve çizgi matrisleri merkezi temayla tam eşitlendi.
        // =========================================================================
        private void ConfigureGridAppearance()
        {
            if (dgvReport.DataSource == null) return;

            if (dgvReport.Columns.Contains("MachineName")) dgvReport.Columns["MachineName"].HeaderText = "Machine Name";
            if (dgvReport.Columns.Contains("BatchId")) dgvReport.Columns["BatchId"].HeaderText = "Batch No";

            if (dgvReport.Columns.Contains("EndTime"))
            {
                dgvReport.Columns["EndTime"].HeaderText = "End Time";
                dgvReport.Columns["EndTime"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }

            if (dgvReport.Columns.Contains("TotalWater"))
            {
                dgvReport.Columns["TotalWater"].HeaderText = "Total Water (m³)";
                dgvReport.Columns["TotalWater"].DefaultCellStyle.Format = "N3";
            }

            if (dgvReport.Columns.Contains("TotalElectricity"))
            {
                dgvReport.Columns["TotalElectricity"].HeaderText = "Total Electricity (kWh)";
                dgvReport.Columns["TotalElectricity"].DefaultCellStyle.Format = "N3";
            }

            if (dgvReport.Columns.Contains("TotalSteam"))
            {
                dgvReport.Columns["TotalSteam"].HeaderText = "Total Steam (kg)";
                dgvReport.Columns["TotalSteam"].DefaultCellStyle.Format = "N0";
            }

            dgvReport.BorderStyle = BorderStyle.None;
            dgvReport.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvReport.EnableHeadersVisualStyles = false;
            dgvReport.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Tablo Renk Uyumluluğu (Koyu/Açık Tema Koruyucu)
            dgvReport.BackgroundColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            dgvReport.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254);
            dgvReport.DefaultCellStyle.SelectionForeColor = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(15, 23, 42);
            dgvReport.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(38, 50, 68) : Color.FromArgb(248, 250, 252);

            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgvReport.ColumnHeadersHeight = 45;
            dgvReport.RowTemplate.Height = 36;
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                ExcelExporter.ExportDataGridViewToExcel(dgvReport);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata}: {ex.Message}", Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radioConsumption_CheckedChanged(object sender, EventArgs e)
        {
            FilterGridByConsumptionType();
        }

        private void FilterGridByConsumptionType()
        {
            if (dgvReport.DataSource == null || dgvReport.Columns.Count == 0) return;

            if (dgvReport.Columns.Contains("TotalWater")) dgvReport.Columns["TotalWater"].Visible = false;
            if (dgvReport.Columns.Contains("TotalElectricity")) dgvReport.Columns["TotalElectricity"].Visible = false;
            if (dgvReport.Columns.Contains("TotalSteam")) dgvReport.Columns["TotalSteam"].Visible = false;

            if (radioSu.Checked && dgvReport.Columns.Contains("TotalWater"))
            {
                dgvReport.Columns["TotalWater"].Visible = true;
            }
            if (radioElektrik.Checked && dgvReport.Columns.Contains("TotalElectricity"))
            {
                dgvReport.Columns["TotalElectricity"].Visible = true;
            }
            if (radioBuhar.Checked && dgvReport.Columns.Contains("TotalSteam"))
            {
                dgvReport.Columns["TotalSteam"].Visible = true;
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