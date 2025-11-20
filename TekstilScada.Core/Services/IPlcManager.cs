using HslCommunication;
using System.Collections.Generic;
using System.Threading.Tasks;
using TekstilScada.Models;

namespace TekstilScada.Services
{
    /// <summary>
    /// Farklı PLC tipleriyle iletişimi soyutlayan arayüz.
    /// Tüm PLC yönetici sınıfları bu arayüzü uygulamalıdır.
    /// </summary>
    public interface IPlcManager
    {
        string IpAddress { get; }

        // --- Bağlantı Yönetimi ---
        OperateResult Connect();
        OperateResult Disconnect();

        // YENİ: Asenkron Bağlantı
        Task<OperateResult> ConnectAsync();

        // --- Canlı Veri Okuma ---
        OperateResult<FullMachineStatus> ReadLiveStatusData();

        // YENİ: Asenkron Canlı Veri Okuma
        Task<OperateResult<FullMachineStatus>> ReadLiveStatusDataAsync();


        // --- Reçete İşlemleri ---
        Task<OperateResult<ScadaRecipe>> ReadFullRecipeDataAsync();
        Task<OperateResult<Dictionary<int, string>>> ReadRecipeNamesFromPlcAsync();
        Task<OperateResult> WriteRecipeNameAsync(int recipeNumber, string recipeName);
        Task<OperateResult<short[]>> ReadRecipeFromPlcAsync();
        Task<OperateResult> WriteRecipeToPlcAsync(ScadaRecipe recipe, int? recipeSlot = null);

        // --- Operatör İşlemleri ---
        Task<OperateResult<List<PlcOperator>>> ReadPlcOperatorsAsync();
        Task<OperateResult> WritePlcOperatorAsync(PlcOperator plcOperator);
        Task<OperateResult<PlcOperator>> ReadSinglePlcOperatorAsync(int slotIndex);

        // --- Diğer İşlemler ---
        Task<OperateResult<BatchSummaryData>> ReadBatchSummaryDataAsync();
        Task<OperateResult> AcknowledgeAlarm();
        Task<OperateResult> ResetOeeCountersAsync();
        Task<OperateResult> IncrementProductionCounterAsync();
    }
}