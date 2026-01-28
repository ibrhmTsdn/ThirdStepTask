using ThirdStepTask.Auth.Domain.Entities;

namespace ThirdStepTask.Auth.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    }
}
