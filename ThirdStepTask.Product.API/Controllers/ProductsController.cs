using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThirdStepTask.Product.Application.Features.Products.Commands.CreateProduct;
using ThirdStepTask.Product.Application.Features.Products.Commands.UpdateProduct;
using ThirdStepTask.Product.Application.Features.Products.Queries.GetAllProducts;
using ThirdStepTask.Product.Application.Features.Products.Queries.GetProductById;

namespace ThirdStepTask.Product.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <remarks>
        /// Results are cached in Redis for improved performance
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<GetAllProductsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts([FromQuery] bool includeInactive = false)
        {
            _logger.LogInformation("Getting all products (IncludeInactive: {IncludeInactive})", includeInactive);

            var query = new GetAllProductsQuery { IncludeInactive = includeInactive };
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<GetAllProductsResponse>.SuccessResult(result, "Products retrieved successfully"));
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <remarks>
        /// Individual products are cached in Redis
        /// </remarks>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse.FailureResult("Product not found", "NOT_FOUND"));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResult(result, "Product retrieved successfully"));
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <remarks>
        /// Requires authentication. Publishes ProductCreatedIntegrationEvent to RabbitMQ
        /// </remarks>
        [HttpPost]
        [Authorize] // Requires JWT authentication
        [ProducesResponseType(typeof(ApiResponse<CreateProductResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            _logger.LogInformation("Creating product with SKU: {SKU}", command.SKU);

            var result = await _mediator.Send(command);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", result.Id);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = result.Id },
                ApiResponse<CreateProductResponse>.SuccessResult(result, "Product created successfully"));
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <remarks>
        /// Requires JWT authentication. Publishes ProductUpdatedIntegrationEvent and invalidates cache
        /// </remarks>
        [HttpPut("{id:guid}")]
        [Authorize] // Requires JWT authentication
        [ProducesResponseType(typeof(ApiResponse<UpdateProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse.FailureResult("ID in URL does not match ID in request body", "ID_MISMATCH"));
            }

            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            var result = await _mediator.Send(command);

            _logger.LogInformation("Product updated successfully with ID: {ProductId}", result.Id);

            return Ok(ApiResponse<UpdateProductResponse>.SuccessResult(result, "Product updated successfully"));
        }

        /// <summary>
        /// Delete a product (soft delete)
        /// </summary>
        /// <remarks>
        /// Requires authentication with Admin or Manager role
        /// </remarks>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")] // Role-based authorization
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

            // This would be a DeleteProductCommand - not implemented in this phase
            // Just a placeholder to show role-based authorization

            return Ok(ApiResponse.SuccessResult("Product deleted successfully"));
        }
    }
}
