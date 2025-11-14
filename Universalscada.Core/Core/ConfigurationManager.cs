// Dosya: Universalscada.Core/Core/ConfigurationManager.cs - KÖKLÜ DEĞİŞİKLİK
using Microsoft.Extensions.Configuration; // IConfiguration'ı kullanmak için bu referans gerekli
using System.IO;

namespace Universalscada.core
{
    /// <summary>
    /// Evrensel kullanım için genişletilmiş ve daha esnek yapılandırma sınıfı.
    /// </summary>
    public static class AppConfig
    {
        public static string PrimaryConnectionString { get; private set; }
        public static string LogConnectionString { get; private set; } // YENİ: İkincil bağlantı dizesi (örn. Log veya Raporlama Veritabanı)

        // YENİ: Evrensel ayarlar için alanlar
        public static string DefaultLanguageCode { get; private set; }
        public static string PlcDefaultProtocol { get; private set; }

        // IConfiguration'dan ayarları yükleyen metot (ASP.NET Core / WebAPI projesi için)
        public static void Initialize(IConfiguration configuration)
        {
            PrimaryConnectionString = configuration.GetConnectionString("PrimaryScadaDb");
            LogConnectionString = configuration.GetConnectionString("LogDb");
            DefaultLanguageCode = configuration["AppSettings:DefaultLanguage"] ?? "tr"; // Varsayılan değer ataması
            PlcDefaultProtocol = configuration["ScadaSettings:DefaultProtocol"] ?? "ModbusTcp";
        }

        // Eski metot, uyumluluk için bırakılabilir veya kaldırılabilir.
        public static void SetConnectionString(string connectionString)
        {
            PrimaryConnectionString = connectionString;
        }
    }
}