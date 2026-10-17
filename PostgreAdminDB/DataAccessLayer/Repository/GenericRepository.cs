using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using X.PagedList;
using X.PagedList.EF;

namespace DataAccessLayer.Repository
{
    public class GenericRepository<T> : IGenericDal<T> where T : class
    {
        protected readonly BaseDBContext _context;

        public GenericRepository(BaseDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T t)
        {
            await _context.AddAsync(t);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<T> t)
        {
            await _context.AddRangeAsync(t);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }

        public async Task DeleteAsync(T t)
        {
            _context.Remove(t);
            await _context.SaveChangesAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IPagedList<T>> GetListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();

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
            _context.Update(t);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateRangeAsync(List<T> entitiesToAdd, List<T> entitiesToUpdate)
        {
            if (entitiesToAdd.Count == 0 && entitiesToUpdate.Count == 0)
            {
                return;
            }

            _context.UpdateRange(entitiesToUpdate);
            await _context.AddRangeAsync(entitiesToAdd);
            await _context.SaveChangesAsync();
        }
    }
}
