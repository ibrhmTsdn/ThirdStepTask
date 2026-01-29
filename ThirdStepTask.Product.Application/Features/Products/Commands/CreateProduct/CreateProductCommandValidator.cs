using FluentValidation;

namespace ThirdStepTask.Product.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Validator for CreateProductCommand
    /// Implements validation rules following business requirements
    /// </summary>
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required")
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
                .Matches("^[A-Z0-9-]*$").WithMessage("SKU can only contain uppercase letters, numbers and hyphens");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(1000000).WithMessage("Price must be less than 1,000,000");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0).WithMessage("Discount percentage must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Discount percentage cannot exceed 100")
                .When(x => x.DiscountPercentage.HasValue);

            RuleFor(x => x.DiscountValidUntil)
                .GreaterThan(DateTime.UtcNow).WithMessage("Discount valid until date must be in the future")
                .When(x => x.DiscountValidUntil.HasValue);

            // Business rule: If discount is set, both percentage and valid until must be provided
            RuleFor(x => x)
                .Must(x => (x.DiscountPercentage.HasValue && x.DiscountValidUntil.HasValue) ||
                          (!x.DiscountPercentage.HasValue && !x.DiscountValidUntil.HasValue))
                .WithMessage("Both discount percentage and valid until date must be provided together");
        }
    }
}
