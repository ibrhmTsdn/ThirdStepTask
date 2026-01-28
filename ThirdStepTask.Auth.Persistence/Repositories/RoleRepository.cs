using Microsoft.EntityFrameworkCore;
using ThirdStepTask.Auth.Domain.Entities;
using ThirdStepTask.Auth.Domain.Interfaces;
using ThirdStepTask.Auth.Persistence.Context;

namespace ThirdStepTask.Auth.Persistence.Repositories
{
    /// <summary>
    /// Role repository implementation
    /// Manages role and permission relationships
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthDbContext _context;

        public RoleRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.ToUpper();
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync(cancellationToken);
        }

        public async Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default)
        {
            role.NormalizedName = role.Name.ToUpper();
            await _context.Roles.AddAsync(role, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return role;
        }

        public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            role.NormalizedName = role.Name.ToUpper();
            _context.Roles.Update(role);
            await _context.SaveChangesAsync(cancellationToken);
            return role;
        }

        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.ToUpper();
            return await _context.Roles.AnyAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        }
    }
}
