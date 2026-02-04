using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.Services
{
    public class UtilityPollingService
    {
        private readonly UtilityRepository _repo;
        private readonly ILogger<UtilityPollingService> _logger;

        // Yönetici nesneleri önbelleği
        private ConcurrentDictionary<int, IUtilityManager> _managers;

        // Döngü kontrolü için Token
        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;

        public UtilityPollingService(UtilityRepository repo, ILogger<UtilityPollingService> logger)
        {
            _repo = repo;
            _logger = logger;
            _managers = new ConcurrentDictionary<int, IUtilityManager>();
        }

        // Dışarıdan çağrılacak Başlat metodu
        public void Start()
        {
            Stop(); // Zaten çalışıyorsa durdur

            _cancellationTokenSource = new CancellationTokenSource();
            _logger.LogInformation("Utility Polling Servisi Başlatılıyor...");

            // Ana döngüyü arka planda başlat
            _pollingTask = Task.Run(() => PollLoop(_cancellationTokenSource.Token));
        }

        // Dışarıdan çağrılacak Durdur metodu
        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                try
                {
                    // Görevin bitmesini bekle (maksimum 3 saniye)
                    _pollingTask?.Wait(3000);
                }
                catch { /* Task iptal hatasını yut */ }

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // Açık bağlantıları temizle
            foreach (var manager in _managers.Values)
            {
                manager.Disconnect();
            }
            _managers.Clear();

            _logger.LogInformation("Utility Polling Servisi Durduruldu.");
        }

        private async Task PollLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await PollAllLinesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Utility döngüsünde genel hata.");
                }

                // 10 Saniye bekle (Token iptal edilirse beklemeden çıkar)
                try
                {
                    await Task.Delay(10000, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task PollAllLinesAsync()
        {
            // Veritabanından hatları çek
            var lines = _repo.GetUtilityLines();
            var logsToSave = new List<UtilityLog>();

            foreach (var line in lines)
            {
                // Manager'ı al veya oluştur (Factory mantığı)
                var manager = _managers.GetOrAdd(line.Id, id => new UtilityModbusManager(line));

                try
                {
                    // Bağlantı kontrolü ve veri okuma
                    // NOT: HslCommunication ConnectServer çağrısı, zaten bağlıysa hızlı döner.
                    // Ancak yine de her döngüde kontrol etmek yerine bağlantı koptuğunda bağlanmak daha performanslı olabilir.
                    // Basitlik adına burada her okuma öncesi bağlantıyı garantiye alıyoruz.
                    await manager.ConnectAsync();

                    var readResult = await manager.ReadUtilityDataAsync();

                    if (readResult.IsSuccess)
                    {
                        logsToSave.Add(readResult.Content);
                    }
                    else
                    {
                        _logger.LogWarning($"Hat {line.LineName} okuma hatası: {readResult.Message}");
                        // Hata varsa bağlantıyı sıfırla
                        manager.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Hat işleme hatası: {line.LineName}");
                }
            }

            // Verileri toplu kaydet
            if (logsToSave.Count > 0)
            {
                _repo.LogData(logsToSave);
            }
        }
    }
}