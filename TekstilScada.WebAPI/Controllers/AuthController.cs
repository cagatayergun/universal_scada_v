using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TekstilScada.Core.Models; // UserViewModel için (yoksa silebilirsiniz, dynamic kullanırız)

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // BASİT DOĞRULAMA (Veritabanı olmadığı için)
            // Kullanıcı Adı: admin
            // Şifre: 1234
            if (request.Username == "admin" && request.Password == "1234")
            {
                var tokenString = GenerateJwtToken(request.Username);
                return Ok(new { Token = tokenString, User = new { FullName = "Sistem Yöneticisi", Role = "Admin" } });
            }

            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }

        private string GenerateJwtToken(string username)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Jwt:Key is missing in appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin") // Herkese Admin yetkisi veriyoruz şimdilik
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(365), // 1 Yıl geçerli token (Rahat test için)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Basit İstek Modeli
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}