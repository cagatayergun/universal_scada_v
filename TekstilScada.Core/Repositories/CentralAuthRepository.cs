using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using TekstilScada.WebAPI.Models;

namespace TekstilScada.WebAPI.Repositories
{
    public class CentralAuthRepository
    {
        private readonly string _connectionString;

        public CentralAuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CentralConnection");
        }

        public CentralUser? Login(string username, string password)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // Not: Gerçek senaryoda şifre hash'lenerek kontrol edilmelidir (SHA256 vb.)
                    // Şimdilik test için düz metin karşılaştırması yapıyoruz.
                    string query = @"SELECT Id, CompanyId, Username, FullName, Role, AllowedFactoryIds 
                                     FROM CentralUsers 
                                     WHERE Username = @User AND PasswordHash = @Pass AND IsActive = 1";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@User", username);
                        cmd.Parameters.AddWithValue("@Pass", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CentralUser
                                {
                                    Id = reader.GetInt32("Id"),
                                    CompanyId = reader.GetInt32("CompanyId"),
                                    Username = reader.GetString("Username"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? "" : reader.GetString("FullName"),
                                    Role = reader.GetString("Role"),
                                    AllowedFactoryIds = reader.IsDBNull(reader.GetOrdinal("AllowedFactoryIds")) ? "" : reader.GetString("AllowedFactoryIds")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Hatası (Auth): {ex.Message}");
                }
            }
            return null;
        }
    }
}