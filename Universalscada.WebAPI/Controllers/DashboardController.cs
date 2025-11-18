// Dosya: Universalscada.WebAPI/Controllers/DashboardController.cs - GÜNCEL VERSİYON
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Universalscada.Core.Repositories;
using Universalscada.Models;
using Universalscada.Repositories;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly DashboardRepository _dashboardRepository;
    private readonly IMetaDataRepository _metaDataRepository; // Meta veri tabanlı KPI'lar için

    public DashboardController(DashboardRepository dashboardRepository, IMetaDataRepository metaDataRepository)
    {
        _dashboardRepository = dashboardRepository;
        _metaDataRepository = metaDataRepository;
    }

    /// <summary>
    /// Jenerik KPI (Key Performance Indicator) verilerini döndürür.
    /// </summary>
    [HttpGet("kpis")]
    public ActionResult<IEnumerable<KpiData>> GetKpiData()
    {
        // KpiData modeli artık sadece Toplam Üretim, OEE vb. gibi evrensel metrikleri içermeli.
        // Sektöre özgü metrikler LiveDataPoints gibi dinamik bir yapıda olmalıdır.

        var allKpis = new List<KpiData>
        {
            new KpiData
            {
                Key = "OEE_TODAY",
                Display = "Bugünkü OEE",
                Value = _dashboardRepository.GetOeeToday(),
                Unit = "%"
            },
            new KpiData
            {
                Key = "TOTAL_PRODUCTION_COUNT",
                Display = "Toplam Üretim Adedi",
                Value = _dashboardRepository.GetTotalProductionCount(),
                Unit = "adet"
            },
            // Yeni bir proses sabitine dayalı jenerik KPI
            new KpiData
            {
                Key = "DEFAULT_DRAIN_TIME",
                Display = "Varsayılan Boşaltma Süresi",
                Value = _metaDataRepository.GetConstantValue("DRAIN_SECONDS", 120.0), //
                Unit = "saniye"
            }
        };

        return Ok(allKpis);
    }

    /// <summary>
    /// En çok alarm veren makineleri veya alarm tiplerini döndürür.
    /// </summary>
    [HttpGet("topAlarms")]
    public ActionResult<IEnumerable<TopAlarmData>> GetTopAlarmData()
    {
        // TopAlarmData zaten jenerik bir modeldir.
        var topAlarms = _dashboardRepository.GetTopAlarmTypes();
        return Ok(topAlarms);
    }
}

// Jenerik KPI Veri Modeli
public class KpiData
{
    public string Key { get; set; }
    public string Display { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
}