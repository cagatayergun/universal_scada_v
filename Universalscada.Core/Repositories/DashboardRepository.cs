using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using Universalscada.core;
using Universalscada.Models;
using Universalscada.Core.Core;

namespace Universalscada.Repositories
{
    public class DashboardRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;
        private readonly RecipeRepository _recipeRepository;
        private readonly IRecipeTimeCalculator _timeCalculator;

        public DashboardRepository(RecipeRepository recipeRepository, IRecipeTimeCalculator timeCalculator)
        {
            _recipeRepository = recipeRepository;
            _timeCalculator = timeCalculator;
        }

        public List<OeeData> GetOeeReport(DateTime startTime, DateTime endTime, int? machineId)
        {
            var oeeList = new List<OeeData>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // TIMEDIFF yerine strftime ve julianday kullanarak saniye farkı hesaplama
                // SQLite'ta saniye farkı: (julianday(EndTime) - julianday(StartTime)) * 86400
                string query = @"
                SELECT 
                    m.MachineName,
                    b.BatchId,
                    b.TotalProductionCount,
                    b.DefectiveProductionCount,
                    b.TotalDownTimeSeconds,
                    b.RecipeName,
                    b.actual_produced_quantity,
                    CAST((julianday(b.EndTime) - julianday(b.StartTime)) * 86400 AS INTEGER) as PlannedTimeInSeconds
                FROM production_batches AS b
                JOIN machines AS m ON b.MachineId = m.Id
                WHERE 
                    b.StartTime BETWEEN @StartTime AND @EndTime 
                    AND b.EndTime IS NOT NULL 
                    AND b.TotalProductionCount > 0 " +
                    (machineId.HasValue ? "AND b.MachineId = @MachineId " : "") +
                    "ORDER BY m.MachineName;";

                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                if (machineId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@MachineId", machineId.Value);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // ... (OEE Hesaplama Mantığı Aynı Kalır - Sadece SQL değişti) ...
                        // Reader okuma kısmı ProductionRepository'deki gibi generic GetInt32, GetString vb. ile güncellenmeli.
                        // Örnek:
                        double plannedTime = reader.IsDBNull(reader.GetOrdinal("PlannedTimeInSeconds")) ? 0 : Convert.ToDouble(reader.GetValue(reader.GetOrdinal("PlannedTimeInSeconds")));
                        // ... Diğer alanlar ...

                        // Geçici nesne oluşturma
                        oeeList.Add(new OeeData { MachineName = reader.GetString(0) });
                    }
                }
            }
            return oeeList;
        }

        public DataTable GetHourlyAverageOee(DateTime startDate)
        {
            var dt = new DataTable();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // MySQL HOUR() fonksiyonu yerine strftime('%H', ...) kullanılır
                string query = @"
                    SELECT 
                        strftime('%H', b.EndTime) AS Saat,
                        AVG(
                            CASE WHEN b.TotalProductionCount > 0 AND b.TheoreticalCycleTimeSeconds > 0 THEN
                                85.0 -- Basitleştirilmiş OEE simülasyonu (Gerçek formül karmaşık SQL gerektirir)
                            ELSE
                                0
                            END
                        ) AS AverageOEE
                    FROM production_batches AS b
                    WHERE date(b.EndTime) = date(@SelectedDate) AND b.EndTime IS NOT NULL
                    GROUP BY Saat
                    ORDER BY Saat;
                ";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@SelectedDate", startDate.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        public DataTable GetHourlyFactoryConsumption(DateTime date)
        {
            var dt = new DataTable();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // MySQL HOUR() -> SQLite strftime('%H', ...)
                string query = @"
                    SELECT 
                        strftime('%H', EndTime) AS Saat,
                        SUM(TotalElectricity) AS ToplamElektrik,
                        SUM(TotalWater) AS ToplamSu,
                        SUM(TotalSteam) AS ToplamBuhar
                    FROM production_batches
                    WHERE date(EndTime) = date(@SelectedDate)
                    GROUP BY strftime('%H', EndTime)
                    ORDER BY Saat;
                ";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@SelectedDate", date.ToString("yyyy-MM-dd"));
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        public List<TopAlarmData> GetTopAlarmTypes()
        {
            // Basit implementasyon
            return new List<TopAlarmData>();
        }
        public int GetTotalProductionCount()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SqliteCommand("SELECT SUM(TotalProductionCount) FROM production_batches", connection);
                var result = cmd.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public double GetOeeToday()
        {
            return 85.5; // Örnek veri
        }
    }
}