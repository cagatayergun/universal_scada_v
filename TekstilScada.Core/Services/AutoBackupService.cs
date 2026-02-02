using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks; // Task yapısı için
using MySql.Data.MySqlClient;
using TekstilScada.Core;

namespace TekstilScada.Services
{
    public class AutoBackupService
    {
        private System.Threading.Timer _timer;

        // Yedeklerin tutulacağı klasör
        private readonly string _backupFolder = @"C:\ScadaYedekleri";

        // MySQL Dump aracının yolu
        private readonly string _mysqlDumpPath = @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe";

        // Veritabanı Bilgileri (Güvenlik için App.config'den çekilmesi önerilir)
        private const string DbUser = "user1";
        private const string DbPass = "Cagatay.19";
        private const string DbName = "scada_db";

        // --- AYARLAR ---
        private const int ZipSaklamaSuresiGun = 15;
        private const int VeriSaklamaSuresiGun = 1095; // 3 Yıl

        public void Start()
        {
            // Hedef: Gece 02:00 (Sistemin en az yoğun olduğu saat)
            DateTime now = DateTime.Now;
            DateTime nextRun = now.Date.AddHours(2).AddMinutes(0);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            TimeSpan timeToGo = nextRun - now;

            // Timer'ı kuruyoruz. 
            // DİKKAT: Timer callback'i thread pool thread'inde çalışır, UI'ı dondurmaz.
            _timer = new System.Threading.Timer(OnTimerElapsed, null, timeToGo, TimeSpan.FromHours(24));

            LogToFile($"Yedekleme Servisi Başlatıldı. İlk çalışma: {nextRun}");
        }

        private void OnTimerElapsed(object state)
        {
            // Timer callback'i tekrar çalışmasın diye geçici olarak durdurabiliriz veya 
            // işlemin uzun sürmesi durumunda üst üste binmeyi engellemek için lock kullanabiliriz.
            // Burada basitlik adına Task içinde çalıştırıyoruz.

            Task.Run(() =>
            {
                try
                {
                    LogToFile("Yedekleme işlemi başladı...");

                    // 1. ADIM: Yedeği Al ve Ziple
                    PerformBackupAndZip();

                    // 2. ADIM: Eski Dosyaları Temizle
                    CleanupOldBackupFiles();

                    // 3. ADIM: Veritabanı Temizliği
                    PruneOldDatabaseData();

                    LogToFile("Günlük bakım başarıyla tamamlandı.");
                }
                catch (Exception ex)
                {
                    LogToFile($"KRİTİK HATA (Bakım): {ex.Message}");
                }
            });
        }

        private void PerformBackupAndZip()
        {
            if (!Directory.Exists(_backupFolder)) Directory.CreateDirectory(_backupFolder);

            string tarihEtiketi = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string sqlFileName = $"Yedek_{tarihEtiketi}.sql";
            string sqlFilePath = Path.Combine(_backupFolder, sqlFileName);
            string zipFilePath = Path.Combine(_backupFolder, sqlFileName.Replace(".sql", ".zip"));

            // --- GÜVENLİ YEDEKLEME PARAMETRELERİ ---
            // --single-transaction: Tabloları kilitlemez (InnoDB). SCADA veri yazmaya devam edebilir.
            // --quick: Verileri satır satır okur, RAM'i şişirmez.
            // --routines: Stored procedure'leri de alır.
            string dumpArgs = $"--single-transaction --quick --routines -u {DbUser} -p{DbPass} {DbName} -r \"{sqlFilePath}\"";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = _mysqlDumpPath,
                Arguments = dumpArgs,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true, // Hataları yakalamak için
                UseShellExecute = false,
                CreateNoWindow = true // Pencere açma (Sessiz mod)
            };

            using (Process process = Process.Start(psi))
            {
                // Hata çıktısını oku (Varsa)
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Mysqldump Hatası: {errors}");
                }
            }

            // Zipleme ve Temizlik
            if (File.Exists(sqlFilePath))
            {
                try
                {
                    if (File.Exists(zipFilePath)) File.Delete(zipFilePath);

                    using (ZipArchive zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(sqlFilePath, sqlFileName);
                    }

                    File.Delete(sqlFilePath); // Ham dosyayı sil
                    LogToFile($"Yedek alındı: {zipFilePath}");
                }
                catch (Exception zipEx)
                {
                    LogToFile($"Zip Hatası: {zipEx.Message}");
                }
            }
        }

        private void CleanupOldBackupFiles()
        {
            if (!Directory.Exists(_backupFolder)) return;

            var directory = new DirectoryInfo(_backupFolder);
            var files = directory.GetFiles("*.*");

            foreach (var file in files)
            {
                // Sadece bizim formatımızdaki dosyaları işle (.zip ve .sql)
                if (file.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
                    file.Extension.Equals(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    DateTime fileDate;
                    bool dateParsed = false;

                    // 1. YÖNTEM: Dosya İsminden Tarih Okuma (En Güvenlisi)
                    // Dosya adı formatı: "Yedek_yyyy-MM-dd_HH-mm.zip"
                    try
                    {
                        // Uzantıyı at: "Yedek_2026-01-10_09-00"
                        string fileNameNoExt = Path.GetFileNameWithoutExtension(file.Name);

                        // "Yedek_" kısmını at: "2026-01-10_09-00"
                        string dateString = fileNameNoExt.Replace("Yedek_", "");

                        // Tarihi Parse et
                        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd_HH-mm",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fileDate))
                        {
                            dateParsed = true;
                        }
                        else
                        {
                            // Format tutmazsa (eski veya farklı dosyalar) dosya tarihini kullan
                            fileDate = file.CreationTime;
                        }
                    }
                    catch
                    {
                        // İsimden okuyamazsa dosya sistem tarihine dön
                        fileDate = file.CreationTime;
                    }

                    // -----------------------------------------------------------
                    // KONTROL VE SİLME İŞLEMİ
                    // -----------------------------------------------------------
                    if (fileDate < DateTime.Now.AddDays(-ZipSaklamaSuresiGun))
                    {
                        try
                        {
                            file.Delete();
                            // Loga detaylı yazalım: İsimden mi sildi, tarihten mi?
                            string source = dateParsed ? "İsimden Tarih" : "Dosya Tarihi";
                            LogToFile($"Eski yedek silindi ({source}): {file.Name} (Tarih: {fileDate})");
                        }
                        catch (Exception ex)
                        {
                            LogToFile($"Dosya silinemedi ({file.Name}): {ex.Message}");
                        }
                    }
                }
            }
        }

        private void PruneOldDatabaseData()
        {
            using (var conn = new MySqlConnection(AppConfig.ConnectionString)) // Connection String'inizi buraya bağlayın
            {
                try
                {
                    conn.Open();
                    string cutoffDateStr = DateTime.Now.AddDays(-VeriSaklamaSuresiGun).ToString("yyyy-MM-dd");

                    string[] tablesToPrune = { "process_data_log", "manual_mode_log", "alarm_history" };
                    string[] timeColumns = { "LogTimestamp", "LogTimestamp", "StartTime" };

                    for (int i = 0; i < tablesToPrune.Length; i++)
                    {
                        string tableName = tablesToPrune[i];
                        string dateCol = timeColumns[i];

                        int totalDeleted = 0;
                        int batchSize = 2000; // Chunk boyutunu düşürdük (Daha güvenli)
                        bool continueDeleting = true;

                        while (continueDeleting)
                        {
                            // --- DÜŞÜK ÖNCELİKLİ SİLME ---
                            // LOW_PRIORITY anahtar kelimesi (MyISAM için etkilidir, InnoDB'de etkisi azdır ama zarar vermez)
                            // Önemli olan LIMIT kullanımıdır.
                            string deleteQuery = $"DELETE FROM {tableName} WHERE {dateCol} < '{cutoffDateStr}' LIMIT {batchSize};";

                            using (var cmd = new MySqlCommand(deleteQuery, conn))
                            {
                                cmd.CommandTimeout = 120;
                                int deletedCount = cmd.ExecuteNonQuery();
                                totalDeleted += deletedCount;

                                if (deletedCount == 0)
                                {
                                    continueDeleting = false;
                                }
                                else
                                {
                                    // Veri tabanına nefes aldırma molası (Veri kaybını önlemek için kritik)
                                    // Bu süre içinde SCADA gelen verileri yazar.
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        if (totalDeleted > 0)
                            LogToFile($"{tableName}: {totalDeleted} eski kayıt temizlendi.");
                    }

                    // Optimize işlemi tablonun boyutuna göre tabloyu kilitleyebilir!
                    // Bu yüzden bunu sadece veritabanı çok şişerse ve gece geç saatte yapın.
                    // Riskli olduğu için comment satırına aldım, ihtiyaç varsa açılabilir.
                    /*
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        using (var cmd = new MySqlCommand("OPTIMIZE TABLE process_data_log, manual_mode_log;", conn))
                        {
                            cmd.CommandTimeout = 600;
                            cmd.ExecuteNonQuery();
                            LogToFile("Tablolar optimize edildi.");
                        }
                    }
                    */
                }
                catch (Exception ex)
                {
                    LogToFile($"Veritabanı Temizleme Hatası: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
            LogToFile("Yedekleme servisi durduruldu.");
        }

        // Basit bir loglama yardımcısı (Hata takibi için)
        private void LogToFile(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupLog.txt");
                string logLine = $"{DateTime.Now}: {message}{Environment.NewLine}";
                File.AppendAllText(logPath, logLine);
            }
            catch { }
        }
    }
}