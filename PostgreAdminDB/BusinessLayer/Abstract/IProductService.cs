using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CofeeAdmin.Dto.Category;
using EntityLayer.Entities;

namespace BusinessLayer.Abstract
{
    public interface IProductService:IGenericService<Product>
    {
        public Task<List<CategoryListDto>> CategoryWithProducts();
        public Task<decimal> AveragePriceAsync();
    }
}
