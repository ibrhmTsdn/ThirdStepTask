# Microservices Project - .NET 8

## 📋 Proje Özeti

Bu proje, **Onion Architecture**, **CQRS Pattern**, **12-Factor App** prensipleri ve **SOLID** ilkelerine uygun olarak geliştirilmiş bir mikroservis mimarisidir.

## 🏗️ Mimari Yapı

### Onion Architecture Katmanları

```
┌─────────────────────────────────────────┐
│         API Layer (Presentation)        │
│  - Controllers                          │
│  - Middleware                           │
│  - Program.cs                           │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│      Infrastructure Layer               │
│  - External Services                    │
│  - JWT Token Generator                  │
│  - Password Hasher                      │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│      Persistence Layer                  │
│  - DbContext                            │
│  - Repositories                         │
│  - Configurations                       │
│  - Migrations                           │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│      Application Layer                  │
│  - CQRS Commands & Queries              │
│  - Command/Query Handlers               │
│  - DTOs                                 │
│  - Validators                           │
│  - Application Interfaces               │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│      Domain Layer (Core)                │
│  - Entities                             │
│  - Domain Interfaces                    │
│  - Value Objects                        │
│  - Domain Events                        │
└─────────────────────────────────────────┘
```

## 🎯 Tamamlanan Bileşenler

### ✅ 1. Building Blocks (Ortak Kütüphaneler)

#### Common Library
- ✅ `BaseEntity` - Tüm entity'ler için temel sınıf
- ✅ `BaseDomainEvent` - Domain event'ler için temel sınıf
- ✅ `ApiResponse<T>` - Standart API response wrapper
- ✅ `PaginatedResult<T>` - Sayfalama desteği
- ✅ Exception sınıfları:
  - `NotFoundException`
  - `BadRequestException`
  - `ValidationException`
  - `UnauthorizedException`
  - `ForbiddenException`
  - `ConflictException`
  - `InternalServerException`

#### EventBus Library
- ✅ `IntegrationEvent` - Event base class
- ✅ `IEventBus` - Event bus interface
- ✅ `IIntegrationEventHandler` - Event handler interface

#### EventBus.RabbitMQ Library
- ✅ `RabbitMQEventBus` - RabbitMQ implementation
- ✅ `IRabbitMQPersistentConnection` - Connection interface
- ✅ `DefaultRabbitMQPersistentConnection` - Connection implementation

### ✅ 2. Auth Service (Kimlik Doğrulama Mikroservisi)

#### Domain Layer
**Entities:**
- ✅ `User` - Kullanıcı entity'si
- ✅ `Role` - Rol entity'si
- ✅ `Permission` - İzin entity'si
- ✅ `UserRole` - Kullanıcı-Rol ilişkisi
- ✅ `RolePermission` - Rol-İzin ilişkisi
- ✅ `RefreshToken` - Refresh token entity'si

**Interfaces:**
- ✅ `IUserRepository`
- ✅ `IRoleRepository`
- ✅ `IRefreshTokenRepository`

#### Application Layer (CQRS)
**Commands:**
- ✅ `RegisterCommand` + Handler + Validator
- ✅ `LoginCommand` + Handler + Validator
- ✅ `RefreshTokenCommand` + Handler

**Services:**
- ✅ `IPasswordHasher` - Password hashing interface
- ✅ `IJwtTokenGenerator` - JWT token generation interface

#### Infrastructure Layer
- ✅ `PasswordHasher` - BCrypt implementation
- ✅ `JwtTokenGenerator` - JWT token generation

#### Persistence Layer
**DbContext:**
- ✅ `AuthDbContext` - EF Core context

**Configurations:**
- ✅ `UserConfiguration`
- ✅ `RoleConfiguration`
- ✅ `PermissionConfiguration`
- ✅ `RefreshTokenConfiguration`

**Repositories:**
- ✅ `UserRepository`
- ✅ `RoleRepository`
- ✅ `RefreshTokenRepository`

**Seeders:**
- ✅ `AuthDbSeeder` - Initial data (Admin, User, Manager roles ve permissions)

#### API Layer
**Controllers:**
- ✅ `AuthController`
  - POST `/api/auth/register` - Kullanıcı kaydı
  - POST `/api/auth/login` - Giriş yapma
  - POST `/api/auth/refresh-token` - Token yenileme

**Middleware:**
- ✅ `ExceptionHandlerMiddleware` - Global exception handling

**Behaviors:**
- ✅ `ValidationBehavior` - FluentValidation pipeline behavior

**Configuration:**
- ✅ `Program.cs` - DI, middleware pipeline, JWT auth
- ✅ `appsettings.json` - Configuration
- ✅ `appsettings.Development.json` - Development config

### ✅ 3. Product Service (Ürün Yönetimi Mikroservisi)

#### Domain Layer
**Entities:**
- ✅ `Product` - Ürün entity'si (calculated properties ile)
- ✅ `ProductCategory` - Ürün kategorisi entity'si

**Events:**
- ✅ `ProductCreatedDomainEvent`
- ✅ `ProductUpdatedDomainEvent`
- ✅ `ProductDeletedDomainEvent`
- ✅ `ProductStockChangedDomainEvent`

**Interfaces:**
- ✅ `IProductRepository` - Comprehensive repository interface

#### Application Layer (CQRS)
**Commands:**
- ✅ `CreateProductCommand` + Handler + Validator
  - Event publishing (ProductCreatedIntegrationEvent)
- ✅ `UpdateProductCommand` + Handler + Validator
  - Event publishing (ProductUpdatedIntegrationEvent)
  - Cache invalidation

**Queries:**
- ✅ `GetAllProductsQuery` + Handler
  - Redis caching support
- ✅ `GetProductByIdQuery` + Handler
  - Individual product caching

**Integration Events:**
- ✅ `ProductCreatedIntegrationEvent`
- ✅ `ProductUpdatedIntegrationEvent`
- ✅ `ProductDeletedIntegrationEvent`
- ✅ `ProductPriceChangedIntegrationEvent`

**Services:**
- ✅ `ICacheService` - Cache service interface

#### Infrastructure Layer
**Services:**
- ✅ `RedisCacheService` - Complete Redis implementation
  - Get/Set/Remove operations
  - Batch operations
  - Prefix-based removal

#### Persistence Layer
**DbContext:**
- ✅ `ProductDbContext` - EF Core context

**Configurations:**
- ✅ `ProductConfiguration` - Indexes, constraints, query filters
- ✅ `ProductCategoryConfiguration`

**Repositories:**
- ✅ `ProductRepository` - Full CRUD + pagination + filtering

**Seeders:**
- ✅ `ProductDbSeeder` - 10 sample products, 5 categories

#### API Layer
**Controllers:**
- ✅ `ProductsController`
  - GET `/api/products` - Tüm ürünleri getir (cached)
  - GET `/api/products/{id}` - ID'ye göre ürün getir (cached)
  - POST `/api/products` - Yeni ürün oluştur (JWT required)
  - PUT `/api/products/{id}` - Ürün güncelle (JWT required)
  - DELETE `/api/products/{id}` - Ürün sil (Admin/Manager role required)

**Middleware:**
- ✅ `RateLimitingMiddleware` - Sliding window rate limiting
- ✅ `ExceptionHandlerMiddleware` - Global error handling

**Behaviors:**
- ✅ `ValidationBehavior` - FluentValidation integration

**Configuration:**
- ✅ `Program.cs` - Full DI setup with:
  - JWT Authentication
  - Redis Cache
  - RabbitMQ Event Bus
  - Rate Limiting
  - Health Checks (SQL Server + Redis)
- ✅ `appsettings.json`
- ✅ `appsettings.Development.json`

## 🎨 Uygulanan Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)
- ✅ Komutlar (Commands) ve Sorgular (Queries) ayrıştırıldı
- ✅ MediatR kullanılarak handler'lar implement edildi

### 2. Repository Pattern
- ✅ Data access logic abstract edildi
- ✅ Domain layer'da interface, Persistence layer'da implementation

### 3. Dependency Injection
- ✅ Tüm bağımlılıklar constructor injection ile sağlanıyor
- ✅ Dependency Inversion Principle uygulandı

### 4. Mediator Pattern
- ✅ MediatR library kullanılarak command/query dispatch

### 5. Pipeline Behavior
- ✅ Validation behavior ile otomatik validasyon

## 📐 SOLID Prensipleri

### ✅ Single Responsibility Principle (SRP)
- Her class tek bir sorumluluğa sahip
- Örnek: `RegisterCommandHandler` sadece kayıt işlemini yapar

### ✅ Open/Closed Principle (OCP)
- Yeni özellikler için açık, değişim için kapalı
- Örnek: Yeni command eklemek için mevcut kodu değiştirmeye gerek yok

### ✅ Liskov Substitution Principle (LSP)
- Base class'lar yerine derived class'lar kullanılabilir
- Örnek: `BaseEntity` ve türevleri

### ✅ Interface Segregation Principle (ISP)
- İnterface'ler küçük ve spesifik
- Örnek: `IPasswordHasher`, `IJwtTokenGenerator`

### ✅ Dependency Inversion Principle (DIP)
- Yüksek seviye modüller, düşük seviye modüllere bağımlı değil
- Herşey interface'lere bağımlı

## 🔧 12-Factor App Prensipleri

### ✅ 1. Codebase
- Tek bir kod tabanı, version control altında

### ✅ 2. Dependencies
- NuGet packages ile explicit dependency management

### ✅ 3. Config
- Environment variables ve appsettings.json
- Hassas bilgiler environment'ta

### ✅ 4. Backing Services
- Database, RabbitMQ external resource olarak

### ✅ 5. Build, Release, Run
- Aşamalar net bir şekilde ayrılmış

### ✅ 6. Processes
- Stateless process design
- JWT token kullanımı (stateless auth)

### ✅ 7. Port Binding
- Self-contained, port üzerinden expose

### ✅ 8. Concurrency
- Async/await kullanımı
- Horizontal scalability için hazır

### ✅ 9. Disposability
- Graceful shutdown
- Fast startup

### ✅ 10. Dev/Prod Parity
- appsettings.Development.json
- Environment-specific configuration

### ✅ 11. Logs
- Serilog ile structured logging
- Console ve File sink'ler

### ✅ 12. Admin Processes
- Database migration ve seeding
- Ayrı admin komutları

## 🔐 Güvenlik Özellikleri

### JWT Authentication
- ✅ Access Token (30 dakika)
- ✅ Refresh Token (7 gün)
- ✅ Token rotation mechanism
- ✅ Secure token storage

### Password Security
- ✅ BCrypt hashing
- ✅ Password validation rules:
  - Minimum 8 karakter
  - En az 1 büyük harf
  - En az 1 küçük harf
  - En az 1 rakam
  - En az 1 özel karakter

### Role-Based Access Control
- ✅ Role tabanlı yetkilendirme
- ✅ Permission tabanlı yetkilendirme
- ✅ User-Role-Permission ilişkileri

## 📊 Database Schema

### Users Table
- Id (PK)
- UserName (Unique)
- Email (Unique)
- PasswordHash
- FirstName
- LastName
- EmailConfirmed
- IsActive
- LastLoginDate
- (BaseEntity fields)

### Roles Table
- Id (PK)
- Name
- NormalizedName (Unique)
- Description
- (BaseEntity fields)

### Permissions Table
- Id (PK)
- Name
- NormalizedName (Unique)
- Description
- Category
- (BaseEntity fields)

### RefreshTokens Table
- Id (PK)
- Token (Unique)
- UserId (FK)
- ExpiresAt
- IsRevoked
- RevokedAt
- RevokedByIp
- ReplacedByToken
- CreatedByIp
- (BaseEntity fields)

## 🚀 Sonraki Adımlar

### Sırada Olanlar:
1. ⏳ **Log Service** - Merkezi loglama servisi
2. ⏳ **API Gateway** - Yarp Gateway ile routing
3. ⏳ **CI/CD Pipeline** - GitHub Actions / Azure DevOps

### Tamamlanan:
1. ✅ **Building Blocks** - Common, EventBus, EventBus.RabbitMQ
2. ✅ **Auth Service** - JWT, Refresh Token, Role-Based Auth
3. ✅ **Product Service** - CQRS, Redis Cache, Event-Driven, Rate Limiting
4. ✅ **Docker & Docker Compose** - Full containerization with development & production configs
5. ✅ **API Gateway** - Yarp Reverse Proxy with JWT Auth, Rate Limiting, Request/Response Logging

## 📝 Notlar

- Tüm kod SOLID prensiplerine uygun
- 12-Factor App metodolojisi uygulandı
- Clean Architecture / Onion Architecture
- Test edilmeye hazır yapı
- Production-ready code quality

---

## 🛠️ Geliştirme

### Docker ile Çalıştırma (Önerilen) 🐳

#### Hızlı Başlangıç:
```bash
# Linux/Mac
chmod +x docker.sh
./docker.sh start-dev

# Windows PowerShell
.\docker.ps1 start-dev
```

#### Manuel Docker Compose:
```bash
# Tüm servisleri başlat
docker-compose up -d

# Logları izle
docker-compose logs -f

# Servisleri durdur
docker-compose down
```

#### Erişim URL'leri:
- 🌐 **API Gateway: http://localhost:5000** (Ana giriş noktası)
- 🔐 Auth API: http://localhost:5001/swagger
- 📦 Product API: http://localhost:5002/swagger
- 🐰 RabbitMQ: http://localhost:15672 (guest/guest)
- 🗄️ SQL Server: localhost:1433 (sa/**MyProject2024!@#**)
- 🔴 Redis: localhost:6379

> **💡 Önerilen:** Tüm API isteklerini API Gateway (port 5000) üzerinden yapın.

### Manuel Kurulum

```bash
# Database migration oluşturma
cd src/Services/Auth/Auth.API
dotnet ef migrations add InitialCreate --project ../Auth.Persistence

cd src/Services/Product/Product.API  
dotnet ef migrations add InitialCreate --project ../Product.Persistence

# Database güncelleme
dotnet ef database update --project ../Auth.Persistence
dotnet ef database update --project ../Product.Persistence

# Uygulamayı çalıştırma
dotnet run
```

## 📞 API Endpoints

### 🌐 API Gateway (Port: 5000) - Recommended Entry Point

#### Gateway Info
- `GET /` - Gateway bilgisi
- `GET /health` - Gateway health check
- `GET /api/gateway/info` - Detaylı bilgi
- `GET /api/gateway/ping` - Test endpoint

#### Authentication (via Gateway)
- `POST /auth/register` - Yeni kullanıcı kaydı
- `POST /auth/login` - Kullanıcı girişi
- `POST /auth/refresh-token` - Token yenileme

#### Products (via Gateway)
- `GET /products` - Tüm ürünleri getir (Redis cache)
- `GET /products/{id}` - ID'ye göre ürün getir (Redis cache)
- `POST /products` - Yeni ürün oluştur (🔐 JWT required)
- `PUT /products/{id}` - Ürün güncelle (🔐 JWT required)
- `DELETE /products/{id}` - Ürün sil (🔐 Admin/Manager only)

### Direct Service Access (For Development/Testing)

#### Auth Service (Port: 5001)
- `POST /api/auth/register` - Yeni kullanıcı kaydı
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/refresh-token` - Token yenileme
- `GET /health` - Health check

#### Product Service (Port: 5002)
- `GET /api/products` - Tüm ürünleri getir (Redis cache)
- `GET /api/products/{id}` - ID'ye göre ürün getir (Redis cache)
- `POST /api/products` - Yeni ürün oluştur (🔐 JWT required)
- `PUT /api/products/{id}` - Ürün güncelle (🔐 JWT required)
- `DELETE /api/products/{id}` - Ürün sil (🔐 Admin/Manager only)
- `GET /health` - Health check

> **📚 Detaylı kullanım için:** `API_GATEWAY_GUIDE.md` dosyasına bakın.

---
