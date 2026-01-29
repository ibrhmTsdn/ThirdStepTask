using MediatR;

namespace ThirdStepTask.Product.Application.Features.Products.Commands.UpdateProduct
{
    /// <summary>
    /// Command to update an existing product
    /// Requires JWT authentication (will be validated in API layer)
    /// </summary>
    public class UpdateProductCommand : IRequest<UpdateProductResponse>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountValidUntil { get; set; }
    }
}
