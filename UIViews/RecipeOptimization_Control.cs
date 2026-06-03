// UIViews/RecipeOptimization_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Localization1;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class RecipeOptimization_Control : UserControl
    {
        private RecipeRepository _recipeRepository;

        public RecipeOptimization_Control()
        {
            // Statik dil değişim olayına kayıt (Hafıza sızıntısını önlemek için OnHandleDestroyed'da sökülecek)
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // SPEED OPTİMİZASYON: Panel sekme geçişlerinde ve filtre yüklemelerinde titremeyi engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimi üst ebeveyne devredildi

            ApplyLocalization();
        }

        public void InitializeControl(RecipeRepository recipeRepo)
        {
            _recipeRepository = recipeRepo;
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            label1.Text = Resources.anarecete;
            label7.Text = Resources.karsilastirilacak;
            btnAnalyze.Text = Resources.analizet;
            label3.Text = Resources.ortalamasutuketimi;
            label4.Text = Resources.ortalamacevrimsuresi;
            label5.Text = Resources.ortalamaelektriktuketimi;
            label6.Text = Resources.ortalamabuhartuketimi;
            label14.Text = Resources.ortalamasutuketimi;
            label12.Text = Resources.ortalamacevrimsuresi;
            label10.Text = Resources.ortalamaelektriktuketimi;
            label8.Text = Resources.ortalamabuhartuketimi;
            label2.Text = Resources.gecmisuretimler;
        }

        private void RecipeOptimization_Control_Load(object sender, EventArgs e)
        {
            if (_recipeRepository != null)
            {
                var recipes = _recipeRepository.GetAllRecipes();
                var recipes2 = new List<ScadaRecipe>(recipes); // İkinci ComboBox için kopyasını oluştur

                cmbRecipes.DataSource = recipes;
                cmbRecipes.DisplayMember = "RecipeName";
                cmbRecipes.ValueMember = "Id";

                cmbRecipe2.DataSource = recipes2;
                cmbRecipe2.DisplayMember = "RecipeName";
                cmbRecipe2.ValueMember = "Id";
            }

            // Tablo kaydırma hızını maksimuma çıkaran çift tamponlama ivmesi açıldı
            EnableDoubleBuffer(dgvHistory);
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            if (cmbRecipes.SelectedValue == null || cmbRecipe2.SelectedValue == null)
            {
                MessageBox.Show("Please select both recipes to analyze.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int recipe1Id = (int)cmbRecipes.SelectedValue;
            int recipe2Id = (int)cmbRecipe2.SelectedValue;

            // SPEED OPTİMİZASYON: Veri kaynağı yenilenirken düzen motorunu askıya al
            this.SuspendLayout();
            dgvHistory.SuspendLayout();

            try
            {
                // İlk reçete için verileri ve ortalamaları hesapla
                var history1 = _recipeRepository.GetRecipeUsageHistory(recipe1Id) ?? new List<ProductionReportItem>();
                var averages1 = CalculateAverages(history1);
                DisplayAverages(averages1, 1);

                // İkinci reçete için verileri ve ortalamaları hesapla
                var history2 = _recipeRepository.GetRecipeUsageHistory(recipe2Id) ?? new List<ProductionReportItem>();
                var averages2 = CalculateAverages(history2);
                DisplayAverages(averages2, 2);

                // Geçmiş tablosunu birleştirerek göster
                var combinedHistory = history1.Concat(history2).OrderByDescending(h => h.StartTime).ToList();
                dgvHistory.DataSource = null;
                dgvHistory.DataSource = combinedHistory;

                // Gereksiz teknik kolonları gizle
                string[] hiddenColumns = {
                    "MachineId", "BatchId", "MachineAlarmDurationSeconds", "OperatorPauseDurationSeconds",
                    "TheoreticalCycleTimeSeconds", "GoodCount", "ScrapCount", "TotalProductionCount",
                    "DefectiveProductionCount", "TotalDownTimeSeconds", "actual_produced_quantity",
                    "OperatorName", "MusteriNo", "SiparisNo", "RecipeName"
                };

                foreach (string colName in hiddenColumns)
                {
                    if (dgvHistory.Columns[colName] != null)
                        dgvHistory.Columns[colName].Visible = false;
                }

                ConfigureGridAppearance();

                // Sonuçları renklendirerek karşılaştır
                CompareAndHighlight(averages1, averages2);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during analysis: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Çizim kilitlerini kaldır ve toplu render et
                dgvHistory.ResumeLayout(true);
                this.ResumeLayout(true);
            }
        }

        private (double Water, double Electricity, double Steam, TimeSpan CycleTime) CalculateAverages(List<ProductionReportItem> history)
        {
            if (history == null || !history.Any())
                return (0, 0, 0, TimeSpan.Zero);

            double avgWater = history.Average(h => h.TotalWater);
            double avgElectricity = history.Average(h => h.TotalElectricity);
            double avgSteam = history.Average(h => h.TotalSteam);

            var avgCycleTime = TimeSpan.FromSeconds(history.Average(h =>
            {
                if (TimeSpan.TryParse(h.CycleTime, out TimeSpan parsedTime))
                    return parsedTime.TotalSeconds;
                return 0;
            }));

            return (avgWater, avgElectricity, avgSteam, avgCycleTime);
        }

        private void DisplayAverages((double Water, double Electricity, double Steam, TimeSpan CycleTime) averages, int panelIndex)
        {
            if (panelIndex == 1)
            {
                lblAvgWater.Text = $"{averages.Water:F0} L";
                lblAvgElectricity.Text = $"{averages.Electricity:F1} kW";
                lblAvgSteam.Text = $"{averages.Steam:F1} kg";
                lblAvgCycleTime.Text = averages.CycleTime.ToString(@"hh\:mm\:ss");
            }
            else // Panel 2
            {
                lblAvgWater2.Text = $"{averages.Water:F0} L";
                lblAvgElectricity2.Text = $"{averages.Electricity:F1} kW";
                lblAvgSteam2.Text = $"{averages.Steam:F1} kg";
                lblAvgCycleTime2.Text = averages.CycleTime.ToString(@"hh\:mm\:ss");
            }
        }

        private void CompareAndHighlight(
            (double Water, double Electricity, double Steam, TimeSpan CycleTime) avg1,
            (double Water, double Electricity, double Steam, TimeSpan CycleTime) avg2)
        {
            HighlightLabel(lblAvgWater, lblAvgWater2, avg1.Water, avg2.Water);
            HighlightLabel(lblAvgElectricity, lblAvgElectricity2, avg1.Electricity, avg2.Electricity);
            HighlightLabel(lblAvgSteam, lblAvgSteam2, avg1.Steam, avg2.Steam);
            HighlightLabel(lblAvgCycleTime, lblAvgCycleTime2, avg1.CycleTime.TotalSeconds, avg2.CycleTime.TotalSeconds);
        }

        // =========================================================================
        // MODERNİZASYON: ADAPTİF TEMA UYUMLU KARŞILAŞTIRMA VE BOYAMA MOTORU
        // Koyu mod scheme'inde yazıların kaybolması (beyaz üstüne beyaz) kesin önlenmiştir.
        // =========================================================================
        private void HighlightLabel(Label label1, Label label2, double value1, double value2)
        {
            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Varsayılan zemin ve yazı renk atamaları (Sıfırlama)
            label1.BackColor = Color.Transparent;
            label2.BackColor = Color.Transparent;
            label1.ForeColor = isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(15, 23, 42);
            label2.ForeColor = isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(15, 23, 42);

            if (value1 == 0 || value2 == 0) return;

            // Temaya göre akıllı kontrast renk şemaları (Düşük tüketim = Yeşil, Yüksek = Kırmızı)
            Color greenBg = isDark ? Color.FromArgb(27, 94, 32) : Color.FromArgb(200, 230, 201);  // Koyu Orman Yeşili / Pastel Yumuşak Yeşil
            Color redBg = isDark ? Color.FromArgb(183, 28, 28) : Color.FromArgb(255, 205, 210);   // Derin Mat Kırmızı / Pastel Yumuşak Kırmızı

            Color darkText = Color.FromArgb(245, 245, 245);
            Color lightGreenText = Color.FromArgb(46, 125, 50);
            Color lightRedText = Color.FromArgb(198, 40, 40);

            if (value1 < value2)
            {
                label1.BackColor = greenBg;
                label1.ForeColor = isDark ? darkText : lightGreenText;
                label2.BackColor = redBg;
                label2.ForeColor = isDark ? darkText : lightRedText;
            }
            else if (value2 < value1)
            {
                label2.BackColor = greenBg;
                label2.ForeColor = isDark ? darkText : lightGreenText;
                label1.BackColor = redBg;
                label1.ForeColor = isDark ? darkText : lightRedText;
            }
        }

        private void ConfigureGridAppearance()
        {
            dgvHistory.BorderStyle = BorderStyle.None;
            dgvHistory.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Tablo Renk Uyumluluğu (Koyu/Açık Tema Koruyucu)
            dgvHistory.BackgroundColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            dgvHistory.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254);
            dgvHistory.DefaultCellStyle.SelectionForeColor = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(15, 23, 42);
            dgvHistory.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(38, 50, 68) : Color.FromArgb(248, 250, 252);

            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            dgvHistory.ColumnHeadersHeight = 45;
            dgvHistory.RowTemplate.Height = 36;
        }

        // =========================================================================
        // KUSURSUZ BELLEK TEMİZLİĞİ: STATİK Dil EVENT ABONELİK BAĞLANTISI KOPARILDI
        // Kontrolün RAM'de asılı kalarak şişme yapmasını kesin olarak engeller.
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