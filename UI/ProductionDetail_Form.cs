// UI/ProductionDetail_Form.cs
using ScottPlot; // ScottPlot'u kullanmak için
using System;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Telemetry.Core;
using Telemetry.Core.Models;
using Telemetry.Models;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm bileşenleri için
using ChartLegend = System.Windows.Forms.DataVisualization.Charting.Legend;

namespace Telemetry.UI
{
    // Form yerine MaterialForm sınıfından türetiyoruz
    public partial class ProductionDetail_Form : MaterialForm
    {
        private readonly ProductionReportItem _reportItem;
        private readonly ProductionRepository _productionRepo;
        private readonly ProcessLogRepository _processLogRepo;
        private readonly AlarmRepository _alarmRepo;
        private readonly RecipeRepository _recipeRepository;

        public ProductionDetail_Form(ProductionReportItem reportItem, RecipeRepository recipeRepo, ProcessLogRepository processLogRepo, AlarmRepository alarmRepo)
        {
            InitializeComponent();
            _reportItem = reportItem;
            _productionRepo = new ProductionRepository();
            _processLogRepo = processLogRepo;
            _alarmRepo = alarmRepo;
            _recipeRepository = recipeRepo;

            // =========================================================================
            // MATERIALSKIN FORM KAYDI VE PERFORMANS AYARLARI
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this); // Formu merkezi temaya bağla

            this.DoubleBuffered = true; // Formun genel kırpışmalarını engelle
        }

        private void ProductionDetail_Form_Load(object sender, EventArgs e)
        {
            this.Text = $"Production Report Detail - {_reportItem.BatchId}";

            // Form yüklenirken düzen hesaplamalarını askıya al (Hız Optimizasyonu)
            this.SuspendLayout();

            // 1. Başlık bilgilerini doldur
            txtMachineName.Text = _reportItem.MachineName;
            txtRecipeName.Text = _reportItem.RecipeName;
            txtOperator.Text = _reportItem.OperatorName;
            txtStartTime.Text = _reportItem.StartTime.ToString("dd.MM.yyyy HH:mm:ss");
            txtStopTime.Text = _reportItem.EndTime.ToString("dd.MM.yyyy HH:mm:ss");

            var elektrikvalue = _reportItem.TotalElectricity;
            txtElectricity.Text = elektrikvalue.ToString();

            var suvale = _reportItem.TotalWater;
            txtWater.Text = suvale.ToString();

            var steamvalue = _reportItem.TotalSteam;
            txtSteam.Text = steamvalue.ToString();

            txtTotalDuration.Text = _reportItem.CycleTime;

            // Teorik Süreyi Yaz
            TimeSpan theoreticalSpan = TimeSpan.FromSeconds(_reportItem.TheoreticalCycleTimeSeconds);
            txtTheoreticalDuration.Text = theoreticalSpan.ToString(@"hh\:mm\:ss");

            // Gerçekleşen Süreyi Hesapla
            TimeSpan actualSpan;
            if (TimeSpan.TryParse(_reportItem.CycleTime, out actualSpan))
            {
                TimeSpan diff = actualSpan - theoreticalSpan;
                string sign = diff.TotalSeconds >= 0 ? "+" : "-";
                txtDurationDiff.Text = $"{sign}{diff.Duration():hh\\:mm\\:ss}";

                // DARK MODE UYUMLU UYARI RENKLERİ: Göz yormayan pastel tonlar seçildi
                if (diff.TotalSeconds > 60) // 1 dakikadan fazla gecikme
                {
                    txtDurationDiff.ForeColor = System.Drawing.Color.FromArgb(239, 83, 80); // Pastel Kırmızı
                    txtDurationDiff.BackColor = System.Drawing.Color.FromArgb(58, 30, 30);  // Koyu Kırmızı Arka Plan
                }
                else if (diff.TotalSeconds < -60) // 1 dakikadan fazla erken bitiş
                {
                    txtDurationDiff.ForeColor = System.Drawing.Color.FromArgb(102, 187, 106); // Pastel Yeşil
                    txtDurationDiff.BackColor = System.Drawing.Color.FromArgb(30, 50, 30);   // Koyu Yeşil Arka Plan
                }
            }
            else
            {
                txtDurationDiff.Text = "---";
            }

            txtCustomerNo.Text = _reportItem.MusteriNo;
            txtOrderNo.Text = _reportItem.SiparisNo;

            // 2. Tablo Verilerini Yükle
            dgvStepDetails.DataSource = _productionRepo.GetProductionStepDetails(_reportItem.BatchId, _reportItem.MachineId);
            dgvAlarms.DataSource = _alarmRepo.GetAlarmDetailsForBatch(_reportItem.BatchId, _reportItem.MachineId);

            dgvStepDetails.CellFormatting += dgvStepDetails_CellFormatting;

            // SPEED OPTİMİZASYON: Tablolar kaydırılırken veya yenilenirken oluşan gecikmeleri engeller
            EnableDoubleBuffer(dgvStepDetails);
            EnableDoubleBuffer(dgvAlarms);

            // 3. Grafikleri Yükle
            LoadTimelineChart(); // ScottPlot süreç grafiği
            LoadPieChart();      // Verimlilik pasta grafiği

            // Düzen hesaplamalarını devreye al ve toplu çiz (Hız Optimizasyonu)
            this.ResumeLayout(true);
        }

        private void LoadPieChart()
        {
            DateTime batchStart = _reportItem.StartTime;
            DateTime batchEnd = (_reportItem.EndTime == DateTime.MinValue) ? DateTime.Now : _reportItem.EndTime;
            double totalBatchSeconds = (batchEnd - batchStart).TotalSeconds;

            var allAlarms = _alarmRepo.GetAlarmDetailsForBatch(_reportItem.BatchId, _reportItem.MachineId);

            var machineAlarms = allAlarms.Where(a => a.AlarmNumber >= 0 && a.AlarmNumber <= 499).ToList();
            var operatorAlarms = allAlarms.Where(a => a.AlarmNumber >= 500 && a.AlarmNumber <= 600).ToList();

            double netMachineAlarmSec = CalculateUniqueDuration(machineAlarms, batchEnd);
            double netOperatorAlarmSec = CalculateUniqueDuration(operatorAlarms, batchEnd);
            double totalNetDowntimeSec = CalculateUniqueDuration(allAlarms, batchEnd);

            double activeWorkSec = totalBatchSeconds - totalNetDowntimeSec;
            if (activeWorkSec < 0) activeWorkSec = 0;

            // =========================================================================
            // MODERNİZASYON: PASTA GRAFİĞİNİ (PIE CHART) DARK MODE UYUMLU YAPMA
            // =========================================================================
            pieChartControl.Series.Clear();
            pieChartControl.Legends.Clear();

            // Grafik arka planını şeffaf ve kenarsız yap
            pieChartControl.BackColor = System.Drawing.Color.Transparent;
            pieChartControl.ChartAreas[0].BackColor = System.Drawing.Color.Transparent;
            pieChartControl.ChartAreas[0].BorderColor = System.Drawing.Color.Transparent;

            var series = new System.Windows.Forms.DataVisualization.Charting.Series("Efficiency")
            {
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie,
                Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
                IsValueShownAsLabel = true,
                Label = "#PERCENT{P1}",
                LabelForeColor = System.Drawing.Color.White // Pasta dilimleri üzerindeki yazı rengi beyaz
            };
            pieChartControl.Series.Add(series);

            // Yumuşak Material Design renk paletiyle dilimleri ekle
            AddPiePoint(series, activeWorkSec, "Aktif Çalışma", System.Drawing.Color.FromArgb(33, 150, 243));   // Material Blue
            AddPiePoint(series, netMachineAlarmSec, "Makine Alarmları", System.Drawing.Color.FromArgb(244, 67, 54)); // Material Red
            AddPiePoint(series, netOperatorAlarmSec, "Operatör Alarmları", System.Drawing.Color.FromArgb(255, 152, 0)); // Material Orange

            // Ğösterge paneli (Legend) Dark Mode uyarlaması
            var legend = new System.Windows.Forms.DataVisualization.Charting.Legend("Default")
            {
                Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom,
                Alignment = System.Drawing.StringAlignment.Center,
                BackColor = System.Drawing.Color.Transparent,
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 220) // Yazı rengi açık gri
            };
            pieChartControl.Legends.Add(legend);

            pieChartControl.Invalidate();
        }

        private double CalculateUniqueDuration(List<AlarmDetail> alarms, DateTime limitEndTime)
        {
            if (alarms == null || !alarms.Any()) return 0;

            var sortedIntervals = alarms
                .Select(a => new {
                    Start = a.StartTime,
                    End = a.EndTime ?? limitEndTime
                })
                .OrderBy(a => a.Start)
                .ToList();

            if (!sortedIntervals.Any()) return 0;

            double totalSeconds = 0;
            DateTime currentStart = sortedIntervals[0].Start;
            DateTime currentEnd = sortedIntervals[0].End;

            for (int i = 1; i < sortedIntervals.Count; i++)
            {
                if (sortedIntervals[i].Start < currentEnd)
                {
                    if (sortedIntervals[i].End > currentEnd)
                    {
                        currentEnd = sortedIntervals[i].End;
                    }
                }
                else
                {
                    totalSeconds += (currentEnd - currentStart).TotalSeconds;
                    currentStart = sortedIntervals[i].Start;
                    currentEnd = sortedIntervals[i].End;
                }
            }

            totalSeconds += (currentEnd - currentStart).TotalSeconds;
            return totalSeconds;
        }

        private void AddPiePoint(System.Windows.Forms.DataVisualization.Charting.Series series, double seconds, string label, System.Drawing.Color color)
        {
            if (seconds > 0)
            {
                var dp = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0, seconds);
                dp.Color = color;
                dp.LegendText = $"{label} ({TimeSpan.FromSeconds(seconds):hh\\:mm\\:ss})";
                series.Points.Add(dp);
            }
        }

        private void dgvStepDetails_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvStepDetails.Columns[e.ColumnIndex].Name == "WorkingTime")
            {
                if (e.Value == null || e.Value.ToString() == "Processing...")
                {
                    e.CellStyle.BackColor = dgvStepDetails.DefaultCellStyle.BackColor;
                    return;
                }

                try
                {
                    TimeSpan theoreticalTime;
                    TimeSpan.TryParse(dgvStepDetails.Rows[e.RowIndex].Cells["TheoreticalTime"].Value.ToString(), out theoreticalTime);

                    TimeSpan workingTime;
                    TimeSpan.TryParse(e.Value.ToString(), out workingTime);

                    int theoreticalMinutes = (int)theoreticalTime.TotalMinutes;
                    int workingMinutes = (int)workingTime.TotalMinutes;

                    // DARK MODE UYUMLU GRID HÜCRE RENKLERİ: Yazının net okunabilmesi için Soft tonlar uygulandı
                    if (workingMinutes > theoreticalMinutes)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.FromArgb(239, 83, 80); // Yumuşak Kırmızı
                        e.CellStyle.ForeColor = System.Drawing.Color.Black;
                    }
                    else if (workingMinutes < theoreticalMinutes)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.FromArgb(102, 187, 106); // Yumuşak Yeşil
                        e.CellStyle.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        e.CellStyle.BackColor = dgvStepDetails.DefaultCellStyle.BackColor;
                        e.CellStyle.ForeColor = dgvStepDetails.DefaultCellStyle.ForeColor;
                    }
                }
                catch (Exception)
                {
                    e.CellStyle.BackColor = dgvStepDetails.DefaultCellStyle.BackColor;
                }
            }
        }

        private void LoadTimelineChart()
        {
            var dataPoints = _processLogRepo.GetLogsForBatch(_reportItem.MachineId, _reportItem.BatchId);
            formsPlot1.Plot.Clear();

            // =========================================================================
            // ÇÖZÜM (CS0619 & CS1061): SCOTTPLOT 5 EN GÜNCEL DOSDOĞRU KARANLIK MOD API'Sİ
            // =========================================================================
            formsPlot1.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1a202c"); // Dış panel arka planı
            formsPlot1.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#111827");   // Grafik alanı arka planı
            formsPlot1.Plot.Axes.Color(ScottPlot.Color.FromHex("#d7d7d7"));              // Eksen çizgileri ve yazılar
            formsPlot1.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#2d3748");    // Izgara çizgileri

            if (dataPoints.Any())
            {
                double[] timeData = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                double divisor = 10.0;

                try
                {
                    var machineRepo = new MachineRepository();
                    var machine = machineRepo.GetAllMachines().FirstOrDefault(m => m.Id == _reportItem.MachineId);

                    if (machine != null && !string.IsNullOrEmpty(machine.MachineType) &&
                        machine.MachineType.Contains("Kurutma Makinesi", StringComparison.OrdinalIgnoreCase))
                    {
                        divisor = 100.0;
                    }
                }
                catch (Exception) { }

                double[] tempData = dataPoints.Select(p => (double)p.Temperature / divisor).ToArray();

                // Gerçekleşen Sıcaklık Eğrisi
                var tempPlot = formsPlot1.Plot.Add.Scatter(timeData, tempData);
                tempPlot.Color = ScottPlot.Colors.Red;
                tempPlot.LegendText = "Temperature";
                tempPlot.MarkerSize = 0;
                tempPlot.LineWidth = 2;

                // Teorik Ramp Verisi
                var productionRepo = new ProductionRepository();
                var batchRecipe = productionRepo.GetBatchRecipe(_reportItem.MachineId, _reportItem.BatchId);

                if (batchRecipe != null && batchRecipe.Steps.Any())
                {
                    var (theoTimestamps, theoTemperatures) = RampCalculator.GenerateTheoreticalRamp(batchRecipe, _reportItem.StartTime);

                    if (theoTimestamps.Any())
                    {
                        var theoPlot = formsPlot1.Plot.Add.Scatter(theoTimestamps, theoTemperatures);
                        theoPlot.Color = ScottPlot.Colors.Cyan; // Koyu zeminde Mavi yerine Açık Mavi (Cyan) çok daha belirgindir
                        theoPlot.LegendText = "Theoretical Temperature";
                        theoPlot.LineStyle.Pattern = ScottPlot.LinePattern.Dashed;
                        theoPlot.LineWidth = 2;
                    }
                }

                // Grafik Eksen Yazı Tipleri ve Ayarları
                formsPlot1.Plot.Axes.DateTimeTicksBottom();
                formsPlot1.Plot.Title($"{_reportItem.MachineName} - Process Chart");
                formsPlot1.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft);

                // =========================================================================
                // ÇÖZÜM (CS1061): BORDERCOLOR MÜLKİYETİ -> OUTLINECOLOR OLARAK DEĞİŞTİRİLDİ
                // =========================================================================
                formsPlot1.Plot.Legend.FontColor = ScottPlot.Colors.White;
                formsPlot1.Plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#1a202c");
                formsPlot1.Plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#4a5568"); // Düzeltme sağlandı

                // Grafik Eksen Sınır Otomasyonu
                DateTime startTime = _reportItem.StartTime.AddMinutes(-5);
                DateTime endTime = (_reportItem.EndTime == DateTime.MinValue) ? DateTime.Now.AddMinutes(5) : _reportItem.EndTime.AddMinutes(5);

                formsPlot1.Plot.Axes.SetLimitsX(startTime.ToOADate(), endTime.ToOADate());
                formsPlot1.Plot.Axes.AutoScaleY();

                formsPlot1.Refresh();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            ExcelExporter.ExportProductionDetailToExcel(_reportItem, dgvStepDetails, dgvAlarms, formsPlot1);
        }

        private void EnableDoubleBuffer(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }
}