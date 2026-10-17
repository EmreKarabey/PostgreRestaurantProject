using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using MediatR;
using X.PagedList;

namespace Application.Features.Product.Queries.GetActiveWithProductsList
{
    public class GetActiveWithProductsListQuery:IRequest<List<GetActiveWithProductsListDto>>
    {
    }
    public class GetActiveWithProductsListHandler : IRequestHandler<GetActiveWithProductsListQuery, List<GetActiveWithProductsListDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetActiveWithProductsListHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<GetActiveWithProductsListDto>> Handle(GetActiveWithProductsListQuery request, CancellationToken cancellationToken)
        {
            var list = await _productRepository.GetActiveWithCategory();
            return list;
        }
    }
}
