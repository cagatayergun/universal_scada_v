// Universalscada.Core/Services/IPlcManager.cs - KÖKLÜ DEĞİŞİKLİK
using HslCommunication;
using System.Collections.Generic;
using System.Threading.Tasks;
// Domain modellerine olan bağımlılık kaldırıldı.
// using Universalscada.Models; // BU SATIRLAR KALDIRILMALI

namespace Universalscada.Services
{
    /// <summary>
    /// Farklı PLC tipleriyle iletişimi soyutlayan evrensel arayüz.
    /// Arayüz, domaine özel veri tipleri yerine JENERİK HAM VERİ okuma/yazma üzerine odaklanarak evrensel hale getirildi.
    /// </summary>
    public interface IPlcManager
    {
        string IpAddress { get; }

        OperateResult Connect();
        OperateResult Disconnect();

        // JENERİK HAM VERİ OKUMA/YAZMA İŞLEMLERİ (short[] = Word, byte[] = Byte)

        /// <summary>
        /// PLC'den belirtilen adresten ham Word (short[]) verisi okur.
        /// </summary>
        Task<OperateResult<short[]>> ReadDataWordsAsync(string address, ushort length);

        /// <summary>
        /// PLC'ye belirtilen adrese ham Word (short[]) verisi yazar.
        /// </summary>
        Task<OperateResult> WriteDataWordsAsync(string address, short[] data);

        // Not: Eski ReadFullRecipeDataAsync, ReadLiveStatusData, ReadBatchSummaryData gibi 
        // metotlar artık üst katmanda bu jenerik metotları kullanarak dönüştürme yapmalıdır.

        // Ancak, kritik ve ortak PLC komutları bu arayüzde tutulabilir:

        /// <summary>
        /// Reçete adı gibi metinsel verileri PLC'ye yazar. (Hala bir miktar domain bilgisi içerir, 
        /// ancak yaygın kullanım için tutulabilir.)
        /// </summary>
        Task<OperateResult> WriteRecipeNameAsync(int recipeNumber, string recipeName);

        // Temel SCADA Komutları
        Task<OperateResult> AcknowledgeAlarm();
        Task<OperateResult> ResetOeeCountersAsync();
        Task<OperateResult> IncrementProductionCounterAsync();
    }
}