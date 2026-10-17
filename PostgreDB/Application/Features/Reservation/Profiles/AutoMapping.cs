using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Reservation.Command.Create;
using Application.Features.Reservation.Queries.GetActiveList;
using Application.Features.Reservation.Queries.GetMyReservatioLs;
using AutoMapper;
using Domain.Entities;
using X.PagedList;

namespace Application.Features.Reservation.Profiles
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<CreateReservationCommand, Domain.Entities.Reservation>().ReverseMap();
            CreateMap<CreateReservationResponse, Domain.Entities.Reservation>().ReverseMap();


            CreateMap<GetActiveReservationTableListDto, Domain.Entities.Reservation>().ReverseMap();
            CreateMap<Domain.Entities.Reservation, GetMyRservationListDto>().ReverseMap();
        }
    }
}
