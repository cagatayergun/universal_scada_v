// Dosya: Universalscada.core/Core/ConfigurationManager.cs
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Universalscada.core
{
    public static class AppConfig
    {
        public static string ConnectionString { get; private set; }

        // YENİ: Bağlantı dizesini dışarıdan ayarlamak için metot
        public static void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}