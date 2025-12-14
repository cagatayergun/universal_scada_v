// UIViews/GenelUretimRaporu_Control.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;

namespace TekstilScada.UI.Views
{
    public partial class GenelUretimRaporu_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProductionRepository _productionRepository;
        private DataTable _reportData;

        public GenelUretimRaporu_Control()
        {
            InitializeComponent();
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
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

            // Tablo görünüm ayarları
            dgvReport.Dock = DockStyle.Fill;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // --- YENİ: DİNAMİK GRUPLAMA VE LİSTELEME METODU ---
        private void LoadMachineGroups()
        {
            try
            {
                // Paneli temizle
                flpMachineGroups.Controls.Clear();

                // Tüm aktif makineleri getir
                var allMachines = _machineRepository.GetAllEnabledMachines();

                if (allMachines == null || !allMachines.Any()) return;

                // Makineleri Alt Tipe (SubType) göre grupla. 
                // Eğer SubType boşsa, Ana Tipi (Type) kullan.
                var groupedMachines = allMachines
                    .GroupBy(m => !string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType)
                    .OrderBy(g => g.Key); // Alfabetik sırala

                foreach (var group in groupedMachines)
                {
                    // 1. Her grup için bir GroupBox oluştur
                    GroupBox grpBox = new GroupBox();
                    grpBox.Text = group.Key; // Grup Başlığı (Örn: "Kurutma-Tip1")
                    grpBox.Width = 200;      // Genişlik ayarı
                    grpBox.Height = 150;     // Yükseklik ayarı
                    grpBox.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    grpBox.Margin = new Padding(5); // Kutular arası boşluk

                    // 2. İçine CheckedListBox ekle
                    CheckedListBox chkList = new CheckedListBox();
                    chkList.Dock = DockStyle.Fill; // Kutuyu doldur
                    chkList.CheckOnClick = true;   // Tek tıkla seç
                    chkList.BorderStyle = BorderStyle.None;
                    chkList.BackColor = SystemColors.Control; // Arka plan rengi
                    chkList.Font = new Font("Segoe UI", 9, FontStyle.Regular);

                    // 3. Makineleri listeye ekle
                    foreach (var machine in group)
                    {
                        chkList.Items.Add(machine, false); // Varsayılan olarak seçili değil
                    }

                    // DisplayMember ayarı (Listede ne görünecek)
                    chkList.DisplayMember = "MachineName";

                    // 4. Kontrolleri birbirine ekle
                    grpBox.Controls.Add(chkList);
                    flpMachineGroups.Controls.Add(grpBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine listesi yüklenirken hata oluştu: {ex.Message}", "Hata");
            }
        }

        // --- YENİ: SEÇİLİ MAKİNELERİ TOPLAYAN METOT ---
        private List<Machine> GetSelectedMachines()
        {
            var selectedList = new List<Machine>();

            // FlowLayoutPanel içindeki her kontrolü (GroupBox) gez
            foreach (Control ctrl in flpMachineGroups.Controls)
            {
                if (ctrl is GroupBox grp)
                {
                    // GroupBox içindeki CheckedListBox'ı bul
                    var chkList = grp.Controls.OfType<CheckedListBox>().FirstOrDefault();
                    if (chkList != null)
                    {
                        // Seçili olanları listeye ekle
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
            // Yeni çoklu seçim fonksiyonunu kullan
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

                // Veriyi asenkron olarak çek (UI donmaması için)
                await Task.Run(() =>
                {
                    _reportData = _productionRepository.GetGeneralProductionReport(dtpStartTime.Value, dtpEndTime.Value, selectedMachineNames);

                    // --- KURUTMA MAKİNESİ İÇİN SU TÜKETİMİNİ SIFIRLA ---
                    // "MachineName" kolonunu kontrol edip "Kurutma" geçenlerin "TotalWater" değerini 0 yapıyoruz.
                    // Bu işlem birim dönüşümünden ÖNCE yapılmalıdır.
                    if (_reportData != null && _reportData.Columns.Contains("MachineName") && _reportData.Columns.Contains("TotalWater"))
                    {
                        // Seçilen makineler arasında "Kurutma" tipinde olanları bul
                        // İsimden kontrol etmek yerine Machine objesinden kontrol etmek daha güvenlidir
                        var dryingMachineNames = selectedMachineObjects
                            .Where(m => (m.MachineType != null && m.MachineType.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                        (m.MachineSubType != null && m.MachineSubType.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                        m.MachineName.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0) // İsimde kurutma geçiyorsa
                            .Select(m => m.MachineName)
                            .ToHashSet();

                        foreach (DataRow row in _reportData.Rows)
                        {
                            string machineName = row["MachineName"].ToString();
                            // Eğer makine ismi kurutma makineleri listesinde varsa veya isminde "Kurutma" geçiyorsa
                            if (dryingMachineNames.Contains(machineName) || machineName.IndexOf("Kurutma", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                row["TotalWater"] = 0m;
                            }
                        }
                    }
                    // -----------------------------------------------------

                    // 1. ADIM: Birimleri Dönüştür (Litre -> m3, Watt -> kW)
                    ConvertUnits(_reportData);
                });

                // 2. ADIM: Tabloya Bağla
                dgvReport.DataSource = null;
                dgvReport.DataSource = _reportData;

                // 3. ADIM: Başlıkları ve Görünümü Ayarla
                ConfigureGridAppearance();

                // 4. ADIM: Seçime göre filtrele
                FilterGridByConsumptionType();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata}: {ex.Message}", $"{Resources.Error}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnRaporOlustur.Enabled = true;
            }
        }

        // --- BİRİM DÖNÜŞÜMÜ ---
        private void ConvertUnits(DataTable table)
        {
            if (table == null || table.Rows.Count == 0) return;

            // Yardımcı yerel fonksiyon: Sütunu Decimal'e çevirir ve böler
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

            // Dönüşümleri Uygula
            ConvertColumnToDecimalAndDivide("TotalWater", 1000m);       // Litre -> m3
            ConvertColumnToDecimalAndDivide("TotalElectricity", 1000m); // Watt -> kWh
            ConvertColumnToDecimalAndDivide("TotalSteam", 1000m);       // Litre -> m3
        }

        // --- BAŞLIK VE FORMAT AYARLARI ---
        private void ConfigureGridAppearance()
        {
            if (dgvReport.DataSource == null) return;

            if (dgvReport.Columns.Contains("MachineName"))
                dgvReport.Columns["MachineName"].HeaderText = "Machine Name";

            if (dgvReport.Columns.Contains("BatchId"))
                dgvReport.Columns["BatchId"].HeaderText = "Batch No";

            if (dgvReport.Columns.Contains("EndTime"))
            {
                dgvReport.Columns["EndTime"].HeaderText = "End Time";
                dgvReport.Columns["EndTime"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }

            if (dgvReport.Columns.Contains("TotalWater"))
            {
                dgvReport.Columns["TotalWater"].HeaderText = "Total Water (m³)";
                dgvReport.Columns["TotalWater"].DefaultCellStyle.Format = "N2";
            }

            if (dgvReport.Columns.Contains("TotalElectricity"))
            {
                dgvReport.Columns["TotalElectricity"].HeaderText = "Total Electricity (kWh)";
                dgvReport.Columns["TotalElectricity"].DefaultCellStyle.Format = "N2";
            }

            if (dgvReport.Columns.Contains("TotalSteam"))
            {
                dgvReport.Columns["TotalSteam"].HeaderText = "Total Steam (m³)";
                dgvReport.Columns["TotalSteam"].DefaultCellStyle.Format = "N2";
            }
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
    }
}