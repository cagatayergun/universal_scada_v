using Microsoft.AspNetCore.Mvc;
using TekstilScada.Models; // Core Models (User, Role vb.)
using TekstilScada.Core.Models; // KRİTİK: UserViewModel'ın yeni konumu
using TekstilScada.Repositories;
using TekstilScada.Core.Services; // Eğer AuthService enjekte edilecekse
using System.Collections.Generic;
using System.Linq;
using System; // Exception için gerekli

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        // Not: AuthService, şifre hashleme vb. işlemler için buraya enjekte edilebilir.
        // Ancak Repository metotları şifreyi parametre olarak aldığı için şimdilik UserRepository'ye odaklanıyoruz.

        // KRİTİK DÜZELTME: Dependency Injection (DI) ile Repository enjekte ediliyor.
        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
            // _userRepository = new UserRepository(); kullanımı kaldırıldı.
        }

        [HttpGet]
        public ActionResult<List<User>> GetAll()
        {
            try
            {
                return Ok(_userRepository.GetAllUsers());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kullanıcı listesi alınamadı: {ex.Message}");
            }
        }

        // YENİ: Rolleri listelemek için endpoint
        [HttpGet("roles")]
        public ActionResult<List<Role>> GetRoles()
        {
            try
            {
                return Ok(_userRepository.GetAllRoles());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Roller alınamadı: {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult Add([FromBody] UserViewModel model)
        {
            // Model Validation (DataAnnotations) otomatik çalışır, ancak manuel kontrol gerekebilir.
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // ViewModel'den temel User nesnesine dönüştürme
                var user = new User
                {
                    // Id'yi burada set etmiyoruz, DB verecek
                    Username = model.Username,
                    FullName = model.FullName,
                    IsActive = model.IsActive
                };

                // Repository'nin AddUser metodu: (User user, string password, List<int> roleIds)
                _userRepository.AddUser(user, model.Password, model.SelectedRoleIds);

                // Başarılı oluşturma (201 Created)
                return CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kullanıcı eklenirken hata: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] UserViewModel model)
        {
            // ID uyuşmazlığı kontrolü
            if (id != model.Id) return BadRequest("ID uyuşmazlığı.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Update için User nesnesi oluşturulur (ID'nin set edilmesi zorunludur)
                var user = new User
                {
                    Id = id,
                    Username = model.Username,
                    FullName = model.FullName,
                    IsActive = model.IsActive
                };

                // Repository'nizdeki UpdateUser metodu
                // Not: Repository bu metot içinde şifrenin boş olup olmadığını kontrol edip eski şifreyi korumalıdır.
                _userRepository.UpdateUser(user, model.SelectedRoleIds, model.Password);

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kullanıcı güncellenirken hata: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                _userRepository.DeleteUser(id);
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kullanıcı silinirken hata: {ex.Message}");
            }
        }
    }
}