using System;

namespace Universalscada.Models
{
    /// <summary>
    /// Trend grafiğinde gösterilecek tek bir veri noktasını temsil eder.
    /// </summary>
    public class TrendDataPoint
    {
        /// <summary>
        /// Verinin zaman damgası.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Verinin sayısal değeri (örn: sıcaklık, basınç).
        /// </summary>
        public double Value { get; set; }
    }
}
