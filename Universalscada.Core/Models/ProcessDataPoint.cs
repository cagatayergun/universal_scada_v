using System;

namespace Universalscada.Models
{
    /// <summary>
    /// Trend grafikleri ve loglar için kullanılan veri noktası.
    /// </summary>
    public class ProcessDataPoint
    {
        public int MachineId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Temperature { get; set; }
        public decimal WaterLevel { get; set; }
        public int Rpm { get; set; }
    }
}