using Common.Events;

namespace ThirdStepTask.Product.Domain.Events
{
    /// <summary>
    /// Event raised when a product is updated
    /// </summary>
    public class ProductUpdatedDomainEvent : BaseDomainEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public decimal OldPrice { get; }
        public decimal NewPrice { get; }
        public int OldStock { get; }
        public int NewStock { get; }

        public ProductUpdatedDomainEvent(
            Guid productId,
            string productName,
            decimal oldPrice,
            decimal newPrice,
            int oldStock,
            int newStock)
        {
            ProductId = productId;
            ProductName = productName;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            OldStock = oldStock;
            NewStock = newStock;
        }
    }
}
