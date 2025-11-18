// Universalscada.Core/Core/RecipeCostCalculator.cs
using System.Collections.Generic;
using System.Linq;
using Universalscada.Models;
using Universalscada.Core.Repositories;
using Universalscada.Core.Meta;
using System; // Math.Abs kullanımı için

namespace Universalscada.Core.Core
{
    // COMMENT UPDATE: Maliyet Hesaplamasını Evrenselleştirmek için sınıf yeniden düzenlendi
    public class DynamicRecipeCostCalculator
    {
        private readonly IMetaDataRepository _metaDataRepository;

        // DI ile repository'yi alır
        public DynamicRecipeCostCalculator(IMetaDataRepository metaDataRepository)
        {
            _metaDataRepository = metaDataRepository;
        }

        /// <summary>
        /// Reçetenin tahmini maliyetini dinamik olarak hesaplar. Sektörden bağımsız kaynak (enerji, hammadde, su vb.) tüketimini hesaplar.
        /// </summary>
        /// <param name="recipe">Hesaplanacak reçete.</param>
        /// <param name="costParams">Maliyet birimlerini içeren dış parametreler (Örn: Elektrik TL/kWh, Hammadde A TL/kg).</param>
        /// <returns>Toplam Maliyet, Para Birimi Sembolü ve Detaylı Döküm.</returns>
        public (decimal TotalCost, string CurrencySymbol, string Breakdown) Calculate(ScadaRecipe recipe, List<CostParameter> costParams)
        {
            if (recipe == null || !recipe.Steps.Any() || costParams == null)
            {
                return (0, "TRY", "No data.");
            }

            // Sabitleri DB'den dinamik olarak çek. Tüm sektörlere hitap etmesi için daha jenerik sabitler kullanıyoruz.
            decimal genericEnergyFactor = (decimal)_metaDataRepository.GetConstantValue("GENERIC_ENERGY_FACTOR_KW", 15.0); // Örn: 15.0 kW/makine
            decimal processResourceFactor = (decimal)_metaDataRepository.GetConstantValue("PROCESS_RESOURCE_FACTOR", 5.0); // Örn: 5.0 kg/dakika

            // Metadata'yı çek (Tüm Step Tipleri)
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();

            // Sektörden bağımsız olarak tüm tüketim miktarlarını tutacak jenerik sözlük (Dictionary<KaynakAdi, ToplamTuketimMiktari>)
            var totalConsumptions = new Dictionary<string, decimal>();

            // Kaynak ve Tüketim Anahtarları (Bu anahtarların CostParameter.ParameterName ile eşleşmesi gerekir)
            const string RESOURCE_KEY_DURATION = "DURATION_MINUTES";
            const string RESOURCE_KEY_QUANTITY = "QUANTITY_UNITS";
            const string CONSUMABLE_ENERGY = "ELECTRICITY";
            const string CONSUMABLE_PROCESS_RESOURCE = "PROCESS_RESOURCE_1";

            foreach (var step in recipe.Steps)
            {
                // Jenerik veri erişimcisi
                var accessor = new DynamicStepDataAccessor(step.StepDataWords);
                // Kontrol word'ü index 24 (varsayılan)
                short controlWord = accessor.GetShort(24);

                foreach (var stepDef in stepDefinitions)
                {
                    // Adımın bit'i kontrol word'ünde aktif mi?
                    if ((controlWord & (1 << stepDef.ControlWordBit)) != 0)
                    {
                        // 1. HAMMADDE/KAYNAK TÜKETİMİ (Önceki: WATER_TRANSFER)
                        if (stepDef.UniversalName == "MATERIAL_TRANSFER")
                        {
                            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey.Contains(RESOURCE_KEY_QUANTITY));
                            if (paramDef != null)
                            {
                                // StepDataWords'ten okunan değeri, uygun kaynak tüketimi kategorisine ekle
                                decimal quantity = accessor.GetShort(paramDef.WordIndex);
                                string consumptionKey = CONSUMABLE_PROCESS_RESOURCE;

                                if (totalConsumptions.ContainsKey(consumptionKey))
                                    totalConsumptions[consumptionKey] += quantity;
                                else
                                    totalConsumptions[consumptionKey] = quantity;
                            }
                        }

                        // 2. ENERJİ TÜKETEN ADIMLAR (Önceki: HEAT_RAMP, MECHANICAL_WORK, SPIN_DRY)
                        if (stepDef.UniversalName == "PROCESS_RAMP" || stepDef.UniversalName == "ENERGY_CONSUMING_STEP" || stepDef.UniversalName == "MECHANICAL_WORK")
                        {
                            var durationDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey.Contains(RESOURCE_KEY_DURATION));
                            if (durationDef != null)
                            {
                                decimal durationMinutes = accessor.GetShort(durationDef.WordIndex);

                                // Enerji tüketimi (Süre bazlı)
                                string energyKey = CONSUMABLE_ENERGY;
                                if (totalConsumptions.ContainsKey(energyKey))
                                    totalConsumptions[energyKey] += durationMinutes;
                                else
                                    totalConsumptions[energyKey] = durationMinutes;

                                // Proses Kaynak Tüketimi (Süre * Katsayı)
                                // Not: Bu, HEAT_RAMP'in buhar tüketimi gibi genelleştirildi.
                                if (stepDef.UniversalName == "PROCESS_RAMP")
                                {
                                    string processKey = CONSUMABLE_PROCESS_RESOURCE;
                                    if (totalConsumptions.ContainsKey(processKey))
                                        totalConsumptions[processKey] += durationMinutes * processResourceFactor;
                                    else
                                        totalConsumptions[processKey] = durationMinutes * processResourceFactor;
                                }
                            }
                        }
                    }
                }
            }

            // Maliyet Hesaplaması (Dinamik Katsayılar Kullanılarak)
            decimal finalTotalCost = 0;
            string currencySymbol = costParams.FirstOrDefault()?.CurrencySymbol ?? "TRY";
            var breakdownList = new List<string>();

            // Tüketim miktarlarını maliyet parametreleriyle eşleştirerek toplam maliyeti hesapla
            foreach (var consumption in totalConsumptions)
            {
                var costParam = costParams.FirstOrDefault(p => p.ParameterName.ToUpper() == consumption.Key.ToUpper());

                if (costParam != null)
                {
                    decimal resourceCost = 0;
                    if (consumption.Key == CONSUMABLE_ENERGY)
                    {
                        // Enerji Maliyeti = (Süre / 60) * Güç(kW) * Birim Maliyet (TL/kWh)
                        resourceCost = (consumption.Value / 60.0m) * genericEnergyFactor * costParam.CostValue;
                        breakdownList.Add($"Energy ({consumption.Key}): {resourceCost:F2} {currencySymbol} (Total Minutes: {consumption.Value})");
                    }
                    else // Diğer kaynaklar (Malzeme A, Su vb.)
                    {
                        // Kaynak Maliyeti = Tüketim Miktarı * Birim Maliyet (TL/Birim)
                        resourceCost = consumption.Value * costParam.CostValue;
                        breakdownList.Add($"Resource ({consumption.Key}): {resourceCost:F2} {currencySymbol} (Total Quantity: {consumption.Value})");
                    }
                    finalTotalCost += resourceCost;
                }
                else
                {
                    breakdownList.Add($"Resource ({consumption.Key}) cost not found in cost parameters.");
                }
            }

            string breakdown = string.Join("\n", breakdownList);

            return (finalTotalCost, currencySymbol, breakdown);
        }
    }
}