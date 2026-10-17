using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using DataAccessLayer.Abstract;
using Microsoft.EntityFrameworkCore.Query;
using X.PagedList;

namespace BusinessLayer.Concrete
{
    public class GenericManager<T> : IGenericService<T> where T : class
    {
        private readonly IGenericDal<T> _genericDal;

        public GenericManager(IGenericDal<T> genericDal)
        {
            _genericDal = genericDal;
        }

        public async Task AddAsync(T t)
        {
            await _genericDal.AddAsync(t);
        }

        public async Task AddRangeAsync(List<T> t)
        {
            await _genericDal.AddRangeAsync(t);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {

            return await _genericDal.CountAsync(predicate: predicate);
        }

        public async Task DeleteAsync(T t)
        {
            await _genericDal.DeleteAsync(t);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _genericDal.GetByIdAsync(id);
        }

        public async Task<IPagedList<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, int? page = null, int? pageSize = null)
        {
            return await _genericDal.GetListAsync(predicate, include, orderBy, page, pageSize);
        }

        public async Task UpdateAsync(T t)
        {
            await _genericDal.UpdateAsync(t);
        }

        public async Task AddOrUpdateRangeAsync(List<T> entitiesToAdd, List<T> entitiesToUpdate)
        {
            await _genericDal.AddOrUpdateRangeAsync(entitiesToAdd, entitiesToUpdate);
        }
    }
}
