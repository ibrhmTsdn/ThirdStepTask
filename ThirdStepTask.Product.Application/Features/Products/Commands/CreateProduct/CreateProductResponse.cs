namespace ThirdStepTask.Product.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
