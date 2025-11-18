// Dosya: Universalscada.Core/Repositories/MetaDataRepository.cs - DÜZELTİLMİŞ VERSİYON

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Task için gerekli
using System; // NotImplementedException için
using Universalscada.Core.Core; // ScadaDbContext için
using Universalscada.Core.Meta;
using Universalscada.Core.Models;

namespace Universalscada.Core.Repositories
{
    public class MetaDataRepository : IMetaDataRepository
    {
        private readonly ScadaDbContext _context;

        public MetaDataRepository(ScadaDbContext context)
        {
            _context = context;
        }

        public IEnumerable<StepTypeDefinition> GetAllStepDefinitions()
        {
            // İlgili parametre tanımlarını (WordIndex, DataType vb.) yüklemek için Include kullanılır.
            return _context.StepTypeDefinitions
                .Include(s => s.Parameters)
                .AsNoTracking()
                .ToList();
        }

        public StepTypeDefinition GetStepDefinitionByUniversalName(string universalName)
        {
            return _context.StepTypeDefinitions
                .Include(s => s.Parameters)
                .AsNoTracking()
                .FirstOrDefault(s => s.UniversalName.ToLower() == universalName.ToLower());
        }

        public double GetConstantValue(string key, double defaultValue = 0.0)
        {
            var constant = _context.ProcessConstants
                .AsNoTracking()
                .FirstOrDefault(c => c.Key.ToLower() == key.ToLower());

            if (constant != null && double.TryParse(constant.Value.ToString(), out double result))
            {
                return result;
            }

            return defaultValue;
        }

        // === PLC TAG YÖNETİMİ METOTLARI DÜZELTMELERİ ===

        /// <summary>
        /// Belirtilen makine ID'sine ait tüm PLC Tag Tanımlarını asenkron olarak getirir.
        /// </summary>
        public async Task<List<PlcTagDefinition>> GetPlcTagDefinitionsAsync(int machineId, string machineType)
        {
            // Bu metot, GetAllPlcTagsAsync metodunu çağırarak iş mantığını yeniden kullanır.
            // machineType, gelecekte özel filtreleme için kullanılabilir.
            return await GetAllPlcTagsAsync(machineId);
        }

        /// <summary>
        /// Yeni bir PLC Tag Tanımını kaydeder veya mevcut olanı günceller.
        /// </summary>
        public async Task<PlcTagDefinition> SavePlcTagDefinitionAsync(PlcTagDefinition tagDefinition)
        {
            if (tagDefinition.Id == 0)
            {
                // Yeni kayıt (Add)
                return await AddPlcTagAsync(tagDefinition);
            }
            else
            {
                // Mevcut kayıt (Update)
                await UpdatePlcTagAsync(tagDefinition);
                return tagDefinition;
            }
        }

        // --- Diğer Tag Yönetimi Metotları (Tamamlanmış Hali) ---

        public async Task<List<PlcTagDefinition>> GetAllPlcTagsAsync(int machineId)
        {
            return await _context.PlcTagDefinitions
                                 .Where(t => t.MachineId == machineId)
                                 .ToListAsync();
        }

        public async Task<PlcTagDefinition> GetPlcTagByIdAsync(int id)
        {
            return await _context.PlcTagDefinitions.FindAsync(id);
        }

        public async Task<PlcTagDefinition> AddPlcTagAsync(PlcTagDefinition tag)
        {
            // Aynı MachineId ve TagName kombinasyonunun benzersizliğini kontrol etmek iyi bir pratik olabilir (Unique Index ScadaDbContext'te tanımlı).
            _context.PlcTagDefinitions.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task UpdatePlcTagAsync(PlcTagDefinition tag)
        {
            _context.PlcTagDefinitions.Update(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePlcTagAsync(int id)
        {
            var tag = await _context.PlcTagDefinitions.FindAsync(id);
            if (tag != null)
            {
                _context.PlcTagDefinitions.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }
    }
}