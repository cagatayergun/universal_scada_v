// Dosya: Universalscada.WebApp/Program.cs - GÜNCEL VERSÝYON
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Universalscada.WebApp;
using Universalscada.WebApp.Components;
using Universalscada.WebApp.Services; // Yeni ScadaDataService için

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Configuration'dan WebAPI adresini oku
var webApiBaseAddress = builder.Configuration["WebAPI:BaseAddress"] ?? throw new InvalidOperationException("WebAPI:BaseAddress is not configured.");

// 2. HTTP Client Registration: WebAPI'yi hedef alan IHttpClientFactory'ye scoped olarak kaydet
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(webApiBaseAddress) });

// 3. Jenerik API Ýstemci Servisini kaydet
builder.Services.AddScoped<ScadaDataService>(); //

// 4. Yetkilendirme ve Kimlik Doðrulama Servislerini kaydet
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>(); // Varsayýlan Custom Auth State Provider
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthStateProvider>());

// 5. SignalR Baðlantý Servisini kaydet (Canlý veri akýþý için)
// Bu servis, WebAPI'daki SignalR Hub'ýna (/scadaHub) baðlanmaktan sorumlu olmalýdýr.
// NOT: Bu kýsým, WebApp'in SignalR istemcisini kurar.
builder.Services.AddSingleton<SignalRClientService>(sp =>
{
    // Hub adresi WebAPI adresiyle birleþtirilir
    var hubUrl = webApiBaseAddress.TrimEnd('/') + "/scadaHub";
    return new SignalRClientService(hubUrl);
});

// Hata yönetimi (Opsiyonel)
builder.Services.AddScoped<UnhandledCircuitExceptionHandler>();

await builder.Build().RunAsync();