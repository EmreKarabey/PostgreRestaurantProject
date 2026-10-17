using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Reviews.Queries.GetActiveReviewsList
{
    public class GetActiveReviewsListDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageURL { get; set; }

        public List<ActiveReviews> ActiveReviews { get; set; }
      
    }

    public class ActiveReviews
    {
        public int EntityID { get; set; }
        public string? Comment { get; set; }

        public double Score { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
