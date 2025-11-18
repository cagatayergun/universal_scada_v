// Universalscada.Core/Services/LiveStepAnalyzer.cs
using System.Collections.Generic;
using System.Linq;
using Universalscada.Core.Meta;
using Universalscada.Core.Repositories;
using Universalscada.Models;

namespace Universalscada.Core.Services
{
    /// <summary>
    /// PLC'den gelen ham veriyi (short[]) alıp, MetaDataRepository'den okunan
    /// adrese dayalı yapılandırmayı kullanarak evrensel FullMachineStatus'a dönüştüren hizmet.
    /// </summary>
    public class LiveStepAnalyzer
    {
        private readonly IMetaDataRepository _metaDataRepository;

        public LiveStepAnalyzer(IMetaDataRepository metaDataRepository)
        {
            _metaDataRepository = metaDataRepository;
        }

        /// <summary>
        /// Ham PLC verisini (PLC'den okunan tam blok) FullMachineStatus nesnesine eşler.
        /// </summary>
        /// <param name="machine">İşlenen makine.</param>
        /// <param name="rawPlcData">PLC'den okunan tüm ham veri (short[]).</param>
        /// <returns>Dinamik LiveDataPoints içeren FullMachineStatus.</returns>
        public FullMachineStatus Analyze(Machine machine, short[] rawPlcData)
        {
            var status = new FullMachineStatus
            {
                MachineId = machine.Id,
                MachineName = machine.MachineName,
                ConnectionState = ConnectionStatus.Connected, // Başarılı okundu varsayımı
            };

            // Ham verilere erişim için DynamicStepDataAccessor'ı kullanalım
            var accessor = new DynamicStepDataAccessor(rawPlcData);

            // NOT: Gerçek projede LiveDataPoint eşlemesi Machine.MachineConfigurationJson'dan çekilmelidir.
            // Örnek eşleme:
            status.LiveDataPoints.Add("PROCESS_TEMPERATURE", new LiveValue
            {
                Key = "TEMPERATURE_PV",
                DisplayName = "Anlık Sıcaklık",
                Unit = "°C",
                Value = accessor.GetShort(400) // Ham verideki D400'ün indexi (örnektir)
            });

            // Reçete Adımı Analizi:
            // Adım Numarası (Örn: D99'dan oku) - Word indexi 99 varsayımı
            status.AktifAdimNo = accessor.GetShort(99);

            if (status.AktifAdimNo > 0)
            {
                // Kontrol Word'ü (Örn: D124'ten oku) - Word indexi 124 varsayımı
                short activeControlWord = accessor.GetShort(124);

                status.AktifAdimAdi = FindActiveStepName(activeControlWord);
            }

            // Temel Durum Bayrakları
            // Alarm Durumu (Örn: M100 - Word indexi 0, 4. biti varsayımı)
            status.HasActiveAlarm = accessor.GetBit(0, 4);
            status.ConnectionState = ConnectionStatus.Connected; // Örnek olarak bırakıldı.

            return status;
        }

        private string FindActiveStepName(short controlWord)
        {
            // Veritabanından tüm adım tiplerini ve bit maskelerini getir.
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();

            foreach (var stepDef in stepDefinitions)
            {
                // DÜZELTME: CS1061 hatası giderildi. 
                // ControlWordBit'in int olduğu (veya int? olmasına rağmen hata verdiği) varsayılarak, 
                // null kontrolü yerine 0'dan büyük kontrolü yapılır ve değer doğrudan kullanılır.
                if (stepDef.ControlWordBit > 0 && (controlWord & (1 << stepDef.ControlWordBit)) != 0)
                {
                    // Aktif olan ilk adımı döndür.
                    return stepDef.DisplayNameKey;
                }
            }

            return "Boşta";
        }
    
}
}