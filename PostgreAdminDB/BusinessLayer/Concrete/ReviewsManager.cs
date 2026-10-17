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
    public class ReviewsManager : GenericManager<Reviews>, IReviewsService
    {
        public ReviewsManager(IGenericDal<Reviews> genericDal) : base(genericDal)
        {
        }
    }
}
