using Common.Events;

namespace ThirdStepTask.Product.Domain.Events
{
    /// <summary>
    /// Event raised when a new product is created
    /// </summary>
    public class ProductCreatedDomainEvent : BaseDomainEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public decimal Price { get; }
        public string Category { get; }

        public ProductCreatedDomainEvent(Guid productId, string productName, decimal price, string category)
        {
            ProductId = productId;
            ProductName = productName;
            Price = price;
            Category = category;
        }
    }
}
