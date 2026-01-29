using MediatR;

namespace ThirdStepTask.Product.Application.Features.Products.Queries.GetAllProducts
{
    /// <summary>
    /// Query to get all products
    /// Implements CQRS pattern - Query side
    /// Results will be cached in Redis
    /// </summary>
    public class GetAllProductsQuery : IRequest<GetAllProductsResponse>
    {
        public bool IncludeInactive { get; set; } = false;
    }
}
