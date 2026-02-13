public class AlarmDetail
{
    public int StepNumber { get; set; } // UI'daki liste sırası için
    public int AlarmNumber { get; set; } // ID aralığı kontrolü için (0-499, 500-600)
    public string AlarmDescription { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    // UI'ın beklediği süre formatı
    public string Duration
    {
        get
        {
            if (!EndTime.HasValue) return "Active";
            TimeSpan ts = EndTime.Value - StartTime;
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}