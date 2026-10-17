using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CofeeAdmin.Dto.AppUser
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta formatı giriniz.")]
        public string Email { get; set; } = string.Empty;

        public string? ProfileImageURL { get; set; }

        public IFormFile? Image { get; set; }

        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Yeni şifreler birbiriyle eşleşmiyor.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
