// Universalscada.Core/Models/FullMachineStatus.cs - KÖKLÜ DEĞİŞİKLİK
using System.Collections.Generic;
using System.Linq; // Dictionary kullanmak için eklendi

namespace Universalscada.Models
{
    // Yeni bir yardımcı model oluşturalım:
    public class LiveValue
    {
        public string Key { get; set; } // Örn: "ANLIK_SU_SEVIYESI"
        public object Value { get; set; } // Değer (short, int, bool vb. olabilir)
        public string Unit { get; set; } // Örn: "Litre", "°C"
        public string DisplayName { get; set; } // Örn: "Anlık Su Seviyesi"
    }

    /// <summary>
    /// Bir makinenin tüm anlık durumunu barındıran evrensel veri taşıma nesnesi.
    /// Domaine özgü özellikler yerine dinamik anahtar/değer çiftlerini kullanır.
    /// </summary>
    public class FullMachineStatus
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public ConnectionStatus ConnectionState { get; set; }

        // YENİ: Makineye özel tüm anlık proses verileri ve durum bayrakları bu koleksiyonda tutulur.
        // IPlcManager'dan gelen ham veri (short[]) bu yapıya dönüştürülmelidir.
        public Dictionary<string, LiveValue> LiveDataPoints { get; set; } = new Dictionary<string, LiveValue>();

        // YENİ: Alarm ve Temel Bilgiler Jenerikleştirildi
        public bool HasActiveAlarm { get; set; }
        public int ActiveAlarmNumber { get; set; }
        public string ActiveAlarmText { get; set; }
        public bool IsInRecipeMode { get; set; }
        public string BatchNumarasi { get; set; }
        // Temel Operasyonel Durumlar - Çoğu SCADA için ortaktır.
        public bool IsMachineInProduction { get; set; }
        public bool IsPaused { get; set; }
        public string RecipeName { get; set; }
        public short AktifAdimNo { get; set; }
        public string AktifAdimAdi { get; set; }
        // EKLENDİ: ProductionRepository tarafından kullanılan alanlar
        public string OperatorName { get; set; }
        public string CustomerNumber { get; set; }
        public string OrderNumber { get; set; }
        public int TotalUnitsProduced { get; set; }
        public int DefectiveUnitsCount { get; set; }

        // EKLENDİ: ProcessLogRepository tarafından hala kullanılan eski sensör değerleri (CS1061 Hatalarını Çözmek İçin)
        public short AnlikSicaklik { get; set; }
        public short AnlikSuSeviyesi { get; set; }
        public short AnlikDevirRpm { get; set; }
        public short SuMiktari { get; set; }
        public short ElektrikHarcama { get; set; }
        public short BuharHarcama { get; set; }

        // KALDIRILMASI GEREKEN ESKİ ALANLARIN YENİDEN TANIMLANMASI İÇİN YORUM SATIRI:
        // Eski: public short AnlikSuSeviyesi { get; set; } -> LiveDataPoints["WATER_LEVEL"]
        // Eski: public short AnlikSicaklik { get; set; } -> LiveDataPoints["TEMPERATURE_PV"]
        // Eski: public string MakineTipi { get; set; } -> Machine sınıfına taşındı.
        // ... Diğer tüm domain-specific özellikler kaldırılmalıdır.
        public short AktifAdimTipiWordu { get; set; }
        public string OperatorIsmi { get; set; }
        public string SiparisNumarasi { get; set; }
        public string MusteriNumarasi { get; set; }
        public short CalismaSuresiDakika { get; set; }
        public int TotalDownTimeSeconds { get; set; }
        public short TotalProductionCount { get; set; }
        public short DefectiveProductionCount { get; set; }
        public short ActualQuantityProduction { get; set; }
        public short[] AktifAdimDataWords { get; set; }
        public bool manuel_status { get; set; }
        
        public FullMachineStatus()
        {
            // İhtiyaç olursa başlangıçta ortak/temel noktalar eklenebilir.
        }
    }

    // ConnectionStatus enum'ı yerinde kalabilir.
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectionLost
    }
}