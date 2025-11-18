// Dosya: Universalscada.Module.Textile/KurutmaMakinesiManager.cs - DÜZELTİLMİŞ VERSİYON

using HslCommunication;
using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Universalscada.Models;
using Universalscada.Services;

namespace Universalscada.Module.Textile.Services
{
    public class KurutmaMakinesiManager : IPlcManager
    {
        private readonly ModbusTcpNet _plcClient;
        public string IpAddress { get; private set; }

        // Adresler, eski ReadLiveStatusData'dan kalan, referans amaçlıdır.
        private const string STEP_NO = "3000";
        private const int MODBUS_PORT = 502; // Modbus için varsayılan port

        public KurutmaMakinesiManager(string ipAddress, int port)
        {
            // Port bilgisi machine modelinden alınsa bile Modbus için default 502 kullanılabilir.
            _plcClient = new ModbusTcpNet(ipAddress, port > 0 ? port : MODBUS_PORT);
            this.IpAddress = ipAddress;
            _plcClient.ReceiveTimeOut = 5000;
        }

        public OperateResult Connect()
        {
            // Tekrar bağlanmayı denemeden önce client bağlantısının açık olup olmadığını kontrol edebiliriz.
            if (_plcClient.IsConnected) return OperateResult.CreateSuccessResult();

            var result = _plcClient.ConnectServer();
            // Not: HslCommunication'da ConnectServer tekrar tekrar çağrılabilir
            return result;
        }

        public OperateResult Disconnect()
        {
            return _plcClient.ConnectClose();
        }

        // === IPlcManager - JENERİK BLOK OKUMA/YAZMA METOTLARI ===

        /// <summary>
        /// PLC'den belirtilen adresten ham Word (short[]) verisi okur (Jenerik Polling için kullanılır).
        /// </summary>
        public async Task<OperateResult<short[]>> ReadDataWordsAsync(string address, ushort length)
        {
            // Modbus'ta ReadInt16 metodu word dizisi okur.
            // LIVE_DATA_START_ADDRESS ve LIVE_DATA_LENGTH bu metot tarafından okunacaktır.
            // Bu tek ve büyük okuma, eski 20+ küçük okumanın yerini alır.
            return await _plcClient.ReadInt16Async(address, length);
        }

        /// <summary>
        /// PLC'ye belirtilen adrese ham Word (short[]) verisi yazar.
        /// </summary>
        public async Task<OperateResult> WriteDataWordsAsync(string address, short[] data)
        {
            // Modbus'ta Write(string address, short[] data) metodunu kullanır.
            return await _plcClient.WriteAsync(address, data);
        }

        // === KALDIRILAN/GÜNCELLENEN METOTLAR ===

        // Polling servisinin blok okuma mimarisine geçmesi nedeniyle KALDIRILDI.
        // Artık PollingService/LiveStepAnalyzer, ReadDataWordsAsync'ten gelen ham short[]'u analiz eder.
        [Obsolete("Bu metot, ReadDataWordsAsync ile değiştirildi ve LiveStepAnalyzer'a taşındı. Eski kod ile uyumluluk için geçici olarak kaldırıldı.")]
        // public OperateResult<FullMachineStatus> ReadLiveStatusData() { ... } 
        // Eski gövdesi silinmelidir:
        public OperateResult<FullMachineStatus> ReadLiveStatusData()
        {
            // Implementasyon kaldırıldığı için NotImplementedException fırlatılmalı
            // Ancak PollingService'in eski koddan kalma bir çağrısı varsa kırılmaması için
            // geçici olarak hata döndürülebilir veya arayüzden çıkarılmalıdır.
            throw new NotImplementedException("ReadLiveStatusData metodu, ReadDataWordsAsync ile değiştirildi ve LiveStepAnalyzer'a taşındı.");
        }

        // Diğer Eski Metotlar (Arayüze uymayanlar) kaldırıldı.
        public async Task<OperateResult> ResetOeeCountersAsync()
        {
            throw new NotImplementedException("ResetOeeCountersAsync henüz uygulanmadı.");
        }

        public async Task<OperateResult> IncrementProductionCounterAsync()
        {
            throw new NotImplementedException("IncrementProductionCounterAsync henüz uygulanmadı.");
        }

        // AcknowledgeAlarm metodu, MachinesController.cs'de çağrıldığı için korundu.
        public async Task<OperateResult> AcknowledgeAlarm()
        {
            // Basit bir örnek: D1000.0'ı True yap (Modbus Coil 1000'i)
            // Bu adresi MachineConfigurationJson'dan okumak idealdir.
            const string ACK_ADDRESS = "1000"; // Örnek adres
            var result = await _plcClient.WriteCoilAsync(ACK_ADDRESS, true);
            if (result.IsSuccess)
            {
                // Belki bir süre sonra biti sıfırlama işlemi gerekebilir. (Tekrar false yapma)
                // await Task.Delay(100); 
                // await _plcClient.WriteCoilAsync(ACK_ADDRESS, false);
            }
            return result;
        }

        // Reçete Yazma (WriteRecipeToPlcAsync) ve diğer tüm yardımcı metotlar KALDIRILMALI VEYA WriteDataWordsAsync'i kullanacak şekilde güncellenmelidir.
        // Ancak bu dosyanın kapsamı dışında oldukları için şimdilik sadece domain modeli döndürenler silinmiştir.
        // Aşağıdaki metotlar, IPlcManager'a uymadığı için arayüzden kaldırıldı (veya kaldırılmalı).

        public Task<OperateResult> WriteRecipeToPlcAsync(ScadaRecipe recipe, int? recipeSlot = null)
        {
            throw new NotImplementedException("WriteRecipeToPlcAsync, WriteDataWordsAsync'i kullanacak şekilde güncellenmelidir.");
        }

        // ReadStringFromWords yardımcı metot olarak kaldı, ancak artık dışarıdan çağrılmamalı.
        private OperateResult<string> ReadStringFromWords(string address, ushort wordLength)
        {
            // ... (İçerik değişmedi, sadece ReadInt16'nın senkron versiyonu kullanıldı)
            var readResult = _plcClient.ReadInt16(address, wordLength);
            if (!readResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(new OperateResult($"Could not read address block: {address}, Error: {readResult.Message}"));
            }

            try
            {
                byte[] byteData = new byte[readResult.Content.Length * 2];
                Buffer.BlockCopy(readResult.Content, 0, byteData, 0, byteData.Length);
                string value = Encoding.ASCII.GetString(byteData).Trim('\0', ' ');
                return OperateResult.CreateSuccessResult(value);
            }
            catch (Exception ex)
            {
                return new OperateResult<string>($"Error during string conversion: {ex.Message}");
            }
        }

        // Bu metotlar arayüzden kaldırıldığı için kaldırıldı.
        // public async Task<OperateResult<ScadaRecipe>> ReadFullRecipeDataAsync() { ... }
        // public async Task<OperateResult<Dictionary<int, string>>> ReadRecipeNamesFromPlcAsync() { ... }
        // public async Task<OperateResult<short[]>> ReadRecipeFromPlcAsync() { ... }
        // public async Task<OperateResult<List<PlcOperator>>> ReadPlcOperatorsAsync() { ... }
        // public async Task<OperateResult<PlcOperator>> ReadSinglePlcOperatorAsync(int slotIndex) { ... }
        // public async Task<OperateResult<BatchSummaryData>> ReadBatchSummaryDataAsync() { ... }
        // public Task<OperateResult<List<ChemicalConsumptionData>>> ReadChemicalConsumptionDataAsync() { ... }
        // public Task<OperateResult<List<ProductionStepDetail>>> ReadStepAnalysisDataAsync() { ... }

        // WriteRecipeNameAsync metodu arayüzde olduğu için kaldı.
        public async Task<OperateResult> WriteRecipeNameAsync(int recipeNumber, string recipeName)
        {
            try
            {
                // Recipe names start from D3212, each name is 6 words (12 bytes)
                const int startAddress = 4000;
                const int wordsPerName = 6;

                // Calculate the PLC address (for recipe number starting from 1)
                int currentAddress = startAddress + (recipeNumber - 1) * wordsPerName;

                // Convert the recipe name into a byte array with a length of 12 bytes (6 words).
                // Truncate if too long, pad with null characters if too short.
                byte[] dataToWrite = new byte[wordsPerName * 2];
                byte[] nameBytes = Encoding.ASCII.GetBytes(recipeName);
                Buffer.BlockCopy(nameBytes, 0, dataToWrite, 0, Math.Min(nameBytes.Length, dataToWrite.Length));

                // Start the write operation to the PLC.
                var writeResult = await Task.Run(() => _plcClient.Write(currentAddress.ToString(), dataToWrite));

                return writeResult;
            }
            catch (Exception ex)
            {
                return new OperateResult($"An error occurred while writing the recipe name: {ex.Message}");
            }
        }

        // WritePlcOperatorAsync metodu arayüze uymadığı için kaldırıldı.
        public async Task<OperateResult> WritePlcOperatorAsync(PlcOperator plcOperator)
        {
            throw new NotImplementedException("WritePlcOperatorAsync metodu, WriteDataWordsAsync ile değiştirilmelidir.");
        }

    }
}