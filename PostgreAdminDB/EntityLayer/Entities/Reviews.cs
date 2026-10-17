using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Packages;

namespace EntityLayer.Entities
{
    public class Reviews:Entity<int>
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string? Comment { get; set; }

        public double Score { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
