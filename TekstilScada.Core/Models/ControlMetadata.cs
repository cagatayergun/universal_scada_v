using System.Text.Json.Serialization;

namespace TekstilScada.Models
{
    public class ControlMetadata
    {
        public string? ControlType { get; set; }
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Location { get; set; }
        public string? Size { get; set; }

        // --- GÖRSEL VE YAZI AYARLARI ---
        public string? BackColor { get; set; }
        public string? ForeColor { get; set; }
        public float FontSize { get; set; } = 9.75f;
        public bool FontBold { get; set; } = false;

        // --- HİZALAMA ---
        public string? ContentAlignment { get; set; }
        public string? HorizontalAlignment { get; set; }

        // --- YENİ: BUTON ÖZELLEŞTİRME (TOGGLE & STİL) ---
        public bool IsToggleButton { get; set; } = false;      // Basılı kalma özelliği
        public string? PressedText { get; set; }               // Basılıyken yazacak metin
        public string? PressedBackColor { get; set; }          // Basılıyken arka plan rengi
        public string? PressedForeColor { get; set; }          // Basılıyken yazı rengi
        public string? ButtonStyle { get; set; } = "Standard"; // Standard (Kabartma) veya Flat (Solid)
        // ----------------------------------------------------
        public bool ShowNumericArrows { get; set; } = true;
        public decimal Maximum { get; set; } = 1000;
        public decimal Minimum { get; set; } = 0;
        public bool IsMultiStateButton { get; set; } = false;
        public int MaxStateValue { get; set; } = 0;
        public List<MultiStateSetting> MultiStates { get; set; } = new List<MultiStateSetting>();

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
    public class MultiStateSetting
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public string BackColor { get; set; }
        public string ForeColor { get; set; }
        public string ImageBase64 { get; set; }
    }
    public class StepTypeDtoDesign
    {
        public int Id { get; set; }
        public string StepName { get; set; }
    }

}