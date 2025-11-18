using HslCommunication;
using HslCommunication.ModBus;
using System;
using System.Threading.Tasks;
using Universalscada.Models;
using Universalscada.Services;

namespace Universalscada.Module.Textile.Services
{
    public class KurutmaMakinesiManager : IPlcManager
    {
        private readonly ModbusTcpNet _plcClient;
        public string IpAddress { get; private set; }

        public KurutmaMakinesiManager(string ipAddress, int port)
        {
            _plcClient = new ModbusTcpNet(ipAddress, port > 0 ? port : 502);
            this.IpAddress = ipAddress;
            _plcClient.ReceiveTimeOut = 5000;
        }

        public OperateResult Connect()
        {
            // IsConnected özelliği her zaman mevcut olmayabilir, ConnectServer kullanın
            return _plcClient.ConnectServer();
        }

        public OperateResult Disconnect()
        {
            return _plcClient.ConnectClose();
        }

        // === IPlcManager Implementation ===

        public async Task<OperateResult<short[]>> ReadDataWordsAsync(string address, ushort length)
        {
            return await _plcClient.ReadInt16Async(address, length);
        }

        public async Task<OperateResult> WriteDataWordsAsync(string address, short[] data)
        {
            return await _plcClient.WriteAsync(address, data);
        }

        public async Task<OperateResult> AcknowledgeAlarm()
        {
            // HATA DÜZELTME: WriteCoilAsync yerine standart WriteAsync(bool) kullanılır
            return await _plcClient.WriteAsync("1000", true);
        }

        public async Task<OperateResult> ResetOeeCountersAsync()
        {
            // Implementasyon eklenebilir
            await Task.Delay(10);
            return OperateResult.CreateSuccessResult();
        }

        public async Task<OperateResult> IncrementProductionCounterAsync()
        {
            await Task.Delay(10);
            return OperateResult.CreateSuccessResult();
        }

        public async Task<OperateResult> WriteRecipeNameAsync(int recipeNumber, string recipeName)
        {
            // Basit implementasyon
            return await Task.FromResult(OperateResult.CreateSuccessResult());
        }
    }
}