using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telemetry.Models;
using Telemetry.Core;

namespace Telemetry.Repositories
{
    public class DashboardRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;
        private readonly RecipeRepository _recipeRepository;
        private static bool _isDatabaseInitialized = false;
        private static readonly object _initLock = new object();

        public DashboardRepository(RecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;

            if (!_isDatabaseInitialized)
            {
                lock (_initLock)
                {
                    if (!_isDatabaseInitialized)
                    {
                        EnsureDatabaseArchitecture();
                        _isDatabaseInitialized = true;
                    }
                }
            }
        }

        private void EnsureDatabaseArchitecture()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS manual_mode_log_summary (
                            Zaman DATETIME NOT NULL,
                            MachineId INT NOT NULL,
                            ToplamElektrik DOUBLE DEFAULT 0,
                            ToplamSu DOUBLE DEFAULT 0,
                            ToplamBuhar DOUBLE DEFAULT 0,
                            PRIMARY KEY (Zaman, MachineId)
                        );";

                    using (var cmd = new MySqlCommand(createTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    string checkEmptyQuery = "SELECT COUNT(*) FROM manual_mode_log_summary LIMIT 1;";
                    long recordCount = 0;
                    using (var cmd = new MySqlCommand(checkEmptyQuery, connection))
                    {
                        recordCount = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    if (recordCount == 0)
                    {
                        Task.Run(() => RunHistoricalMigrationInBackground());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"VERITABANI OTOMATIK KURULUM HATASI: {ex.Message}");
            }
        }

        private void RunHistoricalMigrationInBackground()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string migrationQuery = @"
                        INSERT INTO manual_mode_log_summary (Zaman, MachineId, ToplamElektrik, ToplamSu, ToplamBuhar)
                        SELECT 
                            DATE_FORMAT(LogTimestamp, '%Y-%m-%d %H:00:00') as Zaman,
                            MachineId,
                            (MAX(LiveElectricity) - MIN(LiveElectricity)) as ToplamElektrik,
                            (MAX(LiveWaterLevel) - MIN(LiveWaterLevel)) as ToplamSu,
                            (MAX(LiveSteam) - MIN(LiveSteam)) as ToplamBuhar
                        FROM manual_mode_log
                        GROUP BY MachineId, DATE_FORMAT(LogTimestamp, '%Y-%m-%d %H:00:00')
                        ON DUPLICATE KEY UPDATE ToplamElektrik=ToplamElektrik;";

                    using (var cmd = new MySqlCommand(migrationQuery, connection))
                    {
                        cmd.CommandTimeout = 600;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"ARKA PLAN GEÇMİŞ TAŞIMA HATASI: {ex.Message}");
            }
        }

        public void MaintainHourlySummary()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO manual_mode_log_summary (Zaman, MachineId, ToplamElektrik, ToplamSu, ToplamBuhar)
                        SELECT 
                            DATE_FORMAT(LogTimestamp, '%Y-%m-%d %H:00:00') as Zaman,
                            MachineId,
                            (MAX(LiveElectricity) - MIN(LiveElectricity)) as ToplamElektrik,
                            (MAX(LiveWaterLevel) - MIN(LiveWaterLevel)) as ToplamSu,
                            (MAX(LiveSteam) - MIN(LiveSteam)) as ToplamBuhar
                        FROM manual_mode_log
                        WHERE LogTimestamp >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
                        GROUP BY MachineId, DATE_FORMAT(LogTimestamp, '%Y-%m-%d %H:00:00')
                        ON DUPLICATE KEY UPDATE 
                            ToplamElektrik = VALUES(ToplamElektrik),
                            ToplamSu = VALUES(ToplamSu),
                            ToplamBuhar = VALUES(ToplamBuhar);";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.CommandTimeout = 30;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"MAINTAIN SUMMARY ERROR: {ex.Message}");
            }
        }

        // ESKİ PARAMETRE DÜZENİNE GERİ DÖNDÜ (DateTime date)
        public DataTable GetHourlyFactoryConsumption(DateTime date)
        {
            MaintainHourlySummary();

            var dt = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        T.Saat,
                        SUM(IFNULL(T.ToplamElektrik, 0)) AS ToplamElektrik,
                        SUM(IFNULL(T.ToplamSu, 0)) AS ToplamSu,
                        SUM(IFNULL(T.ToplamBuhar, 0)) AS ToplamBuhar
                    FROM (
                        SELECT 
                            HOUR(EndTime) AS Saat,
                            SUM(TotalElectricity) AS ToplamElektrik,
                            SUM(TotalWater) AS ToplamSu,
                            SUM(TotalSteam) AS ToplamBuhar
                        FROM production_batches
                        WHERE EndTime >= @StartDate AND EndTime < @EndDate
                        GROUP BY HOUR(EndTime)

                        UNION ALL

                        SELECT
                            HOUR(Zaman) as Saat,
                            SUM(ToplamElektrik) as ToplamElektrik,
                            SUM(ToplamSu) as ToplamSu,
                            SUM(ToplamBuhar) as ToplamBuhar
                        FROM manual_mode_log_summary
                        WHERE Zaman >= @StartDate AND Zaman < @EndDate
                        GROUP BY HOUR(Zaman)
                    ) AS T
                    GROUP BY T.Saat
                    ORDER BY T.Saat ASC;";

                var cmd = new MySqlCommand(query, connection);
                cmd.CommandTimeout = 30;
                cmd.Parameters.AddWithValue("@StartDate", date.Date);
                cmd.Parameters.AddWithValue("@EndDate", date.Date.AddDays(1));

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        // ESKİ PARAMETRE DÜZENİNE GERİ DÖNDÜ (DateTime startDate)
        public DataTable GetHourlyAverageOee(DateTime startDate)
        {
            var dt = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        HOUR(b.EndTime) AS Saat,
                        AVG(
                            CASE WHEN b.TotalProductionCount > 0 AND b.TheoreticalCycleTimeSeconds > 0 AND TIME_TO_SEC(TIMEDIFF(b.EndTime, b.StartTime)) > 0 THEN
                                (TIME_TO_SEC(TIMEDIFF(b.EndTime, b.StartTime)) - b.TotalDownTimeSeconds) / TIME_TO_SEC(TIMEDIFF(b.EndTime, b.StartTime)) *
                                (b.TheoreticalCycleTimeSeconds / TIME_TO_SEC(TIMEDIFF(b.EndTime, b.StartTime)) - b.TotalDownTimeSeconds) *
                                ( (b.TotalProductionCount - b.DefectiveProductionCount) / b.TotalProductionCount ) * 10000
                            ELSE
                                0
                            END
                        ) AS AverageOEE
                    FROM production_batches AS b
                    WHERE b.EndTime >= @StartDate AND b.EndTime < @EndDate
                    GROUP BY Saat
                    ORDER BY Saat ASC;
                ";
                var cmd = new MySqlCommand(query, connection);
                cmd.CommandTimeout = 60;
                cmd.Parameters.AddWithValue("@StartDate", startDate.Date);
                cmd.Parameters.AddWithValue("@EndDate", startDate.Date.AddDays(1));

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        public List<OeeData> GetOeeReport(DateTime startTime, DateTime endTime, int? machineId)
        {
            var oeeList = new List<OeeData>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT  
                        m.MachineName,
                        b.BatchId,
                        b.TotalProductionCount,
                        b.DefectiveProductionCount,
                        b.TotalDownTimeSeconds,
                        b.RecipeName,
                        b.actual_produced_quantity,
                        TIME_TO_SEC(TIMEDIFF(b.EndTime, b.StartTime)) as PlannedTimeInSeconds
                    FROM production_batches AS b
                    JOIN machines AS m ON b.MachineId = m.Id
                    WHERE  
                        b.StartTime BETWEEN @StartTime AND @EndTime 
                        AND b.EndTime IS NOT NULL 
                        AND b.TotalProductionCount > 0 " +
                        (machineId.HasValue ? "AND b.MachineId = @MachineId " : "") +
                        "ORDER BY m.MachineName;";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                if (machineId.HasValue) cmd.Parameters.AddWithValue("@MachineId", machineId.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double plannedTime = reader.IsDBNull(reader.GetOrdinal("PlannedTimeInSeconds")) ? 0 : reader.GetDouble("PlannedTimeInSeconds");
                        double downTime = reader.IsDBNull(reader.GetOrdinal("TotalDownTimeSeconds")) ? 0 : reader.GetDouble("TotalDownTimeSeconds");
                        double runTime = plannedTime > downTime ? plannedTime - downTime : 0;
                        int defectiveCount = reader.IsDBNull(reader.GetOrdinal("DefectiveProductionCount")) ? 0 : reader.GetInt32("DefectiveProductionCount");
                        string recipeName = reader.GetString("RecipeName");
                        int actualQuantity = reader.IsDBNull(reader.GetOrdinal("actual_produced_quantity")) ? 0 : reader.GetInt32("actual_produced_quantity");

                        int totalPiecesProduced = actualQuantity;
                        int goodCount = totalPiecesProduced - defectiveCount;

                        double availability = (plannedTime > 0) ? (runTime / plannedTime) * 100 : 0;
                        double performance = 0;
                        if (runTime > 0 && totalPiecesProduced > 0 && !string.IsNullOrEmpty(recipeName))
                        {
                            var recipe = _recipeRepository.GetRecipeByName(recipeName);
                            if (recipe != null)
                            {
                                double totalTheoreticalTimeSeconds = RecipeAnalysis.CalculateTotalTheoreticalTimeSeconds(recipe);
                                if (totalTheoreticalTimeSeconds > 0) performance = (totalTheoreticalTimeSeconds / runTime) * 100;
                            }
                        }

                        double quality = (totalPiecesProduced > 0) ? ((double)goodCount / totalPiecesProduced) * 100 : 0;
                        double oee = (availability * performance * quality) / 10000;

                        oeeList.Add(new OeeData
                        {
                            MachineName = reader.GetString("MachineName"),
                            BatchId = reader.GetString("BatchId"),
                            Availability = Math.Max(0, Math.Round(availability, 2)),
                            Performance = Math.Max(0, Math.Round(performance, 2)),
                            Quality = Math.Max(0, Math.Round(quality, 2)),
                            OEE = Math.Max(0, Math.Round(oee, 2))
                        });
                    }
                }
            }
            return oeeList;
        }

        public DataTable GetTop5ConsumingMachines(DateTime date, string consumptionType)
        {
            var dt = new DataTable();
            string consumptionColumn = consumptionType.ToLower() switch
            {
                "su" => "TotalWater",
                "buhar" => "TotalSteam",
                _ => "TotalElectricity"
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = $@"
                    SELECT  
                        m.MachineName,
                        SUM({consumptionColumn}) AS ToplamTuketim
                    FROM production_batches b
                    JOIN machines m ON b.MachineId = m.Id
                    WHERE b.EndTime >= @StartDate AND b.EndTime < @EndDate
                    GROUP BY m.MachineName
                    ORDER BY ToplamTuketim DESC
                    LIMIT 5;";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartDate", date.Date);
                cmd.Parameters.AddWithValue("@EndDate", date.Date.AddDays(1));
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }
    }
}