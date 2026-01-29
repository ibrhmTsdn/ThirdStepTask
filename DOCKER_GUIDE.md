# Docker & Docker Compose Kullanım Kılavuzu

## 📋 İçindekiler
1. [Gereksinimler](#gereksinimler)
2. [Hızlı Başlangıç](#hızlı-başlangıç)
3. [Servis Detayları](#servis-detayları)
4. [Komutlar](#komutlar)
5. [Troubleshooting](#troubleshooting)
6. [Production Deployment](#production-deployment)

## 🔧 Gereksinimler

### Yazılım Gereksinimleri
- Docker Desktop 4.0+ veya Docker Engine 20.10+
- Docker Compose 2.0+
- En az 8GB RAM (16GB önerilir)
- En az 20GB boş disk alanı

### Kurulum Kontrolü
```bash
# Docker versiyonunu kontrol et
docker --version

# Docker Compose versiyonunu kontrol et
docker-compose --version

# Docker'ın çalıştığını kontrol et
docker ps
```

## 🚀 Hızlı Başlangıç

### Development Ortamı

#### Linux/Mac:
```bash
# Çalıştırma yetkisi ver
chmod +x docker.sh

# Tüm servisleri başlat
./docker.sh start-dev

# Logları izle
./docker.sh logs
```

#### Windows PowerShell:
```powershell
# Tüm servisleri başlat
.\docker.ps1 start-dev

# Logları izle
.\docker.ps1 logs
```

#### Manuel Başlatma:
```bash
# Tüm servisleri başlat
docker-compose up -d

# Detached mode olmadan (logları görmek için)
docker-compose up
```

### İlk Çalıştırma Sonrası

Servisler başladıktan sonra şu URL'lere erişebilirsiniz:

| Servis | URL | Kullanıcı Adı | Şifre |
|--------|-----|---------------|--------|
| Auth API | http://localhost:5001 | - | - |
| Auth API Swagger | http://localhost:5001/swagger | - | - |
| Product API | http://localhost:5002 | - | - |
| Product API Swagger | http://localhost:5002/swagger | - | - |
| RabbitMQ Management | http://localhost:15672 | guest | guest |
| Portainer | http://localhost:9000 | admin | (ilk kurulumda belirle) |
| SQL Server | localhost:1433 | sa | Ibrahim38- |
| Redis | localhost:6379 | - | - |

## 📦 Servis Detayları

### Infrastructure Services

#### 1. SQL Server
- **Image:** `mcr.microsoft.com/mssql/server:2022-latest`
- **Port:** 1433
- **Databases:** AuthDb, ProductDb
- **Volume:** Persistent storage for databases

#### 2. Redis
- **Image:** `redis:7-alpine`
- **Port:** 6379
- **Purpose:** Caching layer for Product API
- **Volume:** Persistent storage for cache data

#### 3. RabbitMQ
- **Image:** `rabbitmq:3-management-alpine`
- **Ports:** 
  - 5672 (AMQP)
  - 15672 (Management UI)
- **Purpose:** Message broker for event-driven architecture
- **Volume:** Persistent storage for message queue

### Application Services

#### 1. Auth API
- **Port:** 5001
- **Features:**
  - User registration
  - Login with JWT
  - Refresh token mechanism
  - Role-based authorization
- **Dependencies:** SQL Server

#### 2. Product API
- **Port:** 5002
- **Features:**
  - Product CRUD operations
  - Redis caching
  - Event publishing to RabbitMQ
  - Rate limiting
  - JWT authentication
- **Dependencies:** SQL Server, Redis, RabbitMQ

## 🎮 Komutlar

### Helper Script Komutları

| Komut | Açıklama |
|-------|----------|
| `start-dev` | Development modunda tüm servisleri başlat |
| `start-prod` | Production modunda tüm servisleri başlat |
| `stop` | Tüm servisleri durdur |
| `restart` | Tüm servisleri yeniden başlat |
| `logs` | Tüm servislerin loglarını göster |
| `logs-auth` | Sadece Auth API loglarını göster |
| `logs-product` | Sadece Product API loglarını göster |
| `build` | Tüm Docker image'larını yeniden build et |
| `clean` | Tüm container, volume ve image'ları sil |
| `status` | Container'ların durumunu göster |
| `db-migrate` | Database migration'ları çalıştır |

### Docker Compose Komutları

```bash
# Servisleri başlat
docker-compose up -d

# Servisleri durdur
docker-compose down

# Belirli bir servisi restart et
docker-compose restart auth-api

# Belirli bir servisin loglarını göster
docker-compose logs -f product-api

# Container içine gir
docker-compose exec auth-api bash

# Tüm servisleri rebuild et
docker-compose build

# Sadece belirli servisleri başlat
docker-compose up -d sqlserver redis rabbitmq

# Resource kullanımını göster
docker stats
```

### Database Migration Komutları

```bash
# Auth Database Migration
docker-compose exec auth-api dotnet ef migrations add MigrationName --project Auth.Persistence
docker-compose exec auth-api dotnet ef database update --project Auth.Persistence

# Product Database Migration
docker-compose exec product-api dotnet ef migrations add MigrationName --project Product.Persistence
docker-compose exec product-api dotnet ef database update --project Product.Persistence
```

## 🔍 Troubleshooting

### Problem: Container'lar başlamıyor

**Çözüm 1:** Port çakışması kontrolü
```bash
# Port kullanımını kontrol et
# Windows
netstat -ano | findstr :1433
netstat -ano | findstr :5001
netstat -ano | findstr :5002

# Linux/Mac
lsof -i :1433
lsof -i :5001
lsof -i :5002
```

**Çözüm 2:** Docker'ı restart et
```bash
# Docker Desktop'u restart et
# veya
sudo systemctl restart docker  # Linux
```

**Çözüm 3:** Volume'ları temizle
```bash
docker-compose down -v
docker-compose up -d
```

### Problem: SQL Server bağlantı hatası

**Çözüm:**
```bash
# SQL Server container'ının hazır olmasını bekle
docker-compose logs sqlserver

# Health check durumunu kontrol et
docker inspect sqlserver | grep -A 10 Health
```

### Problem: Redis bağlantı hatası

**Çözüm:**
```bash
# Redis'in çalıştığını kontrol et
docker-compose exec redis redis-cli ping
# Beklenen yanıt: PONG

# Redis loglarını kontrol et
docker-compose logs redis
```

### Problem: RabbitMQ bağlantı hatası

**Çözüm:**
```bash
# RabbitMQ'nun hazır olmasını bekle (30 saniye kadar sürebilir)
docker-compose logs rabbitmq

# Management UI'dan kontrol et
# http://localhost:15672
```

### Problem: Migration hatası

**Çözüm:**
```bash
# Container'ı yeniden başlat
docker-compose restart auth-api

# Migration'ı manuel çalıştır
docker-compose exec auth-api dotnet ef database update --project Auth.Persistence
```

### Problem: Image build hatası

**Çözüm:**
```bash
# Cache'i temizle ve rebuild et
docker-compose build --no-cache

# Eski image'ları temizle
docker system prune -a
```

## 🏭 Production Deployment

### 1. Environment Variables Hazırlama

```bash
# .env.example dosyasını kopyala
cp .env.example .env

# .env dosyasını düzenle
nano .env
```

### 2. SSL Sertifikası Hazırlama

```bash
# Self-signed certificate oluştur (Development için)
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p YourPassword

# Production için Let's Encrypt kullan
# https://letsencrypt.org/
```

### 3. Production'da Çalıştırma

```bash
# Linux/Mac
./docker.sh start-prod

# Windows
.\docker.ps1 start-prod

# Manuel
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### 4. Production Monitoring

```bash
# Container durumlarını kontrol et
docker-compose ps

# Resource kullanımını izle
docker stats

# Logları kontrol et
docker-compose logs -f --tail=100
```

### 5. Backup & Restore

#### SQL Server Backup
```bash
# Backup
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost \
  -U sa -P 'Ibrahim38-' \
  -Q "BACKUP DATABASE AuthDb TO DISK = '/var/opt/mssql/data/AuthDb.bak'"

# Backup dosyasını kopyala
docker cp sqlserver:/var/opt/mssql/data/AuthDb.bak ./backups/
```

#### Redis Backup
```bash
# RDB snapshot oluştur
docker-compose exec redis redis-cli SAVE

# Backup dosyasını kopyala
docker cp redis:/data/dump.rdb ./backups/
```

## 📊 Health Checks

### Servislerin Sağlık Durumu

```bash
# Tüm health check'leri göster
curl http://localhost:5001/health
curl http://localhost:5002/health

# Detaylı JSON output
curl http://localhost:5001/health | jq
curl http://localhost:5002/health | jq
```

### Container Health Status

```bash
# Tüm container'ların health durumunu göster
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## 🔒 Security Best Practices

1. **Production'da mutlaka .env dosyası kullan**
2. **Default şifreleri değiştir**
3. **SSL/TLS kullan**
4. **Sadece gerekli portları expose et**
5. **Regular olarak güvenlik güncellemelerini yap**
6. **Log dosyalarını düzenli temizle**
7. **Backup stratejisi oluştur**

## 📈 Scaling

### Horizontal Scaling

```yaml
# docker-compose.prod.yml
deploy:
  replicas: 3  # 3 instance çalıştır
```

```bash
# Scale up/down
docker-compose up -d --scale product-api=3
```

### Load Balancer Ekleme

```yaml
# nginx load balancer ekle
nginx:
  image: nginx:alpine
  ports:
    - "80:80"
  volumes:
    - ./nginx.conf:/etc/nginx/nginx.conf
  depends_on:
    - auth-api
    - product-api
```

## 🎯 Performance Tuning

### Docker Engine Ayarları

```json
// daemon.json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
```

### Resource Limits

```yaml
deploy:
  resources:
    limits:
      cpus: '1'
      memory: 1G
    reservations:
      cpus: '0.5'
      memory: 512M
```

---

## 📝 Notlar

- Her servis kendi container'ında çalışır (isolation)
- Volume'lar ile data persistence sağlanır
- Health check'ler ile servis durumu izlenir
- Multi-stage build ile image size optimize edilir
- Development ve Production için ayrı konfigürasyonlar

## 🆘 Destek

Problem yaşarsanız:
1. Logları kontrol edin: `docker-compose logs`
2. Health check'leri kontrol edin
3. Resource kullanımını kontrol edin: `docker stats`
4. Troubleshooting bölümüne bakın

---

**Güncellenme:** $(date)
