using MediatR;
using ThirdStepTask.Product.Application.Features.Products.Queries.GetAllProducts;

namespace ThirdStepTask.Product.Application.Features.Products.Queries.GetProductById
{
    /// <summary>
    /// Query to get a product by ID
    /// Cached in Redis for performance
    /// </summary>
    public class GetProductByIdQuery : IRequest<ProductDto?>
    {
        public Guid Id { get; set; }

        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
