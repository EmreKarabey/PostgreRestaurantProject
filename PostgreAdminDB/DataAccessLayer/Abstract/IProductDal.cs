using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CofeeAdmin.Dto.Category;
using EntityLayer.Entities;

namespace DataAccessLayer.Abstract
{
    public interface IProductDal:IGenericDal<Product>
    {
        public Task< List<CategoryListDto>> CategoryWithProducts();
        public Task<decimal> AveragePriceAsync();
    }
}
