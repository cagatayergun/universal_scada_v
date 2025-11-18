using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Universalscada.Core.Core;
using Universalscada.Core.Models;
using Universalscada.Core.Repositories;
using Universalscada.Repositories; // CostRepository ve ProductionRepository burada olabilir (Namespaceleri kontrol edin)
using Universalscada.Models;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
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

    [HttpGet("production")]
    public ActionResult<IEnumerable<ProductionReportItem>> GetProductionReports([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var filters = new ReportFilters { StartTime = startDate, EndTime = endDate };
        var reports = _productionRepository.GetProductionReports(filters);
        return Ok(reports);
    }

    [HttpGet("consumptionSummary")]
    public ActionResult<ConsumptionReportDto> GetConsumptionSummary([FromQuery] string batchId)
    {
        var consumptionData = _productionRepository.GetBatchConsumptionSummary(batchId);
        if (consumptionData == null)
        {
            return NotFound();
        }

        // Basit maliyet hesabı (Örnek)
        decimal totalCost = 0;
        var water = consumptionData.GetMetricValue("TotalWaterLiters");
        var elec = consumptionData.GetMetricValue("TotalElectricityKwh");

        totalCost = (decimal)(water * 0.005 + elec * 1.5); // Örnek katsayılar

        consumptionData.TotalCost = totalCost;
        consumptionData.CurrencySymbol = "TL";

        return Ok(consumptionData);
    }
}