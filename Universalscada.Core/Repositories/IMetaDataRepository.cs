// Universalscada.Core/Repositories/IMetaDataRepository.cs
using System.Collections.Generic;
using Universalscada.Core.Meta;
using Universalscada.Core.Models;

namespace Universalscada.Core.Repositories
{
    /// <summary>
    /// PLC programlama modelini, reçete yapısını ve proses sabitlerini
    /// veritabanından okumak için kullanılan arayüz.
    /// Tüm dinamik hesaplama ve konfigürasyon servislerinin temel bağımlılığıdır.
    /// </summary>
    public interface IMetaDataRepository
    {
        // Reçete adımı tiplerini (Su alma, Isıtma vb.) ve parametre tanımlarını getirir.
        IEnumerable<StepTypeDefinition> GetAllStepDefinitions();

        // Evrensel isme göre tek bir adım tipini ve ilişkili parametrelerini getirir.
        StepTypeDefinition GetStepDefinitionByUniversalName(string universalName);

        // Bir proses sabitinin değerini anahtarına göre getirir.
        double GetConstantValue(string key, double defaultValue = 0.0);
        Task<List<PlcTagDefinition>> GetPlcTagDefinitionsAsync(int machineId, string viewContext);
        Task<PlcTagDefinition> SavePlcTagDefinitionAsync(PlcTagDefinition tag);
        // TODO: Makineye özel PLC adres haritasını getiren metot eklenebilir.
        // MachineConfiguration GetMachineConfiguration(string machineType, string sector); 
        Task<List<PlcTagDefinition>> GetAllPlcTagsAsync(int machineId);
        Task<PlcTagDefinition> GetPlcTagByIdAsync(int id);
        Task<PlcTagDefinition> AddPlcTagAsync(PlcTagDefinition tag);
        Task UpdatePlcTagAsync(PlcTagDefinition tag);
        Task DeletePlcTagAsync(int id);
    }
}