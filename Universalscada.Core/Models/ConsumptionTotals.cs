namespace Universalscada.Models
{
    public class ConsumptionTotals
    {
        public decimal TotalWater { get; set; }
        public decimal TotalElectricity { get; set; }
        public decimal TotalSteam { get; set; }
        // Eski/Genel Toplamlar (Gelen içerikte var)
        

        // EKLENDİ: ProductionRepository.GetConsumptionTotalsForPeriod tarafından kullanılan alanlar
        public decimal TotalMaterialFlowA { get; set; }
        public decimal TotalEnergyKWH { get; set; }
        public decimal TotalProcessResourceB { get; set; }

        // EKLENDİ: ProductionRepository.LogResourceConsumption ve GetBatchResourceConsumptionAsync tarafından kullanılan detay alanları
        public int StepNumber { get; set; }
        public string MaterialName { get; set; }
        public short AmountUnits { get; set; }
    }
}