// Dosya: Universalscada.WebAPI/Controllers/ReportsController.cs - GÜNCEL VERSİYON
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Universalscada.Core.Core;
using Universalscada.Core.Models; // Yeni jenerik modeller için
using Universalscada.Core.Repositories;
using Universalscada.Models;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    // ProductionRepository, ProcessLogRepository ve CostRepository'nin yeni jenerik arayüzleri varsayılır
    private readonly ProductionRepository _productionRepository;
    private readonly CostRepository _costRepository;
    private readonly DynamicRecipeCostCalculator _costCalculator;

    public ReportsController(
        ProductionRepository productionRepository,
        CostRepository costRepository,
        DynamicRecipeCostCalculator costCalculator)
    {
        _productionRepository = productionRepository;
        _costRepository = costRepository;
        _costCalculator = costCalculator;
    }

    /// <summary>
    /// Belirli bir aralıktaki Üretim Raporlarını jenerik ReportItem formatında döndürür.
    /// </summary>
    [HttpGet("production")]
    public ActionResult<IEnumerable<ProductionReportItem>> GetProductionReports([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        // ProductionRepository'nin artık jenerik rapor modellerini döndürmesi gerekir.
        var reports = _productionRepository.GetProductionReports(startDate, endDate);
        return Ok(reports);
    }

    /// <summary>
    /// Üretim sonu tüketim verilerini döndürür ve dinamik maliyet analizi yapar.
    /// </summary>
    [HttpGet("consumptionSummary")]
    public ActionResult<ConsumptionReportDto> GetConsumptionSummary([FromQuery] string batchId)
    {
        // 1. Jenerik Tüketim Verisini Çek (BatchSummaryData artık jenerik key/value içerir)
        var consumptionData = _productionRepository.GetBatchConsumptionSummary(batchId); //
        if (consumptionData == null)
        {
            return NotFound();
        }

        // 2. Mevcut maliyet parametrelerini çek
        var costParams = _costRepository.GetAllCostParameters();

        // 3. Maliyet Hesaplaması (Dinamik ve sektör bağımsız)
        // NOT: Mevcut Calculator, ScadaRecipe bekler, burada bir DTO adaptasyonu gerekebilir.
        // Basitlik için, CostCalculator'ın doğrudan BatchSummaryData'yı analiz edebildiği varsayılır.

        // Örnek: Tüketim verisini kullanarak Maliyet Hesapla
        // Gerçek implementasyon, bu veriyi RecipeCostCalculator'a uyarlamalıdır.
        decimal totalCost = (decimal)consumptionData.GetMetricValue("TotalWaterLiters") * 0.005m +
                            (decimal)consumptionData.GetMetricValue("TotalElectricityKwh") * 1.2m;

        var dto = new ConsumptionReportDto
        {
            BatchId = batchId,
            ConsumptionMetrics = consumptionData.ConsumptionMetrics, // Jenerik tüketimler
            TotalCost = totalCost,
            CurrencySymbol = "TRY",
            // Dinamik döküm metnini de burada oluşturabiliriz.
        };

        return Ok(dto);
    }
}

// Rapor sonuçlarını taşımak için DTO (İstemciye sunulacak format)
public class ConsumptionReportDto
{
    public string BatchId { get; set; }
    public Dictionary<string, double> ConsumptionMetrics { get; set; }
    public decimal TotalCost { get; set; }
    public string CurrencySymbol { get; set; }
    // Diğer jenerik rapor alanları...
}