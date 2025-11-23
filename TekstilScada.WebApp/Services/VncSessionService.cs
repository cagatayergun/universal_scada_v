using System.Collections.Concurrent;

namespace TekstilScada.WebApp.Services
{
    /// <summary>
    /// VNC bağlantılarını yöneten ve aynı anda tek kullanıcı kısıtlamasını sağlayan servis.
    /// </summary>
    public class VncSessionService
    {
        // Key: MachineId, Value: UserName (Kullanıcı Adı)
        // ConcurrentDictionary, thread-safe (eş zamanlı) erişim için gereklidir.
        private readonly ConcurrentDictionary<int, string> _activeSessions = new();

        /// <summary>
        /// Bir makineye bağlanmayı dener.
        /// </summary>
        /// <param name="machineId">Makine ID</param>
        /// <param name="userName">Bağlanmak isteyen kullanıcı</param>
        /// <returns>Başarılıysa (True, null), başarısızsa (False, MevcutKullaniciAdi)</returns>
        public (bool Success, string? CurrentUser) TryEnterSession(int machineId, string userName)
        {
            // Eğer makine zaten listede varsa ve kullanıcı farklıysa, başarısız dön
            if (_activeSessions.TryGetValue(machineId, out var currentViewer))
            {
                // Eğer izleyen kişi zaten kendisiyse (örn: F5 attı), izin ver
                if (currentViewer == userName)
                {
                    return (true, null);
                }

                // Başkası izliyor
                return (false, currentViewer);
            }

            // Kimse izlemiyorsa, listeye ekle (Lock)
            bool added = _activeSessions.TryAdd(machineId, userName);

            if (added)
            {
                return (true, null);
            }
            else
            {
                // Tam bu milisaniyede başka biri girdiyse tekrar kontrol et
                _activeSessions.TryGetValue(machineId, out var winner);
                return (false, winner);
            }
        }

        /// <summary>
        /// Makine bağlantısını sonlandırır (Kilidi açar).
        /// </summary>
        public void ExitSession(int machineId, string userName)
        {
            // Sadece kilidi koyan kişi kaldırabilir
            if (_activeSessions.TryGetValue(machineId, out var currentViewer))
            {
                if (currentViewer == userName)
                {
                    _activeSessions.TryRemove(machineId, out _);
                }
            }
        }

        /// <summary>
        /// Belirli bir kullanıcının tüm oturumlarını temizler (Logout durumunda)
        /// </summary>
        public void ClearUserSessions(string userName)
        {
            var machines = _activeSessions.Where(x => x.Value == userName).Select(x => x.Key).ToList();
            foreach (var mId in machines)
            {
                _activeSessions.TryRemove(mId, out _);
            }
        }
    }
}