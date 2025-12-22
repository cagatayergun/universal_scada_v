using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERÝTABANI BAÐLANTISI (AÇILMALI) ---
// ScadaHub içerisinde Repository kullandýðýmýz için API'nin veritabanýna eriþmesi ÞARTTIR.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Core katmanýndaki statik ayarý set ediyoruz (Repository'lerin çalýþmasý için kritik)
if (!string.IsNullOrEmpty(connectionString))
{
    TekstilScada.Core.AppConfig.SetConnectionString(connectionString);
}

// --- 2. TEMEL SERVÝSLER ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        // NaN ve Infinity deðerlerini "NaN", "Infinity" stringleri olarak gönder
        options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        // Döngüsel referanslarý (Parent -> Child -> Parent) yoksay
        options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Property isimleri büyük/küçük harf duyarsýz olsun
        options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// --- 3. JWT KÝMLÝK DOÐRULAMA (Güvenlik Ýçin Kalmalý) ---
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

builder.Services.AddAuthorization();
//builder.Services.AddSingleton<RecipeRepository>();
// --- 4. REPOSITORY'LERÝ KALDIRIYORUZ ---
// API artýk veritabaný sorgusu yapmayacak.
// builder.Services.AddSingleton<MachineRepository>(); // SÝLÝNDÝ
//builder.Services.AddSingleton<UserRepository>();
// builder.Services.AddSingleton<AlarmRepository>();   // SÝLÝNDÝ
// ... Diðer tüm repository'ler silindi.
//builder.Services.AddSingleton<AlarmRepository>();
//builder.Services.AddSingleton<DashboardRepository>();
//builder.Services.AddSingleton<MachineRepository>();     // Muhtemelen gerekecek
//builder.Services.AddSingleton<ProductionRepository>();  // Muhtemelen gerekecek
//builder.Services.AddSingleton<ProcessLogRepository>();
//builder.Services.AddSingleton<FtpTransferService>();
//builder.Services.AddSingleton<PlcPollingService>();
//builder.Services.AddSingleton<RecipeConfigurationRepository>();
//builder.Services.AddSingleton<PlcOperatorRepository>();
//builder.Services.AddSingleton<UserRepository>();
//builder.Services.AddSingleton<CostRepository>();

// --- 5. HÝZMET AYARLARI ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// --- PÝPELÝNE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseWebSockets();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ScadaHub>("/scadaHub"); // Köprü Hub

app.Run();