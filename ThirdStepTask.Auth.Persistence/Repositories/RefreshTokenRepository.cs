using Microsoft.EntityFrameworkCore;
using ThirdStepTask.Auth.Domain.Entities;
using ThirdStepTask.Auth.Domain.Interfaces;
using ThirdStepTask.Auth.Persistence.Context;

namespace ThirdStepTask.Auth.Persistence.Repositories
{
    /// <summary>
    /// Refresh token repository implementation
    /// Manages refresh token lifecycle and security
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }

        public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp, CancellationToken cancellationToken = default)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = revokedByIp;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeTokenAsync(string token, string revokedByIp, CancellationToken cancellationToken = default)
        {
            var refreshToken = await GetByTokenAsync(token, cancellationToken);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = revokedByIp;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days after expiration
                .ToListAsync(cancellationToken);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
