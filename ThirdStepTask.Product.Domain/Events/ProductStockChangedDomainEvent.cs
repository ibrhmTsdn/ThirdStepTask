using Common.Events;

namespace ThirdStepTask.Product.Domain.Events
{
    /// <summary>
    /// Event raised when product stock changes
    /// </summary>
    public class ProductStockChangedDomainEvent : BaseDomainEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public int OldQuantity { get; }
        public int NewQuantity { get; }
        public string ChangeReason { get; }

        public ProductStockChangedDomainEvent(
            Guid productId,
            string productName,
            int oldQuantity,
            int newQuantity,
            string changeReason)
        {
            ProductId = productId;
            ProductName = productName;
            OldQuantity = oldQuantity;
            NewQuantity = newQuantity;
            ChangeReason = changeReason;
        }
    }
}
