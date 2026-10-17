using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Product.Queries.GetActiveWithProductsList;
using Domain.Entities;
using X.PagedList;

namespace Application.Services.Repository
{
    public interface IProductRepository : IAsyncRepository<Product>
    {
        public Task<List<GetActiveWithProductsListDto>> GetActiveWithCategory();
    }
}
