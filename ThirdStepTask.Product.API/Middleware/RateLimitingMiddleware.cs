using System.Collections.Concurrent;

namespace ThirdStepTask.Product.API.Middleware
{
    /// <summary>
    /// Rate limiting middleware to prevent API abuse
    /// Implements sliding window rate limiting
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestLog = new();
        private readonly int _requestLimit;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _requestLimit = configuration.GetValue<int>("RateLimiting:RequestLimit", 100);
            _timeWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimiting:TimeWindowMinutes", 1));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIdentifier = GetClientIdentifier(context);

            if (!IsRequestAllowed(clientIdentifier))
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientIdentifier);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = "Rate limit exceeded. Please try again later.",
                    errorCode = "RATE_LIMIT_EXCEEDED",
                    retryAfter = _timeWindow.TotalSeconds
                };

                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get user ID from claims if authenticated
            var userId = context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            // Fall back to IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ipAddress}";
        }

        private bool IsRequestAllowed(string clientIdentifier)
        {
            var now = DateTime.UtcNow;

            var requestQueue = _requestLog.GetOrAdd(clientIdentifier, _ => new Queue<DateTime>());

            lock (requestQueue)
            {
                // Remove old requests outside the time window
                while (requestQueue.Count > 0 && (now - requestQueue.Peek()) > _timeWindow)
                {
                    requestQueue.Dequeue();
                }

                // Check if limit is exceeded
                if (requestQueue.Count >= _requestLimit)
                {
                    return false;
                }

                // Add current request
                requestQueue.Enqueue(now);
                return true;
            }
        }
    }
}
