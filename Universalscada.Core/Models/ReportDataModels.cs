// Universalscada.Core/Models/ReportDataModels.cs - KÖKLÜ DEĞİŞİKLİK
using System.Collections.Generic;
using System.Linq;

namespace Universalscada.Models
{
    /// <summary>
    /// Üretim sonu toplam tüketim verilerini tutan evrensel anahtar/değer yapısı.
    /// </summary>
    public class BatchSummaryData
    {
        public string MachineUserDefinedId { get; set; }
        public string BatchId { get; set; }
        /// <summary> YENİ: Tüketim metriklerini tutan jenerik koleksiyon. Örn: "TotalElectricityKwh", "TotalWaterLiters". </summary>
        public Dictionary<string, double> ConsumptionMetrics { get; set; } = new Dictionary<string, double>();

        public double GetMetricValue(string key)
        {
            return ConsumptionMetrics.GetValueOrDefault(key, 0);
        }
    }

    /// <summary>
    /// Üretim sonu gerçekleşen detaylı madde tüketim verilerini tutan evrensel model.
    /// Eski ChemicalConsumptionData modelinin yerini alır.
    /// </summary>
    public class MaterialConsumptionData
    {
        public int StepNumber { get; set; }
        public string MaterialName { get; set; }
        public string UniversalMaterialType { get; set; } // Örn: Chemical, Alloy, Gas
        public double Amount { get; set; }
        public string Unit { get; set; }
    }
}