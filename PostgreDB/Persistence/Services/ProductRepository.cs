using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Product.Queries.GetActiveWithProductsList;
using Application.Services.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using X.PagedList;

namespace Persistence.Services
{
    public class ProductRepository : EFRepositoryAsync<Product>, IProductRepository
    {
        private readonly BaseDBContext _dBContext;
        public ProductRepository(BaseDBContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<List<GetActiveWithProductsListDto>> GetActiveWithCategory()
        {
            return await _dBContext.Categories
                .AsNoTracking()
                .Where(category => category.Status && category.Products.Any(product => product.Status))
                .OrderBy(category => category.CategoryName)
                .Select(category => new GetActiveWithProductsListDto
                {
                    CategoryName = category.CategoryName,
                    ImageURL = category.ImageURL,
                    Status = category.Status,
                    ProductListWithCategories = category.Products
                        .Where(product => product.Status)
                        .OrderBy(product => product.ProductName)
                        .Select(product => new ProductListWithCategory
                        {
                            Descrption = product.Descrption,
                            ImageURL = product.ImageURL,
                            Price = product.Price,
                            ProductName = product.ProductName,
                            Status = product.Status
                        })
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
