using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using Telemetry.Core; // AppConfig.ConnectionString'in bulunduğu namespace

namespace Telemetry.Repositories
{
    public static class DatabaseUpdater
    {
        public static void CheckAndUpgradeDatabase()
        {
            // Bağlantı dizesini projenizin yapısına göre çekin
            string connectionString = AppConfig.ConnectionString;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS laundry_machine_reports (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Date DATE NOT NULL,
                            Machine_ID VARCHAR(50),
                            Machine_IP VARCHAR(45),
                            `Machine Name` VARCHAR(100),
                            Machine_Type VARCHAR(50),
                            Start_time TIME,
                            End_Time TIME,
                            Duration_mins INT,
                            Type VARCHAR(50),
                            `Reason Type` VARCHAR(100),
                            Reason TEXT,
                            Recipe_id VARCHAR(100),
                            Factory_Order VARCHAR(50),
                            telematric_user VARCHAR(50),
                            machine_operator_id VARCHAR(50),
                            machine_operator_name VARCHAR(100)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                    using (var createCmd = new MySqlCommand(createTableQuery, connection))
                    {
                        createCmd.ExecuteNonQuery();
                    }

                    // OTOMATİK DÜZELTME: Eğer tablo daha önceki kodla INT olarak oluşturalıysa, veri kaybı olmadan VARCHAR'a çevirir
                    string alterColumnQuery = "ALTER TABLE laundry_machine_reports MODIFY COLUMN Recipe_id VARCHAR(100);";
                    using (var alterColCmd = new MySqlCommand(alterColumnQuery, connection))
                    {
                        alterColCmd.ExecuteNonQuery();
                        Debug.WriteLine("[DB UPDATE] 'Recipe_id' sütunu otomatik olarak VARCHAR(100) tipine yükseltildi.");
                    }
                    // 1. 'machines' tablosunda 'DisplayOrder' sütunu var mı diye kontrol et
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM information_schema.COLUMNS 
                        WHERE TABLE_SCHEMA = DATABASE() 
                          AND TABLE_NAME = 'machines' 
                          AND COLUMN_NAME = 'DisplayOrder';";

                    using (var checkCmd = new MySqlCommand(checkQuery, connection))
                    {
                        // Sütun sayısını al
                        long count = Convert.ToInt64(checkCmd.ExecuteScalar());

                        // 2. Eğer sütun yoksa (count == 0), otomatik olarak ekle
                        if (count == 0)
                        {
                            // 1. Sütunu ekle
                            string alterQuery = "ALTER TABLE machines ADD COLUMN DisplayOrder INT NOT NULL DEFAULT 0;";
                            using (var alterCmd = new MySqlCommand(alterQuery, connection))
                            {
                                alterCmd.ExecuteNonQuery();
                            }

                            // 2. ESKİ MAKİNELERİ KURTAR: Hepsine kendi ID'sini Sıra No olarak ata
                            string updateQuery = "UPDATE machines SET DisplayOrder = Id WHERE DisplayOrder = 0;";
                            using (var updateCmd = new MySqlCommand(updateQuery, connection))
                            {
                                updateCmd.ExecuteNonQuery();
                            }

                            Debug.WriteLine("[DB UPDATE] Sütun eklendi ve mevcut makineler otomatik numaralandırıldı.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Bağlantı yoksa veya başka hata varsa çökmemesi için catch'te tutuyoruz
                    Debug.WriteLine($"[DB UPDATE ERROR] Veritabanı güncellenirken hata oluştu: {ex.Message}");
                }
            }
        }
    }
}