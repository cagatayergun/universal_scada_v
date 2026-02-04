using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper; // Mevcut projenizde Dapper vardı, performans için onu kullanalım
using MySql.Data.MySqlClient;
using TekstilScada.Core;
using TekstilScada.Models;

namespace TekstilScada.Repositories
{
    public class UtilityRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        // 1. Hat Tanımlarını Getir
        public List<UtilityLine> GetUtilityLines()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                // Tablo yoksa oluştur (Code-First pratikliği için)
                conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS utility_lines (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        LineName VARCHAR(100),
                        IpAddress VARCHAR(50),
                        Port INT,
                        SlaveId INT,
                        WaterAddress INT,
                        ElecAddress INT,
                        SteamAddress INT,
                        AirAddress INT
                    );
                    CREATE TABLE IF NOT EXISTS utility_logs (
                        Id BIGINT AUTO_INCREMENT PRIMARY KEY,
                        LineId INT,
                        LogTime DATETIME,
                        WaterCounter DOUBLE,
                        ElecCounter DOUBLE,
                        SteamCounter DOUBLE,
                        AirCounter DOUBLE,
                        INDEX idx_time (LogTime),
                        INDEX idx_line (LineId)
                    );");

                return conn.Query<UtilityLine>("SELECT * FROM utility_lines").ToList();
            }
        }

        // 2. Log Kaydet (10 saniyede bir çalışacak)
        public void LogData(List<UtilityLog> logs)
        {
            if (!logs.Any()) return;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    conn.Execute(@"
                        INSERT INTO utility_logs (LineId, LogTime, WaterCounter, ElecCounter, SteamCounter, AirCounter) 
                        VALUES (@LineId, @LogTime, @WaterCounter, @ElecCounter, @SteamCounter, @AirCounter)",
                        logs, transaction: trans);
                    trans.Commit();
                }
            }
        }

        // 3. Rapor Sorgusu (Tarih Aralığı)
        public List<UtilityLog> GetUtilityReport(int lineId, DateTime start, DateTime end)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                return conn.Query<UtilityLog>(@"
                    SELECT * FROM utility_logs 
                    WHERE LineId = @lineId AND LogTime BETWEEN @start AND @end 
                    ORDER BY LogTime", new { lineId, start, end }).ToList();
            }
        }

        // 4. Dashboard Verisi (Son 24 Saat Özeti)
        public List<UtilityDashboardDto> GetDashboardStats()
        {
            var result = new List<UtilityDashboardDto>();
            var lines = GetUtilityLines(); // Önce hatları al

            using (var conn = new MySqlConnection(_connectionString))
            {
                foreach (var line in lines)
                {
                    // Şu anki son değer ve 24 saat önceki ilk değer
                    var stats = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT 
                            (SELECT WaterCounter FROM utility_logs WHERE LineId = @id ORDER BY LogTime DESC LIMIT 1) as CurrentWater,
                            (SELECT WaterCounter FROM utility_logs WHERE LineId = @id AND LogTime >= DATE_SUB(NOW(), INTERVAL 24 HOUR) ORDER BY LogTime ASC LIMIT 1) as OldWater,
                            (SELECT ElecCounter FROM utility_logs WHERE LineId = @id ORDER BY LogTime DESC LIMIT 1) as CurrentElec,
                            (SELECT ElecCounter FROM utility_logs WHERE LineId = @id AND LogTime >= DATE_SUB(NOW(), INTERVAL 24 HOUR) ORDER BY LogTime ASC LIMIT 1) as OldElec,
                            (SELECT SteamCounter FROM utility_logs WHERE LineId = @id ORDER BY LogTime DESC LIMIT 1) as CurrentSteam,
                            (SELECT SteamCounter FROM utility_logs WHERE LineId = @id AND LogTime >= DATE_SUB(NOW(), INTERVAL 24 HOUR) ORDER BY LogTime ASC LIMIT 1) as OldSteam,
                            (SELECT AirCounter FROM utility_logs WHERE LineId = @id ORDER BY LogTime DESC LIMIT 1) as CurrentAir,
                            (SELECT AirCounter FROM utility_logs WHERE LineId = @id AND LogTime >= DATE_SUB(NOW(), INTERVAL 24 HOUR) ORDER BY LogTime ASC LIMIT 1) as OldAir
                        ", new { id = line.Id });

                    if (stats != null)
                    {
                        var dto = new UtilityDashboardDto
                        {
                            LineId = line.Id,
                            LineName = line.LineName,
                            // Basit Fark Alma (Sayaç sıfırlanırsa negatif çıkabilir, Math.Max ile koruyoruz)
                            DailyWaterUsage = Math.Max(0, (double)(stats.CurrentWater ?? 0) - (double)(stats.OldWater ?? 0)),
                            DailyElecUsage = Math.Max(0, (double)(stats.CurrentElec ?? 0) - (double)(stats.OldElec ?? 0)),
                            DailySteamUsage = Math.Max(0, (double)(stats.CurrentSteam ?? 0) - (double)(stats.OldSteam ?? 0)),
                            DailyAirUsage = Math.Max(0, (double)(stats.CurrentAir ?? 0) - (double)(stats.OldAir ?? 0))
                        };
                        result.Add(dto);
                    }
                }
            }
            return result;
        }
    }
}