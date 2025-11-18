// Universalscada.Core/Core/IRecipeTimeCalculator.cs (Yeni Arabirim)
using Universalscada.Models;

namespace Universalscada.Core.Core
{
    public interface IRecipeTimeCalculator
    {
        /// <summary>
        /// Bir reçetedeki adımların toplam teorik süresini saniye cinsinden hesaplar.
        /// </summary>
        double CalculateTotalTheoreticalTimeSeconds(IEnumerable<ScadaRecipeStep> steps);
    }
}