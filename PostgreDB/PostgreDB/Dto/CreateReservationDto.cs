namespace PostgreDB.Dto
{
    public class CreateReservationDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }

        public int? TableNumber { get; set; }
    }
}
