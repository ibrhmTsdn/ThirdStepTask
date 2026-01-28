namespace ThirdStepTask.Auth.Application.Services
{
    /// <summary>
    /// Interface for password hashing operations
    /// Follows Interface Segregation Principle - clients only depend on methods they use
    /// </summary>
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
