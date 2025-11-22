using Microsoft.AspNetCore.Mvc;
using TekstilScada.Core.Models;
using TekstilScada.Repositories;
using TekstilScada.WebApp.Models; // DTO'lar için (ActionLogFilters vb.)

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionLogsController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        // UserRepository zaten Singleton/Scoped olarak kayıtlı olmalı
        public ActionLogsController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // 1. LOGLAMA ENDPOINT'İ (UI'dan işlem yapıldığında çağrılır)
        [HttpPost]
        public IActionResult LogAction([FromBody] ActionLogEntry entry)
        {
            try
            {
                // Güvenlik Notu: Gerçek senaryoda UserId'yi Token'dan (User.Claims) almak daha güvenlidir.
                // Şimdilik UI'dan gelen ID'ye güveniyoruz.
                if (entry.UserId <= 0)
                {
                    // Eğer UI userId göndermediyse veya anonimse işlem yapma (veya System ID ata)
                    return BadRequest("Geçersiz Kullanıcı ID");
                }

                _userRepository.LogAction(entry.UserId, entry.ActionType, entry.Details);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Loglama hatası: {ex.Message}");
            }
        }

        // 2. RAPORLAMA ENDPOINT'İ (Raporlar sayfasında listelemek için)
        // Filters nesnesi ScadaDataService.cs içinde tanımladığımız "ActionLogFilters" sınıfıdır.
        [HttpPost("report")]
        public IActionResult GetActionLogs([FromBody] ActionLogFilters filters)
        {
            try
            {
                var logs = _userRepository.GetActionLogs(
                    filters.StartTime,
                    filters.EndTime,
                    filters.Username,
                    filters.Details
                );

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Logları getirme hatası: {ex.Message}");
            }
        }
    }
}