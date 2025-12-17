using TekstilScada.WebAPI.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERÝTABANI BAÐLANTISINI KALDIRIYORUZ ---
// API'de veritabaný olmayacak. Veritabaný WinForms'ta.
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); 
// TekstilScada.Core.AppConfig.SetConnectionString(connectionString);

// --- 2. TEMEL SERVÝSLER ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(); // Anlýk iletiþim için kritik

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

// --- 4. REPOSITORY'LERÝ KALDIRIYORUZ ---
// API artýk veritabaný sorgusu yapmayacak.
// builder.Services.AddSingleton<MachineRepository>(); // SÝLÝNDÝ
// builder.Services.AddSingleton<AlarmRepository>();   // SÝLÝNDÝ
// ... Diðer tüm repository'ler silindi.

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