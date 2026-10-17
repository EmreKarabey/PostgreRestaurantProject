using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Reservation.Queries.GetActiveList;
using Domain.Entities;

namespace Application.Services.Repository
{
    public interface IReservationRepository:IAsyncRepository<Reservation>
    {
    }
}
