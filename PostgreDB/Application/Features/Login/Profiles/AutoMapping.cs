using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Login.Command.Register;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Login.Profiles
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<RegisterCommand, AppUser>().ReverseMap();
        }
    }
}
