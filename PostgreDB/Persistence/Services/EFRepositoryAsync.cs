using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using X.PagedList;
using Persistence.Context;
using X.PagedList.EF;
namespace Persistence.Services
{
    public class EFRepositoryAsync<T> : IAsyncRepository<T> where T : class
    {
        private readonly BaseDBContext _dBContext;

        public EFRepositoryAsync(BaseDBContext dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task AddAsync(T t)
        {
            await _dBContext.AddAsync(t);
            await _dBContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T t)
        {
            _dBContext.Remove(t);
            await _dBContext.SaveChangesAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dBContext.Set<T>().FindAsync(id);
        }

        public async Task<IPagedList<T>> GetListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<T> query = _dBContext.Set<T>().AsNoTracking();

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (page.HasValue)
            {
                var ps = pageSize.GetValueOrDefault(15);
                return await query.ToPagedListAsync(page.Value, ps);
            }

            if (pageSize.HasValue)
            {
                return await query.ToPagedListAsync(1, pageSize.Value);
            }

            var allItems = await query.ToListAsync();
            return new StaticPagedList<T>(allItems, 1, allItems.Count > 0 ? allItems.Count : 1, allItems.Count);
        }


        public async Task UpdateAsync(T t)
        {
            _dBContext.Update(t);
            await _dBContext.SaveChangesAsync();
        }
    }
}
