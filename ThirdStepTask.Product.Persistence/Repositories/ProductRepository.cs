using Common.Models;
using Microsoft.EntityFrameworkCore;
using ThirdStepTask.Product.Domain.Interfaces;
using ThirdStepTask.Product.Persistence.Context;

namespace ThirdStepTask.Product.Persistence.Repositories
{
    /// <summary>
    /// Product repository implementation
    /// Implements Repository Pattern for data access abstraction
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Domain.Entities.Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
        }

        public async Task<IEnumerable<Domain.Entities.Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaginatedResult<Domain.Entities.Product>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? category = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PaginatedResult<Domain.Entities.Product>.Create(
                products,
                totalCount,
                pageNumber,
                pageSize);
        }

        public async Task<IEnumerable<Domain.Entities.Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.Category == category && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Domain.Entities.Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.StockQuantity <= threshold && p.IsActive)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync(cancellationToken);
        }

        public async Task<Domain.Entities.Product> CreateAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task<Domain.Entities.Product> UpdateAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(id, cancellationToken);

            if (product == null)
            {
                return false;
            }

            // Soft delete
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.SKU == sku, cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.CountAsync(cancellationToken);
        }
    }
}
