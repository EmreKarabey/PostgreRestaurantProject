using Application.Services.Repository;
using Domain.Entities;
using Persistence.Context;

namespace Persistence.Services
{
    public class RestaurantTableRepository : EFRepositoryAsync<RestaurantTable>, IRestaurantTableRepository
    {
        public RestaurantTableRepository(BaseDBContext dbContext) : base(dbContext)
        {
        }
    }
}
