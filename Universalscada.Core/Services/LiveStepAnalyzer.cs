// Dosya: Universalscada.Core/Services/LiveStepAnalyzer.cs - DÜZELTİLMİŞ VE DİNAMİK VERSİYON

using System.Collections.Generic;
using System.Linq;
using Universalscada.Core.Meta;
using Universalscada.Core.Repositories;
using Universalscada.Models;
using Universalscada.Core.Models;
using System.Threading.Tasks;

namespace Universalscada.Core.Services
{
    // *** YARDIMCI SINIF EKLEMESİ: HAM VERİYE ERİŞİM İÇİN ***
    /// <summary>
    /// PLC'den okunan ham word dizisindeki (short[]) verilere index ve bit bazında erişim sağlar.
    /// </summary>
    public class DynamicStepDataAccessor
    {
        private readonly short[] _data;

        public DynamicStepDataAccessor(short[] rawPlcData)
        {
            _data = rawPlcData;
        }

        public short GetShort(int wordIndex)
        {
            // Index sınırları içinde mi kontrol et
            if (wordIndex >= 0 && wordIndex < _data.Length)
            {
                return _data[wordIndex];
            }
            return 0;
        }

        public bool GetBit(int wordIndex, int bitIndex)
        {
            if (wordIndex >= 0 && wordIndex < _data.Length)
            {
                short word = _data[wordIndex];
                // Bit maskeleme: (word >> bitIndex) & 1
                return ((word >> bitIndex) & 1) != 0;
            }
            return false;
        }

        // Diğer veri tipleri (Int32, Float, String) için de metotlar buraya eklenebilir.
    }
    // ********************************************************


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
        public FullMachineStatus Analyze(Machine machine, short[] rawPlcData)
        {
            var status = new FullMachineStatus
            {
                MachineId = machine.Id,
                MachineName = machine.MachineName,
                ConnectionState = ConnectionStatus.Connected,
            };

            var accessor = new DynamicStepDataAccessor(rawPlcData);

            // 1. Makineye özel tüm PLC Tag Tanımlarını getir.
            // NOT: Repository metodu asenkron (Task<List<...>>) olsa da, Polling Service içinden 
            // senkron çağrıldığı için burada .Result (veya GetAwaiter().GetResult()) ile bekletmek gerekir.
            // En iyi çözüm önbelleklemedir, ancak kodun çalışması için şimdilik senkron sonuç beklenir:
            List<PlcTagDefinition> tags;
            try
            {
                // Task.Run ile asenkron metodu senkron çağırıyoruz (Dikkatli kullanılmalıdır!)
                tags = Task.Run(() => _metaDataRepository.GetAllPlcTagsAsync(machine.Id)).GetAwaiter().GetResult();
            }
            catch
            {
                // Veritabanı hatası durumunda boş liste ile devam et.
                tags = new List<PlcTagDefinition>();
            }


            // 2. Dinamik olarak canlı veri noktalarını eşle
            foreach (var tag in tags.Where(t => t.IsLiveStatus)) // Sadece canlı durum taglerini analiz et
            {
                short value = accessor.GetShort(tag.WordIndex); // Tag'in indexinden değeri oku

                status.LiveDataPoints.Add(tag.TagName, new LiveValue
                {
                    Key = tag.TagName,
                    DisplayName = tag.DisplayName,
                    Unit = tag.Unit,
                    Value = value
                });

                // 3. Kritik Durum Taglerini Ana Statü Alanlarına Eşle
                switch (tag.TagName)
                {
                    case "ACTIVE_STEP_NO":
                        status.AktifAdimNo = value;
                        break;
                    case "CONTROL_WORD":
                        // Kontrol Word'ü (Adım Analizi için kullanılır)
                        status.AktifAdimAdi = FindActiveStepName(value);
                        break;
                    case "ALARM_ACTIVE":
                        // Alarm Bayrağı (Genellikle 1 Word, 1 Bit veya doğrudan Alarm No)
                        status.HasActiveAlarm = value > 0;
                        break;
                        // Diğer temel durumlar (IsInRecipeMode, IsPaused vb.) buraya eklenebilir
                }
            }

            // Eğer Control Word Tags listesinde değilse, eski hardcoded mantığı koruyabiliriz:
            // if (status.AktifAdimNo > 0 && !status.LiveDataPoints.ContainsKey("CONTROL_WORD"))
            // {
            //     // Eski hardcoded mantık
            //     short activeControlWord = accessor.GetShort(124); 
            //     status.AktifAdimAdi = FindActiveStepName(activeControlWord);
            // }

            return status;
        }

        /// <summary>
        /// Kontrol Word'ündeki bitlere göre aktif adımı belirler.
        /// </summary>
        private string FindActiveStepName(short controlWord)
        {
            // Veritabanından tüm adım tiplerini ve bit maskelerini getir.
            // Bu metot zaten senkron olduğu için sorun yok.
            var stepDefinitions = _metaDataRepository.GetAllStepDefinitions();

            foreach (var stepDef in stepDefinitions)
            {
                // ControlWordBit'in 0'dan büyük olması gerekir (0. bit = 1)
                // Kontrol Word'ünde ilgili bit maskesi ayarlanmış mı?
                if (stepDef.ControlWordBit > 0)
                {
                    int bitMask = 1 << stepDef.ControlWordBit;

                    if ((controlWord & bitMask) != 0)
                    {
                        // Aktif olan ilk adımı döndür.
                        return stepDef.DisplayNameKey;
                    }
                }
            }

            return "Boşta";
        }
    }
}