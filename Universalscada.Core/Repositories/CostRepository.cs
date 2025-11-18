using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using Universalscada.Models;
using System;
using Universalscada.core;

namespace Universalscada.Repositories
{
    public class CostRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public List<CostParameter> GetAllParameters()
        {
            var parameters = new List<CostParameter>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // Tablo adını kontrol edin: cost_parameters veya CostParameters
                string query = "SELECT Id, ParameterName, CostValue, Unit, Multiplier, CurrencySymbol FROM CostParameters;";
                var cmd = new SqliteCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        parameters.Add(new CostParameter
                        {
                            Id = reader.GetInt32(0),
                            ParameterName = reader.GetString(1),
                            CostValue = reader.GetDecimal(2),
                            Unit = reader.GetString(3),
                            Multiplier = reader.GetDecimal(4),
                            CurrencySymbol = reader.IsDBNull(5) ? "TL" : reader.GetString(5)
                        });
                    }
                }
            }
            return parameters;
        }

        // Diğer güncelleme metotları da benzer şekilde SqliteCommand kullanmalı
    }
}