using MediatR;
using Microsoft.Extensions.Logging;
using ThirdStepTask.Product.Application.Services;
using ThirdStepTask.Product.Domain.Interfaces;

namespace ThirdStepTask.Product.Application.Features.Products.Queries.GetAllProducts
{
    /// <summary>
    /// Handler for GetAllProductsQuery
    /// Implements caching strategy with Redis for improved performance
    /// </summary>
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, GetAllProductsResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;
        private const string CACHE_KEY = "products:all";
        private const int CACHE_EXPIRATION_MINUTES = 10;

        public GetAllProductsQueryHandler(
            IProductRepository productRepository,
            ICacheService cacheService,
            ILogger<GetAllProductsQueryHandler> logger)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<GetAllProductsResponse> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all products (IncludeInactive: {IncludeInactive})", request.IncludeInactive);

            // Try to get from cache first (Redis Cache Strategy)
            try
            {
                var cachedResponse = await _cacheService.GetAsync<GetAllProductsResponse>(CACHE_KEY);
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Products retrieved from cache");
                    return cachedResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve products from cache, falling back to database");
            }

            // Get from database
            var products = await _productRepository.GetAllAsync(cancellationToken);

            if (!request.IncludeInactive)
            {
                products = products.Where(p => p.IsActive);
            }

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Price = p.Price,
                FinalPrice = p.FinalPrice,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                IsInStock = p.IsInStock,
                HasDiscount = p.HasDiscount,
                DiscountPercentage = p.DiscountPercentage,
                DiscountValidUntil = p.DiscountValidUntil,
                CreatedAt = p.CreatedAt
            }).ToList();

            var response = new GetAllProductsResponse
            {
                Products = productDtos,
                TotalCount = productDtos.Count
            };

            // Cache the response (Redis Cache)
            try
            {
                await _cacheService.SetAsync(CACHE_KEY, response, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
                _logger.LogInformation("Products cached successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache products");
            }

            _logger.LogInformation("Retrieved {Count} products from database", response.TotalCount);

            return response;
        }
    }
}
