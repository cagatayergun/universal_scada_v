using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using TekstilScada.WebAPI.Models;

namespace TekstilScada.WebAPI.Repositories
{
    public class CentralFactoryRepository
    {
        private readonly string _connectionString;

        public CentralFactoryRepository(IConfiguration configuration)
        {
            // appsettings.json'dan okur
            _connectionString = configuration.GetConnectionString("CentralConnection");
        }

        public CentralFactory? GetFactoryByHardwareKey(string hardwareKey)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Id, CompanyId, FactoryName, HardwareKey FROM Factories WHERE HardwareKey = @Key AND IsActive = 1";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Key", hardwareKey);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CentralFactory
                                {
                                    Id = reader.GetInt32("Id"),
                                    CompanyId = reader.GetInt32("CompanyId"),
                                    FactoryName = reader.GetString("FactoryName"),
                                    HardwareKey = reader.GetString("HardwareKey")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Hatası (Factory): {ex.Message}");
                }
            }
            return null; // Bulunamadı veya hata oluştu
        }
        public List<CentralFactory> GetFactoriesByIds(string allowedIds, int companyId)
        {
            var list = new List<CentralFactory>();
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
            {
                connection.Open();
                string query;

                if (allowedIds == "ALL")
                {
                    // Firmaya ait TÜM aktif fabrikalar
                    query = "SELECT * FROM Factories WHERE CompanyId = @CompId AND IsActive = 1";
                }
                else
                {
                    // Sadece ID listesindekiler (SQL Injection riskine karşı parametre veya FIND_IN_SET kullanılabilir)
                    // Basitlik adına string interpolation yapıyoruz ama ID'leri integer'a çevirip güvenli hale getirmek en iyisidir.
                    // Örn: allowedIds "1,2,3" gelir.
                    query = $"SELECT * FROM Factories WHERE CompanyId = @CompId AND IsActive = 1 AND Id IN ({allowedIds})";
                }

                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CompId", companyId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CentralFactory
                            {
                                Id = reader.GetInt32("Id"),
                                CompanyId = reader.GetInt32("CompanyId"),
                                FactoryName = reader.GetString("FactoryName"),
                                HardwareKey = reader.GetString("HardwareKey")
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}