// UI/Views/ManualUsageReport_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks; // Task kullanımı için gerekli
namespace TekstilScada.UI.Views
{
    public partial class ManualUsageReport_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProcessLogRepository _processLogRepository;
        private List<Machine> _selectedMachinesCache = new List<Machine>();
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
            //label3.Text = Resources.Machine;
            btnGenerateReport.Text = Resources.Reports;
            btnExportToExcel.Text = Resources.ExportToExcel;
        }
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

                // --- DEĞİŞİKLİK BURADA: Kurutma Makinesi tipindeki makineleri filtreliyoruz ---
                var groupedMachines = allMachines
                    .Where(m => m.MachineType != "Kurutma Makinesi") // Kurutma makinelerini hariç tut
                    .GroupBy(m => !string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType)
                    .OrderBy(g => g.Key); // Alfabetik sırala
                // -----------------------------------------------------------------------------

                foreach (var group in groupedMachines)
                {
                    // 1. Her grup için bir GroupBox oluştur
                    GroupBox grpBox = new GroupBox();
                    grpBox.Text = group.Key; // Grup Başlığı (Örn: "Boyama-Tip1")
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
                    chkList.DisplayMember = "MachineName"; // Machine nesnesindeki özellik adı

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
        private void ManualUsageReport_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Today;
            dtpEndTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            var machines = _machineRepository.GetAllMachines();
            LoadMachineGroups();
           
        }

        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            DateTime startTime = dtpStartTime.Value;
            DateTime endTime = dtpEndTime.Value;

            // Çoklu seçim fonksiyonunu çağır
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

                // Rapor verilerini tutacak liste
                var reportData = new List<ManualConsumptionSummary>();

                // Seçili her makine için veriyi çek (Paralel çalıştırılabilir veya döngü ile)
                // UI donmaması için Task.Run içinde yapıyoruz
                await Task.Run(() =>
                {
                    foreach (var machine in selectedMachines)
                    {
                        // Repository'deki mevcut metodunuzu her makine için çağırıyoruz
                        var summary = _processLogRepository.GetManualConsumptionSummary(machine.Id, machine.MachineName, startTime, endTime);

                        if (summary != null)
                        {
                            reportData.Add(summary);
                        }
                    }
                });
                foreach (var item in reportData)
                {
                    // Makine adından ID veya Tip bulmak zor olabilir, bu yüzden 'selectedMachines' listesinden eşleştiriyoruz.
                    var machineInfo = selectedMachines.FirstOrDefault(m => m.MachineName == item.Makine);

                    if (machineInfo != null &&
                       (machineInfo.MachineType == "Kurutma Makinesi"))
                    {
                        item.ToplamSuTuketimi_Litre = 0;
                    }
                }
                // Grid'i doldur
                dgvManualUsage.DataSource = null;
                dgvManualUsage.DataSource = reportData;

                CustomizeGridAppearance();

                // Toplam özeti bir Label'a veya mesaja yazdırmak isterseniz burada hesaplayabilirsiniz
                // UpdateTotalSummary(reportData); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata} {ex.Message}", $"{Resources.Error}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnGenerateReport.Enabled = true;
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
            if (dgvManualUsage.Columns["Makine"] != null)
                dgvManualUsage.Columns["Makine"].HeaderText = "Machine Name";
            if (dgvManualUsage.Columns["RaporAraligi"] != null)
                dgvManualUsage.Columns["RaporAraligi"].HeaderText = "Report Interval";
            if (dgvManualUsage.Columns["ToplamManuelSure"] != null)
                dgvManualUsage.Columns["ToplamManuelSure"].HeaderText = "Total Manual Time";
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