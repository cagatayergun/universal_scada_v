// File: TekstilScada.Core/Services/LiveStepAnalyzer.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.Core.Services
{
    public class LiveStepAnalyzer
    {
        private ScadaRecipe _recipe; // readonly kaldırıldı, güncellenebilir olmalı
        private readonly ProductionRepository _productionRepository;
        private int _currentStepNumber = 0;
        private DateTime _currentStepStartTime;
        private DateTime? _currentPauseStartTime;
        private double _currentStepPauseSeconds = 0;

        private int _pendingStepNumber = 0;
        private DateTime? _pendingStepTimestamp = null;
        private const int StepReadDelaySeconds = 2;
        private const double SECONDS_PER_LITER = 0.5; // Sabit eklendi

        public List<ProductionStepDetail> AnalyzedSteps { get; }
        public ScadaRecipe Recipe { get; private set; }
        public DateTime CurrentStepStartTime { get; private set; }

        public LiveStepAnalyzer(ScadaRecipe recipe, ProductionRepository productionRepository)
        {
            _recipe = recipe;
            this.Recipe = recipe;
            _productionRepository = productionRepository;
            AnalyzedSteps = new List<ProductionStepDetail>();
            CurrentStepStartTime = DateTime.Now;
        }

        // YENİ: Reçete değişirse analyzer'ı güncellemek için
        public void UpdateRecipe(ScadaRecipe newRecipe)
        {
            _recipe = newRecipe;
            Recipe = newRecipe;
        }

        // YENİ: Dinamik Toplam Süre Hesaplama (Geçmiş Gerçek + Gelecek Teorik)
        public double RecalculateTotalDuration(int currentStepNo)
        {
            double totalSeconds = 0;

            // 1. Tamamlanmış adımların GERÇEKLEŞEN sürelerini topla
            foreach (var step in AnalyzedSteps.Where(s => s.WorkingTime != "Processing..."))
            {
                if (TimeSpan.TryParse(step.WorkingTime, out TimeSpan duration))
                {
                    totalSeconds += duration.TotalSeconds;
                }
            }

            // 2. Şu anki ve sonraki adımların TEORİK sürelerini topla
            // Not: AnalyzedSteps içinde şu anki adım da "Processing..." olarak var, onu atlayıp
            // reçetedeki teorik karşılığını alacağız.
            var remainingSteps = _recipe.Steps.Where(s => s.StepNumber >= currentStepNo);
            foreach (var step in remainingSteps)
            {
                totalSeconds += CalculateStepSeconds(step);
            }

            return totalSeconds > 0 ? totalSeconds : 1; // 0'a bölünme hatasını önle
        }

        // YENİ: Tamamlanan adımların toplam süresini verir (Progress Bar Payı İçin)
        public double GetCompletedStepsDuration()
        {
            double totalSeconds = 0;
            foreach (var step in AnalyzedSteps.Where(s => s.WorkingTime != "Processing..."))
            {
                if (TimeSpan.TryParse(step.WorkingTime, out TimeSpan duration))
                {
                    totalSeconds += duration.TotalSeconds;
                }
            }
            return totalSeconds;
        }

        public bool ProcessData(FullMachineStatus status)
        {
            bool hasStepChanged = false;

            // Pause/Resume mantığı
            if (status.IsPaused && !_currentPauseStartTime.HasValue)
            {
                _currentPauseStartTime = DateTime.Now;
            }
            else if (!status.IsPaused && _currentPauseStartTime.HasValue)
            {
                _currentStepPauseSeconds += (DateTime.Now - _currentPauseStartTime.Value).TotalSeconds;
                _currentPauseStartTime = null;
            }

            // Adım değişimi algılandığında
            if (status.AktifAdimNo != _pendingStepNumber)
            {
                _pendingStepNumber = status.AktifAdimNo;
                _pendingStepTimestamp = DateTime.Now;
            }

            // Kararlı yeni adım (3 saniye kuralı)
            if (status.AktifAdimTipiWordu != 0 && _pendingStepNumber != 0 && _pendingStepNumber == status.AktifAdimNo && (DateTime.Now - _pendingStepTimestamp.Value).TotalSeconds >= StepReadDelaySeconds)
            {
                if (_currentStepNumber != status.AktifAdimNo)
                {
                    if (_currentStepNumber > 0)
                    {
                        FinalizeStep(_currentStepNumber, status.BatchNumarasi, status.MachineId);
                    }

                    HandleSkippedSteps(status, _currentStepNumber + 1, status.AktifAdimNo);
                    StartNewStep(status);

                    _currentStepNumber = status.AktifAdimNo;
                    hasStepChanged = true;
                }

                _pendingStepNumber = 0;
                _pendingStepTimestamp = null;
            }
            else if (_pendingStepNumber != status.AktifAdimNo)
            {
                _pendingStepNumber = 0;
                _pendingStepTimestamp = null;
            }

            return hasStepChanged;
        }

        public void FinalizeStep(int stepNumber, string batchId, int machineId)
        {
            var stepToFinalize = AnalyzedSteps.LastOrDefault(s => s.StepNumber == stepNumber && s.WorkingTime == "Processing...");
            if (stepToFinalize == null) return;

            TimeSpan workingTime = DateTime.Now - CurrentStepStartTime;
            stepToFinalize.WorkingTime = workingTime.ToString(@"hh\:mm\:ss");

            if (_currentPauseStartTime.HasValue)
            {
                _currentStepPauseSeconds += (DateTime.Now - _currentPauseStartTime.Value).TotalSeconds;
                _currentPauseStartTime = null;
            }
            stepToFinalize.StopTime = TimeSpan.FromSeconds(_currentStepPauseSeconds).ToString(@"hh\:mm\:ss");

            TimeSpan theoreticalTime;
            TimeSpan.TryParse(stepToFinalize.TheoreticalTime, out theoreticalTime);

            TimeSpan actualWorkTime = workingTime - TimeSpan.FromSeconds(_currentStepPauseSeconds);
            TimeSpan deflection = actualWorkTime - theoreticalTime;

            string sign = deflection.TotalSeconds >= 0 ? "+" : "";
            stepToFinalize.DeflectionTime = $"{sign}{deflection:hh\\:mm\\:ss}";
        }

        public void StartNewStep(FullMachineStatus status)
        {
            CurrentStepStartTime = DateTime.Now;
            _currentPauseStartTime = null;
            _currentStepPauseSeconds = 0;

            AnalyzedSteps.Add(new ProductionStepDetail
            {
                StepNumber = status.AktifAdimNo,
                StepName = GetStepTypeName(status.AktifAdimTipiWordu),
                TheoreticalTime = CalculateTheoreticalTime(status.AktifAdimDataWords),
                WorkingTime = "Processing...",
                StopTime = "00:00:00",
                DeflectionTime = ""
            });

            // Kimyasal loglama mantığı...
            if ((status.AktifAdimTipiWordu & 8) != 0)
            {
                try
                {
                    var dozajParams = new DozajParams(status.AktifAdimDataWords);
                    if (!string.IsNullOrEmpty(dozajParams.Kimyasal) && dozajParams.DozajLitre > 0)
                    {
                        var consumptionData = new List<ChemicalConsumptionData>
                        {
                            new ChemicalConsumptionData
                            {
                                StepNumber = status.AktifAdimNo,
                                ChemicalName = dozajParams.Kimyasal,
                                AmountLiters = dozajParams.DozajLitre
                            }
                        };
                        _productionRepository.LogChemicalConsumption(status.MachineId, status.BatchNumarasi, consumptionData);
                    }
                }
                catch { }
            }
        }

        private void HandleSkippedSteps(FullMachineStatus status, int fromStep, int toStep)
        {
            for (int i = fromStep; i < toStep; i++)
            {
                var recipeStep = _recipe.Steps.FirstOrDefault(s => s.StepNumber == i);
                if (recipeStep != null && recipeStep.StepDataWords[24] != 0)
                {
                    string skippedStepName = GetStepTypeName(recipeStep.StepDataWords[24]) + " (Skipped)";
                    AnalyzedSteps.Add(new ProductionStepDetail
                    {
                        StepNumber = i,
                        StepName = skippedStepName,
                        TheoreticalTime = CalculateTheoreticalTime(recipeStep), // TimeSpan string döner
                        WorkingTime = "00:00:00",
                        StopTime = "00:00:00",
                        DeflectionTime = ""
                    });
                }
            }
        }

        public ProductionStepDetail GetLastCompletedStep()
        {
            return AnalyzedSteps.LastOrDefault(s => s.WorkingTime != "Processing...");
        }

        // YENİ: Saniye cinsinden hesaplayan metot (Logic reuse)
        private double CalculateStepSeconds(ScadaRecipeStep step)
        {
            var parallelDurations = new List<double>();
            short controlWord = step.StepDataWords[24];

            if ((controlWord & 1) != 0) parallelDurations.Add(new SuAlmaParams(step.StepDataWords).MiktarLitre * SECONDS_PER_LITER);
            if ((controlWord & 8) != 0)
            {
                var dozajParams = new DozajParams(step.StepDataWords);
                double dozajSuresi = 0;
                if (dozajParams.AnaTankMakSu || dozajParams.AnaTankTemizSu) { dozajSuresi += 60; }
                dozajSuresi += dozajParams.CozmeSure;
                if (dozajParams.Tank1Dozaj) { dozajSuresi += dozajParams.DozajSure; }
                parallelDurations.Add(dozajSuresi);
            }
            if ((controlWord & 2) != 0) parallelDurations.Add(new IsitmaParams(step.StepDataWords).Sure * 60);
            if ((controlWord & 4) != 0) parallelDurations.Add(new CalismaParams(step.StepDataWords).CalismaSuresi * 60);
            if ((controlWord & 16) != 0) parallelDurations.Add(120);
            if ((controlWord & 32) != 0) parallelDurations.Add(new SikmaParams(step.StepDataWords).SikmaSure * 60);

            return parallelDurations.Any() ? parallelDurations.Max() : 0;
        }

        // Eski metot artık yeni double metodunu kullanıyor (DRY Prensibi)
        private string CalculateTheoreticalTime(ScadaRecipeStep step)
        {
            double seconds = CalculateStepSeconds(step);
            return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
        }

        private string CalculateTheoreticalTime(short[] stepDataWords)
        {
            // Veri wordlerinden geçici bir step oluşturup hesaplatıyoruz
            var tempStep = new ScadaRecipeStep { StepDataWords = stepDataWords };
            return CalculateTheoreticalTime(tempStep);
        }

        private string GetStepTypeName(short controlWord)
        {
            if (controlWord == 0) return "Undefined Step";
            var stepTypes = new List<string>();
            // TODO: Bu stringler localization servisinden çekilmeli
            if ((controlWord & 1) != 0) stepTypes.Add("Take Water");
            if ((controlWord & 2) != 0) stepTypes.Add("Heating");
            if ((controlWord & 4) != 0) stepTypes.Add("Working");
            if ((controlWord & 8) != 0) stepTypes.Add("Dosing");
            if ((controlWord & 16) != 0) stepTypes.Add("Drain");
            if ((controlWord & 32) != 0) stepTypes.Add("Extraction");
            if ((controlWord & 64) != 0) stepTypes.Add("Humidity Working");
            if ((controlWord & 128) != 0) stepTypes.Add("Timed Working");
            if ((controlWord & 256) != 0) stepTypes.Add("Humidity/Timed Working");
            if ((controlWord & 512) != 0) stepTypes.Add("Cooling");

            return stepTypes.Any() ? string.Join(" + ", stepTypes) : "Waiting....";
        }
        public void SyncActiveStepParameters(short[] liveDataWords, int currentStepNo)
        {
            if (liveDataWords == null || liveDataWords.Length == 0) return;

            // Şu anki adımı bul
            var step = _recipe.Steps.FirstOrDefault(s => s.StepNumber == currentStepNo);

            // Eğer adım bulunduysa ve veriler farklıysa güncelle
            if (step != null && step.StepDataWords != null)
            {
                // Veri değişmiş mi diye kontrol et (Gereksiz işlem yapmamak için)
                if (!step.StepDataWords.SequenceEqual(liveDataWords))
                {
                    // Değişiklik var! PLC'den gelen güncel değerleri hafızaya kopyala.
                    // Bu sayede süre/sıcaklık değişirse hesaplamalar hemen düzelir.
                    Array.Copy(liveDataWords, step.StepDataWords, Math.Min(liveDataWords.Length, step.StepDataWords.Length));
                }
            }
        }
    }
}