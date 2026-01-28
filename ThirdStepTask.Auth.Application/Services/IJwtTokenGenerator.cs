using ThirdStepTask.Auth.Domain.Entities;

namespace ThirdStepTask.Auth.Application.Services
{
    /// <summary>
    /// Interface for JWT token generation
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(User user, List<string> roles, List<string> permissions);
        string GenerateRefreshToken();
        int AccessTokenExpirationMinutes { get; }
        int RefreshTokenExpirationDays { get; }
    }
}
