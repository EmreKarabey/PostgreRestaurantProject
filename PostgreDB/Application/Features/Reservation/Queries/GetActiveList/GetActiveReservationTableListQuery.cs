using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using AutoMapper;
using MediatR;

namespace Application.Features.Reservation.Queries.GetActiveList
{
    public class GetActiveReservationTableListQuery:IRequest<List<GetActiveReservationTableListDto>>
    {
    }

    public class GetActiveReservationTableListHandler : IRequestHandler<GetActiveReservationTableListQuery, List<GetActiveReservationTableListDto>>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IMapper _mapper;

        public GetActiveReservationTableListHandler(IReservationRepository reservationRepository, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _mapper = mapper;
        }

        public async Task<List<GetActiveReservationTableListDto>> Handle(GetActiveReservationTableListQuery request, CancellationToken cancellationToken)
        {
            var activeSince = DateTime.UtcNow.AddHours(-4);
            var list = await _reservationRepository.GetListAsync(
                predicate: reservation => reservation.DeletedAt == null
                    && reservation.Status == "Confirm"
                    && reservation.ReservationDate >= activeSince);

            var activeTableList = _mapper.Map<List<GetActiveReservationTableListDto>>(list);

            return activeTableList;
        }
    }
}
