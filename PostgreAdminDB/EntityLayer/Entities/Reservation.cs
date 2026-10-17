using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Packages;

namespace EntityLayer.Entities
{
    public class Reservation : Entity<int>
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }

        public int? TableNumber { get; set; }

        public Reservation() { }

        public Reservation(string firstName, string lastName, string email, string phoneNumber, DateTime reservationDate, int numberOfPeople, int? tableNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            ReservationDate = reservationDate;
            NumberOfPeople = numberOfPeople;
            TableNumber = tableNumber;
        }

    }
}
