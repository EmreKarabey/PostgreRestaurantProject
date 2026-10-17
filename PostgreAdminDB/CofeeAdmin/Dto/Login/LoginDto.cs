using System.ComponentModel.DataAnnotations;

namespace CofeeAdmin.Dto.Login
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adınızı veya e-posta adresinizi giriniz.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
