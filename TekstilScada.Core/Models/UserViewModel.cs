// Dosya: TekstilScada.Core/Models/UserViewModel.cs

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // List için eklendi

// KRİTİK: Namespace'i TekstilScada.Core olarak ayarlayın
namespace TekstilScada.Models
{
    // Bu sınıf artık Core'dadır ve WebAPI ile WebApp tarafından kullanılabilir.
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