using ThirdStepTask.Auth.Domain.Entities;

namespace ThirdStepTask.Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailOrUserNameAsync(string emailOrUserName, CancellationToken cancellationToken = default);
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    }
}
