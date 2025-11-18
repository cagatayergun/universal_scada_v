using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks; // Task kullanımı için
using Universalscada.Core.Core; // CS0103 Hata Düzeltme: ConfigurationManager için eklendi.
using Universalscada.Core.Models; // ConsumptionTotals gibi modeller için eklendi.
using Universalscada.Models;
using Universalscada.core;
namespace Universalscada.Core.Repositories // Namespace'in doğru olduğu varsayılmıştır.
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
        // CS0103 Hata Düzeltme: ConfigurationManager kullanıldı.
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public List<ProductionReportItem> GetProductionReport(ReportFilters filters)
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
                    TIMEDIFF(b.EndTime, b.StartTime) as CycleTime,
                    b.RecipeName,
                    b.OperatorName,
                    b.CustomerNumber,
                    b.MachineAlarmDurationSeconds, 
                    b.OperatorPauseDurationSeconds,
                    b.OrderNumber,
                    b.TheoreticalCycleTimeSeconds
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

            queryBuilder.Append(" WHERE " + string.Join(" AND ", whereClauses));
            queryBuilder.Append(" ORDER BY b.StartTime DESC;");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand(queryBuilder.ToString(), connection);

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
                        reportItems.Add(new ProductionReportItem
                        {
                            MachineId = reader.GetInt32("MachineId"),
                            MachineName = reader.GetString("MachineName"),
                            BatchId = reader.GetString("BatchId"),
                            StartTime = reader.GetDateTime("StartTime"),
                            EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? DateTime.MinValue : reader.GetDateTime("EndTime"),
                            CycleTime = reader.IsDBNull(reader.GetOrdinal("CycleTime"))
                                ? "Devam Ediyor"
                                : reader.GetTimeSpan(reader.GetOrdinal("CycleTime")).ToString(@"hh\:mm\:ss"),
                            RecipeName = reader.IsDBNull(reader.GetOrdinal("RecipeName")) ? "" : reader.GetString("RecipeName"),
                            OperatorName = reader.IsDBNull(reader.GetOrdinal("OperatorName")) ? "" : reader.GetString("OperatorName"),
                            // CS0117 Hata Düzeltme: Modelde CustomerNumber ve OrderNumber kullanıldı
                            CustomerNumber = reader.IsDBNull(reader.GetOrdinal("CustomerNumber")) ? "" : reader.GetString("CustomerNumber"),
                            OrderNumber = reader.IsDBNull(reader.GetOrdinal("OrderNumber")) ? "" : reader.GetString("OrderNumber"),
                            MachineAlarmDurationSeconds = reader.IsDBNull(reader.GetOrdinal("MachineAlarmDurationSeconds")) ? 0 : reader.GetInt32("MachineAlarmDurationSeconds"),
                            OperatorPauseDurationSeconds = reader.IsDBNull(reader.GetOrdinal("OperatorPauseDurationSeconds")) ? 0 : reader.GetInt32("OperatorPauseDurationSeconds"),
                            TheoreticalCycleTimeSeconds = reader.IsDBNull(reader.GetOrdinal("TheoreticalCycleTimeSeconds")) ? 0 : reader.GetInt32("TheoreticalCycleTimeSeconds")
                        });
                    }
                }
            }
            return reportItems;
        }

        public void StartNewBatch(FullMachineStatus status)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO production_batches (MachineId, BatchId, RecipeName, OperatorName, CustomerNumber, OrderNumber, StartTime) VALUES (@MachineId, @BatchId, @RecipeName, @OperatorName, @CustomerNumber, @OrderNumber, @StartTime) ON DUPLICATE KEY UPDATE StartTime=@StartTime, EndTime=NULL;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", status.MachineId);

                // DÜZELTME Onaylandı: FullMachineStatus'a BatchId eklendiği varsayılarak düzeltme yapıldı.
                cmd.Parameters.AddWithValue("@BatchId", status.BatchNumarasi);

                cmd.Parameters.AddWithValue("@RecipeName", status.RecipeName);

                // DÜZELTME Onaylandı: Türkçe alanlar evrenselleştirildi.
                cmd.Parameters.AddWithValue("@OperatorName", status.OperatorName);
                cmd.Parameters.AddWithValue("@CustomerNumber", status.CustomerNumber);
                cmd.Parameters.AddWithValue("@OrderNumber", status.OrderNumber);

                cmd.Parameters.AddWithValue("@StartTime", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        public void EndBatch(int machineId, string batchId, FullMachineStatus finalStatus, int machineAlarmSeconds, int operatorPauseSeconds, int actualProducedQuantity, int calculatedTotalDowntimeSeconds, double theoreticalCycleTimeSeconds)
        {
            using (var connection = new MySqlConnection(_connectionString))
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

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@EndTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);

                // CS1061 Hata Düzeltme: Üretim sayacı alanları evrenselleştirildi.
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

        public void UpdateBatchSummary(int machineId, string batchId, BatchSummaryData summary)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                UPDATE production_batches 
                SET 
                    TotalMaterialFlowA = @TotalMaterialFlowA, 
                    TotalEnergyKWH = @TotalEnergyKWH,
                    TotalProcessResourceB = @TotalProcessResourceB
                WHERE 
                    MachineId = @MachineId AND BatchId = @BatchId;";

                var cmd = new MySqlCommand(query, connection);

                // DÜZELTME: BatchSummaryData'daki ConsumptionMetrics dictionary'si kullanıldı.
                cmd.Parameters.AddWithValue("@TotalMaterialFlowA", summary.GetMetricValue("TotalMaterialFlowA"));
                cmd.Parameters.AddWithValue("@TotalEnergyKWH", summary.GetMetricValue("TotalEnergyKWH"));
                cmd.Parameters.AddWithValue("@TotalProcessResourceB", summary.GetMetricValue("TotalProcessResourceB"));

                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);
                cmd.ExecuteNonQuery();
            }
        }

        public void LogAllStepDetails(int machineId, string batchId, List<ProductionStepDetail> steps)
        {
            // ... (Değişiklik yok)
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var step in steps)
                {
                    string query = @"
                        INSERT INTO production_step_logs 
                        (MachineId, BatchId, StepNumber, TheoreticalTime, WorkingTime, StopTime, DeflectionTime, LogTimestamp) 
                        VALUES 
                        (@MachineId, @BatchId, @StepNumber, @TheoreticalTime, @WorkingTime, @StopTime, @DeflectionTime, @LogTimestamp);";

                    var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                    cmd.Parameters.AddWithValue("@BatchId", batchId);
                    cmd.Parameters.AddWithValue("@StepNumber", step.StepNumber);
                    cmd.Parameters.AddWithValue("@TheoreticalTime", step.TheoreticalTime);
                    cmd.Parameters.AddWithValue("@WorkingTime", step.WorkingTime);
                    cmd.Parameters.AddWithValue("@StopTime", step.StopTime);
                    cmd.Parameters.AddWithValue("@DeflectionTime", step.DeflectionTime);
                    cmd.Parameters.AddWithValue("@LogTimestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ProductionStepDetail> GetProductionStepDetails(string batchId, int machineId)
        {
            // ... (Değişiklik yok)
            var details = new List<ProductionStepDetail>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM production_step_logs WHERE MachineId = @MachineId AND BatchId = @BatchId ORDER BY StepNumber;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        details.Add(new ProductionStepDetail
                        {
                            StepNumber = reader.GetInt32("StepNumber"),
                            StepName = reader.IsDBNull(reader.GetOrdinal("StepName")) ? "" : reader.GetString("StepName"),
                            TheoreticalTime = reader.IsDBNull(reader.GetOrdinal("TheoreticalTime")) ? "" : reader.GetString("TheoreticalTime"),
                            WorkingTime = reader.IsDBNull(reader.GetOrdinal("WorkingTime")) ? "" : reader.GetString("WorkingTime"),
                            StopTime = reader.IsDBNull(reader.GetOrdinal("StopTime")) ? "" : reader.GetString("StopTime"),
                            DeflectionTime = reader.IsDBNull(reader.GetOrdinal("DeflectionTime")) ? "" : reader.GetString("DeflectionTime")
                        });
                    }
                }
            }
            return details;
        }

        // LogResourceConsumption metodu güncellendi
        public void LogResourceConsumption(int machineId, string batchId, List<ConsumptionTotals> consumptionData)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var data in consumptionData)
                {
                    string query = @"
                        INSERT INTO production_resource_logs 
                        (MachineId, BatchId, StepNumber, MaterialName, AmountUnits) 
                        VALUES 
                        (@MachineId, @BatchId, @StepNumber, @MaterialName, @AmountUnits);";

                    var cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                    cmd.Parameters.AddWithValue("@BatchId", batchId);

                    // CS1061/CS0117 Hata Düzeltme: ConsumptionTotals'daki alanlar düzeltildi.
                    cmd.Parameters.AddWithValue("@StepNumber", data.StepNumber);
                    cmd.Parameters.AddWithValue("@MaterialName", data.MaterialName);
                    cmd.Parameters.AddWithValue("@AmountUnits", data.AmountUnits);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // GetMonthlyResourceConsumptionAsync metodu (İmza ve dönüş tipi düzeltildi, içerik basit tutuldu)
        public async Task<List<ConsumptionTotals>> GetMonthlyResourceConsumptionAsync(ReportFilters filters)
        {
            await Task.CompletedTask;
            return new List<ConsumptionTotals>();
        }

        // Batch Kaynak Tüketim Detaylarını Çekme Metodu
        public async Task<List<ConsumptionTotals>> GetBatchResourceConsumptionAsync(int machineId, string batchId)
        {
            var consumptionList = new List<ConsumptionTotals>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT StepNumber, MaterialName, AmountUnits FROM production_resource_logs WHERE MachineId = @MachineId AND BatchId = @BatchId ORDER BY StepNumber;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        consumptionList.Add(new ConsumptionTotals
                        {
                            // CS1061 Hata Düzeltme: ConsumptionTotals'daki alanlar düzeltildi.
                            StepNumber = reader.GetInt32("StepNumber"),
                            MaterialName = reader.GetString("MaterialName"),
                            AmountUnits = reader.GetInt16("AmountUnits")
                        });
                    }
                }
            }
            return consumptionList;
        }

        public DataTable GetGeneralProductionReport(DateTime startTime, DateTime endTime, List<string> machineNames)
        {
            var dt = new DataTable();
            if (machineNames == null || !machineNames.Any())
            {
                return dt;
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        m.MachineName,
                        b.BatchId,
                        b.EndTime,
                        b.TotalMaterialFlowA, 
                        b.TotalEnergyKWH,
                        b.TotalProcessResourceB
                    FROM production_batches b
                    JOIN machines m ON b.MachineId = m.Id
                    WHERE b.EndTime BETWEEN @StartTime AND @EndTime 
                    AND m.MachineName IN ({0})
                    ORDER BY m.MachineName, b.EndTime;";

                var machineParams = machineNames.Select((s, i) => "@machine" + i).ToArray();
                var formattedQuery = string.Format(query, string.Join(", ", machineParams));

                var cmd = new MySqlCommand(formattedQuery, connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                for (int i = 0; i < machineNames.Count; i++)
                {
                    cmd.Parameters.AddWithValue(machineParams[i], machineNames[i]);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        public ConsumptionTotals GetConsumptionTotalsForPeriod(DateTime startTime, DateTime endTime)
        {
            var totals = new ConsumptionTotals();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        SUM(TotalMaterialFlowA) as FlowA, 
                        SUM(TotalEnergyKWH) as Energy, 
                        SUM(TotalProcessResourceB) as ResourceB
                    FROM production_batches 
                    WHERE EndTime BETWEEN @StartTime AND @EndTime;";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // CS1061 Hata Düzeltme: ConsumptionTotals'daki alanlar düzeltildi.
                        totals.TotalMaterialFlowA = reader["FlowA"] == DBNull.Value ? 0 : reader.GetDecimal("FlowA");
                        totals.TotalEnergyKWH = reader["Energy"] == DBNull.Value ? 0 : reader.GetDecimal("Energy");
                        totals.TotalProcessResourceB = reader["ResourceB"] == DBNull.Value ? 0 : reader.GetDecimal("ResourceB");
                    }
                }
            }
            return totals;
        }

        public (DateTime? StartTime, DateTime? EndTime) GetBatchTimestamps(string batchId, int machineId)
        {
            // ... (Değişiklik yok)
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT StartTime, EndTime FROM production_batches WHERE BatchId = @BatchId AND MachineId = @MachineId;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BatchId", batchId);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var startTime = reader.GetDateTime("StartTime");
                        var endTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? (DateTime?)null : reader.GetDateTime("EndTime");
                        return (startTime, endTime);
                    }
                }
            }
            return (null, null);
        }

        public void LogSingleStepDetail(ProductionStepDetail stepDetail, int machineId, string batchId)
        {
            // ... (Değişiklik yok)
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
            INSERT INTO production_step_logs 
            (MachineId, BatchId, StepNumber, StepName, TheoreticalTime, WorkingTime, StopTime, DeflectionTime, LogTimestamp) 
            VALUES 
            (@MachineId, @BatchId, @StepNumber, @StepName, @TheoreticalTime, @WorkingTime, @StopTime, @DeflectionTime, @LogTimestamp);";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);
                cmd.Parameters.AddWithValue("@StepNumber", stepDetail.StepNumber);
                cmd.Parameters.AddWithValue("@StepName", stepDetail.StepName);
                cmd.Parameters.AddWithValue("@TheoreticalTime", stepDetail.TheoreticalTime);
                cmd.Parameters.AddWithValue("@WorkingTime", stepDetail.WorkingTime);
                cmd.Parameters.AddWithValue("@StopTime", stepDetail.StopTime);
                cmd.Parameters.AddWithValue("@DeflectionTime", stepDetail.DeflectionTime);
                cmd.Parameters.AddWithValue("@LogTimestamp", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveBatchRecipe(int machineId, string batchId, ScadaRecipe recipe)
        {
            // ... (Değişiklik yok)
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = "UPDATE production_batches SET RecipeName = @RecipeName WHERE MachineId = @MachineId AND BatchId = @BatchId;";
                        var updateCmd = new MySqlCommand(updateQuery, connection, transaction);
                        updateCmd.Parameters.AddWithValue("@RecipeName", recipe.RecipeName);
                        updateCmd.Parameters.AddWithValue("@MachineId", machineId);
                        updateCmd.Parameters.AddWithValue("@BatchId", batchId);
                        updateCmd.ExecuteNonQuery();

                        string stepQuery = "INSERT INTO batch_recipe_steps (MachineId, BatchId, StepNumber, Word0, Word1, Word2, Word3, Word4, Word5, Word6, Word7, Word8, Word9, Word10, Word11, Word12, Word13, Word14, Word15, Word16, Word17, Word18, Word19, Word20, Word24) " +
                                            "VALUES (@MachineId, @BatchId, @StepNumber, @Word0, @Word1, @Word2, @Word3, @Word4, @Word5, @Word6, @Word7, @Word8, @Word9, @Word10, @Word11, @Word12, @Word13, @Word14, @Word15, @Word16, @Word17, @Word18, @Word19, @Word20, @Word24);";

                        foreach (var step in recipe.Steps)
                        {
                            var stepCmd = new MySqlCommand(stepQuery, connection, transaction);
                            stepCmd.Parameters.AddWithValue("@MachineId", machineId);
                            stepCmd.Parameters.AddWithValue("@BatchId", batchId);
                            stepCmd.Parameters.AddWithValue("@StepNumber", step.StepNumber);
                            for (int i = 0; i <= 24; i++)
                            {
                                if (i >= 21 && i <= 23) continue;
                                stepCmd.Parameters.AddWithValue($"@Word{i}", step.StepDataWords[i]);
                            }
                            stepCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public ScadaRecipe GetBatchRecipe(int machineId, string batchId)
        {
            // ... (Değişiklik yok)
            var recipe = new ScadaRecipe();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM batch_recipe_steps WHERE MachineId = @MachineId AND BatchId = @BatchId ORDER BY StepNumber;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineId", machineId);
                cmd.Parameters.AddWithValue("@BatchId", batchId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var step = new ScadaRecipeStep
                        {
                            StepNumber = reader.GetInt32("StepNumber")
                        };

                        for (int i = 0; i <= 20; i++)
                        {
                            step.StepDataWords[i] = reader.GetInt16($"Word{i}");
                        }

                        step.StepDataWords[24] = reader.GetInt16("Word24");

                        recipe.Steps.Add(step);
                    }
                }
            }
            return recipe;
        }
    }
}