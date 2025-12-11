// UIViews/GenelUretimRaporu_Control.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        private ListBox _activeSourceListBox;

        public GenelUretimRaporu_Control()
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            InitializeComponent();
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
            LoadMachineLists();

            // Tablo görünüm ayarları
            dgvReport.Dock = DockStyle.Fill;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadMachineLists()
        {
            var allMachines = _machineRepository.GetAllMachines();
            var groupedMachines = allMachines
                .Where(m => !string.IsNullOrEmpty(m.MachineSubType))
                .GroupBy(m => m.MachineSubType);

            flpMachineGroups.Controls.Clear();
            foreach (var group in groupedMachines)
            {
                var groupBox = new GroupBox
                {
                    Text = group.Key,
                    Width = 250,
                    Height = 220
                };
                var listBox = new ListBox
                {
                    DataSource = group.ToList(),
                    DisplayMember = "MachineName",
                    Dock = DockStyle.Fill,
                    SelectionMode = SelectionMode.MultiExtended
                };
                listBox.Enter += (s, a) => { _activeSourceListBox = s as ListBox; };
                listBox.DoubleClick += (s, a) => { AddSelectedItems(); };
                groupBox.Controls.Add(listBox);
                flpMachineGroups.Controls.Add(groupBox);
            }
        }

        private void btnRaporOlustur_Click(object sender, EventArgs e)
        {
            var selectedMachines = listBoxSeciliMakineler.Items.Cast<Machine>().Select(m => m.MachineName).ToList();
            if (!selectedMachines.Any())
            {
                MessageBox.Show($"{Resources.lütfenbirmakinesec}", $"{Resources.Warning}");
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                // Veriyi çek
                _reportData = _productionRepository.GetGeneralProductionReport(dtpStartTime.Value, dtpEndTime.Value, selectedMachines);

                // 1. ADIM: Birimleri Dönüştür (Litre -> m3, Watt -> kW)
                ConvertUnits(_reportData);

                // 2. ADIM: Tabloya Bağla
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
            }
        }

        // --- YENİ METOT: BİRİM DÖNÜŞÜMÜ ---
        private void ConvertUnits(DataTable table)
        {
            if (table == null || table.Rows.Count == 0) return;

            // Yardımcı yerel fonksiyon: Sütunu Decimal'e çevirir ve böler
            void ConvertColumnToDecimalAndDivide(string columnName, decimal divisor)
            {
                if (!table.Columns.Contains(columnName)) return;

                // 1. Yeni geçici bir decimal sütun ekle
                string tempColName = columnName + "_Temp";
                if (!table.Columns.Contains(tempColName))
                {
                    table.Columns.Add(tempColName, typeof(decimal));
                }

                // 2. Verileri dönüştürerek yeni sütuna aktar
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

                // 3. Eski sütunu sil ve yeni sütunun adını eski sütunun adı yap
                // (Böylece DataGridView ayarlarını bozmamış oluruz)
                int ordinalIndex = table.Columns[columnName].Ordinal; // Eski sütunun sırasını sakla

                table.Columns.Remove(columnName); // Eski int sütunu sil
                table.Columns[tempColName].ColumnName = columnName; // Yeni sütuna eski ismini ver
                table.Columns[columnName].SetOrdinal(ordinalIndex); // Sırasını eski yerine koy
            }

            // Dönüşümleri Uygula
            ConvertColumnToDecimalAndDivide("TotalWater", 1000m);       // Litre -> m3
            ConvertColumnToDecimalAndDivide("TotalElectricity", 1000m); // Watt -> kWh
            ConvertColumnToDecimalAndDivide("TotalSteam", 1000m);       // Litre -> m3
        }

        // --- YENİ METOT: BAŞLIK VE FORMAT AYARLARI ---
        private void ConfigureGridAppearance()
        {
            if (dgvReport.DataSource == null) return;

            // Sütun Başlıklarını İngilizce Yap ve Birim Ekle
            if (dgvReport.Columns.Contains("MachineName"))
                dgvReport.Columns["MachineName"].HeaderText = "Machine Name";

            if (dgvReport.Columns.Contains("TotalWater"))
            {
                dgvReport.Columns["TotalWater"].HeaderText = "Total Water (m³)";
                dgvReport.Columns["TotalWater"].DefaultCellStyle.Format = "N2"; // 2 hane ondalık
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

            if (dgvReport.Columns.Contains("RecipeCount"))
                dgvReport.Columns["RecipeCount"].HeaderText = "Recipe Count";

            if (dgvReport.Columns.Contains("TotalDuration"))
                dgvReport.Columns["TotalDuration"].HeaderText = "Total Duration (Min)";
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("Empty" ?? "No data to export.", Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                // DataTable üzerinde dönüşüm yaptığımız için Excel'e doğru birimler gidecektir.
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

        private void AddSelectedItems()
        {
            if (_activeSourceListBox == null || _activeSourceListBox.SelectedItems.Count == 0) return;
            foreach (var item in _activeSourceListBox.SelectedItems)
            {
                if (!listBoxSeciliMakineler.Items.Contains(item))
                {
                    listBoxSeciliMakineler.Items.Add(item);
                }
            }
            listBoxSeciliMakineler.DisplayMember = "DisplayInfo";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddSelectedItems();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            for (int i = listBoxSeciliMakineler.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listBoxSeciliMakineler.Items.RemoveAt(listBoxSeciliMakineler.SelectedIndices[i]);
            }
        }

        private void btnAddAll_Click(object sender, EventArgs e)
        {
            listBoxSeciliMakineler.Items.Clear();
            foreach (var groupBox in flpMachineGroups.Controls.OfType<GroupBox>())
            {
                var listBox = groupBox.Controls.OfType<ListBox>().FirstOrDefault();
                if (listBox != null)
                {
                    foreach (var item in listBox.Items)
                    {
                        if (!listBoxSeciliMakineler.Items.Contains(item))
                        {
                            listBoxSeciliMakineler.Items.Add(item);
                        }
                    }
                }
            }
            listBoxSeciliMakineler.DisplayMember = "DisplayInfo";
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            listBoxSeciliMakineler.Items.Clear();
        }
    }
}