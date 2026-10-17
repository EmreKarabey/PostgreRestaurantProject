using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Reservation.Queries.GetActiveList
{
    public class GetActiveReservationTableListDto
    {
        public int? TableNumber { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}
