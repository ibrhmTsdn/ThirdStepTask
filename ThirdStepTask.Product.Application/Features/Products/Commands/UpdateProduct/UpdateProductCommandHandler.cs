using Common.Exceptions;
using EventBus.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStepTask.Product.Application.IntegrationEvents;
using ThirdStepTask.Product.Application.Services;
using ThirdStepTask.Product.Domain.Interfaces;

namespace ThirdStepTask.Product.Application.Features.Products.Commands.UpdateProduct
{
    /// <summary>
    /// Handler for UpdateProductCommand
    /// Publishes integration event and invalidates cache
    /// </summary>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IEventBus _eventBus;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            IEventBus eventBus,
            ICacheService cacheService,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _eventBus = eventBus;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", request.Id);

            // Get existing product
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Product", request.Id);
            }

            // Store old values for event
            var oldPrice = product.Price;
            var oldStock = product.StockQuantity;

            // Update product properties
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.Category = request.Category;
            product.ImageUrl = request.ImageUrl;
            product.IsActive = request.IsActive;
            product.DiscountPercentage = request.DiscountPercentage;
            product.DiscountValidUntil = request.DiscountValidUntil;

            // Save to database
            var updatedProduct = await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product updated successfully with ID: {ProductId}", updatedProduct.Id);

            // Invalidate cache for this product (Cache Invalidation Strategy)
            try
            {
                await _cacheService.RemoveAsync($"product:{updatedProduct.Id}");
                await _cacheService.RemoveAsync("products:all");
                await _cacheService.RemoveByPrefixAsync("products:page:");

                _logger.LogInformation("Cache invalidated for product ID: {ProductId}", updatedProduct.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for product ID: {ProductId}", updatedProduct.Id);
            }

            // Publish integration event (Event-Driven Architecture)
            try
            {
                var integrationEvent = new ProductUpdatedIntegrationEvent(
                    updatedProduct.Id,
                    updatedProduct.Name,
                    updatedProduct.SKU,
                    oldPrice,
                    updatedProduct.Price,
                    oldStock,
                    updatedProduct.StockQuantity
                );

                await _eventBus.PublishAsync(integrationEvent);

                _logger.LogInformation("ProductUpdatedIntegrationEvent published for product ID: {ProductId}", updatedProduct.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish ProductUpdatedIntegrationEvent for product ID: {ProductId}", updatedProduct.Id);
            }

            return new UpdateProductResponse
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Price = updatedProduct.Price,
                Message = "Product updated successfully"
            };
        }
    }
}
