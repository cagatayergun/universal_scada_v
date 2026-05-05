using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace TekstilScada.API.Services
{
    public class TelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly string _botToken;
        private readonly string _chatId;

        public TelegramService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            // Bu bilgileri Web API'nin appsettings.json dosyasından çekeceğiz
            _botToken = config["Telegram:BotToken"];
            _chatId = config["Telegram:ChatId"];
        }

        // YENİ EKLENEN: Metoda 'string factoryName' parametresi eklendi
        // TelegramService.cs içerisine EKLENECEK YENİ METOT
        public async Task SendAlarmListAsync(string factoryName, string machineName, string combinedAlarmsText)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(_chatId))
                return;

            // Eğer mesajda temizlenme ikonu varsa başlığı yeşil yap, yoksa kırmızı yap
            string header = combinedAlarmsText.Contains("✅")
                ? "🟢 <b>MAKİNE NORMALE DÖNDÜ</b>"
                : "🚨 <b>GÜNCEL ALARM LİSTESİ</b>";

            string message = $"{header}\n\n" +
                             $"🏭 <b>Fabrika:</b> {factoryName}\n" +
                             $"⚙️ <b>Makine:</b> {machineName}\n\n" +
                             $"{combinedAlarmsText}\n\n" +
                             $"🕒 <b>Zaman:</b> {System.DateTime.Now:dd.MM.yyyy HH:mm:ss}";

            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={HttpUtility.UrlEncode(message)}&parse_mode=HTML";

            try
            {
                await _httpClient.GetAsync(url);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Telegram gönderim hatası: {ex.Message}");
            }
        }
    }
}