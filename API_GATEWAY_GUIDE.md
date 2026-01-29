# API Gateway - Kullanım Kılavuzu

## 🌐 API Gateway Nedir?

API Gateway, tüm mikroservislere tek bir giriş noktası sağlayan merkezi bir reverse proxy'dir. **Yarp (Yet Another Reverse Proxy)** kullanılarak geliştirilmiştir.

## 🎯 Özellikler

### ✅ 1. Reverse Proxy (Yarp)
- Tüm istekleri doğru mikroservislere yönlendirir
- Dinamik route yapılandırması
- Health check desteği
- Load balancing hazır

### ✅ 2. JWT Authentication
- Merkezi kimlik doğrulama
- Token validation
- Claims-based authorization

### ✅ 3. Rate Limiting
- **Per-IP Limiting:** 100 istek/dakika
- **Global Limiting:** 1000 istek/dakika
- Otomatik blocking (abuse durumunda)
- Rate limit headers

### ✅ 4. Request/Response Logging
- Tüm istekler loglanır
- Performance monitoring
- Request ID tracking
- Slow request detection

### ✅ 5. CORS Support
- Cross-origin resource sharing
- Configurable policies

### ✅ 6. Health Checks
- Gateway health
- Downstream services health
- Auto-retry on failure

## 📍 Endpoints

### Gateway Endpoints

```
GET  /                    - Gateway ana bilgi
GET  /health             - Health check
GET  /swagger            - API Documentation
GET  /api/gateway/info   - Detaylı bilgi
GET  /api/gateway/ping   - Test endpoint
GET  /api/gateway/stats  - İstatistikler
```

### Proxied Routes

#### Authentication Service
```
Base Path: /auth
Target: http://auth-api:80/api

POST /auth/register      → http://auth-api:80/api/register
POST /auth/login         → http://auth-api:80/api/login
POST /auth/refresh-token → http://auth-api:80/api/refresh-token
GET  /auth/health        → http://auth-api:80/health
```

#### Product Service
```
Base Path: /products
Target: http://product-api:80/api/products

GET    /products           → http://product-api:80/api/products
GET    /products/{id}      → http://product-api:80/api/products/{id}
POST   /products           → http://product-api:80/api/products (🔐 Auth required)
PUT    /products/{id}      → http://product-api:80/api/products/{id} (🔐 Auth required)
DELETE /products/{id}      → http://product-api:80/api/products/{id} (🔐 Admin only)
GET    /products/health    → http://product-api:80/health
```

## 🚀 Kullanım Örnekleri

### 1. Kullanıcı Kaydı

```bash
# Doğrudan Auth API'ye (Port 5001)
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "Test@1234",
    "firstName": "Test",
    "lastName": "User"
  }'

# API Gateway üzerinden (Port 5000)
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "Test@1234",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Login

```bash
# API Gateway üzerinden
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUserName": "testuser",
    "password": "Test@1234"
  }'

# Response:
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "abc123...",
    "user": {
      "id": "...",
      "userName": "testuser",
      "email": "test@example.com",
      "roles": ["User"]
    }
  }
}
```

### 3. Ürünleri Listeleme (Cache'den)

```bash
# API Gateway üzerinden
curl http://localhost:5000/products

# Response headers include:
# X-RateLimit-Limit: 100
# X-RateLimit-Remaining: 99
# X-RateLimit-Reset: 1234567890
```

### 4. Ürün Ekleme (JWT Required)

```bash
# Önce token al
TOKEN=$(curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrUserName": "testuser", "password": "Test@1234"}' \
  | jq -r '.data.accessToken')

# Ürün ekle
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "New Product",
    "description": "Product description",
    "sku": "NEWPROD-001",
    "price": 99.99,
    "stockQuantity": 100,
    "category": "Electronics"
  }'
```

### 5. Gateway Bilgisi

```bash
curl http://localhost:5000/api/gateway/info

# Response:
{
  "name": "API Gateway",
  "version": "1.0.0",
  "status": "running",
  "routes": {
    "authentication": { ... },
    "products": { ... }
  },
  "features": [
    "JWT Authentication",
    "Rate Limiting",
    "Request/Response Logging",
    ...
  ]
}
```

## 🛡️ Rate Limiting

### Nasıl Çalışır?

#### Per-IP Limiting
- **Limit:** 100 istek / dakika
- **Window:** Sliding window
- **Block:** 2x limit aşılırsa 5 dakika block

#### Global Limiting
- **Limit:** 1000 total istek / dakika
- **Protection:** DDoS koruması

### Rate Limit Headers

Her response'da:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1234567890
```

### Rate Limit Exceeded Response

```json
HTTP/1.1 429 Too Many Requests
Retry-After: 60

{
  "success": false,
  "message": "Rate limit exceeded",
  "errorCode": "RATE_LIMIT_EXCEEDED",
  "retryAfter": 60,
  "timestamp": "2024-01-29T10:00:00Z"
}
```

## 🔐 Authentication Flow

### 1. Kayıt + Login
```mermaid
Client -> Gateway: POST /auth/register
Gateway -> Auth API: Forward request
Auth API -> Gateway: User created
Gateway -> Client: Success response

Client -> Gateway: POST /auth/login
Gateway -> Auth API: Forward request
Auth API -> Gateway: JWT tokens
Gateway -> Client: Tokens
```

### 2. Protected Endpoint Access
```mermaid
Client -> Gateway: GET /products (with JWT)
Gateway: Validate JWT
Gateway -> Product API: Forward (if valid)
Product API -> Gateway: Response
Gateway -> Client: Response
```

## 📊 Monitoring & Logging

### Log Locations
```
/logs/gateway-{date}.txt
```

### Log Levels
```
Information: Normal requests
Warning: Rate limits, slow requests
Error: Failed requests, exceptions
```

### Example Log Entry
```
[10:00:00 INF] Request abc123 started: GET /products from 192.168.1.100
[10:00:00 INF] Request abc123 completed: 200 in 45ms
```

### Slow Request Detection
```
[10:00:05 WRN] Slow request detected abc456: GET /products/{id} took 5234ms
```

## 🏥 Health Checks

### Gateway Health
```bash
curl http://localhost:5000/health

# Response:
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456"
}
```

### Downstream Services Health

Gateway otomatik olarak her 30 saniyede kontrol eder:
- Auth API: http://auth-api:80/health
- Product API: http://product-api:80/health

Eğer bir servis unhealthy ise, istekler diğer instance'lara yönlendirilir.

## 🔧 Configuration

### appsettings.json

```json
{
  "ReverseProxy": {
    "Routes": {
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": {
          "Path": "/products/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/api/products/{**catch-all}"
          }
        ],
        "RateLimiterPolicy": "fixed-window"
      }
    },
    "Clusters": {
      "product-cluster": {
        "Destinations": {
          "product-destination": {
            "Address": "http://localhost:5002"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:30"
          }
        }
      }
    }
  },
  "RateLimiting": {
    "GlobalLimit": 1000,
    "PerIpLimit": 100,
    "WindowSeconds": 60
  }
}
```

### Environment Variables

```bash
# Docker
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80

# JWT
Jwt__SecretKey=...
Jwt__Issuer=MicroservicesAuth
Jwt__Audience=MicroservicesAPI

# Rate Limiting
RateLimiting__GlobalLimit=1000
RateLimiting__PerIpLimit=100
RateLimiting__WindowSeconds=60
```

## 🚦 Traffic Flow

```
Client Request
    ↓
API Gateway (Port 5000)
    ↓
[Rate Limiting Check]
    ↓
[JWT Validation]
    ↓
[Route Matching]
    ↓
[Request Logging]
    ↓
Microservice (Auth:5001 or Product:5002)
    ↓
[Response]
    ↓
[Response Logging]
    ↓
Client Response
```

## 🎯 Benefits

### For Developers
- ✅ Single endpoint to remember
- ✅ Centralized authentication
- ✅ Rate limiting out of the box
- ✅ Consistent error handling
- ✅ Easy testing

### For Operations
- ✅ Centralized logging
- ✅ Traffic monitoring
- ✅ Easy to add new services
- ✅ Load balancing ready
- ✅ Health check aggregation

### For Security
- ✅ Single point of authentication
- ✅ Rate limiting protection
- ✅ CORS management
- ✅ Request validation
- ✅ DDoS protection

## 📝 Best Practices

### 1. Always Use Gateway in Production
```bash
# ❌ Don't expose services directly
http://product-api:5002/api/products

# ✅ Use gateway
http://gateway:5000/products
```

### 2. Include JWT in Headers
```bash
curl http://localhost:5000/products \
  -H "Authorization: Bearer eyJhbG..."
```

### 3. Handle Rate Limits
```javascript
async function fetchWithRetry(url, options) {
  const response = await fetch(url, options);
  
  if (response.status === 429) {
    const retryAfter = response.headers.get('Retry-After');
    await sleep(retryAfter * 1000);
    return fetchWithRetry(url, options);
  }
  
  return response;
}
```

### 4. Monitor Rate Limit Headers
```javascript
const remaining = response.headers.get('X-RateLimit-Remaining');
if (remaining < 10) {
  console.warn('Rate limit almost exceeded!');
}
```

## 🐛 Troubleshooting

### Gateway Returns 503
**Problem:** Service unavailable
**Solution:**
```bash
# Check downstream services
curl http://localhost:5001/health
curl http://localhost:5002/health

# Check gateway logs
docker logs gateway-api
```

### 401 Unauthorized
**Problem:** JWT token invalid/expired
**Solution:**
```bash
# Get new token
curl -X POST http://localhost:5000/auth/login ...

# Use refresh token
curl -X POST http://localhost:5000/auth/refresh-token ...
```

### 429 Too Many Requests
**Problem:** Rate limit exceeded
**Solution:**
```bash
# Wait for retry-after seconds
# Or reduce request frequency
# Or contact admin to increase limits
```

## 📈 Performance Tips

### 1. Connection Pooling
Gateway automatically pools connections to downstream services.

### 2. Caching
Gateway forwards cache headers from downstream services.

### 3. Compression
Enable gzip compression in gateway configuration.

### 4. Load Balancing
Add multiple destinations for high availability:

```json
{
  "product-cluster": {
    "Destinations": {
      "product-1": { "Address": "http://product-api-1:80" },
      "product-2": { "Address": "http://product-api-2:80" }
    }
  }
}
```

---

## 🎓 Summary

API Gateway provides:
- ✅ **Single Entry Point** for all microservices
- ✅ **Centralized Authentication** with JWT
- ✅ **Rate Limiting** for protection
- ✅ **Request/Response Logging** for monitoring
- ✅ **Health Checks** for reliability
- ✅ **CORS Support** for web applications
- ✅ **Load Balancing** ready for scaling

**Main URL:** http://localhost:5000
**Documentation:** http://localhost:5000/swagger

---

**Created:** $(date)
**Version:** 1.0.0
