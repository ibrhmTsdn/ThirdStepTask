using Common.Models;

namespace ThirdStepTask.Product.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Product entity
    /// Follows Repository Pattern and Interface Segregation Principle
    /// </summary>
    public interface IProductRepository
    {
        Task<Entities.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Entities.Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<IEnumerable<Entities.Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaginatedResult<Entities.Product>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? category = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Entities.Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<Entities.Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default);
        Task<Entities.Product> CreateAsync(Entities.Product product, CancellationToken cancellationToken = default);
        Task<Entities.Product> UpdateAsync(Entities.Product product, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    }
}
