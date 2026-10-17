using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Packages;

namespace Domain.Entities
{
    public class Product:Entity<int>
    {
        public string ProductName { get; set; }
        public string Descrption { get; set; }
        public string ImageURL { get; set; }
        public bool Status { get; set; }
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }


        public Product() { }

        public Product(int Id,string productName, string descrption, string ımageURL, bool status, decimal price, int categoryId) : this()
        {
            EntityID = Id;
            ProductName = productName;
            Descrption = descrption;
            ImageURL = ımageURL;
            Status = status;
            Price = price;
            CategoryId = categoryId;
        }
    }
}
