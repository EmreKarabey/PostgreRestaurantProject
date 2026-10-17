using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using AutoMapper;
using MediatR;

namespace Application.Features.RestaurantTable.Queries.GetActiveTableList
{
    public class GetActiveTableListQuery:IRequest<List<GetActiveTableListDto>>
    {
    }

    public class GetActiveTableListHandler : IRequestHandler<GetActiveTableListQuery, List<GetActiveTableListDto>>
    {
        private readonly IRestaurantTableRepository _restaurantTableRepository;
        private readonly IMapper _mapper;

        public GetActiveTableListHandler(IRestaurantTableRepository restaurantTableRepository, IMapper mapper)
        {
            _restaurantTableRepository = restaurantTableRepository;
            _mapper = mapper;
        }

        public async Task<List<GetActiveTableListDto>> Handle(GetActiveTableListQuery request, CancellationToken cancellationToken)
        {
            var list = await _restaurantTableRepository.GetListAsync(predicate:n=>n.DeletedAt==null, orderBy: query => query.OrderBy(table => table.TableNumber));

            var mapperList = _mapper.Map<List<GetActiveTableListDto>>(list);

            return mapperList;
        }
    }
}
