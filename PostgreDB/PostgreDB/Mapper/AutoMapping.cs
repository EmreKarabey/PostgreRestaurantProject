using Application.Features.Login.Command.Register;
using Application.Features.RestaurantTable.Queries.GetActiveTableList;
using AutoMapper;
using Domain.Entities;
using PostgreDB.Dto;
namespace PostgreDB.Mapper
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<RegisterCommand,RegisterDto>().ReverseMap();
            CreateMap<RestaurantTable, GetActiveTableListDto>();
        }
    }
}
