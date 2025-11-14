// Universalscada.Core/Repositories/MetaDataRepository.cs
using System.Collections.Generic;
using System.Linq;
using Universalscada.Core.Core; // ScadaDbContext için
using Universalscada.Core.Meta;
using Microsoft.EntityFrameworkCore;

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
    }
}