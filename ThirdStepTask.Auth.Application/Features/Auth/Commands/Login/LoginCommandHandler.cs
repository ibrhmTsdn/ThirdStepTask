using Common.Exceptions;
using MediatR;
using ThirdStepTask.Auth.Application.Services;
using ThirdStepTask.Auth.Domain.Interfaces;

namespace ThirdStepTask.Auth.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Handler for user login command
    /// Implements JWT token generation and refresh token mechanism
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Find user by email or username
            var user = await _userRepository.GetByEmailOrUserNameAsync(request.EmailOrUserName, cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email/username or password");
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email/username or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new UnauthorizedException("User account is disabled");
            }

            // Get user roles
            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var roleNames = roles.Select(r => r.Name).ToList();

            // Get user permissions
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
            var permissionNames = permissions.Select(p => p.Name).ToList();

            // Generate JWT access token
            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roleNames, permissionNames);
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtTokenGenerator.AccessTokenExpirationMinutes);

            // Generate refresh token
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtTokenGenerator.RefreshTokenExpirationDays);

            // Save refresh token to database
            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiration,
                CreatedByIp = request.IpAddress ?? "Unknown",
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity, cancellationToken);

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration,
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roleNames
                }
            };
        }
    }
}
