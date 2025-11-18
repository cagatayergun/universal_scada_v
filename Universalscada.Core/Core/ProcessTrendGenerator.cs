// Universalscada.Core/Core/RampCalculator.cs -> Universalscada.Core/Core/ProcessTrendGenerator.cs (YENİ İSİM)
using System;
using System.Collections.Generic;
using Universalscada.Models;
using Universalscada.Core.Repositories; // DI için eklendi
using Universalscada.Core.Meta; // Meta veri sınıfları için eklendi

namespace Universalscada.Core.Core
{
    /// <summary>
    /// Proses adımlarına göre teorik proses trend (gradyan/rampa) grafiğini hesaplar.
    /// Sıcaklık, basınç, hız vb. herhangi bir parametre için kullanılabilir.
    /// </summary>
    public class ProcessTrendGenerator
    {
        private readonly IMetaDataRepository _metaDataRepository;

        // DI ile repository'yi alır
        public ProcessTrendGenerator(IMetaDataRepository metaDataRepository)
        {
            _metaDataRepository = metaDataRepository;
        }

        // Yeni metot: Artık sadece sıcaklık değil, genel bir "TargetValue" trendi üretiliyor.
        public (double[] timestamps, double[] values) GenerateTheoreticalRamp(ScadaRecipe recipe, DateTime startTime, double initialValue)
        {
            var timestamps = new List<double>();
            var values = new List<double>();

            if (recipe?.Steps == null)
                return (timestamps.ToArray(), values.ToArray());

            // Başlangıç değerini al (Örn: 25 °C, 0 Bar, 50 Hz vb.)
            double lastValue = initialValue;
            DateTime currentTime = startTime;

            timestamps.Add(currentTime.ToOADate());
            values.Add(lastValue);

            // Dinamik step tanımlarını çek
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();

            foreach (var step in recipe.Steps)
            {
                // Jenerik veri erişimcisi
                var accessor = new DynamicStepDataAccessor(step.StepDataWords);
                // Kontrol word'ü index 24 (varsayılan)
                short controlWord = accessor.GetShort(24);

                foreach (var stepDef in stepDefinitions)
                {
                    // Adımın bit'i kontrol word'ünde aktif mi? VE Rampa/Gradyan tanımına uygun bir adım mı?
                    if (stepDef.UniversalName == "PROCESS_RAMP" && (controlWord & (1 << stepDef.ControlWordBit)) != 0)
                    {
                        // Parametreleri jenerik anahtarlarla bul
                        var targetParamDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "TARGET_VALUE");
                        var durationParamDef = stepDef.Parameters.FirstOrDefault(p => p.ParameterKey == "DURATION_MINUTES");

                        if (targetParamDef != null && durationParamDef != null)
                        {
                            // Target Value: (Word 3'ten okunan değer artık bir hedef değerdir)
                            double targetValue = accessor.GetShort(targetParamDef.WordIndex) / 10.0;
                            double durationMinutes = accessor.GetShort(durationParamDef.WordIndex);

                            // Eğer bir değişim adımıysa (Süre > 0 ve Hedef Değer farklıysa)
                            if (durationMinutes > 0 && Math.Abs(targetValue - lastValue) > 0.01)
                            {
                                currentTime = currentTime.AddMinutes(durationMinutes);
                                timestamps.Add(currentTime.ToOADate());
                                values.Add(targetValue);
                                lastValue = targetValue;
                            }
                        }
                    }
                }
            }

            return (timestamps.ToArray(), values.ToArray());
        }
    }
}