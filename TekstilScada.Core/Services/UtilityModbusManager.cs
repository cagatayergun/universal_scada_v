using HslCommunication;
using HslCommunication.ModBus;
using System;
using System.Threading.Tasks;
using TekstilScada.Models;

namespace TekstilScada.Services
{
    public class UtilityModbusManager : IUtilityManager
    {
        private readonly ModbusTcpNet _plcClient;
        private readonly UtilityLine _config;

        public string IpAddress => _config.IpAddress;

        public UtilityModbusManager(UtilityLine config)
        {
            _config = config;
            // HslCommunication ModbusTCP (SlaveId parametresi ile)
            _plcClient = new ModbusTcpNet(config.IpAddress, config.Port, (byte)config.SlaveId);
            _plcClient.ReceiveTimeOut = 2000;
        }

        public async Task<OperateResult> ConnectAsync()
        {
            return await Task.Run(() => _plcClient.ConnectServer());
        }

        public OperateResult Disconnect()
        {
            return _plcClient.ConnectClose();
        }

        public async Task<OperateResult<UtilityLog>> ReadUtilityDataAsync()
        {
            return await Task.Run(() =>
            {
                var log = new UtilityLog
                {
                    LineId = _config.Id,
                    LogTime = DateTime.Now
                };

                try
                {
                    // NOT: Sensör veri tipleri (Int32, Float vb.) PLC'ye göre değişebilir.
                    // Burada standart Int32 okuma yapıyoruz.

                    var AirRes = _plcClient.ReadInt32(_config.AirAddress.ToString());
                    if (AirRes.IsSuccess) log.AirCounter = AirRes.Content;
                    else return OperateResult.CreateFailedResult<UtilityLog>(AirRes);

                    var elecRes = _plcClient.ReadInt32(_config.ElecAddress.ToString());
                    if (elecRes.IsSuccess) log.ElecCounter = elecRes.Content;
                    else return OperateResult.CreateFailedResult<UtilityLog>(elecRes);

                    var steamRes = _plcClient.ReadInt32(_config.SteamAddress.ToString());
                    if (steamRes.IsSuccess) log.SteamCounter = steamRes.Content;
                    else return OperateResult.CreateFailedResult<UtilityLog>(steamRes);

                    var airRes = _plcClient.ReadInt32(_config.AirAddress.ToString());
                    if (airRes.IsSuccess) log.AirCounter = airRes.Content;
                    else return OperateResult.CreateFailedResult<UtilityLog>(airRes);

                    return OperateResult.CreateSuccessResult(log);
                }
                catch (Exception ex)
                {
                    return new OperateResult<UtilityLog>($"Okuma hatası: {ex.Message}");
                }
            });
        }
    }
}