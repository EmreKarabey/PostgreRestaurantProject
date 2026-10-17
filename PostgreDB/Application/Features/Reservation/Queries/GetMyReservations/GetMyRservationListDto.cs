using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Features.Reservation.Queries.GetMyReservatioLs
{
    public class GetMyRservationListDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }

        public int? TableNumber { get; set; }

        public int UserId { get; set; }
        public string Status { get; set; } = "Confirm";
    }
}
