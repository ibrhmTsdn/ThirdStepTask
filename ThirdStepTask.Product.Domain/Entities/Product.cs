using Common.Entities;

namespace ThirdStepTask.Product.Domain.Entities
{
    /// <summary>
    /// Product entity representing a product in the system
    /// </summary>
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountValidUntil { get; set; }

        // Calculated properties
        public decimal FinalPrice => DiscountPercentage.HasValue &&
                                     DiscountValidUntil.HasValue &&
                                     DiscountValidUntil.Value > DateTime.UtcNow
            ? Price - (Price * DiscountPercentage.Value / 100)
            : Price;

        public bool IsInStock => StockQuantity > 0;
        public bool HasDiscount => DiscountPercentage.HasValue &&
                                   DiscountValidUntil.HasValue &&
                                   DiscountValidUntil.Value > DateTime.UtcNow;

        public Product()
        {
            IsActive = true;
        }
    }
}
