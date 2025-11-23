using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace TekstilScada.WebApp.Controllers
{
    [Route("[controller]/[action]")]
    public class CultureController : Controller
    {
        public IActionResult Set(string culture, string redirectUri)
        {
            if (culture != null)
            {
                // Seçilen dili tarayıcı çerezine (Cookie) kaydet
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
            }

            // Geldiği sayfaya geri dön
            return LocalRedirect(redirectUri);
        }
    }
}