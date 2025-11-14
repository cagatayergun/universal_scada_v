// Dosya: Universalscada.WebAPI/Program.cs - GÜNCEL VERSÝYON

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // DbContext için
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Universalscada.Core.Core;         // ScadaDbContext için
// Yeni Core Katmaný using'leri:
using Universalscada.Core.Repositories; // Yeni Repository Arayüzleri
using Universalscada.Core.Services;     // Yeni Core Servisleri
using Universalscada.WebAPI.Hubs;
using Universalscada.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1. Core Katmanýndaki Statik Yapýlandýrmayý Koruma (Eski kodlarla uyum için)
Universalscada.Core.Core.AppConfig.SetConnectionString(connectionString);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();


// === CORE KATMANI DI KAYITLARI (EVRENSEL MÝMARÝ) BAÞLANGIÇ ===

// A. DbContext Kaydý (Scoped)
// Not: ScadaDbContext'in kendi içinde UseSqlite ayarý olduðu için options kullanýlmadý,
// ancak en iyi uygulama için Scoped olarak kaydedilmelidir.
builder.Services.AddDbContext<ScadaDbContext>(options =>
{
    // ScadaDbContext'in içindeki OnConfiguring metodu çalýþacaktýr.
    // Eðer harici bir DB kullanýlacaksa, buraya optionsBuilder.UseSqlServer(connectionString) vb. eklenmeli.
});

// B. Repositories (Scoped) - Her HTTP isteði veya Scope için yeni örnek
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IMetaDataRepository, MetaDataRepository>();
// Yeni Core'daki jenerik Repo'larý kaydedin:
// Not: Mevcut WebAPI Program.cs'deki tekil MachineRepository yerine, 
// yeni Core yapýsýndan gelen Repositorler eklendi (eski adlar güncellendi varsayýmýyla):
builder.Services.AddScoped<AlarmRepository>();
builder.Services.AddScoped<ProductionRepository>();
builder.Services.AddScoped<ProcessLogRepository>();
builder.Services.AddScoped<RecipeRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<RecipeConfigurationRepository>();
// Diðer Repositoriler burada Scoped olarak kaydedilmelidir.


// C. Core Servisleri
// LiveEventAggregator (Singleton) - Uygulama yaþam döngüsü boyunca tek bir olay yayýncýsý
builder.Services.AddSingleton<LiveEventAggregator>();
// PlcManagerFactory (Singleton) - Tüm makineler için tek bir fabrika örneði
builder.Services.AddSingleton<IPlcManagerFactory, Universalscada.Module.Textile.Services.PlcManagerFactory>();
// PlcPollingService (Singleton) - Arka plan döngüsü yöneticisi
builder.Services.AddSingleton<PlcPollingService>();
// LiveStepAnalyzer, DynamicCalculator vb. (Scoped)
builder.Services.AddScoped<LiveStepAnalyzer>();
builder.Services.AddScoped<IRecipeTimeCalculator, DynamicRecipeTimeCalculator>();
// TODO: DynamicRecipeCostCalculator, FtpTransferService vb. buraya eklenmeli.


// D. Arka Plan Hizmeti
// PlcPollingBackgroundService'i IHostedService olarak kaydet
builder.Services.AddHostedService<PlcPollingBackgroundService>();

// === CORE KATMANI DI KAYITLARI SONU ===

// Diðer JWT ve CORS yapýlandýrmasý ayný kalýr...
// (JWT Yapýlandýrmasý)
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


// E. Bridge Service (Singleton) - SignalR Hub'ý ile Core arasýnda köprü
// Bu hizmet, LiveEventAggregator'a abone olacak þekilde yeniden yazýlmalýdýr.
builder.Services.AddSingleton<SignalRBridgeService>();

// CORS Politikasý
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ScadaHub>("/scadaHub");

// Bridge servisinin (SignalRBridgeService) constructor'unun çalýþmasý için
// (yani LiveEventAggregator'a abone olmasý için) uygulama baþlarken tetiklenir.
app.Services.GetRequiredService<SignalRBridgeService>();

app.Run();