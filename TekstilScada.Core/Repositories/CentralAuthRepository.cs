using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using TekstilScada.WebAPI.Models;
using System.Collections.Generic;
using System;

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

        public List<Company> GetAllCompanies()
        {
            var list = new List<Company>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM Companies", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Company
                        {
                            Id = reader.GetInt32("Id"),
                            CompanyName = reader.GetString("CompanyName"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
                    }
                }
            }
            return list;
        }

        public bool AddOrUpdateCompany(Company company)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query;
                if (company.Id == 0)
                {
                    // DÜZELTME: CreatedAt kaldırıldı
                    query = "INSERT INTO Companies (CompanyName, IsActive) VALUES (@Name, @Active)";
                }
                else
                {
                    query = "UPDATE Companies SET CompanyName = @Name, IsActive = @Active WHERE Id = @Id";
                }

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", company.CompanyName);
                    cmd.Parameters.AddWithValue("@Active", company.IsActive);
                    if (company.Id > 0) cmd.Parameters.AddWithValue("@Id", company.Id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteCompany(int companyId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd1 = new MySqlCommand("DELETE FROM CentralUsers WHERE CompanyId = @Id", conn, transaction);
                        cmd1.Parameters.AddWithValue("@Id", companyId);
                        cmd1.ExecuteNonQuery();

                        var cmd2 = new MySqlCommand("DELETE FROM Factories WHERE CompanyId = @Id", conn, transaction);
                        cmd2.Parameters.AddWithValue("@Id", companyId);
                        cmd2.ExecuteNonQuery();

                        var cmd3 = new MySqlCommand("DELETE FROM Companies WHERE Id = @Id", conn, transaction);
                        cmd3.Parameters.AddWithValue("@Id", companyId);
                        cmd3.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<CentralUser> GetUsersByCompanyId(int companyId)
        {
            var list = new List<CentralUser>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT Id, CompanyId, Username, FullName, Role, AllowedFactoryIds FROM CentralUsers WHERE CompanyId = @CId";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CId", companyId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CentralUser
                            {
                                Id = reader.GetInt32("Id"),
                                CompanyId = reader.GetInt32("CompanyId"),
                                Username = reader.GetString("Username"),
                                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? "" : reader.GetString("FullName"),
                                Role = reader.GetString("Role"),
                                AllowedFactoryIds = reader.IsDBNull(reader.GetOrdinal("AllowedFactoryIds")) ? "" : reader.GetString("AllowedFactoryIds")
                            });
                        }
                    }
                }
            }
            return list;
        }

        public bool AddUser(CentralUser user, string password)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // 1. SQL SORGUSU: Burada 'AllowedFactoryIds' ve '@Factories' yazıyor mu?
                string query = @"INSERT INTO CentralUsers 
                                (CompanyId, Username, PasswordHash, FullName, Role, AllowedFactoryIds, IsActive) 
                                VALUES 
                                (@CId, @User, @Pass, @Name, @Role, @Factories, 1)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CId", user.CompanyId);
                    cmd.Parameters.AddWithValue("@User", user.Username);
                    cmd.Parameters.AddWithValue("@Pass", password);
                    cmd.Parameters.AddWithValue("@Name", user.FullName ?? "");
                    cmd.Parameters.AddWithValue("@Role", user.Role);

                    // 2. PARAMETRE: Bu satır sizde var mı? Yoksa veri tabana gitmez.
                    cmd.Parameters.AddWithValue("@Factories", user.AllowedFactoryIds ?? "");

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteUser(int userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM CentralUsers WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public bool UpdateUser(CentralUser user, string password = null)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query;

                // Şifre boşsa güncelleme, doluysa güncelle
                if (!string.IsNullOrEmpty(password))
                {
                    query = @"UPDATE CentralUsers 
                              SET Username=@User, PasswordHash=@Pass, FullName=@Name, AllowedFactoryIds=@Factories 
                              WHERE Id=@Id";
                }
                else
                {
                    query = @"UPDATE CentralUsers 
                              SET Username=@User, FullName=@Name, AllowedFactoryIds=@Factories 
                              WHERE Id=@Id";
                }

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@User", user.Username);
                    cmd.Parameters.AddWithValue("@Name", user.FullName ?? "");
                    cmd.Parameters.AddWithValue("@Factories", user.AllowedFactoryIds ?? "");

                    if (!string.IsNullOrEmpty(password))
                    {
                        cmd.Parameters.AddWithValue("@Pass", password);
                    }

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}