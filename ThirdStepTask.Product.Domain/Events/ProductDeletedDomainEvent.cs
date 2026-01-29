using Common.Events;

namespace ThirdStepTask.Product.Domain.Events
{
    /// <summary>
    /// Event raised when a product is deleted
    /// </summary>
    public class ProductDeletedDomainEvent : BaseDomainEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }

        public ProductDeletedDomainEvent(Guid productId, string productName)
        {
            ProductId = productId;
            ProductName = productName;
        }
    }
}
