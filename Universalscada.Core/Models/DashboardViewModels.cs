using System;
using System.Collections.Generic;

namespace Universalscada.Models
{
    // Dashboard Grafikleri
    public class HourlyConsumptionData { public string Hour { get; set; } public double Value { get; set; } }
    public class HourlyOeeData { public string Hour { get; set; } public double OeePercentage { get; set; } }
  

    // Üretim Detay Modeli (DTO Hatalarını Çözer)
    public class ProductionDetailDto
    {
        // "Header" özelliği eksikti, bu nesne başlık bilgilerini tutar
        public ProductionHeaderDto Header { get; set; } = new ProductionHeaderDto();

        public List<ProductionStepDetailDto> Steps { get; set; } = new List<ProductionStepDetailDto>();
        public List<AlarmDetail> Alarms { get; set; } = new List<AlarmDetail>(); // Eksik Alarms listesi
    }

    public class ProductionHeaderDto
    {
        public string MachineName { get; set; }
        public string BatchNumarasi { get; set; }
        public string RecipeName { get; set; }
        public string OperatorName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
    }

    public class ProductionStepDetailDto
    {
        public int StepNumber { get; set; }
        public string StepDescription { get; set; } // Eksikti
        public string StepType { get; set; }
        public double TheoreticalDurationSeconds { get; set; } // Eksikti
        public double WorkingTime { get; set; } // Eksikti (Gerçekleşen süre)
        public double Temperature { get; set; } // Eksikti
        public double WaterConsumption { get; set; }
        public double EnergyConsumption { get; set; }
    }

    // Filtreleme Modelleri
    public class ReportFilters
    {
        public DateTime StartTime { get; set; } = DateTime.Today;
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1).AddTicks(-1);
        public int? MachineId { get; set; }

        // Eksik property'ler eklendi
        public string RecipeName { get; set; }
        public string SiparisNo { get; set; }
        public string MusteriNo { get; set; }
        public string OperatorName { get; set; }
        public string BatchNo { get; set; }
    }

    public class ActionLogFilters
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Username { get; set; }
        public string ActionType { get; set; }
        public string Details { get; set; } // Eksikti
    }
    // Rapor Filtreleri için Eksikler
    public class GeneralDetailedConsumptionFilters
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<int> MachineIds { get; set; }
    }

    public class GeneralConsumptionExportDto
    {
        public List<dynamic> Items { get; set; }
        public string ConsumptionType { get; set; }
    }

    // JS Grafik Modeli
    public class JsReadyTrendDataPoint { public long X { get; set; } public double Y { get; set; } }
}