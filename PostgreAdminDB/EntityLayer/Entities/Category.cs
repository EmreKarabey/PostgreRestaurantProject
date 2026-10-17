using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Packages;

namespace EntityLayer.Entities
{
    public class Category:Entity<int>
    {
        public string CategoryName { get; set; }
        public string ImageURL { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public Category() { }

        public Category(string categoryName, string ımageURL, bool status) : this()
        {
            CategoryName = categoryName;
            ImageURL = ımageURL;
            Status = status;
        }
    }
}
