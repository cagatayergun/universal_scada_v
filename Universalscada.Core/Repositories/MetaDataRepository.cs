// Universalscada.Core/Repositories/MetaDataRepository.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // DBSet ve LINQ metodları için
using Universalscada.Core.Meta; // Meta veri modelleri için

namespace Universalscada.Core.Repositories
{
    /// <summary>
    /// Evrensel SCADA projesinin dinamik meta verilerine (adım tipleri, parametre eşleşmeleri, sabitler) 
    /// SQLite veritabanı üzerinden EF Core ile erişimi sağlar.
    /// </summary>
    public class MetaDataRepository : IMetaDataRepository
    {
        // ScadaDbContext, SQLite bağlantımızı temsil eder.
        private readonly ScadaDbContext _context;

        // Bağımlılık Enjeksiyonu (DI) ile DbContext'i alıyoruz
        public MetaDataRepository(ScadaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm tanımlı StepTypeDefinition nesnelerini, ilişkili StepParameterDefinition'lar ile birlikte getirir.
        /// </summary>
        public IEnumerable<StepTypeDefinition> GetAllStepDefinitions()
        {
            // .Include(s => s.Parameters) kritik öneme sahiptir.
            // Bu, StepTypeDefinition çekilirken, o tipe ait tüm Word Index ve Key eşleşmelerini (StepParameterDefinition) 
            // tek bir sorgu ile (JOIN) getirmeyi sağlar.
            return _context.StepTypeDefinitions
                .Include(s => s.Parameters)
                .AsNoTracking() // Sadece okuma amaçlı olduğu için performansı artırır.
                .ToList();
        }

        /// <summary>
        /// Belirtilen anahtara sahip ProcessConstant değerini getirir.
        /// </summary>
        public double GetConstantValue(string key, double defaultValue)
        {
            // Eğer anahtar bulunamazsa, varsayılan değeri (defaultValue) döndürür.
            return _context.ProcessConstants
                .AsNoTracking()
                .FirstOrDefault(c => c.Key == key)?
                .Value ?? defaultValue;
        }
    }
}