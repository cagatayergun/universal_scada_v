using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ILogger yoksa veya kullanmak istemezseniz aşağıdaki satırı silebilirsiniz.
using Microsoft.Extensions.Logging;
using Telemetry.Models;
using Telemetry.Repositories;

namespace Telemetry.Services
{
    public class UtilityPollingService
    {
        private readonly UtilityRepository _repo;
        // Logger opsiyonel yapıldı, null gelebilir
        private readonly ILogger<UtilityPollingService> _logger;

        // Yönetici nesneleri önbelleği
        private ConcurrentDictionary<int, IUtilityManager> _managers;

        // Döngü kontrolü için Token
        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;

        // --- UI GÜNCELLEMEK İÇİN GEREKLİ EVENTLER (EKLENDİ) ---
        public event Action<List<UtilityLog>> OnUtilityDataRefreshed;
        public event Action<string> OnUtilityError;
        // -----------------------------------------------------

        public UtilityPollingService(UtilityRepository repo, ILogger<UtilityPollingService> logger = null)
        {
            _repo = repo;
            _logger = logger;
            _managers = new ConcurrentDictionary<int, IUtilityManager>();
        }

        public void Start()
        {
            Stop(); // Zaten çalışıyorsa durdur

            _cancellationTokenSource = new CancellationTokenSource();
            LogInfo("Utility Polling Servisi Başlatılıyor...");

            // Ana döngüyü arka planda başlat
            _pollingTask = Task.Run(() => PollLoop(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                try
                {
                    _pollingTask?.Wait(3000);
                }
                catch { /* Task iptal hatasını yut */ }

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            foreach (var manager in _managers.Values)
            {
                manager.Disconnect();
            }
            _managers.Clear();

            LogInfo("Utility Polling Servisi Durduruldu.");
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
                    LogError(ex, "Utility döngüsünde genel hata.");
                    OnUtilityError?.Invoke($"Genel Döngü Hatası: {ex.Message}");
                }

                // 10 Saniye bekle
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
            var lines = _repo.GetUtilityLines();
            if (lines == null || !lines.Any()) return;

            var logsToSave = new List<UtilityLog>();

            foreach (var line in lines)
            {
                // 1. Manager Kontrolü (IP Değişikliği Algılama EKLENDİ)
                IUtilityManager manager;

                if (_managers.TryGetValue(line.Id, out var existingManager))
                {
                    // Eğer veritabanındaki IP ile hafızadaki IP farklıysa, eskiyi sil yenisini oluştur
                    if (existingManager.IpAddress != line.IpAddress)
                    {
                        existingManager.Disconnect();
                        manager = new UtilityModbusManager(line);
                        _managers[line.Id] = manager;
                        LogInfo($"Hat {line.LineName} için IP değişikliği algılandı, servis güncellendi.");
                    }
                    else
                    {
                        manager = existingManager;
                    }
                }
                else
                {
                    manager = new UtilityModbusManager(line);
                    _managers.TryAdd(line.Id, manager);
                }

                // 2. Veri Okuma İşlemi
                try
                {
                    // Bağlantı zaten açıksa HslCommunication bunu hızlıca geçer, sorun yok.
                    await manager.ConnectAsync();

                    var readResult = await manager.ReadUtilityDataAsync();

                    if (readResult.IsSuccess)
                    {
                        logsToSave.Add(readResult.Content);
                    }
                    else
                    {
                        string errMsg = $"Hat {line.LineName} okuma hatası: {readResult.Message}";
                        // Sadece loga yaz, kullanıcıyı her saniye hatayla boğma (veya istersen OnUtilityError çağır)
                        LogWarning(errMsg);
                        manager.Disconnect(); // Hata durumunda bağlantıyı tazelemek iyidir
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Hat işleme hatası: {line.LineName}");
                }
            }

            // 3. Kayıt ve UI Bildirimi
            if (logsToSave.Count > 0)
            {
                _repo.LogData(logsToSave);

                // --- UI TARAFINA VERİYİ FIRLAT (EKLENDİ) ---
                OnUtilityDataRefreshed?.Invoke(logsToSave);
            }
        }

        // Helper methods for logging null check
        private void LogInfo(string message) => _logger?.LogInformation(message);
        private void LogWarning(string message) => _logger?.LogWarning(message);
        private void LogError(Exception ex, string message) => _logger?.LogError(ex, message);
    }
}