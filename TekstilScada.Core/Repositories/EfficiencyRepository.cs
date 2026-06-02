using MySql.Data.MySqlClient;
using Dapper; // Dapper performans için en iyisidir
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telemetry.Core;
using Telemetry.Models;

namespace Telemetry.Repositories
{
    public class EfficiencyRepository
    {
        // AlarmRepository tarzında: Bağlantı bilgisini doğrudan AppConfig'den alır
        private readonly string _connectionString = AppConfig.ConnectionString;
        public string MachineName { get; set; } // Tabloda ID yerine isim göstermek için
        public EfficiencyRepository()
        {
            EnsureTablesCreated();
            EnsureDowntimeDefinitionsTableCreated();
        }

        private void EnsureTablesCreated()
        {
            using var conn = new MySqlConnection(_connectionString);
            string createLogTable = @"
                CREATE TABLE IF NOT EXISTS machine_efficiency_logs (
                    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
                    MachineId INT NOT NULL,
                    MachineSubType VARCHAR(100),
                    State VARCHAR(20) NOT NULL,
                    StartTime DATETIME NOT NULL,
                    EndTime DATETIME,
                    DurationSeconds DOUBLE DEFAULT 0,
                    RecipeName VARCHAR(255),
                    Reason1 VARCHAR(255),
                    Reason2 VARCHAR(255),
                    Reason3 VARCHAR(255),
                    Reason4 VARCHAR(255),
                    Reason5 VARCHAR(255),
                    INDEX idx_machine_time (MachineId, StartTime),
                    INDEX idx_subtype_time (MachineSubType, StartTime)
                ) ENGINE=InnoDB;";
            conn.Execute(createLogTable);
        }

        public void EnsureDowntimeDefinitionsTableCreated()
        {
            using var conn = new MySqlConnection(_connectionString);
            string sql = @"
                CREATE TABLE IF NOT EXISTS downtime_reason_definitions (
                    BitIndex INT PRIMARY KEY,
                    ReasonText VARCHAR(255) NOT NULL
                ) ENGINE=InnoDB;";
            conn.Execute(sql);
        }

        public async Task<long> StartNewLogAsync(EfficiencyLog log)
        {
            using var conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT INTO machine_efficiency_logs 
                           (MachineId, MachineSubType, State, StartTime, RecipeName, Reason1, Reason2, Reason3, Reason4, Reason5) 
                           VALUES (@MachineId, @MachineSubType, @State, @StartTime, @RecipeName, @Reason1, @Reason2, @Reason3, @Reason4, @Reason5);
                           SELECT LAST_INSERT_ID();";
            return await conn.ExecuteScalarAsync<long>(sql, log);
        }

        public async Task EndLogAsync(long id, DateTime endTime)
        {
            using var conn = new MySqlConnection(_connectionString);
            string sql = @"UPDATE machine_efficiency_logs 
                           SET EndTime = @endTime, 
                               DurationSeconds = TIMESTAMPDIFF(SECOND, StartTime, @endTime) 
                           WHERE Id = @id";
            await conn.ExecuteAsync(sql, new { id, endTime });
        }

        public async Task SaveDowntimeDefinitionsAsync(IEnumerable<dynamic> definitions)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var trans = await conn.BeginTransactionAsync();
            try
            {
                await conn.ExecuteAsync("TRUNCATE TABLE downtime_reason_definitions", transaction: trans);
                string sql = "INSERT INTO downtime_reason_definitions (BitIndex, ReasonText) VALUES (@BitIndex, @ReasonText)";
                await conn.ExecuteAsync(sql, definitions, transaction: trans);
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public Dictionary<int, string> GetDowntimeDefinitions()
        {
            using var conn = new MySqlConnection(_connectionString);
            string sql = "SELECT BitIndex, ReasonText FROM downtime_reason_definitions";
            return conn.Query(sql).ToDictionary(
                row => (int)row.BitIndex,
                row => (string)row.ReasonText
            );
        }

        // Hata aldığın eksik metod
        public async Task<IEnumerable<EfficiencyLog>> GetEfficiencyReportAsync(DateTime start, DateTime end, int? machineId = null, string subType = null)
        {
            using var conn = new MySqlConnection(_connectionString);
            // JOIN ile makineler tablosundan MachineName çekiyoruz
            string sql = @"SELECT l.*, m.MachineName 
                   FROM machine_efficiency_logs l
                   LEFT JOIN machines m ON l.MachineId = m.Id
                   WHERE l.StartTime >= @start AND l.StartTime <= @end";

            if (machineId.HasValue) sql += " AND l.MachineId = @machineId";
            if (!string.IsNullOrEmpty(subType)) sql += " AND l.MachineSubType = @subType";

            sql += " ORDER BY l.StartTime ASC"; // Zaman çizelgesi için ASC (artan) daha iyi

            return await conn.QueryAsync<EfficiencyLog>(sql, new { start, end, machineId, subType });
        }
    }
}