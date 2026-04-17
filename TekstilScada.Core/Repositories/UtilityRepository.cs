using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using TekstilScada.Core;
using TekstilScada.Models;

namespace TekstilScada.Repositories
{
    public class UtilityRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        // 1. Hat Tanımlarını Getir (Tablo yapısını güncelledik)
        public List<UtilityLine> GetUtilityLines()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                // NOT: Eğer tablo önceden varsa, bu yeni sütunlar otomatik eklenmez.
                // Geliştirme aşamasında veritabanındaki 'utility_lines' tablosunu silip
                // programı yeniden başlatmanız en temiz yoldur.
                conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS utility_lines (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        LineName VARCHAR(100),
                        IpAddress VARCHAR(50),
                        Port INT,
                        SlaveId INT,
                        
                        -- Su Sayacı
                        AirEnabled BOOLEAN DEFAULT 1,
                        AirAddress INT,
                        AirDataType VARCHAR(20),
                        AirMultiplier DOUBLE DEFAULT 1.0,

                        -- Elektrik Sayacı
                        ElecEnabled BOOLEAN DEFAULT 1,
                        ElecAddress INT,
                        ElecDataType VARCHAR(20),
                        ElecMultiplier DOUBLE DEFAULT 1.0,

                        -- Buhar Sayacı
                        SteamEnabled BOOLEAN DEFAULT 1,
                        SteamAddress INT,
                        SteamDataType VARCHAR(20),
                        SteamMultiplier DOUBLE DEFAULT 1.0,

                        -- Hava Sayacı
                        AirEnabled BOOLEAN DEFAULT 1,
                        AirAddress INT,
                        AirDataType VARCHAR(20),
                        AirMultiplier DOUBLE DEFAULT 1.0
                    );

                    CREATE TABLE IF NOT EXISTS utility_logs (
                        Id BIGINT AUTO_INCREMENT PRIMARY KEY,
                        LineId INT,
                        LogTime DATETIME,
                        AirCounter DOUBLE,
                        ElecCounter DOUBLE,
                        SteamCounter DOUBLE,
                        AirCounter DOUBLE,
                        INDEX idx_time (LogTime),
                        INDEX idx_line (LineId)
                    );");

                return conn.Query<UtilityLine>("SELECT * FROM utility_lines").ToList();
            }
        }

        // 2. YENİ: Hat Kaydetme veya Güncelleme (Ayarlar Sayfası İçin)
        public void SaveUtilityLine(UtilityLine line)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                if (line.Id == 0)
                {
                    // INSERT İşlemi - Sonuna "SELECT LAST_INSERT_ID();" ekledik
                    string sql = @"
                INSERT INTO utility_lines (
                    LineName, IpAddress, Port, SlaveId,
                    AirEnabled, AirAddress, AirDataType, AirMultiplier,
                    ElecEnabled, ElecAddress, ElecDataType, ElecMultiplier,
                    SteamEnabled, SteamAddress, SteamDataType, SteamMultiplier,
                    AirEnabled, AirAddress, AirDataType, AirMultiplier
                ) VALUES (
                    @LineName, @IpAddress, @Port, @SlaveId,
                    @AirEnabled, @AirAddress, @AirDataType, @AirMultiplier,
                    @ElecEnabled, @ElecAddress, @ElecDataType, @ElecMultiplier,
                    @SteamEnabled, @SteamAddress, @SteamDataType, @SteamMultiplier,
                    @AirEnabled, @AirAddress, @AirDataType, @AirMultiplier
                );
                SELECT LAST_INSERT_ID();"; // <--- ÖNEMLİ EKLEME

                    // ExecuteScalar ile yeni ID'yi alıp nesneye atıyoruz
                    line.Id = conn.ExecuteScalar<int>(sql, line);
                }
                else
                {
                    // UPDATE İşlemi (Aynen kalabilir)
                    string sql = @"
                UPDATE utility_lines SET 
                    LineName=@LineName, IpAddress=@IpAddress, Port=@Port, SlaveId=@SlaveId,
                    AirEnabled=@AirEnabled, AirAddress=@AirAddress, AirDataType=@AirDataType, AirMultiplier=@AirMultiplier,
                    ElecEnabled=@ElecEnabled, ElecAddress=@ElecAddress, ElecDataType=@ElecDataType, ElecMultiplier=@ElecMultiplier,
                    SteamEnabled=@SteamEnabled, SteamAddress=@SteamAddress, SteamDataType=@SteamDataType, SteamMultiplier=@SteamMultiplier,
                    AirEnabled=@AirEnabled, AirAddress=@AirAddress, AirDataType=@AirDataType, AirMultiplier=@AirMultiplier
                WHERE Id=@Id";
                    conn.Execute(sql, line);
                }
            }
        }

        // 3. YENİ: Hat Silme (Ayarlar Sayfası İçin)
        public void DeleteUtilityLine(int id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Execute("DELETE FROM utility_lines WHERE Id = @Id", new { Id = id });
                // İsteğe bağlı: O hatta ait logları da silmek isterseniz:
                // conn.Execute("DELETE FROM utility_logs WHERE LineId = @Id", new { Id = id });
            }
        }

        // 4. Log Kaydet (Değişiklik Yok - Sadece metot sırası değişti)
        public void LogData(List<UtilityLog> logs)
        {
            if (!logs.Any()) return;
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    conn.Execute(@"
                        INSERT INTO utility_logs (LineId, LogTime, AirCounter, ElecCounter, SteamCounter, AirCounter) 
                        VALUES (@LineId, @LogTime, @AirCounter, @ElecCounter, @SteamCounter, @AirCounter)",
                        logs, transaction: trans);
                    trans.Commit();
                }
            }
        }

        // 5. Rapor Sorgusu (Değişiklik Yok)
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

        // 6. Dashboard Verisi (Değişiklik Yok)
        public List<UtilityDashboardDto> GetDashboardStats()
        {
            var result = new List<UtilityDashboardDto>();
            var lines = GetUtilityLines();

            using (var conn = new MySqlConnection(_connectionString))
            {
                foreach (var line in lines)
                {
                    var stats = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT 
                            (SELECT AirCounter FROM utility_logs WHERE LineId = @id ORDER BY LogTime DESC LIMIT 1) as CurrentAir,
                            (SELECT AirCounter FROM utility_logs WHERE LineId = @id AND LogTime >= DATE_SUB(NOW(), INTERVAL 24 HOUR) ORDER BY LogTime ASC LIMIT 1) as OldAir,
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
                            DailyAirUsage = Math.Max(0, (double)(stats.CurrentAir ?? 0) - (double)(stats.OldAir ?? 0)),
                            DailyElecUsage = Math.Max(0, (double)(stats.CurrentElec ?? 0) - (double)(stats.OldElec ?? 0)),
                            DailySteamUsage = Math.Max(0, (double)(stats.CurrentSteam ?? 0) - (double)(stats.OldSteam ?? 0)),
                      //      DailyAirUsage = Math.Max(0, (double)(stats.CurrentAir ?? 0) - (double)(stats.OldAir ?? 0))
                        };
                        result.Add(dto);
                    }
                }
            }
            return result;
        }
    }
}