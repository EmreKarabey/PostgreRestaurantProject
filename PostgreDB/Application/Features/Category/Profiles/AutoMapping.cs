using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Category.Queries.GetActiveList;
using AutoMapper;
using Domain.Entities;
using X.PagedList;

namespace Application.Features.Category.Profiles
{
    public class AutoMapping:Profile
    {
        public AutoMapping()
        {
            CreateMap<IPagedList<GetActiveCategoryListDto>, Domain.Entities.Category>().ReverseMap();
        }
    }
}
