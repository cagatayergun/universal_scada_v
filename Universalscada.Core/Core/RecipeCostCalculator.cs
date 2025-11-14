// Universalscada.Core/Core/RecipeCostCalculator.cs - KÖKLÜ DEĞİŞİKLİK
using System.Collections.Generic;
using System.Linq;
using Universalscada.Models;
using Universalscada.Core.Repositories;
using Universalscada.Core.Meta;

namespace Universalscada.Core.Core
{
    // Statik olmaktan çıkarıldı ve DI destekli jenerik hizmet haline getirildi
    public class DynamicRecipeCostCalculator
    {
        private readonly IMetaDataRepository _metaDataRepository;

        // DI ile repository'yi alır
        public DynamicRecipeCostCalculator(IMetaDataRepository metaDataRepository)
        {
            _metaDataRepository = metaDataRepository;
        }

        /// <summary>
        /// Reçetenin tahmini maliyetini dinamik olarak hesaplar.
        /// </summary>
        /// <param name="recipe">Hesaplanacak reçete.</param>
        /// <param name="costParams">Maliyet birimlerini içeren dış parametreler (Örn: Su TL/Litre).</param>
        /// <returns>Toplam Maliyet, Para Birimi Sembolü ve Detaylı Döküm.</returns>
        public (decimal TotalCost, string CurrencySymbol, string Breakdown) Calculate(ScadaRecipe recipe, List<CostParameter> costParams)
        {
            if (recipe == null || !recipe.Steps.Any() || costParams == null)
            {
                return (0, "TRY", "No data.");
            }

            // Sabitleri DB'den dinamik olarak çek
            decimal motorPowerKw = (decimal)_metaDataRepository.GetConstantValue("MOTOR_POWER_KW", 15.0); // Örn: 15.0
            decimal steamKgPerMin = (decimal)_metaDataRepository.GetConstantValue("STEAM_KG_PER_MINUTE_HIGH_TEMP", 5.0); // Örn: 5.0

            // Metadata'yı çek (Tüm Step Tipleri)
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();

            decimal totalWaterLiters = 0;
            decimal totalOperatingMinutes = 0; // Elektrik tüketimini hesaplamak için
            decimal totalHeatingMinutes = 0; // Buhar tüketimini hesaplamak için

            foreach (var step in recipe.Steps)
            {
                // Jenerik veri erişimcisi
                var accessor = new DynamicStepDataAccessor(step.StepDataWords);
                // Kontrol word'ü index 24 (varsayılan)
                short controlWord = accessor.GetShort(24);

                foreach (var stepDef in stepDefinitions)
                {
                    // Adımın bit'i kontrol word'ünde aktif mi?
                    if (stepDef.ControlWordBit.HasValue && (controlWord & (1 << stepDef.ControlWordBit.Value)) != 0)
                    {
                        // 1. SU TÜKETİMİ (WATER_TRANSFER)
                        if (stepDef.UniversalName == "WATER_TRANSFER")
                        {
                            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "QUANTITY_LITERS");
                            if (paramDef != null)
                            {
                                totalWaterLiters += accessor.GetShort(paramDef.WordIndex);
                            }
                        }

                        // 2. ISITMA SÜRESİ (STEAM / HEAT_RAMP)
                        if (stepDef.UniversalName == "HEAT_RAMP")
                        {
                            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "DURATION_MINUTES");
                            if (paramDef != null)
                            {
                                totalHeatingMinutes += accessor.GetShort(paramDef.WordIndex);
                            }
                        }

                        // 3. ÇALIŞMA SÜRESİ (ELECTRICITY / MECHANICAL_WORK, SPIN_DRY)
                        if (stepDef.UniversalName == "MECHANICAL_WORK" || stepDef.UniversalName == "SPIN_DRY")
                        {
                            // Çalışma süresini tutan parametreyi bul (Örn: WORK_DURATION_MINUTES)
                            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey.Contains("DURATION_MINUTES") || p.ParameterKey.Contains("SURE"));
                            if (paramDef != null)
                            {
                                totalOperatingMinutes += accessor.GetShort(paramDef.WordIndex);
                            }
                        }
                    }
                }
            }

            // Maliyet Parametrelerini Al
            var waterParam = costParams.FirstOrDefault(p => p.ParameterName == "Water");
            var electricityParam = costParams.FirstOrDefault(p => p.ParameterName == "Electricity");
            var steamParam = costParams.FirstOrDefault(p => p.ParameterName == "Steam");

            // Maliyet Hesaplaması (Dinamik Katsayılar Kullanılarak)
            decimal waterCost = totalWaterLiters * waterParam?.CostValue ?? 0;
            // (Dakika / 60) * Güç(kW) * Birim Maliyet (TL/kWh)
            decimal electricityCost = (totalOperatingMinutes / 60.0m) * motorPowerKw * electricityParam?.CostValue ?? 0;
            // Dakika * Buhar Tüketimi (Kg/Dakika) * Birim Maliyet (TL/Kg Buhar)
            decimal steamCost = totalHeatingMinutes * steamKgPerMin * steamParam?.CostValue ?? 0;

            decimal totalCost = waterCost + electricityCost + steamCost;
            string currencySymbol = waterParam?.CurrencySymbol ?? "TRY";

            // Detaylı döküm (Breakdown)
            string breakdown = $"Water: {waterCost:F2} {currencySymbol}\nElectricity: {electricityCost:F2} {currencySymbol}\nSteam: {steamCost:F2} {currencySymbol}";

            return (totalCost, currencySymbol, breakdown);
        }
    }
}