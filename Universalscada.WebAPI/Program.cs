using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Universalscada.Core;
using Universalscada.Core.Core;
using Universalscada.Core.Repositories;
using Universalscada.Core.Services;
using Universalscada.Repositories; // Manuel Repositoryler için
using Universalscada.Services;
using Universalscada.WebAPI.Hubs;
using Universalscada.WebAPI.Services;
// Module.Textile referansý artýk .csproj'a eklendiði için bu namespace görülebilir
using Universalscada.Module.Textile.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Statik AppConfig'i de güncelle (Repository'ler buradan okuyor)
Universalscada.core.AppConfig.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// 1. DB Context (SQLite)
builder.Services.AddDbContext<ScadaDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 2. Repositories (Scoped)
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IMetaDataRepository, MetaDataRepository>();
builder.Services.AddScoped<AlarmRepository>();
builder.Services.AddScoped<ProductionRepository>();
builder.Services.AddScoped<RecipeRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<CostRepository>(); // Eklendi
builder.Services.AddScoped<ProcessLogRepository>(); // Eklendi
builder.Services.AddScoped<PlcOperatorRepository>(); // Eklendi
builder.Services.AddScoped<RecipeConfigurationRepository>(); // Eklendi

// 3. Services
builder.Services.AddSingleton<LiveEventAggregator>();
// PlcManagerFactory kaydý - Artýk proje referansý olduðu için hata vermemeli
builder.Services.AddSingleton<IPlcManagerFactory, PlcManagerFactory>();
builder.Services.AddSingleton<PlcPollingService>();
builder.Services.AddSingleton<FtpTransferService>();
builder.Services.AddScoped<LiveStepAnalyzer>();
builder.Services.AddScoped<IRecipeTimeCalculator, DynamicRecipeTimeCalculator>();
builder.Services.AddScoped<DynamicRecipeCostCalculator>();
builder.Services.AddSingleton<SignalRBridgeService>();

// 4. Hosted Services
builder.Services.AddHostedService<PlcPollingBackgroundService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Veritabanýný otomatik oluþtur (Opsiyonel - Üretimde Migration kullanýlmalý)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ScadaDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ScadaHub>("/scadaHub");

// Bridge servisini baþlat
app.Services.GetRequiredService<SignalRBridgeService>();

app.Run();