using EventBus.Abstractions;
using EventBus.RabbitMQ;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using System.Text;
using ThirdStepTask.Product.API.Behaviors;
using ThirdStepTask.Product.API.Middleware;
using ThirdStepTask.Product.Application.Features.Products.Commands.CreateProduct;
using ThirdStepTask.Product.Application.Services;
using ThirdStepTask.Product.Domain.Interfaces;
using ThirdStepTask.Product.Infrastructure.Services;
using ThirdStepTask.Product.Persistence.Context;
using ThirdStepTask.Product.Persistence.Repositories;
using ThirdStepTask.Product.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

// ====================
// 12-FACTOR APP CONFIGURATION
// ====================

var configuration = builder.Configuration;

// Factor 11: Logs - Structured logging with Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Product.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("logs/product-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ====================
// SERVICES CONFIGURATION
// ====================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "Product Management Microservice with CQRS, Redis Cache, and Event-Driven Architecture"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Factor 4: Backing Services - Database Configuration
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connectionString));

// MediatR Configuration (CQRS)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
});

// Add Validation Pipeline Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);

// Repository Pattern - Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Redis Cache Configuration (Factor 4: Backing Services)
var redisConnectionString = configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(redisConnectionString ?? "localhost:6379");
    config.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();

// RabbitMQ Configuration (Event-Driven Architecture)
builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
    var rabbitMQHost = configuration["RabbitMQ:Host"] ?? "localhost";
    var rabbitMQPort = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
    var rabbitMQUser = configuration["RabbitMQ:Username"] ?? "guest";
    var rabbitMQPassword = configuration["RabbitMQ:Password"] ?? "guest";

    var factory = new ConnectionFactory
    {
        HostName = rabbitMQHost,
        Port = rabbitMQPort,
        UserName = rabbitMQUser,
        Password = rabbitMQPassword,
        DispatchConsumersAsync = true
    };

    return new DefaultRabbitMQPersistentConnection(factory, logger, 5);
});

builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
{
    var rabbitMQConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
    var serviceProvider = sp.GetRequiredService<IServiceProvider>();

    return new RabbitMQEventBus(rabbitMQConnection, logger, serviceProvider, 5);
});

// JWT Authentication Configuration
var jwtSettings = configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString ?? throw new InvalidOperationException("Connection string not configured"),
        name: "ProductDatabase",
        tags: new[] { "database", "sql" })
    .AddRedis(
        redisConnectionString ?? "localhost:6379",
        name: "Redis",
        tags: new[] { "cache", "redis" });

var app = builder.Build();

// ====================
// DATABASE MIGRATION AND SEEDING
// ====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await ProductDbSeeder.SeedAsync(context);
        Log.Information("Product database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while seeding the product database");
    }
}

// ====================
// MIDDLEWARE PIPELINE
// ====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handler
app.UseExceptionHandlerMiddleware();

app.UseSerilogRequestLogging();

// Rate Limiting Middleware
app.UseRateLimiting();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Factor 6: Processes - Stateless authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Factor 7: Port binding
var port = configuration["PORT"] ?? "5002";
Log.Information("Starting Product API on port {Port}", port);

try
{
    Log.Information("Product API Starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Product API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}