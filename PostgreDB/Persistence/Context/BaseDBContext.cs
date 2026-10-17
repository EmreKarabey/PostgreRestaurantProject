using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    public class BaseDBContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public BaseDBContext(DbContextOptions<BaseDBContext> options):base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<RestaurantTable> RestaurantTable { get; set; }
    }
}
