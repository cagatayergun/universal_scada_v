using Microsoft.AspNetCore.Mvc;
using TekstilScada.Models; // Core Models (User, Role)
using TekstilScada.Repositories;
using TekstilScada.WebApp.Models; // UserViewModel burada tanımlı değilse DTO pattern uyguluyoruz
using System.Collections.Generic;
using System.Linq;

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UsersController()
        {
            _userRepository = new UserRepository();
        }

        [HttpGet]
        public ActionResult<List<User>> GetAll()
        {
            // Repository'deki metodunuz zaten var
            return Ok(_userRepository.GetAllUsers());
        }

        // YENİ: Rolleri listelemek için endpoint
        [HttpGet("roles")]
        public ActionResult<List<Role>> GetRoles()
        {
            return Ok(_userRepository.GetAllRoles());
        }

        [HttpPost]
        public ActionResult Add([FromBody] UserViewModel model)
        {
            // ViewModel'i gerçek User nesnesine dönüştürüyoruz
            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                IsActive = model.IsActive
            };

            // Repository'nizdeki AddUser metodu: (User user, string password, List<int> roleIds)
            // DÜZELTME: model.SelectedRoleIds listesini doğrudan gönderiyoruz
            _userRepository.AddUser(user, model.Password, model.SelectedRoleIds);
            return Ok();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] UserViewModel model)
        {
            var user = new User
            {
                Id = id,
                Username = model.Username,
                FullName = model.FullName,
                IsActive = model.IsActive
            };

            // Repository'nizdeki UpdateUser metodu
            _userRepository.UpdateUser(user, model.SelectedRoleIds, model.Password);
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            _userRepository.DeleteUser(id);
            return Ok();
        }
    }
}