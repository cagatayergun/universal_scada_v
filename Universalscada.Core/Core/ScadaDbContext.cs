using Microsoft.EntityFrameworkCore;
using Universalscada.Core.Meta; // Yeni meta modeller için eklendi
using Universalscada.Models;
using Universalscada.Core.Models;
namespace Universalscada.Core
{
    public class ScadaDbContext : DbContext
    {
        // Temel Model Setleri
        public DbSet<ScadaRecipe> Recipes { get; set; }
        public DbSet<ScadaRecipeStep> RecipeSteps { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<User> Users { get; set; }
        // ... Diğer DbSet tanımlarınız ...
        public DbSet<PlcTagDefinition> PlcTagDefinitions { get; set; }
        // Yeni EVRENSEL Meta Veri Setleri
        public DbSet<StepTypeDefinition> StepTypeDefinitions { get; set; }
        public DbSet<StepParameterDefinition> StepParameterDefinitions { get; set; }
        public DbSet<ProcessConstant> ProcessConstants { get; set; }
        public ScadaDbContext(DbContextOptions<ScadaDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ProcessConstant için anahtar tanımlama
            modelBuilder.Entity<ProcessConstant>().HasKey(c => c.Key);
            modelBuilder.Entity<PlcTagDefinition>()
                .HasIndex(t => new { t.MachineId, t.TagName })
                .IsUnique();
            // Seed Data (Başlangıç Verileri)
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // --- 1. PROSES SABİTLERİ (ProcessConstants) ---
            modelBuilder.Entity<ProcessConstant>().HasData(
                new ProcessConstant { Key = "WATER_PER_LITER_SECONDS", Value = 0.5, Description = "Su alma süresi katsayısı (saniye/litre)." },
                new ProcessConstant { Key = "DRAIN_SECONDS", Value = 120.0, Description = "Boşaltma işlemi için standart süre (saniye)." }
            );

            // --- 2. ADIM TİPLERİ (StepTypeDefinition) ---
            modelBuilder.Entity<StepTypeDefinition>().HasData(
                new StepTypeDefinition { Id = 1, UniversalName = "WATER_TRANSFER", DisplayNameKey = "Su Alma", ControlWordBit = 0, CalculationServiceKey = "WaterTime" },
                new StepTypeDefinition { Id = 2, UniversalName = "HEAT_RAMP", DisplayNameKey = "Isıtma", ControlWordBit = 1, CalculationServiceKey = "HeatTime" },
                new StepTypeDefinition { Id = 3, UniversalName = "MECHANICAL_WORK", DisplayNameKey = "Çalışma", ControlWordBit = 2, CalculationServiceKey = "SimpleTime" },
                new StepTypeDefinition { Id = 4, UniversalName = "DOSING_CHEMICAL", DisplayNameKey = "Dozaj", ControlWordBit = 3, CalculationServiceKey = "DosingTime" },
                new StepTypeDefinition { Id = 5, UniversalName = "DRAIN", DisplayNameKey = "Boşaltma", ControlWordBit = 4, CalculationServiceKey = "ConstantTime" },
                new StepTypeDefinition { Id = 6, UniversalName = "SPIN_DRY", DisplayNameKey = "Sıkma", ControlWordBit = 5, CalculationServiceKey = "SimpleTime" }
            );

            // --- 3. PARAMETRE EŞLEMESİ (StepParameterDefinition) ---
            modelBuilder.Entity<StepParameterDefinition>().HasData(
                new StepParameterDefinition { Id = 1, StepTypeDefinitionId = 1, ParameterKey = "QUANTITY_LITERS", WordIndex = 1, DataType = "short", Unit = "Litre" },
                new StepParameterDefinition { Id = 2, StepTypeDefinitionId = 2, ParameterKey = "TARGET_TEMP", WordIndex = 3, DataType = "short", Unit = "°C" },
                new StepParameterDefinition { Id = 3, StepTypeDefinitionId = 2, ParameterKey = "DURATION_MINUTES", WordIndex = 4, DataType = "short", Unit = "Dakika" },
                new StepParameterDefinition { Id = 4, StepTypeDefinitionId = 3, ParameterKey = "WORK_DURATION_MINUTES", WordIndex = 18, DataType = "short", Unit = "Dakika" },
                new StepParameterDefinition { Id = 5, StepTypeDefinitionId = 4, ParameterKey = "DOSING_QUANTITY_LITERS", WordIndex = 11, DataType = "short", Unit = "Litre" },
                new StepParameterDefinition { Id = 6, StepTypeDefinitionId = 4, ParameterKey = "CHEMICAL_NAME", WordIndex = 21, DataType = "string(6)", Unit = "" },
                new StepParameterDefinition { Id = 7, StepTypeDefinitionId = 6, ParameterKey = "SPIN_DURATION_MINUTES", WordIndex = 9, DataType = "short", Unit = "Dakika" }
            );
        }
    }
}