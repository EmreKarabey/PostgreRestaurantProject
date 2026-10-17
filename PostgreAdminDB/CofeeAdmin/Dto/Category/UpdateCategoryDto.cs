using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CofeeAdmin.Dto.Category
{
    public class UpdateCategoryDto
    {
        public int EntityID { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string CategoryName { get; set; } = string.Empty;

        public string? ImageURL { get; set; }

        public IFormFile? Image { get; set; }

        public bool Status { get; set; }
    }
}
