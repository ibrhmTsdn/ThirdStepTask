using System.Diagnostics;

namespace Gateway.API.Middleware
{
    /// <summary>
    /// Middleware for logging all requests and responses through the gateway
    /// Useful for debugging and monitoring
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            // Log request
            LogRequest(context, requestId);

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
                stopwatch.Stop();

                // Log response
                LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request {RequestId} failed after {ElapsedMs}ms: {Method} {Path}",
                    requestId, stopwatch.ElapsedMilliseconds, context.Request.Method, context.Request.Path);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private void LogRequest(HttpContext context, string requestId)
        {
            var request = context.Request;
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            _logger.LogInformation(
                "Request {RequestId} started: {Method} {Path} from {ClientIp}",
                requestId,
                request.Method,
                request.Path,
                clientIp);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Request {RequestId} details - QueryString: {QueryString}, Headers: {Headers}",
                    requestId,
                    request.QueryString,
                    string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}")));
            }
        }

        private void LogResponse(HttpContext context, string requestId, long elapsedMs)
        {
            var response = context.Response;

            var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                          response.StatusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(
                logLevel,
                "Request {RequestId} completed: {StatusCode} in {ElapsedMs}ms",
                requestId,
                response.StatusCode,
                elapsedMs);

            // Performance warning
            if (elapsedMs > 5000)
            {
                _logger.LogWarning(
                    "Slow request detected {RequestId}: {Method} {Path} took {ElapsedMs}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs);
            }
        }
    }
}
