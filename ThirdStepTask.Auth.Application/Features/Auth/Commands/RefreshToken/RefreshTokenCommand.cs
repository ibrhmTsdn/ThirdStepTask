using MediatR;

namespace ThirdStepTask.Auth.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}
