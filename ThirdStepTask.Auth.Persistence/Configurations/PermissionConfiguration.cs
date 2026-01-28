using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThirdStepTask.Auth.Domain.Entities;

namespace ThirdStepTask.Auth.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.NormalizedName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(200);

            builder.Property(p => p.Category)
                .HasMaxLength(50);

            builder.HasIndex(p => p.NormalizedName)
                .IsUnique()
                .HasDatabaseName("IX_Permissions_NormalizedName");

            // Relationships
            builder.HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
