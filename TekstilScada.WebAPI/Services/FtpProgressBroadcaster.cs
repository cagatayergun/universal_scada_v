using Microsoft.AspNetCore.SignalR;
using TekstilScada.Core.Services; // FtpTransferService burada
using TekstilScada.Services;
using TekstilScada.WebAPI.Hubs;

namespace TekstilScada.WebAPI.Services
{
    public class FtpProgressBroadcaster : BackgroundService
    {
        private readonly FtpTransferService _ftpService;
        private readonly IHubContext<ScadaHub> _hubContext;

        public FtpProgressBroadcaster(FtpTransferService ftpService, IHubContext<ScadaHub> hubContext)
        {
            _ftpService = ftpService;
            _hubContext = hubContext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Servisteki olaya abone ol
            _ftpService.OnJobProgressChanged += async (job) =>
            {
                // SignalR ile tüm istemcilere işin son durumunu gönder
                await _hubContext.Clients.All.SendAsync("ReceiveFtpProgress", job, stoppingToken);
            };

            return Task.CompletedTask;
        }
    }
}