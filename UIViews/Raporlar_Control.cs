using System;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.Core.Repositories; // IMachineRepository, IProductionRepository vb. için
using Universalscada.Localization;
using Universalscada.Properties;
using Universalscada.Repositories; // ProductionRepository vb. için (Eski namespace kullanılıyorsa)
using Universalscada.UIViews;

// Yeni UI namespace'i eklenmelidir
namespace Universalscada.UI.Views
{
    // CS0103 hatası muhtemelen burada çözüldü.
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

        public Raporlar_Control()
        {
            InitializeComponent();
            ApplyLocalization();
            // LanguageManager_LanguageChanged metodu daha önce eklenmişti.
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            // Kontrol örneklemesi (new'leme)
            _alarmReport = new AlarmReport_Control();
            _productionReport = new ProductionReport_Control();
            _oeeReport = new OeeReport_Control();
            _trendAnaliz = new TrendAnaliz_Control();
            _recipeOptimization = new RecipeOptimization_Control();
            _manualUsageReport = new ManualUsageReport_Control();
            _genelUretimRaporu = new GenelUretimRaporu_Control();
            _actionLogReport_Control = new ActionLogReport_Control();

            // ... (Docking logic) ...
        }

        // CS0103 Hatası için olay işleyici metot
        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            // Tüm kaynak anahtarları standardize edilmiş versiyonlarla değiştirildi.
            tabPageProductionReport.Text = Resources.ProductionReportTitle;
            tabPageAlarmReport.Text = Resources.AlarmReportTitle;
            tabPageGenelUretim.Text = Resources.GeneralConsumptionReportTitle;
            tabPageManualReport.Text = Resources.ManualConsumptionReportTitle;
            tabPageOeeReport.Text = Resources.OeeReportTitle;
            tabPageRecipeOptimization.Text = Resources.RecipeOptimizationTitle;
            tabPageTrendAnalysis.Text = Resources.TrendAnalysisTitle;
            // tabPageActionLog.Text = Resources.ActionLogReportTitle; // Varsa bu satırı kullanın
        }

        /// <summary>
        /// CS1503 Hata Çözümü: Tüm repository parametreleri, DI prensiplerine uygun olarak arayüz tipleriyle değiştirildi.
        /// </summary>
        public void InitializeControl(
            IMachineRepository machineRepo,
            IAlarmRepository alarmRepo,             // AlarmRepository -> IAlarmRepository
            IProductionRepository productionRepo,   // ProductionRepository -> IProductionRepository
            IDashboardRepository dashboardRepo,     // DashboardRepository -> IDashboardRepository
            IProcessLogRepository processLogRepo,   // ProcessLogRepository -> IProcessLogRepository
            IRecipeRepository recipeRepo,           // RecipeRepository -> IRecipeRepository
            ICostRepository costRepo                // CostRepository -> ICostRepository
        )
        {
            // Hata Çözümü: Alt kontrollere arayüzler iletildi.
            // Alt kontrol metotlarının (InitializeControl) da artık I...Repository arayüzlerini kabul etmesi GEREKİR.
            _genelUretimRaporu.InitializeControl(machineRepo, productionRepo, costRepo);
            _alarmReport.InitializeControl(machineRepo, alarmRepo);
            _productionReport.InitializeControl(machineRepo, productionRepo, recipeRepo, processLogRepo, alarmRepo);
            _oeeReport.InitializeControl(machineRepo, dashboardRepo);
            _trendAnaliz.InitializeControl(machineRepo, processLogRepo);
            _recipeOptimization.InitializeControl(recipeRepo);
            _manualUsageReport.InitializeControl(machineRepo, processLogRepo);

            // _actionLogReport_Control.InitializeControl(processLogRepo); 
        }
    }
}