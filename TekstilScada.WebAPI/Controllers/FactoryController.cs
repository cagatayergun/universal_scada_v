using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TekstilScada.WebAPI.Repositories;
using TekstilScada.WebAPI.Models;

namespace TekstilScada.WebAPI.Controllers
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
        public IActionResult GetMyFactories()
        {
            // 1. Token'dan yetkili fabrika ID'lerini oku
            var allowedIdsString = User.FindFirst("AllowedFactoryIds")?.Value;
            var companyIdString = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(allowedIdsString) || string.IsNullOrEmpty(companyIdString))
            {
                return BadRequest("Yetki bilgisi bulunamadı.");
            }

            // 2. Veritabanından bu fabrikaların detaylarını (İsim vs.) çek
            // (Repository'e bu metodu eklememiz gerekecek, aşağıda veriyorum)
            var factories = _factoryRepo.GetFactoriesByIds(allowedIdsString, int.Parse(companyIdString));

            return Ok(factories);
        }
    }
}