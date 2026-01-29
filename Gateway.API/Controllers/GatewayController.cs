using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly ILogger<GatewayController> _logger;
        private readonly IConfiguration _configuration;

        public GatewayController(ILogger<GatewayController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get gateway information and available routes
        /// </summary>
        [HttpGet("info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetInfo()
        {
            var info = new
            {
                name = "API Gateway",
                version = "1.0.0",
                status = "running",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                routes = new
                {
                    authentication = new
                    {
                        basePath = "/auth",
                        endpoints = new[]
                        {
                        "POST /auth/register - Register new user",
                        "POST /auth/login - Login",
                        "POST /auth/refresh-token - Refresh access token"
                    }
                    },
                    products = new
                    {
                        basePath = "/products",
                        endpoints = new[]
                        {
                        "GET /products - List all products (cached)",
                        "GET /products/{id} - Get product by ID (cached)",
                        "POST /products - Create product (requires auth)",
                        "PUT /products/{id} - Update product (requires auth)",
                        "DELETE /products/{id} - Delete product (requires admin)"
                    }
                    }
                },
                features = new[]
                {
                "JWT Authentication",
                "Rate Limiting (100 req/min per IP)",
                "Request/Response Logging",
                "Health Checks",
                "CORS Support",
                "Reverse Proxy (Yarp)"
            },
                rateLimits = new
                {
                    globalLimit = _configuration.GetValue<int>("RateLimiting:GlobalLimit", 1000),
                    perIpLimit = _configuration.GetValue<int>("RateLimiting:PerIpLimit", 100),
                    windowSeconds = _configuration.GetValue<int>("RateLimiting:WindowSeconds", 60)
                }
            };

            return Ok(info);
        }

        /// <summary>
        /// Test endpoint to verify gateway is working
        /// </summary>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok(new
            {
                message = "pong",
                timestamp = DateTime.UtcNow,
                gateway = "API Gateway",
                status = "healthy"
            });
        }

        /// <summary>
        /// Get gateway statistics
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetStats()
        {
            var stats = new
            {
                uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                timestamp = DateTime.UtcNow,
                processId = Process.GetCurrentProcess().Id,
                workingSet = GC.GetTotalMemory(false) / 1024 / 1024, // MB
                gcCollections = new
                {
                    gen0 = GC.CollectionCount(0),
                    gen1 = GC.CollectionCount(1),
                    gen2 = GC.CollectionCount(2)
                }
            };

            return Ok(stats);
        }
    }
}
