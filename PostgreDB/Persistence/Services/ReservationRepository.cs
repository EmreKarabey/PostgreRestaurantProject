using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Reservation.Queries.GetActiveList;
using Application.Services.Repository;
using Domain.Entities;
using Persistence.Context;

namespace Persistence.Services
{
    public class ReservationRepository : EFRepositoryAsync<Reservation>, IReservationRepository
    {
        public ReservationRepository(BaseDBContext dBContext) : base(dBContext)
        {
        }

    }
}
