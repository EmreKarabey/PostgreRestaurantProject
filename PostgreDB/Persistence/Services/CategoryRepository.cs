using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using Domain.Entities;
using Persistence.Context;

namespace Persistence.Services
{
    public class CategoryRepository : EFRepositoryAsync<Category>, ICategoryRepository
    {
        public CategoryRepository(BaseDBContext dBContext) : base(dBContext)
        {
        }
    }
}
