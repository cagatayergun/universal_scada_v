// Dosya: Universalscada.WebAPI/Services/PlcPollingBackgroundService.cs - GÜNCEL VERSİYON

using Universalscada.Core.Services;
using Universalscada.Core.Repositories;
using Universalscada.Core.Core;
using Universalscada.Models;

public class PlcPollingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlcPollingBackgroundService> _logger;
    private readonly PlcPollingService _pollingService; // Polling mantığı Core'da olduğu için Singleton olarak enjekte edilir

    // Yapıcı metot, Singleton servisleri alır
    public PlcPollingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PlcPollingBackgroundService> logger,
        PlcPollingService pollingService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pollingService = pollingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC Polling Background Service is starting.");

        try
        {
            // Polling mantığı (StartPollingLoopAsync) zaten Core'daki PlcPollingService içinde.
            // Bu metot, Core'da yazıldığı gibi iç döngüde çalışır ve kendi Scope'unu yönetir.
            // Bu nedenle, sadece Core servisinin ana döngüsünü çağırıyoruz.

            _logger.LogInformation("Starting Core PlcPollingService loop...");

            // Polling servisinin ana asenkron döngüsünü başlat
            // Polling servisi, kendi içinde IMachineRepository'yi scope oluşturarak kullanacaktır.
            await _pollingService.StartPollingLoopAsync(stoppingToken);

            _logger.LogInformation("PLC Polling Background Service execution completed gracefully (should not happen until shutdown).");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("PLC Polling Background Service is gracefully stopped.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FATAL ERROR: PlcPollingBackgroundService loop terminated unexpectedly.");
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC Polling Background Service is initiating shutdown.");
        return base.StopAsync(stoppingToken);
    }
}