using System;
using AutoMapper;
using System.Linq;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.Category;
using Microsoft.AspNetCore.SignalR;

namespace CofeeAdmin.SignalR
{

    public class SignalRHub : Hub
    {
        private readonly IReservationService _reservationService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewsService _reviewsService;

        public SignalRHub(IReservationService reservationService, IProductService productService, ICategoryService categoryService, IReviewsService reviewsService)
        {
            _reservationService = reservationService;
            _productService = productService;
            _categoryService = categoryService;
            _reviewsService = reviewsService;
        }

        public async Task SendReviewStatistics()
        {
            var reviews = await _reviewsService.GetListAsync();
            var activeReviews = reviews.Where(review => review.DeletedAt == null).ToList();

            await Clients.Caller.SendAsync("ReviewStatistics", new
            {
                totalReviews = reviews.TotalItemCount,
                averageScore = activeReviews.Count > 0 ? activeReviews.Average(review => review.Score) : 0,
                listedProducts = activeReviews.Select(review => review.ProductId).Distinct().Count(),
                fiveStarReviews = activeReviews.Count(review => review.Score == 5)
            });
        }

        public async Task SendCategoryStatistics()
        {
            var totalCategories = await _categoryService.CountAsync();
            var activeCategories = await _categoryService.CountAsync(category => category.Status);
            var passiveCategories = await _categoryService.CountAsync(category => !category.Status);
            var linkedProducts = await _productService.CountAsync();
            await Clients.Caller.SendAsync("CategoryStatistics", new
            {
                totalCategories,
                activeCategories,
                passiveCategories,
                linkedProducts
            });
        }

        public async Task SendProductStatistics()
        {
            var totalProducts = await _productService.CountAsync();
            var activeProducts = await _productService.CountAsync(product => product.Status);
            var passiveProducts = await _productService.CountAsync(product => !product.Status);
            var averagePrice = await _productService.AveragePriceAsync();

            await Clients.Caller.SendAsync("ProductStatistics", new
            {
                totalProducts,
                activeProducts,
                passiveProducts,
                averagePrice
            });
        }

        public async Task SendStatcis()
        {
            var resrevationCount = await _reservationService.CountAsync(predicate: n => n.DeletedAt == null);
            await Clients.All.SendAsync("ReservationCount", resrevationCount);


            var totalProductsCount = await _productService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
            await Clients.All.SendAsync("TotalProducts", totalProductsCount);

            var totalCategories = await _categoryService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
            await Clients.All.SendAsync("TotalCategories", totalCategories);

            var categoriesList = await _productService.CategoryWithProducts();
            await Clients.All.SendAsync("CategoriesList", categoriesList);
        }
    }
}
