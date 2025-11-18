// Universalscada.Core/Models/DynamicStepParams.cs (Yeni dosya, RecipeStepParams.cs yerine)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Universalscada.Core.Meta; // Yeni meta veri namespace'i

namespace Universalscada.Models
{
    /// <summary>
    /// PLC'ye gönderilen 25 word'lük ham veriyi dinamik olarak erişim için tutan temel sınıf.
    /// Parametre adları ve word endeksleri veritabanından alınır.
    /// </summary>
    public class DynamicStepParams // Abstract BaseStepParams'tan concrete bir sınıfa dönüştürülüyor
    {
        private readonly short[] _data;
        // Bu adımın hangi tip olduğunu dinamik olarak tutan bir yapı
        public StepTypeDefinition StepType { get; private set; }

        // Gerekli olacaksa StepTypeDefinition ve Mapping nesneleri constructor'a alınır.
        // Şimdilik sadece ham veriyi alalım ve dinamiği başka bir serviste (StepParamMapper) yapalım.
        public DynamicStepParams(short[] data)
        {
            if (data == null || data.Length != 25)
            {
                throw new ArgumentException("Parametre veri dizisi 25 elemanlı olmalıdır.", nameof(data));
            }
            _data = data;
        }

        public short[] GetRawData() => _data;

        // Bütün parametrelere dinamik erişim için ana metotlar.
        // Bu metotlar, veritabanından alınan Word Index bilgisini kullanır.

        /// <summary>
        /// Tanımlanan parametre anahtarına göre Short (Word) değerini alır.
        /// Örn: GetShortParam("QUANTITY") -> 100
        /// </summary>
        public short GetShortParam(string parameterKey, int wordIndex)
        {
            // Normalde bu wordIndex bilgisi (1, 3, 4 vb.) DB'den gelmelidir.
            if (wordIndex >= 0 && wordIndex < 25)
            {
                return _data[wordIndex];
            }
            throw new IndexOutOfRangeException($"Parametre anahtarı '{parameterKey}' için word indexi ({(wordIndex)}) geçersiz.");
        }

        /// <summary>
        /// Tanımlanan parametre anahtarına göre Short (Word) değerini ayarlar.
        /// </summary>
        public void SetShortParam(string parameterKey, int wordIndex, short value)
        {
            // Normalde bu wordIndex bilgisi (1, 3, 4 vb.) DB'den gelmelidir.
            if (wordIndex >= 0 && wordIndex < 25)
            {
                _data[wordIndex] = value;
            }
            else
            {
                throw new IndexOutOfRangeException($"Parametre anahtarı '{parameterKey}' için word indexi ({(wordIndex)}) geçersiz.");
            }
        }

        // Bit/Boolean parametreleri için de benzer metotlar kullanılabilir.
        // ...

    }
}