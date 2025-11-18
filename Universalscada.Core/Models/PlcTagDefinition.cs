using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Universalscada.Models; // Machine türü için (ScadaDbContext.cs'deki kullanıma göre)
namespace Universalscada.Core.Models
{
    public class PlcTagDefinition
    {
        [Key]
        public int Id { get; set; }

        // Hangi makine veya sektöre özel olduğu. 0: Genel/Universal
        public int MachineId { get; set; }

        // Hangi arayüz ekranında kullanılacağı: ProsesIzleme, ProsesKontrol, Ayarlar, Raporlar
        [Required, MaxLength(50)]
        public string ViewContext { get; set; }

        // PLC Haberleşme Adresi (Örn: DB10.DBD0, M50.0)
        [Required, MaxLength(100)]
        public string PlcAddress { get; set; }

        // Kod içinde kullanılacak benzersiz etiket adı (Örn: Sicaklik_PV)
        [Required, MaxLength(100)]
        public string TagName { get; set; }

        // Arayüzde görünecek dinamik isim (Örn: Kazan Sıcaklığı)
        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        // Ölçü Birimi (Örn: °C, Bar, RPM)
        [MaxLength(20)]
        public string Unit { get; set; }

        // Kullanıcı tarafından ayarlanabilir mi? (Proses Kontrol için önemlidir)
        public bool IsSettable { get; set; }

        // Veri Tipi (Örn: REAL, INT, BOOL)
        [MaxLength(20)]
        public string DataType { get; set; }

        // Ekranda gösterim sırası
        public int DisplayOrder { get; set; }
        // Yeni Alan: Geçmişe Kayıt (Trend)
        public bool IsHistorical { get; set; } = false;
        // İlişki
        [ForeignKey("MachineId")]
        public Machine Machine { get; set; }
        public bool IsLiveStatus { get; set; } = true;
        public int WordIndex { get; set; }
    }
}