using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CofeeAdmin.Dto.Category
{
    public class AddCategoryDto
    {
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen kategori için bir görsel yükleyiniz.")]
        public IFormFile? Image { get; set; }

        public bool Status { get; set; } = true;
    }
}

