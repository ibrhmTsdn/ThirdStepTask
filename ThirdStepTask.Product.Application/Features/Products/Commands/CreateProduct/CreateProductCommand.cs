using MediatR;

namespace ThirdStepTask.Product.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Command to create a new product
    /// Implements CQRS pattern - Command side
    /// </summary>
    public class CreateProductCommand : IRequest<CreateProductResponse>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountValidUntil { get; set; }
    }
}
