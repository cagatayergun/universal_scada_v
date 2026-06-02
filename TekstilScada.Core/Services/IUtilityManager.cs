using HslCommunication;
using System.Threading.Tasks;
using Telemetry.Models;

namespace Telemetry.Services
{
    public interface IUtilityManager
    {
        string IpAddress { get; }

        // Bağlantı
        Task<OperateResult> ConnectAsync();
        OperateResult Disconnect();

        // Veri Okuma
        Task<OperateResult<UtilityLog>> ReadUtilityDataAsync();
    }
}