// UIViews/Raporlar_Control.cs
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.UIViews;

namespace Telemetry.UI.Views
{
    public partial class Raporlar_Control : UserControl
    {
        private readonly AlarmReport_Control _alarmReport;
        private readonly ProductionReport_Control _productionReport;
        private readonly OeeReport_Control _oeeReport;
        private readonly TrendAnaliz_Control _trendAnaliz;
        private readonly RecipeOptimization_Control _recipeOptimization;
        private readonly ManualUsageReport_Control _manualUsageReport;
        private readonly GenelUretimRaporu_Control _genelUretimRaporu;
        private readonly ActionLogReport_Control _actionLogReport_Control;
        private readonly EfficiencyReport_Control _efficiencyReport;

        public Raporlar_Control()
        {
            // Statik dil değişim olayına kayıt (Hafıza sızıntısını önlemek için OnHandleDestroyed'da sökülecek)
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // SPEED OPTİMİZASYON: Sekme geçişlerindeki donma ve ekran kırpışmalarını engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimi üst ebeveyn forma (MaterialTabControl) devredildi

            _alarmReport = new AlarmReport_Control();
            _efficiencyReport = new EfficiencyReport_Control();
            _efficiencyReport.Dock = DockStyle.Fill;
            tabPageEfficiency.Controls.Add(_efficiencyReport);

            _productionReport = new ProductionReport_Control();
            _oeeReport = new OeeReport_Control();
            _trendAnaliz = new TrendAnaliz_Control();
            _recipeOptimization = new RecipeOptimization_Control();
            _manualUsageReport = new ManualUsageReport_Control();

            // ÇÖZÜM: Çift nesne üretim hatasına yol açan mükerrer kod satırı temizlendi
            _genelUretimRaporu = new GenelUretimRaporu_Control();
            _actionLogReport_Control = new ActionLogReport_Control();

            _genelUretimRaporu.Dock = DockStyle.Fill;
            tabPageGenelUretim.Controls.Add(_genelUretimRaporu);

            _alarmReport.Dock = DockStyle.Fill;
            tabPageAlarmReport.Controls.Add(_alarmReport);

            _productionReport.Dock = DockStyle.Fill;
            tabPageProductionReport.Controls.Add(_productionReport);

            _oeeReport.Dock = DockStyle.Fill;
            //tabPageOeeReport.Controls.Add(_oeeReport);

            _trendAnaliz.Dock = DockStyle.Fill;
            tabPageTrendAnalysis.Controls.Add(_trendAnaliz);

            _recipeOptimization.Dock = DockStyle.Fill;
            tabPageRecipeOptimization.Controls.Add(_recipeOptimization);

            _manualUsageReport.Dock = DockStyle.Fill;
            tabPageManualReport.Controls.Add(_manualUsageReport);

            _actionLogReport_Control.Dock = DockStyle.Fill;
            tabPageActionLog.Controls.Add(_actionLogReport_Control);

            ApplyLocalization();
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            tabPageProductionReport.Text = Resources.üretimraporu;
            tabPageAlarmReport.Text = Resources.alarmrapor;
            tabPageGenelUretim.Text = Resources.geneltüketim;
            tabPageManualReport.Text = Resources.manuelrapor;
            //tabPageOeeReport.Text = Resources.OeeReport;
            tabPageRecipeOptimization.Text = Resources.RecipeOptimization;
            tabPageTrendAnalysis.Text = Resources.TrendAnalysis;
            tabPageEfficiency.Text = "Efficiency and Status Report";
        }

        /// <summary>
        /// Raporlama alt ekranlarının merkezi veri entegrasyon kanallarını besleyen başlatıcı fonksiyon.
        /// </summary>
        public void InitializeControl(
            MachineRepository machineRepo,
            AlarmRepository alarmRepo,
            ProductionRepository productionRepo,
            DashboardRepository dashboardRepo,
            ProcessLogRepository processLogRepo,
            RecipeRepository recipeRepo,
            CostRepository costRepo,
            EfficiencyRepository efficiencyRepo)
        {
            _genelUretimRaporu.InitializeControl(machineRepo, productionRepo);
            _alarmReport.InitializeControl(machineRepo, alarmRepo);
            _productionReport.InitializeControl(machineRepo, productionRepo, recipeRepo, processLogRepo, alarmRepo);
            _oeeReport.InitializeControl(machineRepo, dashboardRepo);
            _trendAnaliz.InitializeControl(machineRepo, processLogRepo);
            _recipeOptimization.InitializeControl(recipeRepo);
            _manualUsageReport.InitializeControl(machineRepo, processLogRepo);
            _efficiencyReport.InitializeControl(machineRepo, efficiencyRepo);
        }

        // =========================================================================
        // KUSURSUZ BELLEK TEMİZLİĞİ: STATİK Dil EVENT BAĞLANTISI KOPARILDI
        // Üst panel sekmeleri değiştikçe bu formun RAM'de şişme yapmasını kesin önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnHandleDestroyed(e);
        }
    }
}