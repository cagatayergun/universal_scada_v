using System.Collections.Generic;
using Universalscada.Core.Meta; // StepTypeDefinition ve StepParameterDefinition modelleri için

namespace Universalscada.Core.Repositories
{
    /// <summary>
    /// Projenin evrenselleştirilmesi için gerekli dinamik proses meta verilerini (adım tipleri, parametre eşlemeleri, sabitler)
    /// veritabanından (SQLite) okumaktan sorumlu arayüz.
    /// </summary>
    public interface IMetaDataRepository
    {
        /// <summary>
        /// Tüm tanımlı StepTypeDefinition nesnelerini (Su Alma, Isıtma vb.) getirir.
        /// </summary>
        IEnumerable<StepTypeDefinition> GetAllStepDefinitions();

        /// <summary>
        /// Belirtilen anahtara sahip proses sabitinin değerini getirir.
        /// Örn: "WATER_PER_LITER_SECONDS" için değeri getirir.
        /// </summary>
        double GetConstantValue(string key, double defaultValue);
    }
}