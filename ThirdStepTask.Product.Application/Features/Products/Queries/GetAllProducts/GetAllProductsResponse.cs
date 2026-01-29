namespace ThirdStepTask.Product.Application.Features.Products.Queries.GetAllProducts
{
    public class GetAllProductsResponse
    {
        public List<ProductDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
