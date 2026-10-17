using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Repository;
using MediatR;

namespace Application.Features.Reviews.Queries.GetActiveReviewsList
{
    public class GetActiveReviewsListQuery:IRequest<List<GetActiveReviewsListDto>>
    {
    }

    public class GetActiveReviewsListHandler : IRequestHandler<GetActiveReviewsListQuery, List<GetActiveReviewsListDto>>
    {
        private readonly IReviewsRepository _reviewsRepository;

        public GetActiveReviewsListHandler(IReviewsRepository reviewsRepository)
        {
            _reviewsRepository = reviewsRepository;
        }

        public async Task<List<GetActiveReviewsListDto>> Handle(GetActiveReviewsListQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewsRepository.GetActiveWithProductsAsync(cancellationToken);

            return reviews
                .GroupBy(review => new
                {
                    review.ProductId,
                    review.Product.ProductName,
                    review.Product.ImageURL
                })
                .Select(group => new GetActiveReviewsListDto
                {
                    ProductId = group.Key.ProductId,
                    ProductName = group.Key.ProductName,
                    ImageURL = group.Key.ImageURL,
                    ActiveReviews = group.Select(review => new ActiveReviews
                    {
                        EntityID = review.EntityID,
                        Comment = review.Comment,
                        Score = review.Score,
                        FirstName = review.FirstName,
                        LastName = review.LastName,
                        CreatedAt = review.CreatedAt
                    }).ToList()
                })
                .ToList();
        }
    }
}
