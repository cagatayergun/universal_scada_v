using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using TekstilScada.WebApp.Components;
using TekstilScada.WebApp.Services;

// Core namespace'inizin doðru olduðundan emin olun
using TekstilScada.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Temel Servisler ---
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddControllers();

// --- 2. Authentication (Kimlik Doðrulama) Servisi ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);

        // Redirect Döngüsü Korumasý
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path == options.LoginPath)
            {
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// --- 3. Blazor Servisleri ve SignalR Ayarlarý ---
// DEÐÝÞÝKLÝK 1: HubOptions eklendi (Kopma toleransý için)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => { options.DetailedErrors = true; })
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // 60 sn bekle
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddServerSideBlazor()
       .AddHubOptions(options =>
       {
           options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
           options.HandshakeTimeout = TimeSpan.FromSeconds(30);
       });

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

// --- 4. Custom Auth Provider ---
builder.Services.AddScoped<CustomAuthStateProvider>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WebApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CustomAuthStateProvider>>();

    return new CustomAuthStateProvider(httpClient, localStorage, logger);
});

builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

// --- 5. HttpClient ---
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
//var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7039";
builder.Services.AddHttpClient("WebApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
});

// --- 6. Scada & Diðer Servisler ---

// Mevcut ScadaDataService Kaydý
builder.Services.AddScoped<ScadaDataService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WebApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();

    // YENÝ: Configuration servisini çaðýrýyoruz
    var config = sp.GetRequiredService<IConfiguration>();

    // YENÝ: Constructor'a config parametresini de ekliyoruz
    return new ScadaDataService(httpClient, localStorage, config);
});

// DEÐÝÞÝKLÝK 2: Interface Eþleþtirmesi (Arka plan servisi IScadaDataService arýyor olabilir)
// Eðer IScadaDataService interface'iniz yoksa FactoryStateService.cs içindeki IScadaDataService yerine direkt ScadaDataService yazýnýz.
// Varsa bu satýrý ekleyin:



// DEÐÝÞÝKLÝK 3: Factory-Based Caching Servisi
// Hem Singleton (Sayfalar okusun diye) hem HostedService (Arka planda çalýþsýn diye)
builder.Services.AddSingleton<FactoryStateService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<FactoryStateService>());


builder.Services.AddSingleton<VncSessionService>();
builder.Services.AddScoped<CircuitHandler, UnhandledCircuitExceptionHandler>();
builder.Services.AddLogging();

var app = builder.Build();

// =================================================================
// MIDDLEWARE (AKIÞ) SIRALAMASI
// =================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// 1. Statik Dosyalar
app.UseStaticFiles();

// 2. Dil Desteði
var supportedCultures = new[] { "tr-TR", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// 3. Routing
app.UseRouting();

// 4. Güvenlik
app.UseAuthentication();
app.UseAuthorization();

// 5. Antiforgery
app.UseAntiforgery();

// 6. Endpoint Tanýmlarý
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Global Hata Yakalama ---
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var circuitLogger = loggerFactory.CreateLogger("CircuitLogger");
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        if (context.Request.Path.StartsWithSegments("/_blazor"))
        {
            circuitLogger.LogError(ex, ">>> KRÝTÝK BLZOR DEVRE HATASI YAKALANDI! <<<");
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Blazor devre hatasý: Sunucu baðlantýsý kesildi.");
            return;
        }
        throw;
    }
});

app.Run();