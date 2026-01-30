# ====================
# DOCKER COMPOSE HELPER SCRIPTS (PowerShell)
# ====================

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('start-dev', 'start-prod', 'stop', 'restart', 'logs', 'logs-auth', 'logs-product', 'build', 'clean', 'status', 'db-migrate')]
    [string]$Command
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Microservices Docker Management" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

switch ($Command) {
    'start-dev' {
        Write-Host "Starting services in DEVELOPMENT mode..." -ForegroundColor Yellow
        docker-compose up -d
        Write-Host ""
        Write-Host "✅ Services started!" -ForegroundColor Green
        Write-Host "📝 API Gateway: http://localhost:5000" -ForegroundColor Cyan
        Write-Host "📝 Auth API: http://localhost:5001" -ForegroundColor Cyan
        Write-Host "📝 Product API: http://localhost:5002" -ForegroundColor Cyan
        Write-Host "📝 SQL Server: localhost:1433 (sa/Ibrahim38-)" -ForegroundColor Cyan
        Write-Host "📝 Redis: localhost:6379" -ForegroundColor Cyan
        Write-Host "📝 RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
        Write-Host "📝 Portainer: http://localhost:9000" -ForegroundColor Cyan
    }
    
    'start-prod' {
        Write-Host "Starting services in PRODUCTION mode..." -ForegroundColor Yellow
        if (-not (Test-Path .env)) {
            Write-Host "❌ Error: .env file not found!" -ForegroundColor Red
            Write-Host "Please copy .env.example to .env and configure it first." -ForegroundColor Yellow
            exit 1
        }
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
        Write-Host ""
        Write-Host "✅ Services started in production mode!" -ForegroundColor Green
    }
    
    'stop' {
        Write-Host "Stopping all services..." -ForegroundColor Yellow
        docker-compose down
        Write-Host "✅ All services stopped!" -ForegroundColor Green
    }
    
    'restart' {
        Write-Host "Restarting all services..." -ForegroundColor Yellow
        docker-compose restart
        Write-Host "✅ Services restarted!" -ForegroundColor Green
    }
    
    'logs' {
        docker-compose logs -f
    }
    
    'logs-auth' {
        docker-compose logs -f auth-api
    }
    
    'logs-product' {
        docker-compose logs -f product-api
    }
    
    'build' {
        Write-Host "Building all Docker images..." -ForegroundColor Yellow
        docker-compose build --no-cache
        Write-Host "✅ Build complete!" -ForegroundColor Green
    }
    
    'clean' {
        Write-Host "⚠️  WARNING: This will remove all containers, volumes, and images!" -ForegroundColor Red
        $confirmation = Read-Host "Are you sure? (y/n)"
        if ($confirmation -eq 'y') {
            docker-compose down -v --rmi all
            Write-Host "✅ Cleanup complete!" -ForegroundColor Green
        }
    }
    
    'status' {
        docker-compose ps
    }
    
    'db-migrate' {
        Write-Host "Running database migrations..." -ForegroundColor Yellow
        Write-Host "Auth Database:" -ForegroundColor Cyan
        docker-compose exec auth-api dotnet ef database update --project Auth.Persistence
        Write-Host ""
        Write-Host "Product Database:" -ForegroundColor Cyan
        docker-compose exec product-api dotnet ef database update --project Product.Persistence
        Write-Host "✅ Migrations complete!" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan


