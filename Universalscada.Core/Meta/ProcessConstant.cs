// Universalscada.Core/Meta/ProcessConstant.cs
namespace Universalscada.Core.Meta
{
    /// <summary>
    /// Proses hesaplamaları ve genel yapılandırma için kullanılan dinamik sabit değerleri (örneğin maliyetler, zaman katsayıları) tutar.
    /// Bu tablo SQLite'da yer alacaktır.
    /// </summary>
    public class ProcessConstant
    {
        // Anahtar, birincil anahtar (Primary Key) olarak kullanılacak.
        public string Key { get; set; } // Örn: "WATER_PER_LITER_SECONDS", "DRAIN_SECONDS"

        // Sabitin sayısal değeri
        public double Value { get; set; }

        // Açıklama, kullanıcıya ne işe yaradığını anlatmak için
        public string Description { get; set; }
    }
}