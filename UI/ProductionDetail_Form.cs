// UI/ProductionDetail_Form.cs
using ScottPlot; // ScottPlot'u kullanmak için
using System;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TekstilScada.Core;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;
using ChartLegend = System.Windows.Forms.DataVisualization.Charting.Legend;
namespace TekstilScada.UI
{
    public partial class ProductionDetail_Form : Form
    {
        private readonly ProductionReportItem _reportItem;
        private readonly ProductionRepository _productionRepo;
        private readonly ProcessLogRepository _processLogRepo;
        private readonly AlarmRepository _alarmRepo; // Alarmlar için eklendi
        private readonly RecipeRepository _recipeRepository; // YENİ

        public ProductionDetail_Form(ProductionReportItem reportItem, RecipeRepository recipeRepo, ProcessLogRepository processLogRepo, AlarmRepository alarmRepo)
        {
            InitializeComponent();
            _reportItem = reportItem;
            _productionRepo = new ProductionRepository();
            _processLogRepo = processLogRepo;
            _alarmRepo = alarmRepo;
            _recipeRepository = recipeRepo;
        }

        private void ProductionDetail_Form_Load(object sender, EventArgs e)
        {
            this.Text = $"Production Report Detail - {_reportItem.BatchId}";

            // 1. Başlık bilgilerini doldur
            txtMachineName.Text = _reportItem.MachineName;
            txtRecipeName.Text = _reportItem.RecipeName;
            txtOperator.Text = _reportItem.OperatorName;
            txtStartTime.Text = _reportItem.StartTime.ToString("dd.MM.yyyy HH:mm:ss");
            txtStopTime.Text = _reportItem.EndTime.ToString("dd.MM.yyyy HH:mm:ss");
            var elektrikvalue = _reportItem.TotalElectricity;
            txtElectricity.Text = elektrikvalue.ToString();
            var suvale = _reportItem.TotalAir;
            txtAir.Text = suvale.ToString();
            var steamvalue = _reportItem.TotalSteam;
            txtSteam.Text = steamvalue.ToString();
            txtTotalDuration.Text = _reportItem.CycleTime;
            // --- YENİ HESAPLAMA KISMI ---
            // Teorik Süreyi Yaz
            TimeSpan theoreticalSpan = TimeSpan.FromSeconds(_reportItem.TheoreticalCycleTimeSeconds);
            txtTheoreticalDuration.Text = theoreticalSpan.ToString(@"hh\:mm\:ss");

            // Gerçekleşen Süreyi Hesapla (CycleTime string formatında olduğu için TimeSpan'a çeviriyoruz)
            TimeSpan actualSpan;
            if (TimeSpan.TryParse(_reportItem.CycleTime, out actualSpan))
            {
                // Farkı Hesapla (Gerçekleşen - Teorik)
                TimeSpan diff = actualSpan - theoreticalSpan;

                // Farkı Göster (+/- işaretiyle)
                string sign = diff.TotalSeconds >= 0 ? "+" : "-";
                txtDurationDiff.Text = $"{sign}{diff.Duration():hh\\:mm\\:ss}";

                // Renklendirme (Gecikme varsa kırmızı, erken bittiyse yeşil)
                if (diff.TotalSeconds > 60) // 1 dakikadan fazla gecikme
                {
                    txtDurationDiff.ForeColor = System.Drawing.Color.Red;
                    txtDurationDiff.BackColor = System.Drawing.Color.MistyRose;
                }
                else if (diff.TotalSeconds < -60) // 1 dakikadan fazla erken bitiş
                {
                    txtDurationDiff.ForeColor = System.Drawing.Color.Green;
                    txtDurationDiff.BackColor = System.Drawing.Color.Honeydew;
                }
            }
            else
            {
                txtDurationDiff.Text = "---";
            }
            // ----------------------------
            // Diğer bilgileri de doldur
            txtCustomerNo.Text = _reportItem.MusteriNo;
            txtOrderNo.Text = _reportItem.SiparisNo;


            // 2. Adım detaylarını DataGridView'e yükle
            dgvStepDetails.DataSource = _productionRepo.GetProductionStepDetails(_reportItem.BatchId, _reportItem.MachineId);

            // 3. Alarm detaylarını yükle
            dgvAlarms.DataSource = _alarmRepo.GetAlarmDetailsForBatch(_reportItem.BatchId, _reportItem.MachineId);
            dgvStepDetails.CellFormatting += dgvStepDetails_CellFormatting;
            // 4. Zaman çizgisi grafiğini yükle
            // 1. Alarm ve Grafik Verilerini Yükle
        
            LoadTimelineChart(); // Mevcut zaman çizelgesi grafiğini yükle

            // 2. YENİ: Pasta Grafik Verilerini Hesapla ve Yükle
            LoadPieChart();

            
        }
        // TekstilScada/UI/ProductionDetail_Form.cs

        private void LoadPieChart()
        {
            // 1. TOPLAM BATCH SÜRESİNİ HESAPLA
            DateTime batchStart = _reportItem.StartTime;
            DateTime batchEnd = (_reportItem.EndTime == DateTime.MinValue) ? DateTime.Now : _reportItem.EndTime;
            double totalBatchSeconds = (batchEnd - batchStart).TotalSeconds;

            // 2. ALARMLARI ÇEK
            var allAlarms = _alarmRepo.GetAlarmDetailsForBatch(_reportItem.BatchId, _reportItem.MachineId);

            // 3. KATEGORİZE ET VE LİSTELE (Süreleri henüz toplamıyoruz)
            var machineAlarms = allAlarms
                .Where(a => a.AlarmNumber >= 0 && a.AlarmNumber <= 499)
                .ToList();

            var operatorAlarms = allAlarms
                .Where(a => a.AlarmNumber >= 500 && a.AlarmNumber <= 600)
                .ToList();

            // 4. ÇAKIŞAN ZAMANLARI BİRLEŞTİREREK NET SÜRELERİ HESAPLA
            double netMachineAlarmSec = CalculateUniqueDuration(machineAlarms, batchEnd);
            double netOperatorAlarmSec = CalculateUniqueDuration(operatorAlarms, batchEnd);

            // 5. TOPLAM DURUŞU HESAPLA (Makine + Operatör çakışmalarını da temizle)
            // Eğer bir makine alarmı varken aynı anda operatör pause yapmışsa, o süreyi 2 kere düşmemek için:
            double totalNetDowntimeSec = CalculateUniqueDuration(allAlarms, batchEnd);

            // 6. AKTİF ÇALIŞMAYI HESAPLA
            double activeWorkSec = totalBatchSeconds - totalNetDowntimeSec;
            if (activeWorkSec < 0) activeWorkSec = 0;

            // 7. GRAFİĞİ TEMİZLE VE ÇİZDİR
            pieChartControl.Series.Clear();
            pieChartControl.Legends.Clear();

            var series = new System.Windows.Forms.DataVisualization.Charting.Series("Efficiency")
            {
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie,
                Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                IsValueShownAsLabel = true,
                Label = "#PERCENT{P1}"
            };
            pieChartControl.Series.Add(series);

            // Veri Noktaları
            AddPiePoint(series, activeWorkSec, "Aktif Çalışma", System.Drawing.Color.DodgerBlue);
            AddPiePoint(series, netMachineAlarmSec, "Makine Alarmları", System.Drawing.Color.Crimson);
            AddPiePoint(series, netOperatorAlarmSec, "Operatör Alarmları", System.Drawing.Color.Orange);

            // Legend Ayarları
            var legend = new System.Windows.Forms.DataVisualization.Charting.Legend("Default")
            {
                Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom,
                Alignment = System.Drawing.StringAlignment.Center
            };
            pieChartControl.Legends.Add(legend);
            pieChartControl.Invalidate();
        }

        // --- KRİTİK ALGORİTMA: ÇAKIŞAN ZAMAN ARALIKLARINI BİRLEŞTİRME ---
        private double CalculateUniqueDuration(List<AlarmDetail> alarms, DateTime limitEndTime)
        {
            if (alarms == null || !alarms.Any()) return 0;

            // 1. Alarmları başlangıç zamanına göre sırala
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

            // 2. Kesişen aralıkları birleştir (Merge Intervals)
            for (int i = 1; i < sortedIntervals.Count; i++)
            {
                if (sortedIntervals[i].Start < currentEnd)
                {
                    // Çakışma var, mevcut aralığın sonunu uzat
                    if (sortedIntervals[i].End > currentEnd)
                    {
                        currentEnd = sortedIntervals[i].End;
                    }
                }
                else
                {
                    // Çakışma yok, önceki aralığın süresini ekle ve yeni aralığa geç
                    totalSeconds += (currentEnd - currentStart).TotalSeconds;
                    currentStart = sortedIntervals[i].Start;
                    currentEnd = sortedIntervals[i].End;
                }
            }

            // Son aralığı ekle
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

        // FormLoad metodundan LoadPieChart çağrısını parametresiz yap



        // dgvStepDetails_CellFormatting metodunun içine bu kodu ekleyin.

        private void dgvStepDetails_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Sadece "Gerçekleşen Süre" (WorkingTime) sütununda çalış ve satırın boş olmadığından emin ol.
            if (e.RowIndex >= 0 && dgvStepDetails.Columns[e.ColumnIndex].Name == "WorkingTime")
            {
                // "İşleniyor..." yazan adımları atla
                if (e.Value == null || e.Value.ToString() == "Processing...")
                {
                    e.CellStyle.BackColor = dgvStepDetails.DefaultCellStyle.BackColor;
                    return;
                }

                try
                {
                    // Teorik ve Gerçekleşen süreleri TimeSpan nesnelerine çevir
                    TimeSpan theoreticalTime;
                    TimeSpan.TryParse(dgvStepDetails.Rows[e.RowIndex].Cells["TheoreticalTime"].Value.ToString(), out theoreticalTime);

                    TimeSpan workingTime;
                    TimeSpan.TryParse(e.Value.ToString(), out workingTime);

                    // Dakika bazında farkı hesapla (saniyeleri göz ardı et)
                    int theoreticalMinutes = (int)theoreticalTime.TotalMinutes;
                    int workingMinutes = (int)workingTime.TotalMinutes;

                    // Fark 1 dakikadan fazlaysa kırmızı yap
                    if (workingMinutes > theoreticalMinutes)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.LightCoral;
                        e.CellStyle.ForeColor = System.Drawing.Color.Black; // Yazı rengini siyah yap
                    }
                    // Fark -1 dakikadan azsa yeşil yap
                    else if (workingMinutes < theoreticalMinutes)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
                        e.CellStyle.ForeColor = System.Drawing.Color.Black;
                    }
                    // Fark 1 dakika içindeyse veya eşitse varsayılan renge döndür
                    else
                    {
                        e.CellStyle.BackColor = dgvStepDetails.DefaultCellStyle.BackColor;
                        e.CellStyle.ForeColor = dgvStepDetails.DefaultCellStyle.ForeColor;
                    }
                }
                catch (Exception)
                {
                    // Bir hata olursa varsayılan renkte bırak
                    e.CellStyle.BackColor = dgvStepDetails.DefaultCellStyle.BackColor;
                }
            }
        }
        private void LoadTimelineChart()
        {
            var dataPoints = _processLogRepo.GetLogsForBatch(_reportItem.MachineId, _reportItem.BatchId);
            formsPlot1.Plot.Clear();
            if (dataPoints.Any())
            {
                double[] timeData = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                // DEĞİŞİKLİK: Sıcaklık verisini 10'a böl


                double divisor = 10.0;

                try
                {
                    // Makine bilgilerini çekmek için repository oluştur
                    var machineRepo = new MachineRepository();
                    // Makineyi ID'ye göre bul (GetMachineById veya GetAllMachines içinden)
                    var machine = machineRepo.GetAllMachines().FirstOrDefault(m => m.Id == _reportItem.MachineId);

                    // Makine bulunduysa ve Tipi "Kurutma" ise böleni 100 yap
                    if (machine != null && !string.IsNullOrEmpty(machine.MachineType) &&
                        machine.MachineType.Contains("Kurutma Makinesi", StringComparison.OrdinalIgnoreCase))
                    {
                        divisor = 100.0;
                    }
                }
                catch (Exception)
                {
                    // Olası bir hatada varsayılan (10.0) değer korunur.
                }
                double[] tempData = dataPoints.Select(p => (double)p.Temperature / divisor).ToArray();
                var tempPlot = formsPlot1.Plot.Add.Scatter(timeData, tempData);
                tempPlot.Color = ScottPlot.Colors.Red;
                tempPlot.LegendText = "Temperature";
                tempPlot.MarkerSize = 0;
                // 2. YENİ: Teorik Veri Grafiğini Çiz
                var productionRepo = new ProductionRepository();
                var batchRecipe = productionRepo.GetBatchRecipe(_reportItem.MachineId, _reportItem.BatchId);

                // 2. Eğer reçete verisi bulunduysa, RampCalculator'ı kullanarak teorik veriyi oluşturun.
                if (batchRecipe != null && batchRecipe.Steps.Any())
                {
                    var (theoTimestamps, theoTemperatures) = RampCalculator.GenerateTheoreticalRamp(batchRecipe, _reportItem.StartTime);

                    if (theoTimestamps.Any())
                    {
                        var theoPlot = formsPlot1.Plot.Add.Scatter(theoTimestamps, theoTemperatures);
                        theoPlot.Color = ScottPlot.Colors.Blue;
                        theoPlot.LegendText = "Theoretical Temperature";
                        theoPlot.LineStyle.Pattern = ScottPlot.LinePattern.Dashed;
                        theoPlot.LineWidth = 2;
                    }
                }

                formsPlot1.Plot.Axes.DateTimeTicksBottom();
                formsPlot1.Plot.Title($"{_reportItem.MachineName} - Process Chart");
                formsPlot1.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft);
                
        // --- YENİ KOD: GRAFİĞİ OTOMATİK YAKINLAŞTIRMA ---
        // AutoScale() yerine, eksen limitlerini manuel olarak belirliyoruz.
        DateTime startTime = _reportItem.StartTime;
        // Eğer üretim bitmemişse, bitiş zamanı olarak şimdiki zamanı al
        DateTime endTime = (_reportItem.EndTime == DateTime.MinValue) ? DateTime.Now : _reportItem.EndTime;

        // Grafiğin kenarlara yapışmaması için küçük bir boşluk (marj) ekleyelim
        startTime = startTime.AddMinutes(-5);
        endTime = endTime.AddMinutes(5);

        // X ekseninin (zaman) sınırlarını ayarla
        formsPlot1.Plot.Axes.SetLimitsX(startTime.ToOADate(), endTime.ToOADate());
        // Y eksenini (sıcaklık) ise kendi verisine göre otomatik ayarlamasını söyle
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
            ExcelExporter.ExportProductionDetailToExcel(_reportItem, dgvStepDetails, dgvAlarms,formsPlot1);
            //ExportProductionDetailToExcel(ProductionReportItem headerData, DataGridView dgvSteps, DataGridView dgvAlarms)
        }
    }
}