using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using Universalscada.core;

namespace Universalscada.Repositories
{
    public class RecipeConfigurationRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public string GetLayoutJson(string machineSubType, int stepTypeId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // Tablo adı migration ile uyumlu olmalı (step_editor_layouts -> StepEditorLayouts)
                string query = "SELECT LayoutJson FROM StepEditorLayouts WHERE MachineSubType = @SubType AND StepTypeId = @StepId";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubType", machineSubType ?? "DEFAULT");
                cmd.Parameters.AddWithValue("@StepId", stepTypeId);
                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value) return result.ToString();

                // Özel tip yoksa varsayılanı dene
                cmd.Parameters["@SubType"].Value = "DEFAULT";
                result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        public List<string> GetMachineSubTypes()
        {
            var subTypes = new List<string> { "DEFAULT" };
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // SQLite'ta DISTINCT kullanımı
                string query = "SELECT DISTINCT MachineSubType FROM Machines WHERE MachineType = 'BYMakinesi' AND MachineSubType IS NOT NULL AND MachineSubType <> ''";
                var cmd = new SqliteCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subTypes.Add(reader.GetString(0));
                    }
                }
            }
            return subTypes;
        }

        public void SaveLayout(string layoutName, string machineSubType, int stepTypeId, string layoutJson)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // MySQL: INSERT ... ON DUPLICATE KEY UPDATE
                // SQLite: INSERT ... ON CONFLICT(...) DO UPDATE
                // NOT: Bu sorgunun çalışması için (MachineSubType, StepTypeId) üzerinde UNIQUE INDEX olması gerekir.
                string query = @"
                    INSERT INTO StepEditorLayouts (LayoutName, MachineSubType, StepTypeId, LayoutJson)
                    VALUES (@LayoutName, @MachineSubType, @StepTypeId, @LayoutJson)
                    ON CONFLICT(MachineSubType, StepTypeId) 
                    DO UPDATE SET 
                        LayoutName = excluded.LayoutName, 
                        LayoutJson = excluded.LayoutJson;";

                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@LayoutName", layoutName);
                cmd.Parameters.AddWithValue("@MachineSubType", machineSubType);
                cmd.Parameters.AddWithValue("@StepTypeId", stepTypeId);
                cmd.Parameters.AddWithValue("@LayoutJson", layoutJson);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetStepTypes()
        {
            var dt = new DataTable();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // Tablo adı: step_types -> StepTypeDefinitions
                string query = "SELECT Id, DisplayNameKey as StepName FROM StepTypeDefinitions ORDER BY Id";
                var cmd = new SqliteCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }
    }
}