using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using HslCommunication;
//using HslCommunication.Modbus; // Add HslCommunication.Modbus using for Modbus
using HslCommunication.ModBus;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.Services
{
    public class UtilityPollingService
    {
        private readonly UtilityRepository _repo;
        private readonly System.Timers.Timer _timer;
        private bool _isBusy = false;

        public UtilityPollingService(UtilityRepository repo)
        {
            _repo = repo;
            _timer = new System.Timers.Timer(10000); // 10 Saniye
            _timer.Elapsed += async (s, e) => await PollLines();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private async Task PollLines()
        {
            if (_isBusy) return;
            _isBusy = true;

            try
            {
                var lines = _repo.GetUtilityLines();
                var logsToSave = new List<UtilityLog>();

                foreach (var line in lines)
                {
                    try
                    {
                        // Modbus Okuma Mantığı (Pseudo-code, kütüphanenize göre uyarlayın)
                        // DWORD okumak için 2 register okuyup birleştiriyoruz.
                        using (var client = new ModbusClient(line.IpAddress, line.Port))
                        {
                            client.Connect();

                            // Örnek: Modbus Int32 okuma
                            // Note: Register adresleri ve tipleri PLC'ye göre değişir.
                            int water = ModbusClient.ConvertRegistersToInt(client.ReadHoldingRegisters(line.WaterAddress, 2));
                            int elec = ModbusClient.ConvertRegistersToInt(client.ReadHoldingRegisters(line.ElecAddress, 2));
                            int steam = ModbusClient.ConvertRegistersToInt(client.ReadHoldingRegisters(line.SteamAddress, 2));
                            int air = ModbusClient.ConvertRegistersToInt(client.ReadHoldingRegisters(line.AirAddress, 2));

                            logsToSave.Add(new UtilityLog
                            {
                                LineId = line.Id,
                                LogTime = DateTime.Now,
                                WaterCounter = water,
                                ElecCounter = elec,
                                SteamCounter = steam,
                                AirCounter = air
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Hat Okuma Hatası ({line.LineName}): {ex.Message}");
                    }
                }

                // Toplu Kaydet
                if (logsToSave.Count > 0)
                {
                    _repo.LogData(logsToSave);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Polling Genel Hata: {ex.Message}");
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}