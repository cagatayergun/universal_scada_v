// Models/ScadaRecipe.cs - KÖKLÜ DEĞİŞİKLİK
using System.Collections.Generic;
using System;

namespace Universalscada.Models
{
    public class ScadaRecipe
    {
        public int Id { get; set; }
        public string RecipeName { get; set; }

        // TargetMachineType yerine CompatibilityTags kullanın veya ProcessArea ekleyin
        // public string TargetMachineType { get; set; } // ESKİ SATIR

        /// <summary>
        /// YENİ: Bu reçetenin hangi genel proses alanına ait olduğunu belirtir (örn. Isıtma, Karıştırma, Paketleme).
        /// </summary>
        public string ProcessArea { get; set; }

        public DateTime CreationDate { get; set; }

        // ScadaRecipeStep'in de jenerikleştirilmesi önemlidir:
        // ScadaRecipeStep içindeki parametreler de DynamicStepParams veya JSON gibi jenerik bir yapıya taşınmalıdır.
        public List<ScadaRecipeStep> Steps { get; set; }

        public ScadaRecipe()
        {
            Steps = new List<ScadaRecipeStep>();
        }
    }
}