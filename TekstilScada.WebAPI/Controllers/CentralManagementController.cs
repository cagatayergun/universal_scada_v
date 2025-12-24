using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TekstilScada.WebAPI.Models;
using TekstilScada.WebAPI.Repositories;
using System.Diagnostics;

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CentralManagementController : ControllerBase
    {
        private readonly CentralAuthRepository _authRepo;
        private readonly CentralFactoryRepository _factoryRepo;

        public CentralManagementController(CentralAuthRepository authRepo, CentralFactoryRepository factoryRepo)
        {
            _authRepo = authRepo;
            _factoryRepo = factoryRepo;
        }

        // =========================================================================
        // 1. BÖLÜM: SÜPER ADMIN METOTLARI (Program Üreticisi / Admin Paneli)
        // =========================================================================

        [HttpGet("companies")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult GetCompanies()
        {
            var list = _authRepo.GetAllCompanies();
            return Ok(list);
        }

        [HttpPost("company")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult AddCompany([FromBody] Company company)
        {
            company.Id = 0;
            if (_authRepo.AddOrUpdateCompany(company)) return Ok();
            return BadRequest("Kayıt başarısız");
        }

        [HttpPut("company/{id}")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult UpdateCompany(int id, [FromBody] Company company)
        {
            company.Id = id;
            if (_authRepo.AddOrUpdateCompany(company)) return Ok();
            return BadRequest("Güncelleme başarısız");
        }

        [HttpDelete("company/{id}")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult DeleteCompany(int id)
        {
            if (_authRepo.DeleteCompany(id)) return Ok();
            return BadRequest("Silme başarısız");
        }

        [HttpGet("factories/{companyId}")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult GetFactories(int companyId)
        {
            var list = _factoryRepo.GetFactoriesByCompanyId(companyId);
            return Ok(list);
        }

        [HttpPost("factory")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult AddFactory([FromBody] CentralFactory factory)
        {
            if (_factoryRepo.AddFactory(factory)) return Ok();
            return BadRequest("Fabrika eklenemedi");
        }

        [HttpDelete("factory/{id}")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult DeleteFactory(int id)
        {
            if (_factoryRepo.DeleteFactory(id)) return Ok();
            return BadRequest("Silinemedi");
        }

        [HttpGet("admins/{companyId}")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult GetCompanyAdmins(int companyId)
        {
            var users = _authRepo.GetUsersByCompanyId(companyId);
            return Ok(users);
        }

        [HttpPost("create-company-admin")]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult CreateCompanyAdmin([FromBody] SaveSubUserDto dto)
        {
            var user = new CentralUser
            {
                CompanyId = dto.CompanyId,
                Username = dto.Username,
                FullName = dto.FullName,
                Role = "CompanyAdmin",
                AllowedFactoryIds = "ALL"
            };

            if (_authRepo.AddUser(user, dto.Password)) return Ok();
            return BadRequest("Kullanıcı oluşturulamadı");
        }

        // =========================================================================
        // 2. BÖLÜM: FİRMA YÖNETİCİSİ METOTLARI (Portal Sayfası - EKSİK OLANLAR)
        // =========================================================================

        [HttpGet("my-factories")]
        public IActionResult GetMyFactories()
        {
            // 1. Şirket Bilgisi Al
            var companyIdStr = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out int companyId))
            {
                return BadRequest("Şirket bilgisi bulunamadı.");
            }

            // 2. Rol ve Yetkileri Al
            // "role" claim'i bazen büyük/küçük harf veya URI formatında gelebilir, ikisini de kontrol edelim.
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            // "AllowedFactoryIds" claim'ini Token oluşturulurken eklemiş olmalısınız.
            var allowedIds = User.FindFirst("AllowedFactoryIds")?.Value;
            
            // 3. Mantık Kurulumu
            if (role == "CompanyAdmin")
            {
                // Yönetici ise hepsini görsün
                var list = _factoryRepo.GetFactoriesByCompanyId(companyId);
                return Ok(list);
            }
            else
            {
                // Normal Personel ise sadece izin verilenleri görsün
                if (string.IsNullOrEmpty(allowedIds))
                {
                    // Yetki verilmemişse boş liste dön
                    return Ok(new List<CentralFactory>());
                }

                // Filtreli getir
                var list = _factoryRepo.GetFactoriesByIds(allowedIds, companyId);
                return Ok(list);
            }
        }

        [HttpGet("company-users")]
        [Authorize(Roles = "CompanyAdmin")]
        public IActionResult GetCompanyUsers()
        {
            var companyIdStr = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out int companyId))
            {
                return BadRequest("Şirket bilgisi bulunamadı.");
            }

            var list = _authRepo.GetUsersByCompanyId(companyId);

            // İsterseniz sadece 'User' rolündekileri döndürebilirsiniz, şimdilik hepsini döndürüyoruz.
            return Ok(list.Where(u => u.Role != "CompanyAdmin"));
        }

        [HttpPost("save-sub-user")]
        [Authorize(Roles = "CompanyAdmin")]
        public IActionResult SaveSubUser([FromBody] SaveSubUserDto dto)
        {
            // --- BU SATIRI EKLEYİN (Console çıktısında veriyi göreceğiz) ---
            //($"[API LOG] Gelen Kullanıcı: {dto.Username}, Fabrikalar: {dto.AllowedFactoryIds}");
            //
            // Token'dan CompanyId al (Güvenlik için)
            var companyIdStr = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out int companyId))
            {
                return BadRequest("Şirket bilgisi bulunamadı.");
            }

            var user = new CentralUser
            {
                Id = dto.Id,
                CompanyId = companyId, // Token'dan gelen ID'yi kullanıyoruz
                Username = dto.Username,
                FullName = dto.FullName,
                Role = "User", // Personel her zaman User rolündedir

                // Frontend'den gelen string veriyi (örn: "1,2") buraya aktarıyoruz
                AllowedFactoryIds = dto.AllowedFactoryIds
            };

            bool result;
            if (user.Id == 0)
            {
                // Yeni Kayıt
                result = _authRepo.AddUser(user, dto.Password);
            }
            else
            {
                // Güncelleme
                result = _authRepo.UpdateUser(user, dto.Password);
            }

            if (result) return Ok();
            return BadRequest("İşlem başarısız.");
        }

        // --- ORTAK SİLME METODU ---

        [HttpDelete("user/{id}")]
        // Hem SystemAdmin hem CompanyAdmin silebilir (Kendi şirketindeki kullanıcıyı silme kontrolü eklenebilir)
        public IActionResult DeleteUser(int id)
        {
            // İdeal senaryoda: CompanyAdmin sadece kendi personelini silebilmeli.
            // Şimdilik basitçe silme işlemini yapıyoruz.
            if (_authRepo.DeleteUser(id)) return Ok();
            return BadRequest("Silinemedi");
        }

        // DTO
        public class SaveSubUserDto
        {
            public int Id { get; set; }
            public int CompanyId { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
            public string AllowedFactoryIds { get; set; }
        }
    }
}