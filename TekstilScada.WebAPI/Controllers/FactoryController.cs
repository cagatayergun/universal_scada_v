using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Telemetry.Models;
using Telemetry.WebAPI.Models;
using Telemetry.WebAPI.Repositories;

namespace Telemetry.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class FactoryController : ControllerBase
    {
        private readonly CentralFactoryRepository _factoryRepo;

        public FactoryController(CentralFactoryRepository factoryRepo)
        {
            _factoryRepo = factoryRepo;
        }


        [HttpGet("my-factories")]
        [AllowAnonymous] // <--- 1. KRİTİK: Kapıdaki güvenlik görevlisini kaldırıyoruz
        public IActionResult GetMyFactories()
        {
            try
            {
                // --- 2. BACKDOOR (Arka Kapı) KONTROLÜ ---
                // Gelen istekte bizim belirlediğimiz gizli anahtar var mı?
                string serviceKey = Request.Headers["X-Service-Key"].ToString();

                // Bu şifreyi kod içinde sabitliyoruz (Sadece backend ve servis biliyor)
                if (serviceKey == "UniversalScadaServiceKey_2024")
                {
                    System.Diagnostics.Debug.WriteLine("[API] Özel Servis Anahtarı ile giriş yapıldı. Güvenlik atlanıyor.");
                    var allFactories = _factoryRepo.GetAllFactories();
                    return Ok(allFactories);
                }
                // ------------------------------------------

                // --- 3. NORMAL KULLANICI KONTROLÜ ---
                // Eğer gizli anahtar yoksa, o zaman Token olmak ZORUNDA
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized(); // Token yoksa 401 ver
                }

                // ... Buradan sonrası sizin eski kodunuz (Token okuma işlemleri) ...
                var allowedIdsString = User.FindFirst("AllowedFactoryIds")?.Value;
                var companyIdString = User.FindFirst("CompanyId")?.Value;

                if (allowedIdsString == "ALL")
                {
                    return Ok(_factoryRepo.GetAllFactories());
                }

                if (string.IsNullOrEmpty(allowedIdsString) || string.IsNullOrEmpty(companyIdString))
                {
                    return BadRequest("Yetki bilgisi bulunamadı.");
                }

                int companyId = int.Parse(companyIdString);
                var factories = _factoryRepo.GetFactoriesByIds(allowedIdsString, companyId);

                return Ok(factories);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API EXCEPTION]: {ex.Message}");
                return StatusCode(500, "Sunucu hatası");
            }
        }
    }
}