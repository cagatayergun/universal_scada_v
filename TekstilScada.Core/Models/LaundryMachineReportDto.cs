using System;
using System.Text.Json.Serialization;

namespace Telemetry.Core.Models
{
    public class LaundryMachineReportDto
    {
        [JsonPropertyName("Date")]
        public string Date { get; set; } // yyyy-MM-dd formatında göndermek için string

        [JsonPropertyName("Machine_ID")]
        public string MachineId { get; set; }

        [JsonPropertyName("Machine_IP")]
        public string MachineIp { get; set; }

        // Dikkat: JSON çıktısında boşluk olabilmesi için JsonPropertyName kullanıyoruz
        [JsonPropertyName("Machine Name")]
        public string MachineName { get; set; }

        [JsonPropertyName("Machine_Type")]
        public string MachineType { get; set; }

        [JsonPropertyName("Start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("End_Time")]
        public string EndTime { get; set; }

        [JsonPropertyName("Duration_mins")]
        public int DurationMins { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("Reason Type")]
        public string ReasonType { get; set; }

        [JsonPropertyName("Reason")]
        public string Reason { get; set; }

        [JsonPropertyName("Recipe_id")]
        public string RecipeId { get; set; }

        [JsonPropertyName("Factory_Order")]
        public string AzgardOrder { get; set; }

        [JsonPropertyName("telematric_user")]
        public string TelematricUser { get; set; }

        [JsonPropertyName("machine_operator_id")]
        public string MachineOperatorId { get; set; }

        [JsonPropertyName("machine_operator_name")]
        public string MachineOperatorName { get; set; }
    }
}