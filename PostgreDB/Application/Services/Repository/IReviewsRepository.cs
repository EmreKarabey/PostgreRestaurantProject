using Domain.Entities;

namespace Application.Services.Repository
{
    public interface IReviewsRepository : IAsyncRepository<Reviews>
    {
        Task<List<Reviews>> GetActiveWithProductsAsync(CancellationToken cancellationToken = default);
    }
}
