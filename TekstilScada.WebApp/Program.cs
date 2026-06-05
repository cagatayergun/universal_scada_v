using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System.Globalization;
using TekstilScada.Core.Models;
using TekstilScada.WebApp.Components;
using TekstilScada.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Temel Servisler ---
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // ?? DUZELTME 1: RAM Önbellek Altyapýsý hiyerarþi gereði üste alýndý

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
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => { options.DetailedErrors = true; })
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
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
// ?? DUZELTME 2: CS7036 hatasýný bitiren memoryCache parametresi çözüldü ve Constructor'a eklendi
builder.Services.AddScoped<ScadaDataService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WebApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var config = sp.GetRequiredService<IConfiguration>();
    var memoryCache = sp.GetRequiredService<IMemoryCache>();

    return new ScadaDataService(httpClient, localStorage, config, memoryCache);
});

// 5 Dakikada bir grafik önbelleklerini ýsýtan Arka Plan Ýþçisi


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

app.UseStaticFiles();

var supportedCultures = new[] { "tr-TR", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

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
            circuitLogger.LogError(ex, ">>> KRÝTÝK BLAZOR DEVRE HATASI YAKALANDI! <<<");
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Blazor devre hatasý: Sunucu baðlantýsý kesildi.");
            return;
        }
        throw;
    }
});

app.Run();