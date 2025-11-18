using System.Collections.Generic;

namespace Universalscada.Models
{
    /// <summary>
    /// Tüketim raporu özetini taşıyan veri transfer nesnesi.
    /// </summary>
    public class ConsumptionReportDto
    {
        public string BatchId { get; set; }

        // Dinamik tüketim metrikleri (Örn: "TotalWaterLiters": 500.0)
        public Dictionary<string, double> ConsumptionMetrics { get; set; } = new Dictionary<string, double>();

        public decimal TotalCost { get; set; }
        public string CurrencySymbol { get; set; }
    }
}