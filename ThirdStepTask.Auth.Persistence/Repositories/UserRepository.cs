using ThirdStepTask.Auth.Domain.Entities;
using ThirdStepTask.Auth.Domain.Interfaces;
using ThirdStepTask.Auth.Persistence.Context;

namespace ThirdStepTask.Auth.Persistence.Repositories
{
    /// <summary>
    /// User repository implementation
    /// Implements Repository Pattern for data access abstraction
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<User?> GetByEmailOrUserNameAsync(string emailOrUserName, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == emailOrUserName || u.UserName == emailOrUserName, cancellationToken);
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync(cancellationToken);

            return roles;
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync(cancellationToken);

            return permissions;
        }

        public async Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };

            await _context.UserRoles.AddAsync(userRole, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
