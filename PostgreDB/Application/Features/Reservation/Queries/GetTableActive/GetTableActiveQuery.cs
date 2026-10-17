using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using MediatR;

namespace Application.Features.Reservation.Queries.GetTableActive
{
    public class GetTableActiveQuery : IRequest<bool>
    {
        public DateTime ReservationDate { get; set; }
        public int? TableNo { get; set; }
    }
    public class GetTableActiveHandler : IRequestHandler<GetTableActiveQuery, bool>
    {
        private readonly IReservationRepository _reservationRepository;

        public GetTableActiveHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<bool> Handle(GetTableActiveQuery request, CancellationToken cancellationToken)
        {
            var requestedEnd = request.ReservationDate.AddHours(4);
            var any = await _reservationRepository.GetListAsync(predicate: n =>
                (request.TableNo > 0 ? n.TableNumber == request.TableNo : true) &&
                n.ReservationDate < requestedEnd &&
                n.ReservationDate.AddHours(4) > request.ReservationDate);

            if (any.Any()) return true;

            return false;
        }
    }
}
