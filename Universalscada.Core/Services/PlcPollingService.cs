// Dosya: Universalscada.Core/Services/PlcPollingService.cs - DÜZELTİLMİŞ VERSİYON

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // Scope Factory için gerekli
using Universalscada.Core.Repositories;
using Universalscada.Models;
using Universalscada.Repositories;
using Universalscada.Services;

namespace Universalscada.Core.Services
{
    // WebAPI'da IHostedService veya WinForms'da Thread/Task tabanlı bir Polling döngüsünü temsil eder.
    public class PlcPollingService
    {
        // Varsayılan PLC Adresleri (MachineConfigurationJson'dan dinamikleşecektir)
        private const string LIVE_DATA_START_ADDRESS = "D400";
        private const ushort LIVE_DATA_LENGTH = 200;
        private const int POLLING_INTERVAL_MS = 1000;

        private readonly IServiceScopeFactory _scopeFactory; // DÜZELTME: Repository yerine Factory
        private readonly IPlcManagerFactory _plcManagerFactory;
        private readonly LiveStepAnalyzer _stepAnalyzer;
        private readonly LiveEventAggregator _eventAggregator;

        // Makine ID'sine göre IPlcManager'ları tutacak sözlük
        private readonly Dictionary<int, IPlcManager> _plcManagers = new Dictionary<int, IPlcManager>();
        private readonly object _lock = new object(); // Sözlük erişimi için kilit

        public PlcPollingService(
            IServiceScopeFactory scopeFactory, // DÜZELTME: Dependency Injection hatasını çözer
            IPlcManagerFactory plcManagerFactory,
            LiveStepAnalyzer stepAnalyzer,
            LiveEventAggregator eventAggregator)
        {
            _scopeFactory = scopeFactory;
            _plcManagerFactory = plcManagerFactory;
            _stepAnalyzer = stepAnalyzer;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// FtpTransferService vb. servislerin kullanması için PLC yöneticilerinin salt okunur listesini sağlar.
        /// </summary>
        public IReadOnlyDictionary<int, IPlcManager> GetPlcManagers()
        {
            lock (_lock)
            {
                return new Dictionary<int, IPlcManager>(_plcManagers);
            }
        }

        // Ana Polling Döngüsü
        public async Task StartPollingLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // DÜZELTME: Scope oluşturarak Scoped servis (Repository) çağrılır
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        // Repository'yi bu scope içinden talep et
                        var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

                        // 1. Tüm makineleri veritabanından getir (Db context bu scope ile yaşar ve ölür)
                        var machines = machineRepository.GetAllMachines();

                        // Makineleri paralel olarak sorgula
                        var tasks = machines.Select(machine => PollSingleMachineAsync(machine));
                        await Task.WhenAll(tasks);
                    }
                }
                catch (Exception ex)
                {
                    // Döngünün genel hatadan dolayı tamamen durmasını engellemek için loglama yapılabilir
                    // Console.WriteLine($"Polling Loop Error: {ex.Message}");
                }

                await Task.Delay(POLLING_INTERVAL_MS, cancellationToken);
            }
        }

        private async Task PollSingleMachineAsync(Machine machine)
        {
            IPlcManager plcManager = null;
            FullMachineStatus status = new FullMachineStatus
            {
                MachineId = machine.Id,
                MachineName = machine.MachineName,
                ConnectionState = ConnectionStatus.Disconnected
            };

            try
            {
                // 1. Makineye özel PLC Manager'ı fabrika ile oluştur/al
                plcManager = _plcManagerFactory.CreatePlcManager(machine);

                // 1.5. Manager'ı sözlüğe kaydet veya güncelle
                lock (_lock)
                {
                    if (_plcManagers.ContainsKey(machine.Id))
                    {
                        _plcManagers[machine.Id] = plcManager;
                    }
                    else
                    {
                        _plcManagers.Add(machine.Id, plcManager);
                    }
                }

                // 2. Jenerik metot ile ham data block'u oku (Blok Okuma - Performans İçin)
                // NOT: KurutmaMakinesiManager bu metoda henüz sahip değil, sonraki adımda ekleyeceğiz.
                var readResult = await plcManager.ReadDataWordsAsync(LIVE_DATA_START_ADDRESS, LIVE_DATA_LENGTH);

                if (readResult.IsSuccess)
                {
                    // 3. Raw veriyi (short[]) alıp, MetaData'ya göre FullMachineStatus'a dönüştür
                    status = _stepAnalyzer.Analyze(machine, readResult.Content);
                    status.ConnectionState = ConnectionStatus.Connected;
                }
                else
                {
                    status.ConnectionState = ConnectionStatus.ConnectionLost;
                    status.ActiveAlarmText = readResult.Message;
                }
            }
            catch (Exception ex)
            {
                status.ConnectionState = ConnectionStatus.Disconnected;
                status.ActiveAlarmText = $"Kritik Hata: {ex.Message}";
            }
            finally
            {
                // 4. Güncel durumu abonelere yayımla
                _eventAggregator.Publish(status);
            }
        }
    }
}