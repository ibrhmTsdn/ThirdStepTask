using EventBus.Events;

namespace ThirdStepTask.Product.Application.IntegrationEvents
{
    /// <summary>
    /// Integration event published when a product is created
    /// Other microservices can subscribe to this event
    /// </summary>
    public class ProductCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public string SKU { get; }
        public decimal Price { get; }
        public string Category { get; }
        public int StockQuantity { get; }

        public ProductCreatedIntegrationEvent(
            Guid productId,
            string productName,
            string sku,
            decimal price,
            string category,
            int stockQuantity)
        {
            ProductId = productId;
            ProductName = productName;
            SKU = sku;
            Price = price;
            Category = category;
            StockQuantity = stockQuantity;
        }
    }
}
