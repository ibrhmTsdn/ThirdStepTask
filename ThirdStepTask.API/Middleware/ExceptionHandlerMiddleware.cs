using Common.Exceptions;
using Common.Models;
using System.Net;
using System.Text.Json;

namespace ThirdStepTask.Auth.API.Middleware
{
    /// <summary>
    /// Global exception handler middleware
    /// Implements centralized error handling following best practices
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                BaseException baseEx => new
                {
                    statusCode = baseEx.StatusCode,
                    response = ApiResponse.FailureResult(baseEx.Message, baseEx.ErrorCode)
                },
                FluentValidation.ValidationException validationEx => new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    response = ApiResponse.FailureResult(
                        "Validation failed",
                        validationEx.Errors.Select(e => e.ErrorMessage).ToList())
                },
                _ => new
                {
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    response = ApiResponse.FailureResult(
                        "An internal server error occurred",
                        "INTERNAL_SERVER_ERROR")
                }
            };

            context.Response.StatusCode = response.statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response.response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
