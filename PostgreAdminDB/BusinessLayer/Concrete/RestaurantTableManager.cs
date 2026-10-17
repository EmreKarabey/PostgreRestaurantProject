using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using EntityLayer.Entities;

namespace BusinessLayer.Concrete
{
    public class RestaurantTableManager : GenericManager<RestaurantTable>, IRestaurantTableService
    {
        public RestaurantTableManager(IGenericDal<RestaurantTable> genericDal) : base(genericDal)
        {
        }
    }
}
