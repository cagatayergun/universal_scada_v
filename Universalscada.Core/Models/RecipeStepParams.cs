// Universalscada.Core/Models/RecipeStepParams.cs - KÖKLÜ DEĞİŞİKLİK
using System;
using System.Text;
using System.Linq;

namespace Universalscada.Models
{
    /// <summary>
    /// PLC'den gelen reçete adımı ham verisine, pozisyon veya domain bilgisi olmadan 
    /// dinamik ve güvenli erişimi sağlayan jenerik sınıf (Word Accessor).
    /// Eski domain'e özgü sınıfların yerine bu tek sınıf kullanılacaktır.
    /// </summary>
    public class DynamicStepDataAccessor
    {
        private readonly short[] _data;

        public int Length => _data.Length;

        public DynamicStepDataAccessor(short[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Parametre veri dizisi null olamaz.");
            }
            // Artık sabit boyutu zorunlu kılmıyoruz, esneklik için.
            _data = data;
        }

        /// <summary> Belirtilen word indexindeki short (word) değerini alır. </summary>
        public short GetShort(int wordIndex)
        {
            if (wordIndex < 0 || wordIndex >= _data.Length) return 0;
            return _data[wordIndex];
        }

        /// <summary> Belirtilen word indexindeki short (word) değerini ayarlar. </summary>
        public void SetShort(int wordIndex, short value)
        {
            if (wordIndex >= 0 && wordIndex < _data.Length)
            {
                _data[wordIndex] = value;
            }
        }

        /// <summary> Belirtilen word indexi ve bit indexindeki bool (bit) değerini alır. </summary>
        public bool GetBit(int wordIndex, int bitIndex)
        {
            if (wordIndex < 0 || wordIndex >= _data.Length || bitIndex < 0 || bitIndex > 15) return false;
            return (_data[wordIndex] & (1 << bitIndex)) != 0;
        }

        /// <summary> Belirtilen word indexi ve bit indexindeki bool (bit) değerini ayarlar. </summary>
        public void SetBit(int wordIndex, int bitIndex, bool value)
        {
            if (wordIndex < 0 || wordIndex >= _data.Length || bitIndex < 0 || bitIndex > 15) return;

            if (value)
            {
                _data[wordIndex] = (short)(_data[wordIndex] | (1 << bitIndex));
            }
            else
            {
                _data[wordIndex] = (short)(_data[wordIndex] & ~(1 << bitIndex));
            }
        }

        /// <summary> Belirtilen başlangıç word'ünden belirli bir uzunlukta (word cinsinden) string okur. </summary>
        public string GetString(int startWordIndex, int wordLength)
        {
            int byteLength = wordLength * 2;
            byte[] stringBytes = new byte[byteLength];

            Buffer.BlockCopy(_data, startWordIndex * 2, stringBytes, 0, Math.Min(byteLength, (_data.Length - startWordIndex) * 2));

            return Encoding.ASCII.GetString(stringBytes).Trim('\0');
        }
    }
}