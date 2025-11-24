using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper; // Performanslı okuma için Dapper eklendi
using TekstilScada.Core;
using TekstilScada.Models;

namespace TekstilScada.Repositories
{
    public class ProcessLogRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        // --- BULK INSERT (YAZMA) İŞLEMLERİ ---

        public void LogBulkData(List<FullMachineStatus> statusList)
        {
            if (statusList == null || !statusList.Any()) return;

            // 1000 Makine için StringBuilder'ın sürekli resize olmasını engellemek adına
            // tahmini bir kapasite (RowLength * Count) veriyoruz.
            var queryBuilder = new StringBuilder("INSERT INTO process_data_log (MachineId, BatchId, LogTimestamp, LiveTemperature, LiveWaterLevel, LiveRpm) VALUES ", statusList.Count * 100);

            var parameters = new DynamicParameters(); // Dapper parametreleri veya MySqlParameter kullanılabilir. Burada manuel yönetim daha kontrollü.
            var mySqlParams = new List<MySqlParameter>();

            for (int i = 0; i < statusList.Count; i++)
            {
                var s = statusList[i];
                // Her satırı tek tek ekliyoruz
                queryBuilder.Append($"(@m{i}, @b{i}, @t{i}, @temp{i}, @wl{i}, @rpm{i}),");

                mySqlParams.Add(new MySqlParameter($"@m{i}", s.MachineId));
                mySqlParams.Add(new MySqlParameter($"@b{i}", s.BatchNumarasi));
                mySqlParams.Add(new MySqlParameter($"@t{i}", DateTime.Now));
                mySqlParams.Add(new MySqlParameter($"@temp{i}", s.AnlikSicaklik));
                mySqlParams.Add(new MySqlParameter($"@wl{i}", s.AnlikSuSeviyesi));
                mySqlParams.Add(new MySqlParameter($"@rpm{i}", s.AnlikDevirRpm));
            }

            // Sondaki virgülü silip noktalı virgül koyuyoruz
            queryBuilder.Length--;
            queryBuilder.Append(";");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(queryBuilder.ToString(), connection, transaction))
                        {
                            // 1000 makine * 6 parametre = 6000 parametre. MySQL limiti ~65k'dır, güvenli.
                            cmd.Parameters.AddRange(mySqlParams.ToArray());

                            // Büyük veri paketleri için timeout süresini artırıyoruz (Varsayılan 30sn -> 60sn)
                            cmd.CommandTimeout = 60;

                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"[BulkLog Error] {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public void LogBulkManualData(List<FullMachineStatus> statusList)
        {
            if (statusList == null || !statusList.Any()) return;

            var queryBuilder = new StringBuilder("INSERT INTO manual_mode_log (MachineId, LogTimestamp, LiveTemperature, TotalWater, LiveWaterLevel, LiveRpm, LiveElectricity, LiveSteam) VALUES ", statusList.Count * 120);
            var mySqlParams = new List<MySqlParameter>();

            for (int i = 0; i < statusList.Count; i++)
            {
                var s = statusList[i];
                queryBuilder.Append($"(@m{i}, @t{i}, @temp{i}, @tw{i}, @wl{i}, @rpm{i}, @elec{i}, @stm{i}),");

                mySqlParams.Add(new MySqlParameter($"@m{i}", s.MachineId));
                mySqlParams.Add(new MySqlParameter($"@t{i}", DateTime.Now));
                mySqlParams.Add(new MySqlParameter($"@temp{i}", s.AnlikSicaklik));
                mySqlParams.Add(new MySqlParameter($"@tw{i}", s.SuMiktari));
                mySqlParams.Add(new MySqlParameter($"@wl{i}", s.AnlikSuSeviyesi));
                mySqlParams.Add(new MySqlParameter($"@rpm{i}", s.AnlikDevirRpm));
                mySqlParams.Add(new MySqlParameter($"@elec{i}", s.ElektrikHarcama));
                mySqlParams.Add(new MySqlParameter($"@stm{i}", s.BuharHarcama));
            }

            queryBuilder.Length--;
            queryBuilder.Append(";");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(queryBuilder.ToString(), connection, transaction))
                        {
                            cmd.Parameters.AddRange(mySqlParams.ToArray());
                            cmd.CommandTimeout = 60;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"[BulkManualLog Error] {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // --- DAPPER İLE OKUMA İŞLEMLERİ (OPTİMİZE EDİLDİ) ---

        public List<ProcessDataPoint> GetLogsForBatch(int machineId, string batchId, DateTime? startTime = null, DateTime? endTime = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = new StringBuilder(@"
                    SELECT 
                        LogTimestamp as Timestamp, 
                        LiveTemperature as Temperature, 
                        LiveWaterLevel as WaterLevel, 
                        LiveRpm as Rpm 
                    FROM process_data_log 
                    WHERE MachineId = @MachineId ");

                var p = new DynamicParameters();
                p.Add("@MachineId", machineId);

                if (!string.IsNullOrEmpty(batchId))
                {
                    sql.Append("AND BatchId = @BatchId ");
                    p.Add("@BatchId", batchId);
                }
                if (startTime.HasValue)
                {
                    sql.Append("AND LogTimestamp >= @StartTime ");
                    p.Add("@StartTime", startTime);
                }
                if (endTime.HasValue)
                {
                    sql.Append("AND LogTimestamp <= @EndTime ");
                    p.Add("@EndTime", endTime);
                }
                sql.Append("ORDER BY LogTimestamp;");

                // Dapper ile tek satırda mapping ve listeleme
                return connection.Query<ProcessDataPoint>(sql.ToString(), p).ToList();
            }
        }

        public List<ProcessDataPoint> GetLogsForDateRange(int machineId, DateTime startTime, DateTime endTime)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        LogTimestamp as Timestamp, 
                        LiveTemperature as Temperature, 
                        LiveWaterLevel as WaterLevel, 
                        LiveRpm as Rpm 
                    FROM process_data_log 
                    WHERE MachineId = @MachineId 
                    AND LogTimestamp BETWEEN @StartTime AND @EndTime 
                    ORDER BY LogTimestamp;";

                return connection.Query<ProcessDataPoint>(sql, new { MachineId = machineId, StartTime = startTime, EndTime = endTime }).ToList();
            }
        }

        // Dapper ile çoklu makine sorgusu
        public List<ProcessDataPoint> GetLogsForDateRange(DateTime startTime, DateTime endTime, List<int> machineIds)
        {
            if (machineIds == null || !machineIds.Any()) return new List<ProcessDataPoint>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                // Dapper "IN" sorgusunu otomatik halleder (List<int> verdiğimizde)
                string sql = @"
                    SELECT 
                        MachineId, 
                        LogTimestamp as Timestamp, 
                        LiveTemperature as Temperature, 
                        LiveWaterLevel as WaterLevel, 
                        LiveRpm as Rpm 
                    FROM process_data_log 
                    WHERE LogTimestamp BETWEEN @StartTime AND @EndTime 
                    AND MachineId IN @MachineIds 
                    ORDER BY LogTimestamp;";

                return connection.Query<ProcessDataPoint>(sql, new { StartTime = startTime, EndTime = endTime, MachineIds = machineIds }).ToList();
            }
        }

        // --- MANUEL RAPORLAMA (OPTİMİZE EDİLDİ) ---

        public List<ProcessDataManuel> GetManualLogs1(int machineId, DateTime startTime, DateTime endTime)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        LogTimestamp as Timestamp, 
                        TotalWater as totalwatermanuel, 
                        LiveElectricity as totalelectritymanuel, 
                        LiveSteam as totalsteammanuel 
                    FROM manual_mode_log 
                    WHERE MachineId = @MachineId 
                    AND LogTimestamp BETWEEN @StartTime AND @EndTime 
                    ORDER BY LogTimestamp;";

                return connection.Query<ProcessDataManuel>(sql, new { MachineId = machineId, StartTime = startTime, EndTime = endTime }).ToList();
            }
        }

        public List<ProcessDataPoint> GetManualLogs(int machineId, DateTime startTime, DateTime endTime)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        LogTimestamp as Timestamp, 
                        LiveTemperature as Temperature, 
                        LiveWaterLevel as WaterLevel, 
                        LiveRpm as Rpm 
                    FROM manual_mode_log 
                    WHERE MachineId = @MachineId 
                    AND LogTimestamp BETWEEN @StartTime AND @EndTime 
                    ORDER BY LogTimestamp;";

                return connection.Query<ProcessDataPoint>(sql, new { MachineId = machineId, StartTime = startTime, EndTime = endTime }).ToList();
            }
        }

        // Düzeltilmiş Manuel Tüketim Özeti Mantığı
        public ManualConsumptionSummary GetManualConsumptionSummary(int machineId, string machineName, DateTime startTime, DateTime endTime)
        {
            var dataPoints = GetManualLogs(machineId, startTime, endTime);
            var dataPoints1 = GetManualLogs1(machineId, startTime, endTime);

            if (dataPoints == null || !dataPoints.Any() || dataPoints1 == null || !dataPoints1.Any())
            {
                // Veri yoksa boş ama güvenli bir nesne dön
                return new ManualConsumptionSummary
                {
                    Makine = machineName,
                    RaporAraligi = $"{startTime:dd.MM.yy HH:mm} - {endTime:dd.MM.yy HH:mm}",
                    ToplamManuelSure = "00:00:00",
                    OrtalamaSicaklik = 0,
                    OrtalamaDevir = 0,
                    ToplamSuTuketimi_Litre = 0,
                    ToplamElektrikTuketimi_kW = 0,
                    ToplamBuharTuketimi_kg = 0
                };
            }

            // Yardımcı Metot: Kümülatif artan sayaçların toplam tüketimini hesaplar.
            // Sayaç sıfırlanırsa (reset), önceki maksimum değeri toplama ekler.
            decimal CalculateConsumption(List<ProcessDataManuel> data, Func<ProcessDataManuel, decimal> selector)
            {
                if (data.Count == 0) return 0;

                decimal totalConsumption = 0;
                decimal previousValue = selector(data[0]);

                for (int i = 1; i < data.Count; i++)
                {
                    decimal currentValue = selector(data[i]);

                    if (currentValue >= previousValue)
                    {
                        // Sayaç artıyor, normal durum. Farkı alıyoruz.
                        // Not: Eğer sayaç kümülatif değil anlık ise bu mantık değişmelidir. 
                        // Buradaki mantık: Sayaç kümülatif artıyor (örn: su sayacı).
                        totalConsumption += (currentValue - previousValue);
                    }
                    else
                    {
                        // Sayaç düşmüş (Resetlenmiş). 
                        // Önceki değer o ana kadarki tüketime dahildi, zaten ekledik.
                        // Şimdi yeni başlangıç değerini (currentValue) tüketim kabul ediyoruz (eğer 0'dan başladıysa).
                        totalConsumption += currentValue;
                    }
                    previousValue = currentValue;
                }
                return totalConsumption;
            }

            // Eğer veriler "Toplam Tüketim" değil de "Anlık Değer" ise SumPeaks yerine doğrudan Average veya Sum kullanılmalıdır.
            // Ancak TotalWater gibi alanlar genelde kümülatiftir.

            var summary = new ManualConsumptionSummary
            {
                Makine = machineName,
                RaporAraligi = $"{startTime:dd.MM.yy HH:mm} - {endTime:dd.MM.yy HH:mm}",
                ToplamManuelSure = TimeSpan.FromSeconds(dataPoints.Count * 5).ToString(@"hh\:mm\:ss"), // 5 saniyelik periyot varsayımı
                OrtalamaSicaklik = dataPoints.Average(p => (double)p.Temperature/10.0), // /10.0 gerekip gerekmediğini kontrol edin
                OrtalamaDevir = dataPoints.Average(p => p.Rpm),

                // Kümülatif hesaplama
                ToplamSuTuketimi_Litre = CalculateConsumption(dataPoints1, p => p.totalwatermanuel),
                ToplamElektrikTuketimi_kW = CalculateConsumption(dataPoints1, p => p.totalelectritymanuel),
                ToplamBuharTuketimi_kg = CalculateConsumption(dataPoints1, p => p.totalsteammanuel)
            };

            return summary;
        }

        // --- HELPER CLASSES ---
        public class ProcessDataManuel
        {
            public int MachineId { get; set; }
            public DateTime Timestamp { get; set; }
            public decimal totalwatermanuel { get; set; }
            public decimal totalelectritymanuel { get; set; }
            public decimal totalsteammanuel { get; set; }
        }
        // Grafik için veri noktası modeli
        public class ProcessDataPoint
        {
            public int MachineId { get; set; }
            public DateTime Timestamp { get; set; }
            public decimal Temperature { get; set; }
            public decimal WaterLevel { get; set; }
            public int Rpm { get; set; }
        }
    }
    // public class ProcessDataPoint -> Bu sınıf zaten namespace altında tanımlıysa buraya tekrar yazmaya gerek yok.
}



