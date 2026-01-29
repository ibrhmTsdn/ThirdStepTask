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
using ThirdStepTask.Product.Domain.Interfaces;

namespace ThirdStepTask.Product.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Handler for CreateProductCommand
    /// Implements Single Responsibility Principle - only handles product creation
    /// Publishes integration event for other microservices
    /// </summary>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IEventBus eventBus,
            ILogger<CreateProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating product with SKU: {SKU}", request.SKU);

            // Check if SKU already exists
            if (await _productRepository.SkuExistsAsync(request.SKU, cancellationToken))
            {
                throw new ConflictException($"Product with SKU '{request.SKU}' already exists");
            }

            // Create product entity
            var product = new Domain.Entities.Product
            {
                Name = request.Name,
                Description = request.Description,
                SKU = request.SKU,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = request.Category,
                ImageUrl = request.ImageUrl,
                DiscountPercentage = request.DiscountPercentage,
                DiscountValidUntil = request.DiscountValidUntil,
                IsActive = true
            };

            // Save to database
            var createdProduct = await _productRepository.CreateAsync(product, cancellationToken);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);

            // Publish integration event for other microservices (Event-Driven Architecture)
            try
            {
                var integrationEvent = new ProductCreatedIntegrationEvent(
                    createdProduct.Id,
                    createdProduct.Name,
                    createdProduct.SKU,
                    createdProduct.Price,
                    createdProduct.Category,
                    createdProduct.StockQuantity
                );

                await _eventBus.PublishAsync(integrationEvent);

                _logger.LogInformation("ProductCreatedIntegrationEvent published for product ID: {ProductId}", createdProduct.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish ProductCreatedIntegrationEvent for product ID: {ProductId}", createdProduct.Id);
                // Don't throw - event publishing failure shouldn't fail the command
            }

            return new CreateProductResponse
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                SKU = createdProduct.SKU,
                Price = createdProduct.Price,
                Message = "Product created successfully"
            };
        }
    }
}
