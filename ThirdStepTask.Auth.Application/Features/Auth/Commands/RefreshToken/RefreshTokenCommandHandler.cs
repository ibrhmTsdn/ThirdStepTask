using Common.Exceptions;
using MediatR;
using ThirdStepTask.Auth.Application.Services;
using ThirdStepTask.Auth.Domain.Interfaces;

namespace ThirdStepTask.Auth.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Handler for refresh token command
    /// Implements secure token rotation mechanism
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Get refresh token from database
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                throw new UnauthorizedException("Invalid refresh token");
            }

            // Check if token is active
            if (!refreshToken.IsActive)
            {
                throw new UnauthorizedException("Refresh token is expired or revoked");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedException("User not found or inactive");
            }

            // Revoke old refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = request.IpAddress ?? "Unknown";

            // Get user roles and permissions
            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var roleNames = roles.Select(r => r.Name).ToList();

            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
            var permissionNames = permissions.Select(p => p.Name).ToList();

            // Generate new JWT access token
            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roleNames, permissionNames);
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtTokenGenerator.AccessTokenExpirationMinutes);

            // Generate new refresh token
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtTokenGenerator.RefreshTokenExpirationDays);

            // Update old token with replacement info
            refreshToken.ReplacedByToken = newRefreshToken;
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

            // Save new refresh token to database
            var newRefreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiration,
                CreatedByIp = request.IpAddress ?? "Unknown",
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity, cancellationToken);

            return new RefreshTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };

        }
    }
}