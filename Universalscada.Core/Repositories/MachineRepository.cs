// Universalscada.Core/Repositories/MachineRepository.cs
using System.Collections.Generic;
using System.Linq;
using Universalscada.Core.Core; // ScadaDbContext için
using Universalscada.Models;

namespace Universalscada.Core.Repositories
{
    public class MachineRepository : IMachineRepository
    {
        private readonly ScadaDbContext _context;

        public MachineRepository(ScadaDbContext context)
        {
            _context = context;
        }

        // GÜNCELLEME: Tüm makineleri getirir (Lisans kontrolü ve silme için gereklidir)
        // Hata CS0738'i çözmek için: IEnumerable<Machine> döndürmesi sağlanır.
        public IEnumerable<Machine> GetAllMachines()
        {
            // Lisans kontrolü için tüm makineleri getirir (Enabled veya Disabled)
            return _context.Machines.ToList();
        }

        // ... (Mevcut metot: Bu metodun List<Machine> döndürmesi gereklidir)
        public List<Machine> GetAllEnabledMachines()
        {
            return _context.Machines.Where(m => m.IsEnabled).ToList();
        }

        public Machine GetMachineById(int id)
        {
            return _context.Machines.Find(id);
        }

        public void SaveMachine(Machine machine)
        {
            if (machine.Id == 0)
            {
                _context.Machines.Add(machine);
            }
            else
            {
                _context.Machines.Update(machine);
            }
            _context.SaveChanges();
        }

        public void DeleteMachine(int id)
        {
            var machine = _context.Machines.Find(id);
            if (machine != null)
            {
                _context.Machines.Remove(machine);
                _context.SaveChanges();
            }
        }
    }
}