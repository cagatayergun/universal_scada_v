using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universalscada.core;
using Universalscada.Core.Core;
using Universalscada.Models;

namespace Universalscada.Core.Repositories
{
    public class ReportFilters
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? MachineId { get; set; }
        public string BatchNo { get; set; }
        public string RecipeName { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string OperatorName { get; set; }
    }

    public class ProductionRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        // HATA DÜZELTME: Metot adı çoğul yapıldı (Controller ile uyumlu olması için)
        public List<ProductionReportItem> GetProductionReports(ReportFilters filters)
        {
            return GetProductionReportInternal(filters);
        }

        // İç mantık
        private List<ProductionReportItem> GetProductionReportInternal(ReportFilters filters)
        {
            var reportItems = new List<ProductionReportItem>();
            var queryBuilder = new StringBuilder();

            queryBuilder.Append(@"
                SELECT 
                    m.Id as MachineId,
                    m.MachineName,
                    b.BatchId,
                    b.StartTime,
                    b.EndTime,
                    b.RecipeName,
                    b.OperatorName,
                    b.CustomerNumber,
                    b.MachineAlarmDurationSeconds, 
                    b.OperatorPauseDurationSeconds,
                    b.OrderNumber,
                    b.TheoreticalCycleTimeSeconds,
                    b.TotalWater,
                    b.TotalElectricity,
                    b.TotalSteam,
                    b.TotalProductionCount,
                    b.DefectiveProductionCount
                FROM production_batches AS b
                JOIN machines AS m ON b.MachineId = m.Id
            ");

            var whereClauses = new List<string>();
            whereClauses.Add("b.StartTime BETWEEN @StartTime AND @EndTime");

            if (filters.MachineId.HasValue && filters.MachineId > 0) whereClauses.Add("b.MachineId = @MachineId");
            if (!string.IsNullOrEmpty(filters.BatchNo)) whereClauses.Add("b.BatchId LIKE @BatchNo");
            if (!string.IsNullOrEmpty(filters.RecipeName)) whereClauses.Add("b.RecipeName LIKE @RecipeName");
            if (!string.IsNullOrEmpty(filters.OrderNumber)) whereClauses.Add("b.OrderNumber LIKE @OrderNumber");
            if (!string.IsNullOrEmpty(filters.CustomerNumber)) whereClauses.Add("b.CustomerNumber LIKE @CustomerNumber");
            if (!string.IsNullOrEmpty(filters.OperatorName)) whereClauses.Add("b.OperatorName LIKE @OperatorName");

            if (whereClauses.Any())
                queryBuilder.Append(" WHERE " + string.Join(" AND ", whereClauses));

            queryBuilder.Append(" ORDER BY b.StartTime DESC;");

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SqliteCommand(queryBuilder.ToString(), connection);

                cmd.Parameters.AddWithValue("@StartTime", filters.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", filters.EndTime);

                if (filters.MachineId.HasValue && filters.MachineId > 0) cmd.Parameters.AddWithValue("@MachineId", filters.MachineId.Value);
                if (!string.IsNullOrEmpty(filters.BatchNo)) cmd.Parameters.AddWithValue("@BatchNo", $"%{filters.BatchNo}%");
                if (!string.IsNullOrEmpty(filters.RecipeName)) cmd.Parameters.AddWithValue("@RecipeName", $"%{filters.RecipeName}%");
                if (!string.IsNullOrEmpty(filters.OrderNumber)) cmd.Parameters.AddWithValue("@OrderNumber", $"%{filters.OrderNumber}%");
                if (!string.IsNullOrEmpty(filters.CustomerNumber)) cmd.Parameters.AddWithValue("@CustomerNumber", $"%{filters.CustomerNumber}%");
                if (!string.IsNullOrEmpty(filters.OperatorName)) cmd.Parameters.AddWithValue("@OperatorName", $"%{filters.OperatorName}%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var startTime = reader.GetDateTime(reader.GetOrdinal("StartTime"));
                        var endTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("EndTime"));

                        string cycleTimeStr = "Devam Ediyor";
                        if (endTime != DateTime.MinValue)
                        {
                            TimeSpan diff = endTime - startTime;
                            cycleTimeStr = diff.ToString(@"hh\:mm\:ss");
                        }

                        reportItems.Add(new ProductionReportItem
                        {
                            MachineId = reader.GetInt32(reader.GetOrdinal("MachineId")),
                            MachineName = reader.GetString(reader.GetOrdinal("MachineName")),
                            BatchId = reader.GetString(reader.GetOrdinal("BatchId")),
                            StartTime = startTime,
                            EndTime = endTime,
                            CycleTime = cycleTimeStr,
                            RecipeName = reader.IsDBNull(reader.GetOrdinal("RecipeName")) ? "" : reader.GetString(reader.GetOrdinal("RecipeName")),
                            OperatorName = reader.IsDBNull(reader.GetOrdinal("OperatorName")) ? "" : reader.GetString(reader.GetOrdinal("OperatorName")),
                            CustomerNumber = reader.IsDBNull(reader.GetOrdinal("CustomerNumber")) ? "" : reader.GetString(reader.GetOrdinal("CustomerNumber")),
                            OrderNumber = reader.IsDBNull(reader.GetOrdinal("OrderNumber")) ? "" : reader.GetString(reader.GetOrdinal("OrderNumber")),
                            MachineAlarmDurationSeconds = reader.IsDBNull(reader.GetOrdinal("MachineAlarmDurationSeconds")) ? 0 : reader.GetDouble(reader.GetOrdinal("MachineAlarmDurationSeconds")),
                            OperatorPauseDurationSeconds = reader.IsDBNull(reader.GetOrdinal("OperatorPauseDurationSeconds")) ? 0 : reader.GetDouble(reader.GetOrdinal("OperatorPauseDurationSeconds")),
                            TheoreticalCycleTimeSeconds = reader.IsDBNull(reader.GetOrdinal("TheoreticalCycleTimeSeconds")) ? 0 : reader.GetInt32(reader.GetOrdinal("TheoreticalCycleTimeSeconds")),
                            TotalWater = reader.IsDBNull(reader.GetOrdinal("TotalWater")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalWater")),
                            TotalElectricity = reader.IsDBNull(reader.GetOrdinal("TotalElectricity")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalElectricity")),
                            TotalSteam = reader.IsDBNull(reader.GetOrdinal("TotalSteam")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalSteam")),
                            TotalProductionCount = reader.IsDBNull(reader.GetOrdinal("TotalProductionCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalProductionCount")),
                            DefectiveProductionCount = reader.IsDBNull(reader.GetOrdinal("DefectiveProductionCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("DefectiveProductionCount"))
                        });
                    }
                }
            }
            return reportItems;
        }

        public List<ProductionReportItem> GetProductionReport(ReportFilters filters)
        {
            return GetProductionReportInternal(filters);
        }

        public void StartNewBatch(FullMachineStatus status)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO production_batches (MachineId, BatchId, RecipeName, OperatorName, CustomerNumber, OrderNumber, StartTime) 
                    VALUES (@MachineId, @BatchId, @RecipeName, @OperatorName, @CustomerNumber, @OrderNumber, @StartTime) 
                    ON CONFLICT(MachineId, BatchId) DO UPDATE SET 
                        StartTime = excluded.StartTime,
                        EndTime = NULL;";

                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", status.MachineId);
                cmd.Parameters.AddWithValue("@BatchId", status.BatchNumarasi);
                cmd.Parameters.AddWithValue("@RecipeName", status.RecipeName);
                cmd.Parameters.AddWithValue("@OperatorName", status.OperatorName ?? "");
                cmd.Parameters.AddWithValue("@CustomerNumber", status.CustomerNumber ?? "");
                cmd.Parameters.AddWithValue("@OrderNumber", status.OrderNumber ?? "");
                cmd.Parameters.AddWithValue("@StartTime", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        public void EndBatch(int machineId, string batchId, FullMachineStatus finalStatus, int machineAlarmSeconds, int operatorPauseSeconds, int actualProducedQuantity, int calculatedTotalDowntimeSeconds, double theoreticalCycleTimeSeconds)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE production_batches SET 
                        EndTime = @EndTime,
                        TotalProductionCount = @TotalProductionCount,
                        DefectiveProductionCount = @DefectiveProductionCount,
                        TotalDownTimeSeconds = @TotalDownTimeSeconds, 
                        MachineAlarmDurationSeconds = @MachineAlarmDuration,
                        OperatorPauseDurationSeconds = @OperatorPauseDuration,
                        actual_produced_quantity = @actual_produced_quantity,
                        TheoreticalCycleTimeSeconds = @TheoreticalCycleTimeSeconds
                    WHERE 
                        MachineId = @MachineId AND BatchId = @BatchId AND EndTime IS NULL;";

                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@EndTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);
                cmd.Parameters.AddWithValue("@TotalProductionCount", finalStatus.TotalUnitsProduced);
                cmd.Parameters.AddWithValue("@DefectiveProductionCount", finalStatus.DefectiveUnitsCount);
                cmd.Parameters.AddWithValue("@TotalDownTimeSeconds", calculatedTotalDowntimeSeconds);
                cmd.Parameters.AddWithValue("@MachineAlarmDuration", machineAlarmSeconds);
                cmd.Parameters.AddWithValue("@OperatorPauseDuration", operatorPauseSeconds);
                cmd.Parameters.AddWithValue("@actual_produced_quantity", actualProducedQuantity);
                cmd.Parameters.AddWithValue("@TheoreticalCycleTimeSeconds", theoreticalCycleTimeSeconds);

                cmd.ExecuteNonQuery();
            }
        }

        public ConsumptionReportDto GetBatchConsumptionSummary(string batchId)
        {
            var dto = new ConsumptionReportDto
            {
                BatchId = batchId,
                ConsumptionMetrics = new Dictionary<string, double>(),
                CurrencySymbol = "TL"
            };

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT TotalWater, TotalElectricity, TotalSteam FROM production_batches WHERE BatchId = @BatchId LIMIT 1";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@BatchId", batchId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dto.ConsumptionMetrics.Add("TotalWaterLiters", reader.IsDBNull(0) ? 0 : reader.GetDouble(0));
                        dto.ConsumptionMetrics.Add("TotalElectricityKwh", reader.IsDBNull(1) ? 0 : reader.GetDouble(1));
                        dto.ConsumptionMetrics.Add("TotalSteamKg", reader.IsDBNull(2) ? 0 : reader.GetDouble(2));
                    }
                }
            }
            return dto;
        }
    }
}