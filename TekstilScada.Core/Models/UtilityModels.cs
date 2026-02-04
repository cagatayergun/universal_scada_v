using System;
using System.Collections.Generic;

namespace TekstilScada.Models
{
    // Hattın Tanımı (Veritabanı: utility_lines)
    public class UtilityLine
    {
        public int Id { get; set; }
        public string LineName { get; set; } // Hat İsmi (Örn: Boyahane Hattı 1)
        public string IpAddress { get; set; }
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;

        // Modbus Adresleri (DWORD - Int32)
        public int WaterAddress { get; set; }
        public int ElecAddress { get; set; }
        public int SteamAddress { get; set; }
        public int AirAddress { get; set; }
    }

    // Log Verisi (Veritabanı: utility_logs)
    public class UtilityLog
    {
        public int LineId { get; set; }
        public DateTime LogTime { get; set; }
        public double WaterCounter { get; set; } // Endeks değeri
        public double ElecCounter { get; set; }
        public double SteamCounter { get; set; }
        public double AirCounter { get; set; }
    }

    // Dashboard için DTO (Son 24 Saat Özeti)
    public class UtilityDashboardDto
    {
        public int LineId { get; set; }
        public string LineName { get; set; }

        // Son 24 saatteki tüketim (Fark: Şu anki Endeks - 24s Önceki Endeks)
        public double DailyWaterUsage { get; set; }
        public double DailyElecUsage { get; set; }
        public double DailySteamUsage { get; set; }
        public double DailyAirUsage { get; set; }

        // Grafik için son 24 saatin saatlik verileri
        public List<double> WaterTrend { get; set; } = new();
        public List<double> ElecTrend { get; set; } = new();
    }
}