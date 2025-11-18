// Universalscada.Core/Core/RecipeAnalysis.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Universalscada.Core.Meta;
using Universalscada.Models;
using Universalscada.Core.Repositories; // IMetaDataRepository için gerekli

namespace Universalscada.Core.Core
{
    // IRecipeTimeCalculator arayüzünü uygulayan DI destekli jenerik hizmet
    public class DynamicRecipeTimeCalculator : IRecipeTimeCalculator
    {
        private readonly IMetaDataRepository _metaDataRepository;
        // Hesaplama fonksiyonlarını CalculationServiceKey ile eşleştiren strateji haritası
        private readonly Dictionary<string, Func<ScadaRecipeStep, StepTypeDefinition, double>> _calculationStrategies;

        public DynamicRecipeTimeCalculator(IMetaDataRepository metaDataRepository)
        {
            _metaDataRepository = metaDataRepository;

            // Jenerik hesaplama metotlarını tanımla
            _calculationStrategies = new Dictionary<string, Func<ScadaRecipeStep, StepTypeDefinition, double>>(StringComparer.OrdinalIgnoreCase)
            {
                { "WaterTime", CalculateWaterTime }, // Örn: WATER_TRANSFER
                { "HeatTime", CalculateHeatTime },   // Örn: HEAT_RAMP
                { "SimpleTime", CalculateDurationFromWordIndex }, // Örn: MECHANICAL_WORK, SPIN_DRY
                { "ConstantTime", CalculateConstantTime } // Örn: DRAIN
            };
        }

        public double CalculateTotalTheoreticalTimeSeconds(IEnumerable<ScadaRecipeStep> steps)
        {
            if (steps == null || !steps.Any()) return 0;

            double totalSeconds = 0;
            // Tüm proses tiplerini ve sabitleri DB'den tek seferde al
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();

            foreach (var step in steps)
            {
                var parallelDurations = new List<double>();
                // Kontrol kelimesi, DB'de bu bilgi yoksa varsayılan index 24 kabul edilir.
                int controlWordIndex = 24;
                if (step.StepDataWords == null || step.StepDataWords.Length <= controlWordIndex) continue;

                // Ham verilere dinamik erişim için yeni accessor
                var accessor = new DynamicStepDataAccessor(step.StepDataWords);
                short controlWord = accessor.GetShort(controlWordIndex);

                // Adımdaki her olası proses tipi için kontrol et
                foreach (var stepDef in stepDefinitions)
                {
                    // Adımın bit'i kontrol kelimesinde aktif mi?
                    if (stepDef.ControlWordBit >= 0 && (controlWord & (1 << stepDef.ControlWordBit)) != 0)
                    {
                        double stepDuration = 0;

                        // İlgili hesaplama stratejisini CalculationServiceKey ile bul ve çalıştır
                        if (_calculationStrategies.TryGetValue(stepDef.CalculationServiceKey, out var calculatorFunc))
                        {
                            stepDuration = calculatorFunc(step, stepDef);
                        }

                        if (stepDuration > 0)
                        {
                            parallelDurations.Add(stepDuration);
                        }
                    }
                }

                // Paralel çalışan adımların en uzun olanını toplam süreye ekle
                totalSeconds += parallelDurations.Any() ? parallelDurations.Max() : 0;
            }
            return totalSeconds;
        }

        #region Jenerik Hesaplama Stratejileri

        // WATER_TRANSFER (Su Alma) gibi, miktar tabanlı hesaplamalar için
        private double CalculateWaterTime(ScadaRecipeStep step, StepTypeDefinition stepDef)
        {
            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "QUANTITY_LITERS");
            if (paramDef == null) return 0;

            var accessor = new DynamicStepDataAccessor(step.StepDataWords);
            short litre = accessor.GetShort(paramDef.WordIndex);

            // Katsayıyı dinamik olarak ProcessConstant tablosundan çek
            double waterPerLiterSeconds = _metaDataRepository.GetConstantValue("WATER_PER_LITER_SECONDS", 0.5);

            return litre * waterPerLiterSeconds;
        }

        // HEAT_RAMP (Isıtma) gibi, süresi direk word'den okunan ve saniyeye çevrilen hesaplamalar için
        private double CalculateHeatTime(ScadaRecipeStep step, StepTypeDefinition stepDef)
        {
            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "DURATION_MINUTES");
            if (paramDef == null) return 0;

            var accessor = new DynamicStepDataAccessor(step.StepDataWords);
            // Word'deki değeri al (Dakika cinsinden)
            short sureMinutes = accessor.GetShort(paramDef.WordIndex);

            // NOT: Bu kısma ek olarak, sicaklik artış hızı ve hedef sıcaklık kullanılarak 
            // dinamik ısıtma süresi hesaplaması da eklenebilir (RampCalculator servisi kullanılır).

            return sureMinutes * 60.0;
        }

        // MECHANICAL_WORK, SPIN_DRY gibi, süresi direk word'den okunan basit işlemler için
        private double CalculateDurationFromWordIndex(ScadaRecipeStep step, StepTypeDefinition stepDef)
        {
            // Bu strateji, Duration parametresinin WordIndex'ini kullanır
            var paramDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey.Contains("DURATION") || p.ParameterKey.Contains("TIME") || p.ParameterKey.Contains("SURE"));
            if (paramDef == null) return 0;

            var accessor = new DynamicStepDataAccessor(step.StepDataWords);
            short durationValue = accessor.GetShort(paramDef.WordIndex);

            // Eğer birim dakika ise 60 ile çarp, değilse (saniye ise) direk kullan
            string unit = paramDef.Unit?.ToLower() ?? "";
            return unit.Contains("dakika") ? durationValue * 60.0 : durationValue;
        }

        // DRAIN (Boşaltma) gibi, her zaman sabit süre alan işlemler için
        private double CalculateConstantTime(ScadaRecipeStep step, StepTypeDefinition stepDef)
        {
            // Süreyi ProcessConstant tablosundan çek
            return _metaDataRepository.GetConstantValue("DRAIN_SECONDS", 120.0);
        }

        #endregion
    }
}