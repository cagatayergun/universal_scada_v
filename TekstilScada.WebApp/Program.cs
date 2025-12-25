// Dosya: TekstilScada.WebApp/Program.cs
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
// Core projesinde Localization varsa:
// using TekstilScada.Core.Localization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Temel Servisler ---
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddControllers(); // Controller desteði

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

// --- 3. Blazor Servisleri ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore(); // [Authorize] için gerekli

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
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7039";
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
builder.Services.AddScoped<ScadaDataService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WebApiClient");

    // Eksik olan parametreyi (localStorage) burada servisten çaðýrýp gönderiyoruz
    var localStorage = sp.GetRequiredService<ILocalStorageService>();

    return new ScadaDataService(httpClient, localStorage);
});

// *** YENÝ EKLENEN SERVÝS (Singleton olmalý ki herkes ayný listeyi görsün) ***
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

// Blazor Endpoint'i
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Veri Baþlatma ---
//var scadaDataService = app.Services.GetRequiredService<ScadaDataService>();
//await scadaDataService.InitializeAsync();

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