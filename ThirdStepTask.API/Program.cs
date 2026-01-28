using ThirdStepTask.Auth.API.Behaviors;
using ThirdStepTask.Auth.API.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using ThirdStepTask.Auth.Persistence.Context;
using ThirdStepTask.Auth.Application.Features.Auth.Commands.Register;
using ThirdStepTask.Auth.Application.Services;
using ThirdStepTask.Auth.Infrastructure.Services;
using ThirdStepTask.Auth.Domain.Interfaces;
using ThirdStepTask.Auth.Persistence.Repositories;
using ThirdStepTask.Auth.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

// ====================
// 12-FACTOR APP CONFIGURATION
// ====================

// Factor 3: Config - Store config in environment
// Configuration is loaded from appsettings.json and environment variables
var configuration = builder.Configuration;

// Factor 11: Logs - Treat logs as event streams
// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Auth.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("logs/auth-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ====================
// SERVICES CONFIGURATION
// ====================

// Factor 2: Dependencies - Explicitly declare dependencies
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Authentication and Authorization Microservice"
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

// Database Configuration (Factor 4: Backing services)
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

// MediatR Configuration (CQRS)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly);
});

// Add Validation Pipeline Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(RegisterCommand).Assembly);

// Dependency Injection (Following Dependency Inversion Principle)
// Application Layer Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Domain Layer Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

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
        name: "AuthDatabase",
        tags: new[] { "database", "sql" });

var app = builder.Build();

// ====================
// DATABASE MIGRATION AND SEEDING
// ====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await AuthDbSeeder.SeedAsync(context);
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while seeding the database");
    }
}

// ====================
// MIDDLEWARE PIPELINE
// ====================

// Factor 9: Disposability - Maximize robustness with fast startup and graceful shutdown
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handler
app.UseExceptionHandlerMiddleware();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Factor 6: Processes - Execute the app as stateless processes
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Factor 7: Port binding - Export services via port binding
var port = configuration["PORT"] ?? "5001";
Log.Information("Starting Auth API on port {Port}", port);

try
{
    Log.Information("Auth API Starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}