using System;
using System.Collections.Generic;

namespace TekstilScada.Models
{
    // Hattın Tanımı (Veritabanı: utility_lines)
    public class UtilityLine
    {
        public int Id { get; set; }
        public string LineName { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;

        // -- SU SAYACI --
        public bool AirEnabled { get; set; } = true;
        public int AirAddress { get; set; }
        public string AirDataType { get; set; } = "Float"; // Word, DWord, Float, Int32
        public double AirMultiplier { get; set; } = 1.0; // Virgül kaydırma için (Örn: 0.1)

        // -- ELEKTRİK SAYACI --
        public bool ElecEnabled { get; set; } = true;
        public int ElecAddress { get; set; }
        public string ElecDataType { get; set; } = "Float";
        public double ElecMultiplier { get; set; } = 1.0;

        // -- BUHAR SAYACI --
        public bool SteamEnabled { get; set; } = true;
        public int SteamAddress { get; set; }
        public string SteamDataType { get; set; } = "Float";
        public double SteamMultiplier { get; set; } = 1.0;

        // -- HAVA SAYACI --
      //  public bool AirEnabled { get; set; } = true;
      //  public int AirAddress { get; set; }
      //  public string AirDataType { get; set; } = "Float";
      //  public double AirMultiplier { get; set; } = 1.0;
    }

    // Log Verisi (Veritabanı: utility_logs)
    public class UtilityLog
    {
        public int LineId { get; set; }
        public DateTime LogTime { get; set; }
        public double AirCounter { get; set; } // Endeks değeri
        public double ElecCounter { get; set; }
        public double SteamCounter { get; set; }
      // public double AirCounter { get; set; }
    }

    // Dashboard için DTO (Son 24 Saat Özeti)
    public class UtilityDashboardDto
    {
        public int LineId { get; set; }
        public string LineName { get; set; }

        // Son 24 saatteki tüketim (Fark: Şu anki Endeks - 24s Önceki Endeks)
        public double DailyAirUsage { get; set; }
        public double DailyElecUsage { get; set; }
        public double DailySteamUsage { get; set; }
      //  public double DailyAirUsage { get; set; }

        // Grafik için son 24 saatin saatlik verileri
        public List<double> AirTrend { get; set; } = new();
        public List<double> ElecTrend { get; set; } = new();
    }
}