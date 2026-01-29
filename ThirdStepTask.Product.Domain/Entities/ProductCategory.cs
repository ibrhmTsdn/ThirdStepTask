using Common.Entities;

namespace ThirdStepTask.Product.Domain.Entities
{
    /// <summary>
    /// Product category entity for organizing products
    /// </summary>
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        public ProductCategory()
        {
            IsActive = true;
            DisplayOrder = 0;
        }
    }
}
