// Models/Machine.cs - KÖKLÜ DEĞİŞİKLİK
namespace Universalscada.Models
{
    /// <summary>
    /// Evrensel SCADA için jenerik makine tanımı.
    /// Farklı sektörlere uyum sağlamak için esnek yapılandırılabilir alanlar eklendi.
    /// </summary>
    public class Machine
    {
        public int Id { get; set; }
        public string MachineUserDefinedId { get; set; }
        public string MachineName { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        // YENİ: Makinenin ait olduğu sektörü (örn. Kimya, Tekstil, Metal) belirtir.
        public string Sector { get; set; }

        public string MachineType { get; set; } // Makine ana tipi (örn. Pompa, Isıtıcı, Reaktor)
        public string MachineSubType { get; set; } // Makine alt tipi

        public bool IsEnabled { get; set; }
        public string VncAddress { get; set; }
        public string VncPassword { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }

        // YENİ: Makineye özgü tüm PLC adresleri, HMI ayarları veya diğer özel yapılandırmaları tutan esnek alan.
        // Bu alan sayesinde MachineManager.cs'deki switch-case yapılarından kurtulunabilir.
        public string MachineConfigurationJson { get; set; }

        public string DisplayInfo => $"{MachineName} - ({MachineUserDefinedId})";
    }
}