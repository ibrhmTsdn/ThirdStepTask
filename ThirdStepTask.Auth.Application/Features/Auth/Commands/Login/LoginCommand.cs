using MediatR;

namespace ThirdStepTask.Auth.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string EmailOrUserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}
