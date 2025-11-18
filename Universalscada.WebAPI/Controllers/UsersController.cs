// Dosya: Universalscada.WebAPI/Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Universalscada.Models;
using Universalscada.Repositories;

namespace Universalscada.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            return Ok(_userRepository.GetAllUsers());
        }
    }
}