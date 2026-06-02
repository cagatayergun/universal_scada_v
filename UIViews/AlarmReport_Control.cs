using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using Telemetry.Core;
using Telemetry.Core.Models; // ReportFilters modeli için gerekebilir
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;

namespace Telemetry.UI.Views
{
    public partial class AlarmReport_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private AlarmRepository _alarmRepository;

        // --- SAYFALAMA DURUM DEĞİŞKENLERİ ---
        private int _currentPage = 0;          // Mevcut sayfa (0'dan başlar)
        private const int _pageSize = 200;     // Her sayfada gösterilecek kayıt sayısı
        private int _totalCount = 0;           // Toplam kayıt sayısı (DB'den dönecek)
        private int _totalPages = 0;           // Toplam sayfa sayısı

        // Kullanıcı sayfalar arasında gezerken filtreleri kilitlemek için hafıza değişkenleri
        private DateTime _currentStartTime;
        private DateTime _currentEndTime;
        private int? _currentMachineId;

        public AlarmReport_Control()
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            InitializeComponent();
            ApplyLocalization();
        }

        public void InitializeControl(MachineRepository machineRepo, AlarmRepository alarmRepo)
        {
            _machineRepository = machineRepo;
            _alarmRepository = alarmRepo;
        }

        public void ApplyLocalization()
        {
            label1.Text = Resources.DateRange;
            label3.Text = Resources.Machine;
            btnGenerateReport.Text = Resources.GenerateReport;
            btnExportToExcel.Text = Resources.ExportToExcel;

            // Eğer localization kaynaklarında varsa ileri/geri butonlarını da bağlayabilirsiniz
            if (btnPrev != null) btnPrev.Text = "< Geri";
            if (btnNext != null) btnNext.Text = "İleri >";
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            ExcelExporter.ExportDataGridViewToExcel(dgvAlarmReport);
        }

        private void AlarmReport_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Today;
            dtpEndTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            var machines = _machineRepository.GetAllMachines();
            machines.Insert(0, new Machine { Id = -1, MachineName = Resources.AllMachines });
            cmbMachines.DataSource = machines;
            cmbMachines.DisplayMember = "MachineName";
            cmbMachines.ValueMember = "Id";

            // Başlangıçta sayfalama butonlarını pasif yap
            if (btnPrev != null) btnPrev.Enabled = false;
            if (btnNext != null) btnNext.Enabled = false;
            if (lblPageInfo != null) lblPageInfo.Text = "Report Waiting...";

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvAlarmReport, new object[] { true });
        }

        /// <summary>
        /// Rapor Oluştur butonuna basıldığında filtreleri kilitler ve 0. sayfayı tetikler.
        /// </summary>
        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            _currentPage = 0; // Sayfayı sıfırla

            // Filtreleri o anki seçime göre hafızaya al
            _currentStartTime = dtpStartTime.Value;
            _currentEndTime = dtpEndTime.Value;
            _currentMachineId = (int)cmbMachines.SelectedValue == -1 ? (int?)null : (int)cmbMachines.SelectedValue;

            await FetchPagedAlarmsAsync();
        }

        /// <summary>
        /// Geri Butonuna Basıldığında
        /// </summary>
        private async void btnPrev_Click(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                await FetchPagedAlarmsAsync();
            }
        }

        /// <summary>
        /// İleri Butonuna Basıldığında
        /// </summary>
        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                await FetchPagedAlarmsAsync();
            }
        }

        /// <summary>
        /// Veritabanından sadece ilgili 100 kaydı arka planda çeken asenkron motor metot.
        /// </summary>
        private async Task FetchPagedAlarmsAsync()
        {
            btnGenerateReport.Enabled = false;
            if (btnPrev != null) btnPrev.Enabled = false;
            if (btnNext != null) btnNext.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                // Parametreleri paketle
                var filters = new ReportFilters
                {
                    StartTime = _currentStartTime,
                    EndTime = _currentEndTime,
                    MachineId = _currentMachineId
                };

                // Ağır veritabanı taramasını yapmaz, LIMIT ve OFFSET ile sadece ilgili 100 satırı getirir (< 0.01 sn)
                var pagedResult = await Task.Run(() => _alarmRepository.GetAlarmReportPaged(filters, _currentPage, _pageSize));

                _totalCount = pagedResult.TotalCount;
                _totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);

                dgvAlarmReport.DataSource = null;

                if (pagedResult.Items != null && pagedResult.Items.Count > 0)
                {
                    dgvAlarmReport.DataSource = pagedResult.Items;
                }
                else
                {
                    MessageBox.Show("No alarm records matching the selected criteria were found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Alt bilgi etiketini güncelle (Örn: Sayfa 1 / 45 (Toplam: 4450))
                if (lblPageInfo != null)
                {
                    lblPageInfo.Text = $"Page: {_currentPage + 1} / {Math.Max(1, _totalPages)}  (All Records: {_totalCount})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.raporolusturukenhata} {ex.Message}", $"{Resources.Error}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnGenerateReport.Enabled = true;

                // Butonların durumlarını sayfa sınırlarına göre otomatik ayarla
                if (btnPrev != null) btnPrev.Enabled = _currentPage > 0;
                if (btnNext != null) btnNext.Enabled = _currentPage < _totalPages - 1;
            }
        }
    }
}