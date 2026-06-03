// UI/Views/TrendAnaliz_Control.cs
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Localization1;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

// =========================================================================
// ÇÖZÜM: CS0104 AD ALANI ÇAKIŞMA KORUMASI
// ScottPlot ve WinForms renk yapılarının birbiriyle çakışmasını kesin önler.
// =========================================================================
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;

namespace Telemetry.UI.Views
{
    public partial class TrendAnaliz_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private ProcessLogRepository _processLogRepository;

        public TrendAnaliz_Control()
        {
            InitializeComponent();
            ApplyLocalization();

            // Statik global dil olayına kayıt (OnHandleDestroyed içinde temizlenecektir)
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            this.Load += TrendAnaliz_Control_Load;
            btnGenerateChart.Click += btnGenerateChart_Click;

            // SPEED OPTİMİZASYON: Trend sekmesi yüklenirken pencerelerin titremesini engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            label1.Text = Resources.Baslangic_tarihi;
            label2.Text = Resources.Bitis_tarihi;
            groupBox1.Text = Resources.makineler;
            groupBox2.Text = Resources.görüntülenecek_veriler;
            chkTemperature.Text = Resources.Temperature;
            chkWaterLevel.Text = Resources.suseviyesi;
            chkRpm.Text = Resources.devir;
            btnGenerateChart.Text = Resources.grafigiolustur;
        }

        public void InitializeControl(MachineRepository machineRepo, ProcessLogRepository processLogRepo)
        {
            _machineRepository = machineRepo;
            _processLogRepository = processLogRepo;
        }

        private void TrendAnaliz_Control_Load(object sender, EventArgs e)
        {
            dtpStartTime.Value = DateTime.Now.AddHours(-1);
            dtpEndTime.Value = DateTime.Now;

            var machines = _machineRepository.GetAllMachines() ?? new List<Machine>();
            clbMachines.DataSource = machines;
            clbMachines.DisplayMember = "DisplayInfo";
            clbMachines.ValueMember = "Id";

            // Tasarım elementlerini flat temaya uyarla
            ConfigureComponentAppearance();
        }

        // =========================================================================
        // MODERNİZASYON & OPTİMİZASYON: ASENKRON HISTORICAL DATA SORGULAMA MOTORU
        // Büyük tarih aralıklarında tarama yapılırken SCADA arayüzünün donması önlenmiştir.
        // =========================================================================
        private async void btnGenerateChart_Click(object sender, EventArgs e)
        {
            // UI Thread üzerinde güvenle seçili ID'leri topla (Cross-thread hatasını önlemek için)
            var selectedMachineIds = clbMachines.CheckedItems.OfType<Machine>().Select(m => m.Id).ToList();
            if (!selectedMachineIds.Any())
            {
                MessageBox.Show("Please select at least one machine.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool anyChecked = chkTemperature.Checked || chkWaterLevel.Checked || chkRpm.Checked;
            if (!anyChecked)
            {
                MessageBox.Show("Please select at least one data type (Temperature, Water Level or RPM).", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sorgulama anında imleci kilitle ve butonu pasifleştir
            this.Cursor = Cursors.WaitCursor;
            btnGenerateChart.Enabled = false;

            try
            {
                DateTime start = dtpStartTime.Value;
                DateTime end = dtpEndTime.Value;

                // Ağır SQL log okuma işlemini asenkron arka plan thread'ine (Task) delege ediyoruz
                var dataPoints = await Task.Run(() => _processLogRepository.GetLogsForDateRange(start, end, selectedMachineIds));

                formsPlot1.Plot.Clear();
                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
                ApplyScottPlotTheme(isDark); // Grafik giydirme adaptörü tetiklendi

                if (dataPoints != null && dataPoints.Any())
                {
                    var groupedData = dataPoints.GroupBy(d => d.MachineId);
                    var machineListCache = clbMachines.DataSource as List<Machine> ?? new List<Machine>();

                    foreach (var group in groupedData)
                    {
                        var machine = machineListCache.FirstOrDefault(m => m.Id == group.Key);
                        string machineName = machine?.MachineName ?? $"Makine {group.Key}";

                        // Kurutma makinesi ise hassasiyet çarpanı 100, boyama ise 10 olarak dengelenir
                        double tempDivisor = (machine != null && machine.MachineType == "Kurutma Makinesi") ? 100.0 : 10.0;
                        double[] timeData = group.Select(p => p.Timestamp.ToOADate()).ToArray();

                        if (chkTemperature.Checked)
                        {
                            double[] tempData = group.Select(p => (double)p.Temperature / tempDivisor).ToArray();
                            var scatter = formsPlot1.Plot.Add.Scatter(timeData, tempData);
                            scatter.LegendText = $"{machineName} - Temperature";
                            scatter.LineWidth = 2f; // ScottPlot 5 float standardına eşitlendi
                            scatter.MarkerSize = 0; // Performans için veri noktası noktacıkları gizlendi
                        }

                        if (chkWaterLevel.Checked)
                        {
                            double[] waterData = group.Select(p => (double)p.WaterLevel).ToArray();
                            var scatter = formsPlot1.Plot.Add.Scatter(timeData, waterData);
                            scatter.LegendText = $"{machineName} - Water level";
                            scatter.LineWidth = 2f;
                            scatter.MarkerSize = 0;
                        }

                        if (chkRpm.Checked)
                        {
                            double[] rpmData = group.Select(p => (double)p.Rpm).ToArray();
                            var scatter = formsPlot1.Plot.Add.Scatter(timeData, rpmData);
                            scatter.LegendText = $"{machineName} - RPM";
                            scatter.LineWidth = 2f;
                            scatter.MarkerSize = 0;
                        }
                    }

                    formsPlot1.Plot.Axes.DateTimeTicksBottom();
                    formsPlot1.Plot.Title("Process Variables Trend Chart");
                    formsPlot1.Plot.ShowLegend();
                    formsPlot1.Plot.Axes.AutoScale();
                }
                else
                {
                    formsPlot1.Plot.Title("No data found in the selected range.");
                }

                formsPlot1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while creating the chart: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnGenerateChart.Enabled = true;
            }
        }

        // =========================================================================
        // YENİ METOT: SCOTTPLOT 5 TEMA ENTEGRASYON ADAPTÖRÜ
        // Trend çizgisini karanlık/aydınlık mod zemin şemasına tam uyumlu giydirir.
        // =========================================================================
        private void ApplyScottPlotTheme(bool isDark)
        {
            formsPlot1.Plot.FigureBackground.Color = ScottPlot.Colors.Transparent;
            formsPlot1.Plot.DataBackground.Color = ScottPlot.Colors.Transparent;

            // Koyu modda soft slate beyazı, açık modda grafit grisi eksenler
            var axisColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(148, 163, 184)) : ScottPlot.Color.FromColor(Color.FromArgb(71, 85, 105));
            var gridColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(51, 65, 85)) : ScottPlot.Color.FromColor(Color.FromArgb(241, 245, 249));

            formsPlot1.Plot.Axes.Color(axisColor);
            formsPlot1.Plot.Grid.MajorLineColor = gridColor;

            formsPlot1.Plot.Axes.Left.Label.ForeColor = axisColor;
            formsPlot1.Plot.Axes.Bottom.Label.ForeColor = axisColor;

            // Trend gösterge paneli (Legend) Dark mode giydirmesi
            formsPlot1.Plot.Legend.BackgroundColor = isDark ? ScottPlot.Color.FromColor(Color.FromArgb(30, 41, 59)) : ScottPlot.Color.FromColor(Color.White);
            formsPlot1.Plot.Legend.FontColor = axisColor;
            formsPlot1.Plot.Legend.OutlineColor = gridColor;
        }

        private void ConfigureComponentAppearance()
        {
            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color controlBg = isDark ? Color.FromArgb(44, 52, 64) : Color.White;
            Color controlFg = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(15, 23, 42);

            if (clbMachines != null)
            {
                clbMachines.BackColor = controlBg;
                clbMachines.ForeColor = controlFg;
                clbMachines.BorderStyle = BorderStyle.None;
            }
        }

        // =========================================================================
        // KUSURSUZ BELLEK TEMİZLİĞİ: STATİK GLOBAL EVENT KİLİDİ AÇILDI
        // Kontrolün RAM'de asılı kalmasını ve hafıza sızıntısı yapmasını kesin önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnHandleDestroyed(e);
        }
    }
}