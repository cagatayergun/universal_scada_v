using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TekstilScada.WebAPI.Repositories; // Yeni Repo'lar burada
using TekstilScada.WebAPI.Models;       // Yeni Modeller burada

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CentralAuthRepository _authRepo; // Yeni Repo

        public AuthController(IConfiguration configuration, CentralAuthRepository authRepo)
        {
            _configuration = configuration;
            _authRepo = authRepo;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // 1. Merkezi Veritabanından Kullanıcıyı Sorgula
            var user = _authRepo.Login(login.Username, login.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı (Merkezi DB)" });
            }

            // 2. Token Oluştur ve Yetkileri (Claims) İçine Göm
            var tokenString = GenerateJwtToken(user);

            return Ok(new
            {
                token = tokenString,
                fullName = user.FullName,
                role = user.Role,
                allowedFactories = user.AllowedFactoryIds // Frontend bunu kullanacak
            });
        }

        private string GenerateJwtToken(CentralUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                
                // --- KRİTİK YENİ BİLGİLER ---
                new Claim("CompanyId", user.CompanyId.ToString()),
                new Claim("AllowedFactoryIds", user.AllowedFactoryIds)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}