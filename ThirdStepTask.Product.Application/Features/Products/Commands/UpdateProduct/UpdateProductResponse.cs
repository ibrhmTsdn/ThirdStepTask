namespace ThirdStepTask.Product.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
