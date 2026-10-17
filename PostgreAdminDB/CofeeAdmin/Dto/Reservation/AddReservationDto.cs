using System;
using System.ComponentModel.DataAnnotations;

namespace CofeeAdmin.Dto.Reservation
{
    public class AddReservationDto
    {
        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Müşteri soyadı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Rezervasyon tarihi ve saati zorunludur.")]
        public DateTime ReservationDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Kişi sayısı zorunludur.")]
        [Range(1, 50, ErrorMessage = "Kişi sayısı 1 ile 50 arasında olmalıdır.")]
        public int NumberOfPeople { get; set; } = 2;

        [Range(1, 100, ErrorMessage = "Masa numarası 1 ile 100 arasında olmalıdır.")]
        public int? TableNumber { get; set; }
    }
}
