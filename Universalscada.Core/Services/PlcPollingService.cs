// Universalscada.Core/Services/PlcPollingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly IPlcManagerFactory _plcManagerFactory;
        private readonly IMachineRepository _machineRepository; // Makine listesini çekmek için gerekli
        private readonly LiveStepAnalyzer _stepAnalyzer;
        private readonly LiveEventAggregator _eventAggregator; // Durum güncellemelerini UI/API'a iletmek için

        public PlcPollingService(
            IPlcManagerFactory plcManagerFactory,
            IMachineRepository machineRepository,
            LiveStepAnalyzer stepAnalyzer,
            LiveEventAggregator eventAggregator)
        {
            _plcManagerFactory = plcManagerFactory;
            _machineRepository = machineRepository;
            _stepAnalyzer = stepAnalyzer;
            _eventAggregator = eventAggregator;
        }

        // Ana Polling Döngüsü
        public async Task StartPollingLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 1. Tüm makineleri veritabanından getir
                var machines = _machineRepository.GetAllMachines();

                var tasks = machines.Select(machine => PollSingleMachineAsync(machine));

                // Tüm makineleri paralel olarak sorgula
                await Task.WhenAll(tasks);

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
                plcManager = _plcManagerFactory.CreatePlcManager(machine); //

                // 2. Jenerik metot ile ham data block'u oku
                // Bu metot, IPlcManager'ın tek jenerik veri okuma yöntemidir.
                var readResult = await plcManager.ReadDataWordsAsync(LIVE_DATA_START_ADDRESS, LIVE_DATA_LENGTH); //

                if (readResult.IsSuccess)
                {
                    // 3. Raw veriyi (short[]) alıp, MetaData'ya göre FullMachineStatus'a dönüştür
                    status = _stepAnalyzer.Analyze(machine, readResult.Content);
                    status.ConnectionState = ConnectionStatus.Connected;
                }
                else
                {
                    // Okuma hatası durumunu logla
                    status.ConnectionState = ConnectionStatus.ConnectionLost;
                    status.ActiveAlarmText = readResult.Message;
                }
            }
            catch (Exception ex)
            {
                // Kritik hata durumunu yakala
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