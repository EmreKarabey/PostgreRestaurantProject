using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Application.Features.Product.Queries.GetActiveWithProductsList
{
    public class GetActiveWithProductsListDto
    {
        public string CategoryName { get; set; }
        public string ImageURL { get; set; }
        public bool Status { get; set; }

        public List<ProductListWithCategory> ProductListWithCategories { get; set; }
    }

    public class ProductListWithCategory
    {
        public string ProductName { get; set; }
        public string Descrption { get; set; }
        public string ImageURL { get; set; }
        public bool Status { get; set; }
        public decimal Price { get; set; }
    }

}
