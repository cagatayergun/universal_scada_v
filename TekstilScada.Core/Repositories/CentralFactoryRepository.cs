using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using TekstilScada.WebAPI.Models;
using System.Collections.Generic;
using System;

namespace TekstilScada.WebAPI.Repositories
{
    public class CentralFactoryRepository
    {
        private readonly string _connectionString;

        public CentralFactoryRepository(IConfiguration configuration)
        {
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
            return null;
        }

        public List<CentralFactory> GetFactoriesByIds(string allowedIds, int companyId)
        {
            var list = new List<CentralFactory>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query;

                if (allowedIds == "ALL")
                {
                    query = "SELECT * FROM Factories WHERE CompanyId = @CompId AND IsActive = 1";
                }
                else
                {
                    query = $"SELECT * FROM Factories WHERE CompanyId = @CompId AND IsActive = 1 AND Id IN ({allowedIds})";
                }

                using (var cmd = new MySqlCommand(query, connection))
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

        public List<CentralFactory> GetFactoriesByCompanyId(int companyId)
        {
            var list = new List<CentralFactory>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM Factories WHERE CompanyId = @CId", conn);
                cmd.Parameters.AddWithValue("@CId", companyId);

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
            return list;
        }

        public bool AddFactory(CentralFactory factory)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                // DÜZELTME: CreatedAt kaldırıldı
                string query = "INSERT INTO Factories (CompanyId, FactoryName, HardwareKey, IsActive) VALUES (@CId, @Name, @Key, 1)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CId", factory.CompanyId);
                    cmd.Parameters.AddWithValue("@Name", factory.FactoryName);
                    cmd.Parameters.AddWithValue("@Key", factory.HardwareKey);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteFactory(int factoryId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM Factories WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", factoryId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}