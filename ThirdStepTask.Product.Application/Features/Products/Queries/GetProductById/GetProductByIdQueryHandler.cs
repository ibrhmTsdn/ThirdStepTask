using MediatR;
using Microsoft.Extensions.Logging;
using ThirdStepTask.Product.Application.Features.Products.Queries.GetAllProducts;
using ThirdStepTask.Product.Application.Services;
using ThirdStepTask.Product.Domain.Interfaces;

namespace ThirdStepTask.Product.Application.Features.Products.Queries.GetProductById
{
    /// <summary>
    /// Handler for GetProductByIdQuery
    /// Implements individual product caching strategy
    /// </summary>
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;
        private const int CACHE_EXPIRATION_MINUTES = 15;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository,
            ICacheService cacheService,
            ILogger<GetProductByIdQueryHandler> logger)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"product:{request.Id}";

            _logger.LogInformation("Getting product with ID: {ProductId}", request.Id);

            // Try to get from cache first
            try
            {
                var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
                if (cachedProduct != null)
                {
                    _logger.LogInformation("Product retrieved from cache: {ProductId}", request.Id);
                    return cachedProduct;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve product from cache: {ProductId}", request.Id);
            }

            // Get from database
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                _logger.LogInformation("Product not found: {ProductId}", request.Id);
                return null;
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                FinalPrice = product.FinalPrice,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                IsInStock = product.IsInStock,
                HasDiscount = product.HasDiscount,
                DiscountPercentage = product.DiscountPercentage,
                DiscountValidUntil = product.DiscountValidUntil,
                CreatedAt = product.CreatedAt
            };

            // Cache the product
            try
            {
                await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
                _logger.LogInformation("Product cached successfully: {ProductId}", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache product: {ProductId}", request.Id);
            }

            return productDto;
        }
    }
}
