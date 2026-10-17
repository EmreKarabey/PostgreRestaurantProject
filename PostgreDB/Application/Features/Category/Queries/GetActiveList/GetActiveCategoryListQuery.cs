using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Application.Features.Category.Queries.GetActiveList
{
    public class GetActiveCategoryListQuery : IRequest<IPagedList<GetActiveCategoryListDto>>
    {
    }

    public class GetActiveCategoryListHandler : IRequestHandler<GetActiveCategoryListQuery, IPagedList<GetActiveCategoryListDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetActiveCategoryListHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IPagedList<GetActiveCategoryListDto>> Handle(GetActiveCategoryListQuery request, CancellationToken cancellationToken)
        {
            var list = await _categoryRepository.GetListAsync(predicate: n => n.Status);

            var result = _mapper.Map<IPagedList<GetActiveCategoryListDto>>(list);
            return result;
        }
    }
}
