using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using X.PagedList;

namespace Application.Services.Repository
{
    public interface IAsyncRepository<T> where T : class
    {
        public Task AddAsync(T t);
        public Task DeleteAsync(T t);
        public Task UpdateAsync(T t);
        public Task<IPagedList<T>> GetListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? page = null,
            int? pageSize = null);

        public Task<T> GetByIdAsync(int id);
    }
}
