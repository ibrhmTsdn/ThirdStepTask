using ThirdStepTask.Auth.Application.Services;
using ThirdStepTask.Auth.Domain.Entities;
using ThirdStepTask.Auth.Domain.Interfaces;
using Common.Exceptions;
using MediatR;

namespace ThirdStepTask.Auth.Application.Features.Auth.Commands.Register
{
    /// <summary>
    /// Handler for user registration command
    /// Implements Single Responsibility Principle - only handles registration logic
    /// </summary>
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                throw new ConflictException("Email already registered");
            }

            // Check if username already exists
            if (await _userRepository.UserNameExistsAsync(request.UserName, cancellationToken))
            {
                throw new ConflictException("Username already taken");
            }

            // Create new user entity
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save user to database
            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

            // Assign default "User" role
            var userRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
            if (userRole != null)
            {
                await _userRepository.AddUserToRoleAsync(createdUser.Id, userRole.Id, cancellationToken);
            }

            return new RegisterResponse
            {
                UserId = createdUser.Id,
                UserName = createdUser.UserName,
                Email = createdUser.Email,
                Message = "User registered successfully"
            };
        }
    }
}
