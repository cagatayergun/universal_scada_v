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

        /// <summary>
        /// Task<List<PlcTagDefinition>> dönüş tipini uygulamak için önceki düzeltme.
        /// </summary>
        public Task<List<PlcTagDefinition>> GetPlcTagDefinitionsAsync(int machineId, string machineType)
        {
            // Gerçek sorgulama mantığı buraya eklenecektir.
            return Task.FromResult(new List<PlcTagDefinition>());
        }

        /// <summary>
        /// CS0738 Hata Düzeltme: Dönüş tipi Task<PlcTagDefinition> olarak düzeltildi.
        /// </summary>
        public Task<PlcTagDefinition> SavePlcTagDefinitionAsync(PlcTagDefinition tagDefinition)
        {
            // Bu metot için PLC Tag Tanımlarını kaydetme/güncelleme mantığı uygulanmalıdır.
            // Örneğin: _context.PlcTagDefinitions.Update(tagDefinition); await _context.SaveChangesAsync();

            // Arayüzün beklediği Task<PlcTagDefinition> tipini döndürmek için
            return Task.FromResult(tagDefinition);
        }
        // Tag Yönetimi Metotları
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