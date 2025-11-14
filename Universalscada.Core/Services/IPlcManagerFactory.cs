// Universalscada.Core/Services/IPlcManagerFactory.cs
using Universalscada.Models;
using Universalscada.Services;

namespace Universalscada.Core.Services
{
    /// <summary>
    /// Makine tipine ve protokolüne göre uygun IPlcManager implementasyonunu oluşturan arayüz.
    /// Bu, PLC bağlantı mantığının evrenselleştirilmesi için hayati önem taşır.
    /// </summary>
    public interface IPlcManagerFactory
    {
        /// <summary>
        /// Belirtilen makine konfigürasyonuna uygun PLC Manager örneği oluşturur.
        /// </summary>
        /// <param name="machine">Makine yapılandırmasını içeren model.</param>
        /// <returns>IPlcManager arayüzünü uygulayan, makineye özel bir örnek.</returns>
        IPlcManager CreatePlcManager(Machine machine);

        /// <summary>
        /// Mevcut bağlantı havuzundan bir IPlcManager örneğini IpAddress'e göre bulur.
        /// </summary>
        IPlcManager GetPlcManagerByIp(string ipAddress);

        /// <summary>
        /// Tüm aktif PLC Manager örneklerini temizler.
        /// </summary>
        void DisposeAll();
    }
}