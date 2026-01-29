using System.Collections.Concurrent;

namespace Gateway.API.Middleware
{
    /// <summary>
    /// Advanced rate limiting middleware for API Gateway
    /// Implements both global and per-IP rate limiting
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _requestLog = new();
        private static readonly ConcurrentDictionary<string, DateTime> _blockedClients = new();

        private readonly int _globalLimit;
        private readonly int _perIpLimit;
        private readonly TimeSpan _timeWindow;
        private readonly TimeSpan _blockDuration;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;

            _globalLimit = configuration.GetValue<int>("RateLimiting:GlobalLimit", 1000);
            _perIpLimit = configuration.GetValue<int>("RateLimiting:PerIpLimit", 100);
            _timeWindow = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:WindowSeconds", 60));
            _blockDuration = TimeSpan.FromMinutes(5);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var now = DateTime.UtcNow;

            // Check if client is blocked
            if (_blockedClients.TryGetValue(clientId, out var blockedUntil))
            {
                if (now < blockedUntil)
                {
                    _logger.LogWarning("Blocked client attempted access: {ClientId}", clientId);
                    await ReturnRateLimitError(context, "Client is temporarily blocked due to excessive requests", (int)(blockedUntil - now).TotalSeconds);
                    return;
                }
                else
                {
                    _blockedClients.TryRemove(clientId, out _);
                }
            }

            // Get or create client info
            var clientInfo = _requestLog.GetOrAdd(clientId, _ => new ClientRequestInfo());

            lock (clientInfo)
            {
                // Clean old requests
                clientInfo.Requests.RemoveAll(r => (now - r) > _timeWindow);

                // Check limits
                var currentCount = clientInfo.Requests.Count;

                if (currentCount >= _perIpLimit)
                {
                    _logger.LogWarning("Rate limit exceeded for client: {ClientId} ({Count} requests)", clientId, currentCount);

                    // Block client if excessive abuse
                    if (currentCount >= _perIpLimit * 2)
                    {
                        _blockedClients.TryAdd(clientId, now.Add(_blockDuration));
                        _logger.LogError("Client blocked for excessive requests: {ClientId}", clientId);
                    }

                    await ReturnRateLimitError(context, "Rate limit exceeded", (int)_timeWindow.TotalSeconds);
                    return;
                }

                // Check global limit
                var totalRequests = _requestLog.Values.Sum(ci => ci.Requests.Count);
                if (totalRequests >= _globalLimit)
                {
                    _logger.LogWarning("Global rate limit exceeded: {TotalRequests}", totalRequests);
                    await ReturnRateLimitError(context, "Service temporarily unavailable due to high traffic", 60);
                    return;
                }

                // Add current request
                clientInfo.Requests.Add(now);
            }

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = _perIpLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = (_perIpLimit - clientInfo.Requests.Count).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(_timeWindow).ToUnixTimeSeconds().ToString();

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get user ID from JWT token
            var userId = context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            // Fall back to IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            // Check for X-Forwarded-For header (when behind proxy/load balancer)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                ipAddress = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            }

            return $"ip:{ipAddress ?? "unknown"}";
        }

        private async Task ReturnRateLimitError(HttpContext context, string message, int retryAfter)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Retry-After"] = retryAfter.ToString();

            var response = new
            {
                success = false,
                message = message,
                errorCode = "RATE_LIMIT_EXCEEDED",
                retryAfter = retryAfter,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        private class ClientRequestInfo
        {
            public List<DateTime> Requests { get; } = new();
        }
    }
}
