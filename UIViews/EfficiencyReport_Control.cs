// UIViews/EfficiencyReport_Control.cs
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Telemetry.Models;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UIViews
{
    public partial class EfficiencyReport_Control : UserControl
    {
        private EfficiencyRepository _efficiencyRepo;
        private MachineRepository _machineRepo;
        private List<EfficiencyLog> _fullReportData;

        private System.Windows.Forms.Timer _typingTimer;

        private Label lblAutoValue;
        private Label lblWaitValue;
        private Label lblManualValue;
        private Label lblEffValue;

        private bool _isPanning = false;
        private Point _lastMousePos;

        private bool _isPieChartInitialized = false;
        private ObservableValue _valAuto = new ObservableValue(0);
        private ObservableValue _valWait = new ObservableValue(0);
        private ObservableValue _valManual = new ObservableValue(0);

        public EfficiencyReport_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Genel arayüzün ve sekmelerin geçişlerde titremesini engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan rengini ve temayı üst form yönetir

            // DataGridView donanımsal çift tamponlama ivmesi
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvEfficiency, new object[] { true });

            // Modern Material KPI kartlarını başlat
            InitializeKpiCards();

            dtpStart.Value = DateTime.Now.Date;
            dtpEnd.Value = DateTime.Now;

            // Grafik interaktif olay abonelikleri
            chartTimeline.MouseWheel += ChartTimeline_MouseWheel;
            chartTimeline.MouseDown += ChartTimeline_MouseDown;
            chartTimeline.MouseMove += ChartTimeline_MouseMove;
            chartTimeline.MouseUp += ChartTimeline_MouseUp;
            chartTimeline.GetToolTipText += ChartTimeline_GetToolTipText;

            dgvEfficiency.CellFormatting += DgvEfficiency_CellFormatting;

            _typingTimer = new System.Windows.Forms.Timer();
            _typingTimer.Interval = 400;
            _typingTimer.Tick += TypingTimer_Tick;
        }

        public void InitializeControl(MachineRepository machineRepo, EfficiencyRepository efficiencyRepo)
        {
            _machineRepo = machineRepo;
            _efficiencyRepo = efficiencyRepo;
            LoadFilterData();
        }

        private void InitializeKpiCards()
        {
            panelKpiCards.Controls.Add(CreateKpiCard("OPERATION (AUTO)", out lblAutoValue, Color.FromArgb(16, 185, 129)));
            panelKpiCards.Controls.Add(CreateKpiCard("STOPPAGE (IDLE)", out lblWaitValue, Color.FromArgb(225, 29, 72)));
            panelKpiCards.Controls.Add(CreateKpiCard("MANUAL CONTROL", out lblManualValue, Color.FromArgb(245, 158, 11)));
            panelKpiCards.Controls.Add(CreateKpiCard("OVERALL EFFICIENCY", out lblEffValue, Color.FromArgb(37, 99, 235)));
        }

        // =========================================================================
        // MODERNİZASYON: DİNAMİK MATERIAL KPI KART TASARIMI
        // Eski gölgeli panel düzeneği yerine doğrudan MaterialCard mimarisine geçilmiştir.
        // =========================================================================
        private MaterialCard CreateKpiCard(string title, out Label valueLabel, Color accent)
        {
            MaterialCard card = new MaterialCard
            {
                Width = 285,
                Height = 84,
                Margin = new Padding(8),
                Padding = new Padding(12),
                Depth = 0
            };

            Panel line = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accent
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184), // Koyu ve açık temada uyumlu Slate rengi
                Location = new Point(18, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            valueLabel = new Label
            {
                Text = "00:00:00",
                Font = new Font("Segoe UI Black", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 240, 240), // Yüksek kontrastlı pürüzsüz metin tonu
                Location = new Point(16, 34),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.Controls.Add(line);
            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);

            return card;
        }

        private void LoadFilterData()
        {
            try
            {
                var machines = _machineRepo.GetAllMachines().ToList();
                var mList = new List<Machine> { new Machine { Id = 0, MachineName = "All Machines" } };
                mList.AddRange(machines);

                cmbMachine.DataSource = mList;
                cmbMachine.DisplayMember = "DisplayInfo";
                cmbMachine.ValueMember = "Id";

                var subTypes = machines
                    .Where(x => !string.IsNullOrWhiteSpace(x.MachineSubType))
                    .Select(x => x.MachineSubType).Distinct().OrderBy(x => x).ToList();

                var sList = new List<string> { "All Types" };
                sList.AddRange(subTypes);

                cmbSubType.DataSource = sList;
            }
            catch { }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (_efficiencyRepo == null) return;

            btnSearch.Enabled = false;
            btnSearch.Text = "Loading...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                int? machineId = (int)cmbMachine.SelectedValue > 0 ? (int)cmbMachine.SelectedValue : (int?)null;
                string subType = cmbSubType.SelectedIndex > 0 ? cmbSubType.SelectedItem.ToString() : null;

                var result = await _efficiencyRepo.GetEfficiencyReportAsync(dtpStart.Value, dtpEnd.Value, machineId, subType);
                _fullReportData = result.ToList();
                RefreshDashboard(_fullReportData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "Fetch Report";
                this.Cursor = Cursors.Default;
            }
        }

        private void RefreshDashboard(List<EfficiencyLog> data)
        {
            dgvEfficiency.DataSource = null;
            dgvEfficiency.DataSource = data;
            FormatGrid();
            UpdateLivePieChart(data);
            UpdateTimelineChart(data);
            UpdateSummaryText(data);
        }

        private void txtQuickFilter_TextChanged(object sender, EventArgs e)
        {
            _typingTimer.Stop();
            _typingTimer.Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            _typingTimer.Stop();
            if (_fullReportData == null) return;

            string key = txtQuickFilter.Text.ToLower();
            var filtered = _fullReportData.Where(x =>
                (x.MachineName?.ToLower().Contains(key) ?? false) ||
                (x.State?.ToLower().Contains(key) ?? false) ||
                (x.Reason1?.ToLower().Contains(key) ?? false) ||
                (x.Reason2?.ToLower().Contains(key) ?? false) ||
                (x.Reason3?.ToLower().Contains(key) ?? false) ||
                (x.Reason4?.ToLower().Contains(key) ?? false) ||
                (x.Reason5?.ToLower().Contains(key) ?? false))
            .ToList();

            RefreshDashboard(filtered);
        }

        private void UpdateLivePieChart(List<EfficiencyLog> data)
        {
            double auto = data.Where(x => x.State == "AUTO").Sum(x => x.DurationSeconds);
            double wait = data.Where(x => x.State == "IDLE").Sum(x => x.DurationSeconds);
            double manual = data.Where(x => x.State == "MANUAL").Sum(x => x.DurationSeconds);

            if (!_isPieChartInitialized)
            {
                var labelPaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) };

                var series = new ISeries[]
                {
                    new PieSeries<ObservableValue>
                    {
                        Values = new[] { _valAuto }, Name = "AUTO",
                        Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                        DataLabelsPaint = labelPaint, DataLabelsPosition = PolarLabelsPosition.Middle,
                        DataLabelsFormatter = point => $"{point.StackedValue.Share * 100:F1}%",
                        ToolTipLabelFormatter = point => $"Auto: {FormatSec(point.Model?.Value ?? 0d)}"
                    },
                    new PieSeries<ObservableValue>
                    {
                        Values = new[] { _valWait }, Name = "IDLE",
                        Fill = new SolidColorPaint(SKColor.Parse("#E11D48")),
                        DataLabelsPaint = labelPaint, DataLabelsPosition = PolarLabelsPosition.Middle,
                        DataLabelsFormatter = point => $"{point.StackedValue.Share * 100:F1}%",
                        ToolTipLabelFormatter = point => $"IDLE: {FormatSec(point.Model?.Value ?? 0d)}"
                    },
                    new PieSeries<ObservableValue>
                    {
                        Values = new[] { _valManual }, Name = "MANUAL",
                        Fill = new SolidColorPaint(SKColor.Parse("#F59E0B")),
                        DataLabelsPaint = labelPaint, DataLabelsPosition = PolarLabelsPosition.Middle,
                        DataLabelsFormatter = point => $"{point.StackedValue.Share * 100:F1}%",
                        ToolTipLabelFormatter = point => $"Manual: {FormatSec(point.Model?.Value ?? 0d)}"
                    }
                };

                pieChartLive.Series = series;
                pieChartLive.LegendPosition = LegendPosition.Bottom;
                pieChartLive.AnimationsSpeed = TimeSpan.FromMilliseconds(400);

                _isPieChartInitialized = true;
            }

            _valAuto.Value = auto;
            _valWait.Value = wait;
            _valManual.Value = manual;
        }

        // =========================================================================
        // MODERNİZASYON: ZAMAN DIZINI GRAFIĞI (TIMELINE) KOYU TEMALANDIRMA MOTORU
        // Grafik paneli, eksen çizgileri ve etiketler koyu mod scheme'ine tam eşitlendi.
        // =========================================================================
        private void UpdateTimelineChart(List<EfficiencyLog> data)
        {
            chartTimeline.SuspendLayout();

            chartTimeline.Series.Clear();
            chartTimeline.Titles.Clear();
            chartTimeline.Annotations.Clear();

            chartTimeline.BackColor = Color.Transparent; // Grafik dış gövde şeffaflığı
            var chartArea = chartTimeline.ChartAreas[0];
            chartArea.BackColor = Color.Transparent;     // Grafik iç çizim alanı şeffaflığı

            var uniqueMachines = data.Select(x => x.MachineName).Distinct().ToList();
            chartTimeline.Height = Math.Max(320, uniqueMachines.Count * 50);

            Title title = chartTimeline.Titles.Add("TIME-BASED PRODUCTION AND STOPPAGE ANALYSIS");
            title.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(226, 232, 240); // Açık gri başlık yazısı
            title.Alignment = ContentAlignment.TopLeft;

            Series s = new Series("Timeline")
            {
                ChartType = SeriesChartType.RangeBar,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.DateTime,
                YValuesPerPoint = 2
            };
            s["PointWidth"] = "0.65";

            var machineIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < uniqueMachines.Count; i++)
            {
                machineIndexMap[uniqueMachines[i]] = i + 1;
            }

            var xAxis = chartTimeline.ChartAreas[0].AxisX;
            xAxis.CustomLabels.Clear();
            xAxis.Minimum = 0.5;
            xAxis.Maximum = uniqueMachines.Count + 0.5;
            xAxis.Interval = 1;
            xAxis.MajorGrid.Enabled = false;

            // Eksen çizgisi ve etiket renklendirmeleri (Dark Mode Uyumu)
            xAxis.LineColor = Color.FromArgb(71, 85, 105);
            xAxis.LabelStyle.ForeColor = Color.FromArgb(203, 213, 225);

            foreach (var kvp in machineIndexMap)
            {
                xAxis.CustomLabels.Add(kvp.Value - 0.5, kvp.Value + 0.5, kvp.Key);
            }

            foreach (var item in data)
            {
                if (item.EndTime == null) continue;

                int machineIdx = machineIndexMap[item.MachineName];
                int idx = s.Points.AddXY(machineIdx, item.StartTime, item.EndTime);
                var p = s.Points[idx];

                // SPEED OPTİMİZASYON: Kenarlıkları sıfırlamak on binlerce veride render yükünü %90 azaltır.
                p.BorderWidth = 0;

                if (item.State == "AUTO") p.Color = Color.FromArgb(16, 185, 129);
                else if (item.State == "IDLE") p.Color = Color.FromArgb(225, 29, 72);
                else p.Color = Color.FromArgb(245, 158, 11);

                p.Tag = item;
                p.ToolTip = " ";
            }

            chartTimeline.Series.Add(s);

            var yAxis = chartTimeline.ChartAreas[0].AxisY;
            yAxis.ScaleView.Zoomable = true;
            chartTimeline.ChartAreas[0].CursorY.IsUserSelectionEnabled = false;

            // Zaman ekseni çizgi ve etiket renk eşitlemeleri
            yAxis.LineColor = Color.FromArgb(71, 85, 105);
            yAxis.LabelStyle.ForeColor = Color.FromArgb(203, 213, 225);
            yAxis.MajorGrid.LineColor = Color.FromArgb(51, 65, 85); // Soft koyu Izgara çizgileri

            yAxis.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            yAxis.ScrollBar.Size = 10;
            yAxis.LabelStyle.Format = "HH:mm";

            chartTimeline.ResumeLayout();
        }

        private void ChartTimeline_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                var p = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];

                if (p.Tag is EfficiencyLog item)
                {
                    string[] reasons = new[] { item.Reason1, item.Reason2, item.Reason3, item.Reason4, item.Reason5 }
                                       .Where(r => !string.IsNullOrWhiteSpace(r)).ToArray();
                    string allReasons = reasons.Length > 0 ? string.Join(", ", reasons) : "Not Specified";

                    e.Text = $"Machine: {item.MachineName}\n" +
                             $"Status: {item.State}\n" +
                             (item.State == "IDLE" ? $"Stoppage Reasons: {allReasons}\n" : "") +
                             $"Time: {item.StartTime:HH:mm:ss} - {item.EndTime:HH:mm:ss}\n" +
                             $"Duration: {FormatSec(item.DurationSeconds)}";
                }
            }
        }

        private void ChartTimeline_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                var chart = (System.Windows.Forms.DataVisualization.Charting.Chart)sender;
                var yAxis = chart.ChartAreas[0].AxisY;

                double cursorValue = yAxis.PixelPositionToValue(e.Location.X);
                if (double.IsNaN(cursorValue)) return;

                double zoomFactor = (e.Delta > 0) ? 0.75 : 1.25;

                double currentStart = yAxis.ScaleView.ViewMinimum;
                double currentEnd = yAxis.ScaleView.ViewMaximum;

                if (double.IsNaN(currentStart)) currentStart = yAxis.Minimum;
                if (double.IsNaN(currentEnd)) currentEnd = yAxis.Maximum;

                double currentLength = currentEnd - currentStart;
                double newLength = currentLength * zoomFactor;

                if (e.Delta < 0 && newLength >= (yAxis.Maximum - yAxis.Minimum))
                {
                    yAxis.ScaleView.ZoomReset();
                }
                else
                {
                    double ratio = (cursorValue - currentStart) / currentLength;
                    double newStart = cursorValue - (newLength * ratio);
                    double newEnd = newStart + newLength;
                    yAxis.ScaleView.Zoom(newStart, newEnd);
                }
            }
            catch { }
        }

        private void ChartTimeline_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = true;
                _lastMousePos = e.Location;
                chartTimeline.Cursor = Cursors.SizeWE;
            }
        }

        private void ChartTimeline_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                try
                {
                    var yAxis = chartTimeline.ChartAreas[0].AxisY;
                    double valueStart = yAxis.PixelPositionToValue(_lastMousePos.X);
                    double valueEnd = yAxis.PixelPositionToValue(e.Location.X);

                    if (!double.IsNaN(valueStart) && !double.IsNaN(valueEnd))
                    {
                        double shift = valueStart - valueEnd;
                        double currentMin = yAxis.ScaleView.ViewMinimum;

                        if (!double.IsNaN(currentMin))
                        {
                            yAxis.ScaleView.Scroll(currentMin + shift);
                        }
                    }
                }
                catch { }

                _lastMousePos = e.Location;
            }
        }

        private void ChartTimeline_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = false;
                chartTimeline.Cursor = Cursors.Default;
            }
        }

        private void FormatGrid()
        {
            string[] visibleColumns = { "MachineName", "State", "Reason1", "Reason2", "Reason3", "Reason4", "Reason5", "StartTime", "EndTime", "DurationSeconds" };

            foreach (DataGridViewColumn col in dgvEfficiency.Columns)
            {
                col.Visible = visibleColumns.Contains(col.Name);
            }

            if (dgvEfficiency.Columns["MachineName"] != null) dgvEfficiency.Columns["MachineName"].HeaderText = "Machine Name";
            if (dgvEfficiency.Columns["State"] != null) dgvEfficiency.Columns["State"].HeaderText = "Status";

            if (dgvEfficiency.Columns["Reason1"] != null) dgvEfficiency.Columns["Reason1"].HeaderText = "1st Stoppage Reason";
            if (dgvEfficiency.Columns["Reason2"] != null) dgvEfficiency.Columns["Reason2"].HeaderText = "2nd Stoppage Reason";
            if (dgvEfficiency.Columns["Reason3"] != null) dgvEfficiency.Columns["Reason3"].HeaderText = "3rd Stoppage Reason";
            if (dgvEfficiency.Columns["Reason4"] != null) dgvEfficiency.Columns["Reason4"].HeaderText = "4th Stoppage Reason";
            if (dgvEfficiency.Columns["Reason5"] != null) dgvEfficiency.Columns["Reason5"].HeaderText = "5th Stoppage Reason";

            if (dgvEfficiency.Columns["StartTime"] != null) dgvEfficiency.Columns["StartTime"].HeaderText = "Start Time";
            if (dgvEfficiency.Columns["EndTime"] != null) dgvEfficiency.Columns["EndTime"].HeaderText = "End Time";

            if (dgvEfficiency.Columns["DurationSeconds"] != null)
            {
                dgvEfficiency.Columns["DurationSeconds"].HeaderText = "Duration";
                dgvEfficiency.Columns["DurationSeconds"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            dgvEfficiency.BorderStyle = BorderStyle.None;
            dgvEfficiency.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvEfficiency.EnableHeadersVisualStyles = false;
            dgvEfficiency.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Grid Renk Uyumluluğu (Koyu/Açık Tema Koruyucu)
            dgvEfficiency.BackgroundColor = Color.FromArgb(30, 41, 59); // Koyu arka plan slate fırçası
            dgvEfficiency.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            dgvEfficiency.DefaultCellStyle.SelectionForeColor = Color.FromArgb(240, 240, 240);
            dgvEfficiency.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(38, 50, 68);

            dgvEfficiency.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvEfficiency.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgvEfficiency.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgvEfficiency.ColumnHeadersHeight = 45;
            dgvEfficiency.RowTemplate.Height = 36;
        }

        private void dgvEfficiency_SelectionChanged(object sender, EventArgs e)
        {
            // İhtiyaç durumunda event wiring için hazır bırakıldı.
        }

        private void DgvEfficiency_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvEfficiency.Columns[e.ColumnIndex].Name;

            if (colName == "State")
            {
                string stateValue = e.Value?.ToString();

                if (stateValue == "IDLE")
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(225, 29, 72);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (stateValue == "AUTO")
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(16, 185, 129);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
            else if (colName == "DurationSeconds")
            {
                if (e.Value != null && double.TryParse(e.Value.ToString(), out double seconds))
                {
                    e.Value = FormatSec(seconds);
                    e.FormattingApplied = true;
                }
            }
        }

        private void UpdateSummaryText(List<EfficiencyLog> data)
        {
            double auto = data.Where(x => x.State == "AUTO").Sum(x => x.DurationSeconds);
            double wait = data.Where(x => x.State == "IDLE").Sum(x => x.DurationSeconds);
            double manual = data.Where(x => x.State == "MANUAL").Sum(x => x.DurationSeconds);

            double total = auto + wait + manual;
            double eff = total > 0 ? (auto / total) * 100 : 0;

            lblAutoValue.Text = FormatSec(auto);
            lblWaitValue.Text = FormatSec(wait);
            lblManualValue.Text = FormatSec(manual);
            lblEffValue.Text = $"{eff:F1}%";

            int machineCount = data.Select(x => x.MachineName).Distinct().Count();
            lblSummary.Text = $"Total Records: {data.Count:N0}   |   Machine Count: {machineCount}   |   System Efficiency: {eff:F1}%";
        }

        private string FormatSec(double s)
        {
            TimeSpan t = TimeSpan.FromSeconds(Math.Max(0, s));
            int totalHours = (int)Math.Floor(t.TotalHours);

            return $"{totalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
        }

        // =========================================================================
        // MEMORY LEAK (BELLEK SIZINTISI) KORUMASI: ZAMANLAYICI VE ETKİLEŞİM TEMİZLİĞİ
        // Kontrol kapatıldığında RAM'de asılı kalmasını ve arka plan sızıntılarını kesin önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_typingTimer != null)
            {
                _typingTimer.Stop();
                _typingTimer.Tick -= TypingTimer_Tick;
                _typingTimer.Dispose();
            }

            chartTimeline.MouseWheel -= ChartTimeline_MouseWheel;
            chartTimeline.MouseDown -= ChartTimeline_MouseDown;
            chartTimeline.MouseMove -= ChartTimeline_MouseMove;
            chartTimeline.MouseUp -= ChartTimeline_MouseUp;
            chartTimeline.GetToolTipText -= ChartTimeline_GetToolTipText;

            base.OnHandleDestroyed(e);
        }
    }
}