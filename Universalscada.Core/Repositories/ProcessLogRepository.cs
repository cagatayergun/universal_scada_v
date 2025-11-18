using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using Universalscada.core;
using Universalscada.Models;

namespace Universalscada.Repositories
{
    public class ProcessLogRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public void LogData(FullMachineStatus status)
        {
            if (string.IsNullOrEmpty(status.BatchNumarasi) || !status.IsInRecipeMode) return;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // Tablo adı migration ile uyumlu olmalı (process_data_log vs ProcessDataLogs)
                string query = "INSERT INTO ProcessDataLogs (MachineId, BatchId, LogTimestamp, LiveTemperature, LiveWaterLevel, LiveRpm) VALUES (@MachineId, @BatchId, @LogTimestamp, @LiveTemperature, @LiveWaterLevel, @LiveRpm);";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", status.MachineId);
                cmd.Parameters.AddWithValue("@BatchId", status.BatchNumarasi);
                cmd.Parameters.AddWithValue("@LogTimestamp", DateTime.Now);
                cmd.Parameters.AddWithValue("@LiveTemperature", status.AnlikSicaklik);
                cmd.Parameters.AddWithValue("@LiveWaterLevel", status.AnlikSuSeviyesi);
                cmd.Parameters.AddWithValue("@LiveRpm", status.AnlikDevirRpm);
                cmd.ExecuteNonQuery();
            }
        }

        public List<ProcessDataPoint> GetLogsForDateRange(DateTime startTime, DateTime endTime, List<int> machineIds)
        {
            var dataPoints = new List<ProcessDataPoint>();
            if (machineIds == null || !machineIds.Any()) return dataPoints;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var queryBuilder = new StringBuilder();
                queryBuilder.Append("SELECT MachineId, LogTimestamp, LiveTemperature, LiveWaterLevel, LiveRpm FROM ProcessDataLogs ");
                queryBuilder.Append("WHERE LogTimestamp BETWEEN @StartTime AND @EndTime ");

                // IN clause in SQLite
                queryBuilder.Append($"AND MachineId IN ({string.Join(",", machineIds)}) ");
                queryBuilder.Append("ORDER BY LogTimestamp;");

                var cmd = new SqliteCommand(queryBuilder.ToString(), connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataPoints.Add(new ProcessDataPoint
                        {
                            MachineId = reader.GetInt32(0),
                            Timestamp = reader.GetDateTime(1),
                            Temperature = reader.GetDecimal(2),
                            WaterLevel = reader.GetDecimal(3),
                            Rpm = reader.GetInt32(4)
                        });
                    }
                }
            }
            return dataPoints;
        }
    }
}