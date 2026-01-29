using EventBus.Events;

namespace ThirdStepTask.Product.Application.IntegrationEvents
{
    /// <summary>
    /// Integration event published when a product is updated
    /// </summary>
    public class ProductUpdatedIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public string SKU { get; }
        public decimal OldPrice { get; }
        public decimal NewPrice { get; }
        public int OldStockQuantity { get; }
        public int NewStockQuantity { get; }

        public ProductUpdatedIntegrationEvent(
            Guid productId,
            string productName,
            string sku,
            decimal oldPrice,
            decimal newPrice,
            int oldStockQuantity,
            int newStockQuantity)
        {
            ProductId = productId;
            ProductName = productName;
            SKU = sku;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            OldStockQuantity = oldStockQuantity;
            NewStockQuantity = newStockQuantity;
        }
    }
}
