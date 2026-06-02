namespace Telemetry.Models
{
    public class EfficiencyLog
    {
        public long Id { get; set; } // Primary Key (BIGINT)
        public int MachineId { get; set; }
        public string MachineName { get; set; } // SQL JOIN ile dolacak
        public string MachineSubType { get; set; }
        public string State { get; set; } // "AUTO", "MANUAL", "WAIT"
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double DurationSeconds { get; set; }
        public string? RecipeName { get; set; }

        // PLC'den gelen 5 farklı bekleme nedeni (Seçilenler)
        public string? Reason1 { get; set; }
        public string? Reason2 { get; set; }
        public string? Reason3 { get; set; }
        public string? Reason4 { get; set; }
        public string? Reason5 { get; set; }
    }
}