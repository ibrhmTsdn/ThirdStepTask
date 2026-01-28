using Microsoft.EntityFrameworkCore;
using ThirdStepTask.Auth.Domain.Entities;
using ThirdStepTask.Auth.Persistence.Context;

namespace ThirdStepTask.Auth.Persistence.Seeders
{
    /// <summary>
    /// Database seeder for initial roles and permissions
    /// </summary>
    public static class AuthDbSeeder
    {
        public static async Task SeedAsync(AuthDbContext context)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
            {
                new Role
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator with full access"
                },
                new Role
                {
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Regular user with limited access"
                },
                new Role
                {
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "Manager with elevated access"
                }
            };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed Permissions
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
            {
                // Product permissions
                new Permission
                {
                    Name = "Products.Read",
                    NormalizedName = "PRODUCTS.READ",
                    Description = "Can view products",
                    Category = "Products"
                },
                new Permission
                {
                    Name = "Products.Create",
                    NormalizedName = "PRODUCTS.CREATE",
                    Description = "Can create products",
                    Category = "Products"
                },
                new Permission
                {
                    Name = "Products.Update",
                    NormalizedName = "PRODUCTS.UPDATE",
                    Description = "Can update products",
                    Category = "Products"
                },
                new Permission
                {
                    Name = "Products.Delete",
                    NormalizedName = "PRODUCTS.DELETE",
                    Description = "Can delete products",
                    Category = "Products"
                },
                
                // User permissions
                new Permission
                {
                    Name = "Users.Read",
                    NormalizedName = "USERS.READ",
                    Description = "Can view users",
                    Category = "Users"
                },
                new Permission
                {
                    Name = "Users.Create",
                    NormalizedName = "USERS.CREATE",
                    Description = "Can create users",
                    Category = "Users"
                },
                new Permission
                {
                    Name = "Users.Update",
                    NormalizedName = "USERS.UPDATE",
                    Description = "Can update users",
                    Category = "Users"
                },
                new Permission
                {
                    Name = "Users.Delete",
                    NormalizedName = "USERS.DELETE",
                    Description = "Can delete users",
                    Category = "Users"
                }
            };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // Assign permissions to roles
            if (!await context.RolePermissions.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.NormalizedName == "ADMIN");
                var userRole = await context.Roles.FirstAsync(r => r.NormalizedName == "USER");
                var managerRole = await context.Roles.FirstAsync(r => r.NormalizedName == "MANAGER");

                var allPermissions = await context.Permissions.ToListAsync();
                var productReadPermission = allPermissions.First(p => p.NormalizedName == "PRODUCTS.READ");

                var rolePermissions = new List<RolePermission>();

                // Admin gets all permissions
                foreach (var permission in allPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }

                // User gets only read permissions
                rolePermissions.Add(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = productReadPermission.Id
                });

                // Manager gets product permissions
                var productPermissions = allPermissions.Where(p => p.Category == "Products");
                foreach (var permission in productPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = managerRole.Id,
                        PermissionId = permission.Id
                    });
                }

                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
