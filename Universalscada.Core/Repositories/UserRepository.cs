using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using Universalscada.core;
using Universalscada.Models;
using Universalscada.core.Models;

namespace Universalscada.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public User GetUserByUsername(string username)
        {
            User user = null;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Username, FullName, IsActive FROM Users WHERE Username = @Username;";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            IsActive = reader.GetBoolean(3) // SQLite'ta 1/0 olarak saklanır, GetBoolean çevirir
                        };
                    }
                }

                // Roller SQLite için ayrı bir sorgu veya Include ile (EF) çekilebilir.
                // Basitlik için burada bıraktım.
            }
            return user;
        }

        public bool ValidateUser(string username, string password)
        {
            // Şifre doğrulama mantığı (Hash kontrolü)
            // Bu örnekte basitçe veritabanında kullanıcı var mı ve aktif mi diye bakıyoruz.
            // Gerçekte PasswordHasher.VerifyPassword kullanılmalı.
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // Not: SQLite'da bool true/false yerine 1/0 kullanılır.
                string query = "SELECT count(*) FROM Users WHERE Username = @Username AND IsActive = 1;";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);
                var count = Convert.ToInt32(cmd.ExecuteScalar());

                // HASH KONTROLÜ EKLENMELİ (Burada basit geçildi)
                return count > 0;
            }
        }

        public void UpdateRefreshToken(int userId, string refreshToken)
        {
            // SQLite için UTC tarih formatı
            var expiryDate = DateTime.UtcNow.AddDays(30);

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE Users SET RefreshToken = @RefreshToken, RefreshTokenExpiry = @ExpiryDate WHERE Id = @UserId;";
                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public User GetUserByRefreshToken(string refreshToken)
        {
            // Implementasyon ProductionRepository mantığıyla aynı
            return null;
        }

        public IEnumerable<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT Id, Username, FullName, IsActive FROM Users", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            IsActive = reader.GetBoolean(3)
                        });
                    }
                }
            }
            return list;
        }
    }
}