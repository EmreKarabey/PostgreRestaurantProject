using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using EntityLayer.Entities;

namespace DataAccessLayer.Concrete
{
    public class EFReviewsDal : GenericRepository<Reviews>, IReviewsDal
    {
        public EFReviewsDal(BaseDBContext context) : base(context)
        {
        }
    }
}
