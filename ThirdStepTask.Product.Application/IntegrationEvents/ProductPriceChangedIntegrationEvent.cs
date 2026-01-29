using EventBus.Events;

namespace ThirdStepTask.Product.Application.IntegrationEvents
{
    /// <summary>
    /// Integration event published when product price changes
    /// </summary>
    public class ProductPriceChangedIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public decimal OldPrice { get; }
        public decimal NewPrice { get; }

        public ProductPriceChangedIntegrationEvent(Guid productId, decimal oldPrice, decimal newPrice)
        {
            ProductId = productId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
        }
    }
}
