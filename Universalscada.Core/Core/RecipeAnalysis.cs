// Universalscada.Core/Core/RecipeAnalysis.cs (TAMAMEN YENİ İÇERİK)
using System;
using System.Collections.Generic;
using System.Linq;
using Universalscada.Core.Meta;
using Universalscada.Models;
using Universalscada.Core.Repositories; // Meta veriyi çekecek Repository varsayımı

namespace Universalscada.Core.Core
{
    // Eski statik sınıf yerine DI destekli bir hizmet oluşturuyoruz
    public class DynamicRecipeTimeCalculator : IRecipeTimeCalculator
    {
        // Meta veri ve sabit değerleri bu servis üzerinden alacağız
        private readonly IMetaDataRepository _metaDataRepository;

        // Constructor ile DI üzerinden bağımlılıkları alıyoruz
        public DynamicRecipeTimeCalculator(IMetaDataRepository metaDataRepository)
        {
            _metaDataRepository = metaDataRepository;
        }

        public double CalculateTotalTheoreticalTimeSeconds(IEnumerable<ScadaRecipeStep> steps)
        {
            if (steps == null || !steps.Any()) return 0;

            double totalSeconds = 0;
            // 1. ADIM: Tüm proses tiplerini ve sabitleri DB'den (SQLite) al
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();
            var waterPerLiterSeconds = _metaDataRepository.GetConstantValue("WATER_PER_LITER_SECONDS", 0.5);
            var drainSeconds = _metaDataRepository.GetConstantValue("DRAIN_SECONDS", 120.0);

            foreach (var step in steps)
            {
                var parallelDurations = new List<double>();
                // Kontrol kelimesi, her zaman 25. word (index 24) kabul ediliyor.
                short controlWord = step.StepDataWords[24];

                var dynamicParams = new DynamicStepParams(step.StepDataWords);

                // 2. ADIM: Her step tipi için kontrol et
                foreach (var stepDef in stepDefinitions)
                {
                    // Adımın bit'i kontrol kelimesinde aktif mi?
                    if ((controlWord & (1 << stepDef.ControlWordBit)) != 0)
                    {
                        double stepDuration = 0;

                        // Bu kısım artık dinamik olarak CalculationServiceKey'e göre çalışmalı.
                        // Şimdilik sadece eski mantığı dinamik okumaya çevirelim:

                        if (stepDef.UniversalName == "WATER_TRANSFER") // Eski Su Alma
                        {
                            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "QUANTITY_LITERS");
                            if (paramDef != null)
                            {
                                short litre = dynamicParams.GetShortParam(paramDef.ParameterKey, paramDef.WordIndex);
                                stepDuration = litre * waterPerLiterSeconds;
                            }
                        }
                        else if (stepDef.UniversalName == "HEAT_RAMP") // Eski Isıtma
                        {
                            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "DURATION_MINUTES");
                            if (paramDef != null)
                            {
                                short sure = dynamicParams.GetShortParam(paramDef.ParameterKey, paramDef.WordIndex);
                                stepDuration = sure * 60;
                            }
                        }
                        // ... Diğer tüm adımlar (Dozaj, Çalışma, Boşaltma) bu şekilde DB'den okunan bilgilerle yazılmalıdır.
                        else if (stepDef.UniversalName == "DRAIN") // Eski Boşaltma
                        {
                            stepDuration = drainSeconds;
                        }

                        if (stepDuration > 0)
                        {
                            parallelDurations.Add(stepDuration);
                        }
                    }
                }

                totalSeconds += parallelDurations.Any() ? parallelDurations.Max() : 0;
            }
            return totalSeconds;
        }

        // Eski statik metotları arayüzden kaldırdık, sadece tek bir yöntem bıraktık.
    }
}