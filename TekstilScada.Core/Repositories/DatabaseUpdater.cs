using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using TekstilScada.Core; // AppConfig.ConnectionString'in bulunduğu namespace

namespace TekstilScada.Repositories
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