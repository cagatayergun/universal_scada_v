using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Universalscada.core;
using Universalscada.Models;

namespace Universalscada.Repositories
{
    public class RecipeRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public ScadaRecipe GetRecipeById(int recipeId)
        {
            ScadaRecipe recipe = null;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string recipeQuery = "SELECT Id, RecipeName, TargetMachineType, CreationDate FROM Recipes WHERE Id = @Id;";
                var recipeCmd = new SqliteCommand(recipeQuery, connection);
                recipeCmd.Parameters.AddWithValue("@Id", recipeId);

                using (var reader = recipeCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        recipe = new ScadaRecipe
                        {
                            Id = reader.GetInt32(0),
                            RecipeName = reader.GetString(1),
                            TargetMachineType = reader.GetString(2),
                            CreationDate = reader.GetDateTime(3)
                        };
                    }
                }

                if (recipe != null)
                {
                    string stepsQuery = "SELECT * FROM RecipeSteps WHERE RecipeId = @RecipeId ORDER BY StepNumber;";
                    var stepsCmd = new SqliteCommand(stepsQuery, connection);
                    stepsCmd.Parameters.AddWithValue("@RecipeId", recipeId);

                    using (var reader = stepsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var step = new ScadaRecipeStep
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                StepNumber = reader.GetInt32(reader.GetOrdinal("StepNumber")),
                                StepDataWords = new short[25] // Varsayılan
                            };

                            // SQLite'da array saklamak zordur, genellikle JSON veya binary blob saklanır.
                            // Veya Word0, Word1... diye kolonlar vardır.
                            // Eğer DbContext'te string olarak tanımlıysa parse edilmeli:

                            // Basitlik için Word0...Word24 kolon yapısını varsayıyoruz (eski yapı)
                            // Ancak Migration'da "StepDataWords" string olarak tanımlanmıştı.
                            // Bu durumda JSON parse işlemi yapılmalı:
                            string dataStr = reader.GetString(reader.GetOrdinal("StepDataWords"));
                            // JSON parse logic here...

                            recipe.Steps.Add(step);
                        }
                    }
                }
            }
            return recipe;
        }

        public List<ScadaRecipe> GetAllRecipes()
        {
            var list = new List<ScadaRecipe>();
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT Id, RecipeName, TargetMachineType FROM Recipes", conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ScadaRecipe
                        {
                            Id = r.GetInt32(0),
                            RecipeName = r.GetString(1),
                            TargetMachineType = r.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        public void AddOrUpdateRecipe(ScadaRecipe recipe)
        {
            // SQLite uyumlu INSERT OR REPLACE veya UPDATE mantığı
        }

        public ScadaRecipe GetRecipeByName(string name)
        {
            // GetRecipeById benzeri
            return null;
        }
    }
}