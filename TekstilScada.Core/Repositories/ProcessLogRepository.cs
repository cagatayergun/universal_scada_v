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
                        //($"[BulkLog Error] {ex.Message}");
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
                        //($"[BulkManualLog Error] {ex.Message}");
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
                        LiveWaterLevel as totalwatermanuel, 
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
            // 1. Verileri Çek (Sıcaklık/Devir ve Tüketimler ayrı tablolardan veya sorgulardan geliyor olabilir)
            var dataPoints = GetManualLogs(machineId, startTime, endTime);
            var dataPoints1 = GetManualLogs1(machineId, startTime, endTime);

            // Veri kontrolü
            if (dataPoints == null || !dataPoints.Any() || dataPoints1 == null || !dataPoints1.Any())
            {
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

            // --- YENİ SÜRE HESAPLAMA METODU (Gap Analysis) ---
            // Loglar 1 sn (aktif) ve 7 sn (pasif) aralıklarla atıldığı için;
            // İki kayıt arasındaki fark 2.5 saniyeden az ise bu "Aktif Çalışma" süresidir.
            TimeSpan CalculateAccurateDuration(List<ProcessDataPoint> points)
            {
                if (points == null || points.Count == 0) return TimeSpan.Zero;

                TimeSpan duration = TimeSpan.Zero;
                double activeThresholdSeconds = 4; // 1 saniyelik logları yakalamak için toleranslı eşik

                for (int i = 0; i < points.Count - 1; i++)
                {
                    var diff = points[i + 1].Timestamp - points[i].Timestamp;

                    // Eğer iki log arası 2.5 saniyeden kısaysa (yani 1 sn modundaysa) süreye ekle.
                    // 7 sn modundaysa (bekleme) süreye ekleme.
                    if (diff.TotalSeconds <= activeThresholdSeconds)
                    {
                        duration += diff;
                    }
                }

                // Döngü son kaydı kapsamadığı için, son kayıt için de varsayılan 1 sn ekleyebiliriz (opsiyonel)
                if (duration.TotalSeconds > 0)
                    duration += TimeSpan.FromSeconds(1);

                return duration;
            }

            // --- TÜKETİM HESAPLAMA METODU (Reset ve Gürültü Kontrolü) ---
            decimal CalculateConsumption(List<ProcessDataManuel> data, Func<ProcessDataManuel, decimal> selector)
            {
                if (data.Count == 0) return 0;

                decimal totalConsumption = 0;
                decimal previousValue = selector(data[0]);

                for (int i = 1; i < data.Count; i++)
                {
                    decimal currentValue = selector(data[i]);

                    // Sayaç artıyorsa farkı ekle (Normal Tüketim)
                    if (currentValue >= previousValue)
                    {
                        totalConsumption += (currentValue - previousValue);
                    }
                    else
                    {
                        // Sayaç düşmüşse kontrol et:
                        // Eğer düşüş çok keskinse (örneğin önceki değerin %50'sinden küçükse) bu bir RESET'tir.
                        // Sensör gürültüsü (100.5 -> 100.4) ise RESET değildir, işlem yapma.
                        if (previousValue > 0 && currentValue < (previousValue * 0.5m))
                        {
                            totalConsumption += currentValue; // Reset sonrası yeni değeri ekle
                        }
                        // Aksi takdirde (küçük düşüşlerde) gürültü sayılır, toplama bir şey eklenmez.
                    }
                    previousValue = currentValue;
                }
                return totalConsumption;
            }

            // --- SONUÇ NESNESİNİN OLUŞTURULMASI ---
            var summary = new ManualConsumptionSummary
            {
                Makine = machineName,
                RaporAraligi = $"{startTime:dd.MM.yy HH:mm} - {endTime:dd.MM.yy HH:mm}",

                // Yeni mantık ile hesaplanan süre
                ToplamManuelSure = CalculateAccurateDuration(dataPoints).ToString(@"hh\:mm\:ss"),

                // Ortalamalar
                OrtalamaSicaklik = dataPoints.Average(p => (double)p.Temperature / 10.0), // PLC verisi 10 ile çarpılmış geliyorsa
                OrtalamaDevir = dataPoints.Average(p => p.Rpm),

                // Tüketimler
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



