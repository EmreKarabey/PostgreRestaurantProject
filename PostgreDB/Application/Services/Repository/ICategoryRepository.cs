using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Services.Repository
{
    public interface ICategoryRepository:IAsyncRepository<Category>
    {
    }
}
