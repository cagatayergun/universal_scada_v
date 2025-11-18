using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Universalscada.Models;
using Universalscada.core;

namespace Universalscada.Repositories
{
    public class PlcOperatorRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public List<PlcOperator> GetAll()
        {
            var operators = new List<PlcOperator>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, UserId, Password FROM PlcOperatorTemplates ORDER BY Name;";
                var cmd = new SqliteCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        operators.Add(new PlcOperator
                        {
                            SlotIndex = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            UserId = (short)reader.GetInt32(2),
                            Password = (short)reader.GetInt32(3)
                        });
                    }
                }
            }
            return operators;
        }

        // Add, Update, Delete metotları da SqliteCommand ile güncellenmeli.
    }
}