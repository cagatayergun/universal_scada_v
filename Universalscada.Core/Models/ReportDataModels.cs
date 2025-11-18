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

        /// <summary> 
        /// Tüketim metriklerini tutan jenerik koleksiyon. 
        /// </summary>
        public Dictionary<string, double> ConsumptionMetrics { get; set; } = new Dictionary<string, double>();

        public double GetMetricValue(string key)
        {
            return ConsumptionMetrics.GetValueOrDefault(key, 0);
        }

        // === HATA DÜZELTME: BYMakinesiManager uyumluluğu için Property'ler eklendi ===
        // Bu property'ler arka planda Dictionary'i günceller/okur.
        public double TotalWater
        {
            get => GetMetricValue("TotalWaterLiters");
            set => ConsumptionMetrics["TotalWaterLiters"] = value;
        }

        public double TotalElectricity
        {
            get => GetMetricValue("TotalElectricityKwh");
            set => ConsumptionMetrics["TotalElectricityKwh"] = value;
        }

        public double TotalSteam
        {
            get => GetMetricValue("TotalSteamKg");
            set => ConsumptionMetrics["TotalSteamKg"] = value;
        }
    }

    /// <summary>
    /// Üretim sonu gerçekleşen detaylı madde tüketim verilerini tutan evrensel model.
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