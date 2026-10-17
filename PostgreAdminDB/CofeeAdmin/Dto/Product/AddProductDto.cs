using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CofeeAdmin.Validation;

namespace CofeeAdmin.Dto.Product
{
    public class AddProductDto
    {
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        public string ProductName { get; set; } = string.Empty;

        public string Descrption { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen ürün için bir görsel yükleyiniz.")]
        public IFormFile? Image { get; set; }

        public bool Status { get; set; }

        [PositivePrice(ErrorMessage = "Fiyat sıfırdan büyük olmalıdır.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kategori seçimi zorunludur.")]
        public int CategoryId { get; set; }
    }
}
