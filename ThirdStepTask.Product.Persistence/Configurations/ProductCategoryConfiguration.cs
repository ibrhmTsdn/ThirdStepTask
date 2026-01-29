using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ThirdStepTask.Product.Persistence.Configurations
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<Domain.Entities.ProductCategory>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");

            builder.HasKey(pc => pc.Id);

            builder.Property(pc => pc.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pc => pc.Description)
                .HasMaxLength(500);

            builder.Property(pc => pc.Slug)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(pc => pc.Slug)
                .IsUnique()
                .HasDatabaseName("IX_ProductCategories_Slug");

            builder.HasIndex(pc => pc.DisplayOrder)
                .HasDatabaseName("IX_ProductCategories_DisplayOrder");

            builder.HasQueryFilter(pc => !pc.IsDeleted);
        }
    }
}
