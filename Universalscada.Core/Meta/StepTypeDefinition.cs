// Universalscada.Core/Meta/StepTypeDefinition.cs
namespace Universalscada.Core.Meta
{
    /// <summary>
    /// Proses adımının türünü (örneğin: Su Alma, Isıtma) tanımlar.
    /// Bu bilgiler veritabanında (SQLite) tutulacaktır.
    /// </summary>
    public class StepTypeDefinition
    {
        public int Id { get; set; }
        // Evrensel Ad: Örneğin, "WATER_TRANSFER", "HEAT_RAMP"
        public string UniversalName { get; set; }
        // Kullanıcı arayüzünde görüntülenecek yerelleştirilebilir Ad: "Su Alma"
        public string DisplayNameKey { get; set; }
        // PLC'deki kontrol word'ünde bu adımı aktive eden bit (örneğin: 1. bit)
        public int ControlWordBit { get; set; }
        // Bu adıma ait süre/maliyet hesaplaması için kullanılacak sınıf anahtarı
        public string CalculationServiceKey { get; set; }

        // Adıma ait parametrelerin listesi (EF Core için navigasyon özelliği)
        public ICollection<StepParameterDefinition> Parameters { get; set; } = new List<StepParameterDefinition>();
    }

    /// <summary>
    /// Bir StepTypeDefinition'a ait tüm parametrelerin PLC word adreslerini tanımlar.
    /// </summary>
    public class StepParameterDefinition
    {
        public int Id { get; set; }
        public int StepTypeDefinitionId { get; set; }
        // Parametrenin anahtarı (Örn: QUANTITY_LITERS, TARGET_TEMP)
        public string ParameterKey { get; set; }
        // Ham veri dizisindeki (short[25]) karşılık gelen word indexi (Örn: 1, 3, 4)
        public int WordIndex { get; set; }
        // Veri Tipi (Örn: short, byte, string). UI için kritik.
        public string DataType { get; set; }

        // UI'da kullanılacak ek meta veriler (Birim, Min/Max Değer, UI Kontrol Tipi)
        public string Unit { get; set; }

        // EF Core için navigasyon özelliği
        public StepTypeDefinition StepTypeDefinition { get; set; }
    }
}