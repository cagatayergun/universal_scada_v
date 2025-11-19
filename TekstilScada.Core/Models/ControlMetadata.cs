using System.Text.Json.Serialization;

namespace TekstilScada.Models
{
    public class ControlMetadata
    {
        // DÜZELTME: String alanların sonuna '?' koyarak boş (null) olabileceklerini belirttik.
        // Böylece API, bu alanlar boş gelse bile hata vermez (400 Bad Request yemezsiniz).

        public string? ControlType { get; set; }
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Location { get; set; }
        public string? Size { get; set; }

        public decimal Maximum { get; set; } = 1000;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int DecimalPlaces { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int StringWordLength { get; set; }

        [JsonPropertyName("PLC_WordIndex")]
        public int PLC_WordIndex { get; set; }

        [JsonPropertyName("PLC_BitIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int PLC_BitIndex { get; set; }
    }

    public class PlcMapping
    {
        public int WordIndex { get; set; }
        public int BitIndex { get; set; }
        public int StringWordLength { get; set; }
    }

    public class StepTypeDtoDesign
    {
        public int Id { get; set; }
        public string StepName { get; set; }
    }
}