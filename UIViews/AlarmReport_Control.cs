// UI/Views/AlarmReport_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using Telemetry.Core;
using Telemetry.Core.Models;
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

        // Filtre Hafıza Değişkenleri
        private DateTime _currentStartTime;
        private DateTime _currentEndTime;
        private int? _currentMachineId;

        public AlarmReport_Control()
        {
            // Dil değişim olayına kayıt
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // UX OPTMİZASYONU: Kontrolün arkasında çiğ beyaz alan kalmaması için şeffaflığı açıyoruz
            this.BackColor = System.Drawing.Color.Transparent;
            this.DoubleBuffered = true; // Genel kırpışma koruması

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

            // Tablo kaydırma (Scroll) donanımsal çift tamponlama ivmesi
            EnableDoubleBuffer(dgvAlarmReport);
        }

        private async void btnGenerateReport_Click(object sender, EventArgs e)
        {
            _currentPage = 0; // Sayfayı sıfırla

            // Filtreleri o anki seçime göre hafızaya kilitle
            _currentStartTime = dtpStartTime.Value;
            _currentEndTime = dtpEndTime.Value;
            _currentMachineId = (int)cmbMachines.SelectedValue == -1 ? (int?)null : (int)cmbMachines.SelectedValue;

            await FetchPagedAlarmsAsync();
        }

        private async void btnPrev_Click(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                await FetchPagedAlarmsAsync();
            }
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                await FetchPagedAlarmsAsync();
            }
        }

        private async Task FetchPagedAlarmsAsync()
        {
            if (this.IsDisposed) return;

            // Arayüz kilitleme ve yükleniyor durum hazırlığı
            btnGenerateReport.Enabled = false;
            if (btnPrev != null) btnPrev.Enabled = false;
            if (btnNext != null) btnNext.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            // SPEED OPTİMİZASYON: Tablo verileri yenilenirken tüm kontrol yerleşimini dondurur
            this.SuspendLayout();

            try
            {
                var filters = new ReportFilters
                {
                    StartTime = _currentStartTime,
                    EndTime = _currentEndTime,
                    MachineId = _currentMachineId
                };

                // Arka planda thread kilitlemeden (Non-blocking) veritabanı sorgusunu çalıştırır
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
                // Çizim kilidini aç ve toplu olarak tek frame'de ekrana bas
                this.ResumeLayout(true);

                this.Cursor = Cursors.Default;
                btnGenerateReport.Enabled = true;

                // Butonların durumlarını sayfa sınırlarına göre otomatik ayarla
                if (btnPrev != null) btnPrev.Enabled = _currentPage > 0;
                if (btnNext != null) btnNext.Enabled = _currentPage < _totalPages - 1;
            }
        }

        // =========================================================================
        // MEMORY LEAK (BELLEK SIZINTISI) KORUMASI: STATİK EVENT BAĞLANTI TEMİZLİĞİ
        // Kontrol kapatıldığında RAM'de asılı kalmasını kesin olarak engeller.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnHandleDestroyed(e);
        }

        // SPEED OPTİMİZASYON: DataGrid akıcılığını sağlayan yansıtma (Reflection) metodu
        private void EnableDoubleBuffer(Control control)
        {
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, control, new object[] { true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DoubleBuffering error: {ex.Message}");
            }
        }
    }
}