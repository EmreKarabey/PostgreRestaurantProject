using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.Category;
using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using EntityLayer.Entities;

namespace BusinessLayer.Concrete
{
    public class ProductManager : GenericRepository<Product>, IProductService
    {
        private readonly IProductDal _productDal;
        public ProductManager(BaseDBContext context, IProductDal productDal) : base(context)
        {
            _productDal = productDal;
        }


        public async Task<List<CategoryListDto>> CategoryWithProducts()
        {
           return await _productDal.CategoryWithProducts();
        }

        public async Task<decimal> AveragePriceAsync()
        {
            return await _productDal.AveragePriceAsync();
        }
    }
}
