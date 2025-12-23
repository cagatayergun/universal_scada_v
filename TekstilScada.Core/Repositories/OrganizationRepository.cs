using MySql.Data.MySqlClient;
using TekstilScada.Core;
using TekstilScada.Models;

namespace TekstilScada.Repositories
{
    public class OrganizationRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        // Gateway'den gelen ApiKey'i veritabanında arar
        public Factory? GetFactoryByKey(string apiKey)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM factories WHERE ApiKey = @ApiKey LIMIT 1";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ApiKey", apiKey);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Factory
                        {
                            Id = reader.GetInt32("Id"),
                            CompanyId = reader.GetInt32("CompanyId"),
                            FactoryName = reader.GetString("FactoryName"),
                            ApiKey = reader.GetString("ApiKey")
                        };
                    }
                }
            }
            return null; // Key geçersizse null döner
        }
    }
}