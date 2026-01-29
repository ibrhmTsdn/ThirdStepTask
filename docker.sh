#!/bin/bash

# ====================
# DOCKER COMPOSE HELPER SCRIPTS
# ====================

echo "============================================"
echo "Microservices Docker Management"
echo "============================================"
echo ""
echo "Available commands:"
echo ""
echo "1. start-dev       - Start all services in development mode"
echo "2. start-prod      - Start all services in production mode"
echo "3. stop            - Stop all services"
echo "4. restart         - Restart all services"
echo "5. logs            - View logs from all services"
echo "6. logs-auth       - View Auth API logs"
echo "7. logs-product    - View Product API logs"
echo "8. build           - Build all Docker images"
echo "9. clean           - Remove all containers, volumes, and images"
echo "10. status         - Show status of all containers"
echo "11. db-migrate     - Run database migrations"
echo ""

case "$1" in
  start-dev)
    echo "Starting services in DEVELOPMENT mode..."
    docker-compose up -d
    echo ""
    echo "✅ Services started!"
    echo "📝 API Gateway: http://localhost:5000"
    echo "📝 Auth API: http://localhost:5001"
    echo "📝 Product API: http://localhost:5002"
    echo "📝 SQL Server: localhost:1433 (sa/MyProject2024!@#)"
    echo "📝 Redis: localhost:6379"
    echo "📝 RabbitMQ Management: http://localhost:15672 (guest/guest)"
    echo "📝 Portainer: http://localhost:9000"
    ;;
    
  start-prod)
    echo "Starting services in PRODUCTION mode..."
    if [ ! -f .env ]; then
      echo "❌ Error: .env file not found!"
      echo "Please copy .env.example to .env and configure it first."
      exit 1
    fi
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
    echo ""
    echo "✅ Services started in production mode!"
    ;;
    
  stop)
    echo "Stopping all services..."
    docker-compose down
    echo "✅ All services stopped!"
    ;;
    
  restart)
    echo "Restarting all services..."
    docker-compose restart
    echo "✅ Services restarted!"
    ;;
    
  logs)
    docker-compose logs -f
    ;;
    
  logs-auth)
    docker-compose logs -f auth-api
    ;;
    
  logs-product)
    docker-compose logs -f product-api
    ;;
    
  build)
    echo "Building all Docker images..."
    docker-compose build --no-cache
    echo "✅ Build complete!"
    ;;
    
  clean)
    echo "⚠️  WARNING: This will remove all containers, volumes, and images!"
    read -p "Are you sure? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
      docker-compose down -v --rmi all
      echo "✅ Cleanup complete!"
    fi
    ;;
    
  status)
    docker-compose ps
    ;;
    
  db-migrate)
    echo "Running database migrations..."
    echo "Auth Database:"
    docker-compose exec auth-api dotnet ef database update --project Auth.Persistence
    echo ""
    echo "Product Database:"
    docker-compose exec product-api dotnet ef database update --project Product.Persistence
    echo "✅ Migrations complete!"
    ;;
    
  *)
    echo "Usage: ./docker.sh {start-dev|start-prod|stop|restart|logs|logs-auth|logs-product|build|clean|status|db-migrate}"
    exit 1
    ;;
esac