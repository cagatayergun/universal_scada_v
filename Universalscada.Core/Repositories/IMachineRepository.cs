// Universalscada.Core/Repositories/IMachineRepository.cs
using System.Collections.Generic;
using Universalscada.Models;

namespace Universalscada.Core.Repositories
{
    /// <summary>
    /// Makine yapılandırma verilerine erişim sağlayan arayüz.
    /// Polling hizmeti ve genel yönetim için kullanılır.
    /// </summary>
    public interface IMachineRepository
    {
        /// <summary>
        /// Tanımlı ve etkinleştirilmiş tüm makineleri getirir.
        /// </summary>
        IEnumerable<Machine> GetAllMachines();

        /// <summary>
        /// Belirli bir ID'ye sahip makineyi getirir.
        /// </summary>
        Machine GetMachineById(int id);

        /// <summary>
        /// Yeni bir makineyi kaydeder veya mevcut makineyi günceller.
        /// </summary>
        void SaveMachine(Machine machine);

        /// <summary>
        /// Bir makineyi veritabanından siler.
        /// </summary>
        void DeleteMachine(int id);
    }
}