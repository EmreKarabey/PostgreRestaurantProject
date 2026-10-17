using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Packages;
using Microsoft.AspNetCore.Identity;

namespace EntityLayer.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string ProfileImageURL { get; set; }
    }
}
