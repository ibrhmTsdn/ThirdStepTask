using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThirdStepTask.Auth.Application.Features.Auth.Commands.Login;
using ThirdStepTask.Auth.Application.Features.Auth.Commands.RefreshToken;
using ThirdStepTask.Auth.Application.Features.Auth.Commands.Register;

namespace ThirdStepTask.Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", command.Email);

            var result = await _mediator.Send(command);

            _logger.LogInformation("User registered successfully: {UserId}", result.UserId);

            return Ok(ApiResponse<RegisterResponse>.SuccessResult(result, "User registered successfully"));
        }

        /// <summary>
        /// Login with email/username and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("Login attempt for: {EmailOrUserName}", command.EmailOrUserName);

            var result = await _mediator.Send(command);

            _logger.LogInformation("User logged in successfully: {UserId}", result.User.Id);

            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Login successful"));
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("Refresh token attempt");

            var result = await _mediator.Send(command);

            _logger.LogInformation("Token refreshed successfully");

            return Ok(ApiResponse<RefreshTokenResponse>.SuccessResult(result, "Token refreshed successfully"));
        }
    }
}
