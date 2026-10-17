using Application.Services.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services
{
    public class ReviewsRepository : EFRepositoryAsync<Reviews>, IReviewsRepository
    {
        private readonly BaseDBContext _dbContext;

        public ReviewsRepository(BaseDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Reviews>> GetActiveWithProductsAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.Reviews
                .AsNoTracking()
                .Include(review => review.Product)
                .Where(review => review.DeletedAt == null
                    && review.Product.DeletedAt == null
                    && review.Product.Status)
                .OrderBy(review => review.Product.ProductName)
                .ThenByDescending(review => review.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
