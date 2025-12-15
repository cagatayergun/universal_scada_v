// Repositories/PlcOperatorRepository.cs
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TekstilScada.Models;
using TekstilScada.Core;

namespace TekstilScada.Repositories
{
    public class PlcOperatorRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        public List<PlcOperator> GetAll()
        {
            var operators = new List<PlcOperator>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, UserId, Password FROM plc_operator_templates ORDER BY Name;";
                var cmd = new MySqlCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        operators.Add(new PlcOperator
                        {
                            // SlotIndex veritabanındaki benzersiz ID'yi tutar
                            SlotIndex = reader.GetInt32("Id"),

                            // Null kontrolü ile string okuma
                            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString("Name"),

                            // DÜZELTME: GetInt16 yerine Convert.ToInt16 kullanıldı.
                            // Veritabanında sütun INT olsa bile C# tarafında short'a güvenli çevirir.
                            UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? (short)0 : Convert.ToInt16(reader["UserId"]),
                            Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? (short)0 : Convert.ToInt16(reader["Password"])
                        });
                    }
                }
            }
            return operators;
        }

        public void AddDefaultOperator()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO plc_operator_templates (Name, UserId, Password) VALUES (@Name, @UserId, @Password);";
                var cmd = new MySqlCommand(query, connection);

                // DÜZELTME: Benzersizlik hatası almamak için rastgele bir isim oluşturuyoruz.
                // Kullanıcı daha sonra bunu listeden seçip değiştirebilir.
                string uniqueName = "New Operator " + DateTime.Now.ToString("HHmmss");

                cmd.Parameters.AddWithValue("@Name", uniqueName);
                cmd.Parameters.AddWithValue("@UserId", 0);
                cmd.Parameters.AddWithValue("@Password", 0);

                cmd.ExecuteNonQuery();
            }
        }

        public void SaveOrUpdate(PlcOperator op)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Kontrol sorgusu: İsim veya ID çakışması var mı? (Kendi ID'si hariç)
                // Not: PLC'den okunan veriyi kaydederken SlotIndex (DB ID) elimizde olmayabilir,
                // bu yüzden UserId ve Name üzerinden kontrol etmek mantıklıdır.
                string checkQuery = "SELECT Id FROM plc_operator_templates WHERE Name = @Name AND UserId = @UserId;";
                var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@Name", op.Name);
                checkCmd.Parameters.AddWithValue("@UserId", op.UserId);
                var existingId = checkCmd.ExecuteScalar();

                if (existingId != null)
                {
                    // Varsa Şifreyi Güncelle
                    string updateQuery = "UPDATE plc_operator_templates SET Password = @Password WHERE Id = @Id;";
                    var updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@Password", op.Password);
                    updateCmd.Parameters.AddWithValue("@Id", Convert.ToInt32(existingId));
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    // Yoksa Yeni Ekle
                    string insertQuery = "INSERT INTO plc_operator_templates (Name, UserId, Password) VALUES (@Name, @UserId, @Password);";
                    var insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@Name", op.Name);
                    insertCmd.Parameters.AddWithValue("@UserId", op.UserId);
                    insertCmd.Parameters.AddWithValue("@Password", op.Password);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int templateId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM plc_operator_templates WHERE Id = @Id;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", templateId);
                cmd.ExecuteNonQuery();
            }
        }
        public void Update(PlcOperator op)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // SlotIndex, GetAll metodunda veritabanı 'Id'si olarak atanmıştı.
                // Bu yüzden WHERE Id = @Id kısmında SlotIndex kullanıyoruz.
                string query = "UPDATE plc_operator_templates SET Name = @Name, UserId = @UserId, Password = @Password WHERE Id = @Id;";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Name", op.Name);
                cmd.Parameters.AddWithValue("@UserId", op.UserId);
                cmd.Parameters.AddWithValue("@Password", op.Password);
                cmd.Parameters.AddWithValue("@Id", op.SlotIndex); // DB ID'si burada tutuluyor

                cmd.ExecuteNonQuery();
            }
        }
    }
}