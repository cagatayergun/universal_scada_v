using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TekstilScada.Models;
using TekstilScada.WebAPI.Models;
using TekstilScada.WebAPI.Repositories;

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
            try
            {
                // AŞAMA 1: TOKEN İÇERİĞİNİ DÖKÜM ALALIM
                // Token'ın içinde ne var ne yok hepsini görelim (Claim kontrolü)
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("=== [API DETAYLI DEBUG BAŞLADI] ===");
                System.Diagnostics.Debug.WriteLine("--- 1. Token İçeriği (Claims) ---");
                foreach (var claim in User.Claims)
                {
                    System.Diagnostics.Debug.WriteLine($"   > Anahtar: {claim.Type}, Değer: {claim.Value}");
                }

                // AŞAMA 2: DEĞERLERİ OKUMA
                var allowedIdsString = User.FindFirst("AllowedFactoryIds")?.Value;
                var companyIdString = User.FindFirst("CompanyId")?.Value;

                System.Diagnostics.Debug.WriteLine("--- 2. Ayrıştırılan Değerler ---");
                System.Diagnostics.Debug.WriteLine($"   > Okunan AllowedFactoryIds: '{allowedIdsString}'");
                System.Diagnostics.Debug.WriteLine($"   > Okunan CompanyId: '{companyIdString}'");

                if (string.IsNullOrEmpty(allowedIdsString) || string.IsNullOrEmpty(companyIdString))
                {
                    System.Diagnostics.Debug.WriteLine("   > [HATA] Kritik bilgiler eksik! BadRequest dönülüyor.");
                    return BadRequest("Yetki bilgisi bulunamadı.");
                }

                int companyId = int.Parse(companyIdString);

                // AŞAMA 3: REPOSITORY ÇAĞRISI
                System.Diagnostics.Debug.WriteLine("--- 3. Repository Çağrılıyor ---");
                System.Diagnostics.Debug.WriteLine($"   > Parametreler -> IDs: {allowedIdsString}, CompID: {companyId}");

                // Veritabanı sorgusu yapılıyor
                var factories = _factoryRepo.GetFactoriesByIds(allowedIdsString, companyId);

                // AŞAMA 4: REPOSITORY SONUCU
                System.Diagnostics.Debug.WriteLine("--- 4. Repository Sonucu ---");
                System.Diagnostics.Debug.WriteLine($"   > Dönen Kayıt Sayısı: {factories?.Count ?? 0}");

                if (factories != null)
                {
                    foreach (var f in factories)
                    {
                        // Dönen verinin içinde ne var? Yanlış şirket mi geliyor?
                        System.Diagnostics.Debug.WriteLine($"   > [KAYIT] ID: {f.Id} | ŞirketID: {f.CompanyId} | İsim: {f.FactoryName}");

                        // KONTROL: Eğer dönen fabrikanın şirketi, istenen şirketten farklıysa uyar
                        if (f.CompanyId != companyId)
                        {
                            System.Diagnostics.Debug.WriteLine($"   > [!!! KRİTİK HATA !!!] İstenen Şirket {companyId} ama gelen veri Şirket {f.CompanyId}!");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("=== [API DETAYLI DEBUG BİTTİ] ===");
                System.Diagnostics.Debug.WriteLine("");

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