using EventBus.Events;

namespace ThirdStepTask.Product.Application.IntegrationEvents
{
    /// <summary>
    /// Integration event published when a product is deleted
    /// </summary>
    public class ProductDeletedIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }

        public ProductDeletedIntegrationEvent(Guid productId, string productName)
        {
            ProductId = productId;
            ProductName = productName;
        }
    }
}
