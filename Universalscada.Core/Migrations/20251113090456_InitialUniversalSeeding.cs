using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Universalscada.core.Migrations
{
    /// <inheritdoc />
    public partial class InitialUniversalSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineUserDefinedId = table.Column<string>(type: "TEXT", nullable: false),
                    MachineName = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineType = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    VncAddress = table.Column<string>(type: "TEXT", nullable: false),
                    VncPassword = table.Column<string>(type: "TEXT", nullable: false),
                    FtpUsername = table.Column<string>(type: "TEXT", nullable: false),
                    FtpPassword = table.Column<string>(type: "TEXT", nullable: false),
                    MachineSubType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessConstants",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessConstants", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeName = table.Column<string>(type: "TEXT", nullable: false),
                    TargetMachineType = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StepTypeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UniversalName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayNameKey = table.Column<string>(type: "TEXT", nullable: false),
                    ControlWordBit = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationServiceKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepTypeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    StepNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StepDataWords = table.Column<string>(type: "TEXT", nullable: false),
                    ScadaRecipeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_ScadaRecipeId",
                        column: x => x.ScadaRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StepParameterDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StepTypeDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParameterKey = table.Column<string>(type: "TEXT", nullable: false),
                    WordIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepParameterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepParameterDefinitions_StepTypeDefinitions_StepTypeDefinitionId",
                        column: x => x.StepTypeDefinitionId,
                        principalTable: "StepTypeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleName = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ProcessConstants",
                columns: new[] { "Key", "Description", "Value" },
                values: new object[,]
                {
                    { "DRAIN_SECONDS", "Boşaltma işlemi için standart süre (saniye).", 120.0 },
                    { "WATER_PER_LITER_SECONDS", "Su alma süresi katsayısı (saniye/litre).", 0.5 }
                });

            migrationBuilder.InsertData(
                table: "StepTypeDefinitions",
                columns: new[] { "Id", "CalculationServiceKey", "ControlWordBit", "DisplayNameKey", "UniversalName" },
                values: new object[,]
                {
                    { 1, "WaterTime", 0, "Su Alma", "WATER_TRANSFER" },
                    { 2, "HeatTime", 1, "Isıtma", "HEAT_RAMP" },
                    { 3, "SimpleTime", 2, "Çalışma", "MECHANICAL_WORK" },
                    { 4, "DosingTime", 3, "Dozaj", "DOSING_CHEMICAL" },
                    { 5, "ConstantTime", 4, "Boşaltma", "DRAIN" },
                    { 6, "SimpleTime", 5, "Sıkma", "SPIN_DRY" }
                });

            migrationBuilder.InsertData(
                table: "StepParameterDefinitions",
                columns: new[] { "Id", "DataType", "ParameterKey", "StepTypeDefinitionId", "Unit", "WordIndex" },
                values: new object[,]
                {
                    { 1, "short", "QUANTITY_LITERS", 1, "Litre", 1 },
                    { 2, "short", "TARGET_TEMP", 2, "°C", 3 },
                    { 3, "short", "DURATION_MINUTES", 2, "Dakika", 4 },
                    { 4, "short", "WORK_DURATION_MINUTES", 3, "Dakika", 18 },
                    { 5, "short", "DOSING_QUANTITY_LITERS", 4, "Litre", 11 },
                    { 6, "string(6)", "CHEMICAL_NAME", 4, "", 21 },
                    { 7, "short", "SPIN_DURATION_MINUTES", 6, "Dakika", 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_ScadaRecipeId",
                table: "RecipeSteps",
                column: "ScadaRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_UserId",
                table: "Role",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StepParameterDefinitions_StepTypeDefinitionId",
                table: "StepParameterDefinitions",
                column: "StepTypeDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "ProcessConstants");

            migrationBuilder.DropTable(
                name: "RecipeSteps");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "StepParameterDefinitions");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "StepTypeDefinitions");
        }
    }
}
