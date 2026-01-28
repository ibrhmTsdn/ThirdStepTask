using ThirdStepTask.Auth.Domain.Entities;

namespace ThirdStepTask.Auth.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task<RefreshToken> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp, CancellationToken cancellationToken = default);
        Task RevokeTokenAsync(string token, string revokedByIp, CancellationToken cancellationToken = default);
        Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}
