using System.ComponentModel.DataAnnotations;

namespace TekstilScada.WebApp.Models
{
    // Bu sınıf sadece arayüzde form doldururken kullanılır.
    // Veritabanı modelini (User.cs) kirletmemiş oluruz.
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;

        // Şifre sadece yeni kullanıcı eklerken zorunludur, düzenlerken boş olabilir.
        public string? Password { get; set; }

        [Required(ErrorMessage = "Bir rol seçmelisiniz.")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();
    }
}