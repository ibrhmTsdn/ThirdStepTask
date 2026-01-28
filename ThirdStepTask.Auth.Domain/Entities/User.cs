using Common.Entities;

namespace ThirdStepTask.Auth.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public User()
        {
            IsActive = true;
            EmailConfirmed = false;
        }
    }
}
