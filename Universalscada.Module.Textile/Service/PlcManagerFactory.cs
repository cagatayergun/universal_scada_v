using System;
using Universalscada.Models;
using Universalscada.Module.Textile.Services;
using Universalscada.Services; // IPlcManager burada
using Universalscada.Core.Services; // IPlcManagerFactory burada (Core projesinden)

namespace Universalscada.Module.Textile.Services
{
    /// <summary>
    /// Makine tipine göre uygun IPlcManager nesnesini oluşturan fabrika sınıfı.
    /// </summary>
    public class PlcManagerFactory : IPlcManagerFactory
    {
        // Bu metot IPlcManagerFactory arayüzünden geliyor
        public IPlcManager CreatePlcManager(Machine machine)
        {
            switch (machine.MachineType)
            {
                case "BYMakinesi":
                    return new BYMakinesiManager(machine.IpAddress, machine.Port);

                case "Kurutma Makinesi":
                    return new KurutmaMakinesiManager(machine.IpAddress, machine.Port);

                default:
                    // Bilinmeyen makine tipi için varsayılan bir manager veya hata fırlatılabilir.
                    // Şimdilik BYMakinesiManager döndürüyoruz (veya GenericModbusManager yazılabilir)
                    return new BYMakinesiManager(machine.IpAddress, machine.Port);
            }
        }

        public IPlcManager GetPlcManagerByIp(string ipAddress)
        {
            // Bu metot genellikle PollingService tarafından yönetilen bir havuzdan çekmek için kullanılır.
            // Fabrika sınıfı yeni instance üretmekten sorumludur, havuz yönetiminden değil.
            // Ancak arayüz gereği implemente etmemiz gerekirse:
            throw new NotImplementedException("Bu metot Factory üzerinden değil, PollingService üzerinden kullanılmalıdır.");
        }

        public void DisposeAll()
        {
            // Fabrika sınıfı durum tutmadığı için (stateless), dispose edecek bir şeyi yoktur.
        }
    }
}