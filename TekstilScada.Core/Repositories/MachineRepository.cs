// Repositories/MachineRepository.cs
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TekstilScada.Models;
using TekstilScada.Core;

namespace TekstilScada.Repositories
{
    public class MachineRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        public List<Machine> GetAllMachines()
        {
            var machines = new List<Machine>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // DEĞİŞİKLİK: DisplayOrder eklendi ve sıralama ORDER BY DisplayOrder, Id yapıldı
                    string query = "SELECT Id, MachineUserDefinedId, MachineName, IpAddress, Port, MachineType, IsEnabled, VncAddress, VncPassword, FtpUsername, FtpPassword, MachineSubType, DisplayOrder FROM machines ORDER BY DisplayOrder ASC, Id ASC;";
                    var cmd = new MySqlCommand(query, connection);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var machine = new Machine
                            {
                                Id = reader.GetInt32("Id"),
                                MachineUserDefinedId = reader.GetString("MachineUserDefinedId"),
                                MachineName = reader.IsDBNull(reader.GetOrdinal("MachineName")) ? string.Empty : reader.GetString("MachineName"),
                                IpAddress = reader.GetString("IpAddress"),
                                Port = reader.GetInt32("Port"),
                                MachineType = reader.IsDBNull(reader.GetOrdinal("MachineType")) ? string.Empty : reader.GetString("MachineType"),
                                IsEnabled = reader.GetBoolean("IsEnabled"),
                                VncAddress = reader.IsDBNull(reader.GetOrdinal("VncAddress")) ? string.Empty : reader.GetString("VncAddress"),
                                VncPassword = reader.IsDBNull(reader.GetOrdinal("VncPassword")) ? string.Empty : reader.GetString("VncPassword"),
                                FtpUsername = reader.IsDBNull(reader.GetOrdinal("FtpUsername")) ? string.Empty : reader.GetString("FtpUsername"),
                                FtpPassword = reader.IsDBNull(reader.GetOrdinal("FtpPassword")) ? string.Empty : reader.GetString("FtpPassword"),
                                MachineSubType = reader.IsDBNull(reader.GetOrdinal("MachineSubType")) ? string.Empty : reader.GetString("MachineSubType"),
                                // YENİ SATIR: Sıralama bilgisini okuyoruz
                                DisplayOrder = reader.IsDBNull(reader.GetOrdinal("DisplayOrder")) ? 0 : reader.GetInt32("DisplayOrder")
                            };
                            machines.Add(machine);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine($"Veritabanı hatası (GetAllMachines): {ex.Message}");
                    return new List<Machine>();
                }
            }
            return machines;
        }

        public List<Machine> GetAllEnabledMachines()
        {
            var machines = new List<Machine>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // DEĞİŞİKLİK: DisplayOrder eklendi ve sıralama ORDER BY DisplayOrder, Id yapıldı
                    string query = "SELECT Id, MachineUserDefinedId, MachineName, IpAddress, Port, MachineType, IsEnabled, VncAddress, VncPassword, FtpUsername, FtpPassword, MachineSubType, DisplayOrder FROM machines WHERE IsEnabled = TRUE ORDER BY DisplayOrder ASC, Id ASC;";
                    var cmd = new MySqlCommand(query, connection);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var machine = new Machine
                            {
                                Id = reader.GetInt32("Id"),
                                MachineUserDefinedId = reader.GetString("MachineUserDefinedId"),
                                MachineName = reader.IsDBNull(reader.GetOrdinal("MachineName")) ? string.Empty : reader.GetString("MachineName"),
                                IpAddress = reader.GetString("IpAddress"),
                                Port = reader.GetInt32("Port"),
                                MachineType = reader.IsDBNull(reader.GetOrdinal("MachineType")) ? string.Empty : reader.GetString("MachineType"),
                                IsEnabled = reader.GetBoolean("IsEnabled"),
                                VncAddress = reader.IsDBNull(reader.GetOrdinal("VncAddress")) ? string.Empty : reader.GetString("VncAddress"),
                                VncPassword = reader.IsDBNull(reader.GetOrdinal("VncPassword")) ? string.Empty : reader.GetString("VncPassword"),
                                FtpUsername = reader.IsDBNull(reader.GetOrdinal("FtpUsername")) ? string.Empty : reader.GetString("FtpUsername"),
                                FtpPassword = reader.IsDBNull(reader.GetOrdinal("FtpPassword")) ? string.Empty : reader.GetString("FtpPassword"),
                                MachineSubType = reader.IsDBNull(reader.GetOrdinal("MachineSubType")) ? string.Empty : reader.GetString("MachineSubType"),
                                // YENİ SATIR: Sıralama bilgisini okuyoruz
                                DisplayOrder = reader.IsDBNull(reader.GetOrdinal("DisplayOrder")) ? 0 : reader.GetInt32("DisplayOrder")
                            };
                            machines.Add(machine);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine($"Veritabanı hatası (GetAllEnabledMachines): {ex.Message}");
                    return new List<Machine>();
                }
            }
            return machines;
        }

        public void AddMachine(Machine machine)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // DEĞİŞİKLİK: DisplayOrder eklendi
                string query = "INSERT INTO machines (MachineUserDefinedId, MachineName, IpAddress, Port, MachineType, IsEnabled, VncAddress, VncPassword, FtpUsername, FtpPassword, MachineSubType, DisplayOrder) VALUES (@MachineUserDefinedId, @MachineName, @IpAddress, @Port, @MachineType, @IsEnabled, @VncAddress, @VncPassword, @FtpUsername, @FtpPassword, @MachineSubType, @DisplayOrder);";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MachineUserDefinedId", machine.MachineUserDefinedId);
                cmd.Parameters.AddWithValue("@MachineName", machine.MachineName);
                cmd.Parameters.AddWithValue("@IpAddress", machine.IpAddress);
                cmd.Parameters.AddWithValue("@Port", machine.Port);
                cmd.Parameters.AddWithValue("@MachineType", machine.MachineType);
                cmd.Parameters.AddWithValue("@IsEnabled", machine.IsEnabled);
                cmd.Parameters.AddWithValue("@VncAddress", machine.VncAddress);
                cmd.Parameters.AddWithValue("@VncPassword", machine.VncPassword);
                cmd.Parameters.AddWithValue("@FtpUsername", machine.FtpUsername);
                cmd.Parameters.AddWithValue("@FtpPassword", machine.FtpPassword);
                cmd.Parameters.AddWithValue("@MachineSubType", machine.MachineSubType);
                cmd.Parameters.AddWithValue("@DisplayOrder", machine.DisplayOrder); // YENİ SATIR
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateMachine(Machine machine)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // DEĞİŞİKLİK: DisplayOrder eklendi
                string query = "UPDATE machines SET MachineUserDefinedId = @MachineUserDefinedId, MachineName = @MachineName, IpAddress = @IpAddress, Port = @Port, MachineType = @MachineType, IsEnabled = @IsEnabled, VncAddress = @VncAddress, VncPassword = @VncPassword, FtpUsername = @FtpUsername, FtpPassword = @FtpPassword, MachineSubType = @MachineSubType, DisplayOrder = @DisplayOrder WHERE Id = @Id;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", machine.Id);
                cmd.Parameters.AddWithValue("@MachineUserDefinedId", machine.MachineUserDefinedId);
                cmd.Parameters.AddWithValue("@MachineName", machine.MachineName);
                cmd.Parameters.AddWithValue("@IpAddress", machine.IpAddress);
                cmd.Parameters.AddWithValue("@Port", machine.Port);
                cmd.Parameters.AddWithValue("@MachineType", machine.MachineType);
                cmd.Parameters.AddWithValue("@IsEnabled", machine.IsEnabled);
                cmd.Parameters.AddWithValue("@VncAddress", machine.VncAddress);
                cmd.Parameters.AddWithValue("@VncPassword", machine.VncPassword);
                cmd.Parameters.AddWithValue("@FtpUsername", machine.FtpUsername);
                cmd.Parameters.AddWithValue("@FtpPassword", machine.FtpPassword);
                cmd.Parameters.AddWithValue("@MachineSubType", machine.MachineSubType);
                cmd.Parameters.AddWithValue("@DisplayOrder", machine.DisplayOrder); // YENİ SATIR
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteMachine(int machineId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM machines WHERE Id = @Id;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", machineId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}