using System.ComponentModel.DataAnnotations;

namespace CofeeAdmin.Dto.AppRole
{
    public class UpdateRoleDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Rol adı boş bırakılamaz.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı 2 ile 50 karakter arasında olmalıdır.")]
        [Display(Name = "Rol Adı")]
        public string Name { get; set; } = string.Empty;
    }
}
