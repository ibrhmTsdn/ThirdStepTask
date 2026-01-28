using ThirdStepTask.Auth.Application.Services;

namespace ThirdStepTask.Auth.Infrastructure.Services
{
    /// <summary>
    /// Password hashing implementation using BCrypt
    /// Implements Dependency Inversion Principle - depends on IPasswordHasher abstraction
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
