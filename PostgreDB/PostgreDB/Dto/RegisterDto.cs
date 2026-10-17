using System.ComponentModel.DataAnnotations;

namespace PostgreDB.Dto
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen kullanıcı için bir profil görseli yükleyiniz.")]
        public IFormFile Image { get; set; } = null!;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta formatı giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter uzunluğunda olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler birbiriyle eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
