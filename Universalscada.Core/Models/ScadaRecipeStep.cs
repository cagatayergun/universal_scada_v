// Universalscada.Core/Models/ScadaRecipeStep.cs - KÖKLÜ DEĞİŞİKLİK
namespace Universalscada.Models
{
    public class ScadaRecipeStep
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int StepNumber { get; set; }
        // Reçete adımı için özelleştirilmiş isim
        public string CustomName { get; set; }

        // Adım içindeki dinamik parametreler (JSON olarak saklanabilir)
        // Örn: [{"TagName": "Sicaklik_SP", "Value": 120.0}, {"TagName": "Bekleme_Suresi", "Value": 30}]
        public string StepParametersJson { get; set; }

        // Reçete adımı ile ilgili özel dökümantasyon/notlar
        public string StepDocumentContent { get; set; }
        /// <summary>
        /// Adım verilerini ham olarak tutar. Boyutu, kullanılan adım tipine ve
        /// makineye göre dinamik olarak belirlenir.
        /// </summary>
        public short[] StepDataWords { get; set; }

        public ScadaRecipeStep()
        {
            // Eski: StepDataWords = new short[25];
            // YENİ: Boyut kısıtlamasını kaldırmak için, bu constructor boş bırakıldı.
            // DataWords array'i, adım oluşturulurken dinamik olarak boyutlandırılmalıdır.
        }

        /// <summary>
        /// Reçeteyi dış sistemlere aktarırken/kaydederken kullanılacak metot.
        /// </summary>
        public bool IsStepDataValid()
        {
            return StepDataWords != null && StepDataWords.Length > 0;
        }
    }
}