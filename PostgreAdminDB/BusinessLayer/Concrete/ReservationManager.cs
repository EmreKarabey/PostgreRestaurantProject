using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore.Query;
using X.PagedList;

namespace BusinessLayer.Concrete
{
    public class ReservationManager : GenericRepository<Reservation>, IReservationService
    {
        public ReservationManager(BaseDBContext context) : base(context)
        {
        }
    }
}
