using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CofeeAdmin.Dto.Category;
using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataAccessLayer.Concrete
{
    public class EFProductDal : GenericRepository<Product>, IProductDal
    {
        private readonly BaseDBContext _context;
        public EFProductDal(BaseDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<CategoryListDto>> CategoryWithProducts()
        {
            var list = await _context.Products.GroupBy(n => n.Category).Select(n => new CategoryListDto
            {
                CategoryName=n.Key.CategoryName,
                ProductCount= n.Count()
            }).ToListAsync();

            return list;
        }

        public async Task<decimal> AveragePriceAsync()
        {
            return await _context.Products
                .Select(product => (decimal?)product.Price)
                .AverageAsync() ?? 0m;
        }
    }
}
