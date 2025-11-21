using Microsoft.AspNetCore.SignalR;
using TekstilScada.Core.Services;
using TekstilScada.Models;
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
            _ftpService.OnJobProgressChanged += async (job) =>
            {
                if (stoppingToken.IsCancellationRequested) return;

                try
                {
                    // KRİTİK DÜZELTME: Karmaşık nesneleri temizleyerek "Hafif" bir kopya oluşturuyoruz.
                    // Bu sayede SignalR serileştirme hatası vermez.
                    var safeJob = new TransferJob
                    {
                        Id = job.Id,
                        Progress = job.Progress,
                        Status = job.Status,
                        ErrorMessage = job.ErrorMessage,
                        OperationType = job.OperationType,
                        RemoteFileName = job.RemoteFileName,
                        RecipeNumber = job.RecipeNumber,

                        // Makine ve Reçetenin TAMAMINI göndermek yerine sadece isimlerini taşıyan
                        // sahte (dummy) nesneler oluşturuyoruz.
                        Machine = new Machine { Id = job.Machine.Id, MachineName = job.Machine.MachineName },
                        LocalRecipe = job.LocalRecipe != null
                            ? new ScadaRecipe { Id = job.LocalRecipe.Id, RecipeName = job.LocalRecipe.RecipeName }
                            : null
                    };

                    // Temizlenmiş nesneyi gönder
                    await _hubContext.Clients.All.SendAsync("ReceiveFtpProgress", safeJob, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Gönderim Hatası: {ex.Message}");
                }
            };

            return Task.CompletedTask;
        }
    }
}