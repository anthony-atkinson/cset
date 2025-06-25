# CSET Enterprise Deployment Guide

## Overview
This guide provides comprehensive instructions for deploying CSET in enterprise environments with all enhanced features including real-time collaboration, machine learning, enhanced security, performance monitoring, and advanced analytics.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [System Requirements](#system-requirements)
3. [Installation](#installation)
4. [Configuration](#configuration)
5. [Security Setup](#security-setup)
6. [Performance Monitoring](#performance-monitoring)
7. [Machine Learning Setup](#machine-learning-setup)
8. [Real-time Collaboration](#real-time-collaboration)
9. [Caching Configuration](#caching-configuration)
10. [Backup and Recovery](#backup-and-recovery)
11. [Monitoring and Maintenance](#monitoring-and-maintenance)
12. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Software Requirements
- **Windows Server 2019/2022** or **Linux (Ubuntu 20.04+)**
- **SQL Server 2019/2022** or **PostgreSQL 13+**
- **IIS 10** (Windows) or **Nginx** (Linux)
- **Redis 6.0+** for caching and real-time features
- **Docker 20.10+** (optional, for containerized deployment)
- **.NET 7.0+ Runtime**

### Network Requirements
- **HTTPS Certificate** (SSL/TLS)
- **Firewall Configuration** for required ports
- **Load Balancer** (for high availability)
- **VPN Access** (for secure remote access)

### Hardware Requirements
- **CPU**: 8+ cores (16+ cores recommended)
- **RAM**: 16GB+ (32GB+ recommended)
- **Storage**: 500GB+ SSD (1TB+ recommended)
- **Network**: 1Gbps+ connection

---

## System Requirements

### Minimum Configuration
```
CPU: 8 cores
RAM: 16GB
Storage: 500GB SSD
Network: 1Gbps
Users: Up to 100 concurrent
```

### Recommended Configuration
```
CPU: 16+ cores
RAM: 32GB+
Storage: 1TB+ SSD
Network: 10Gbps
Users: 100+ concurrent
```

### High Availability Configuration
```
Primary Server: 16+ cores, 32GB+ RAM, 1TB+ SSD
Secondary Server: 16+ cores, 32GB+ RAM, 1TB+ SSD
Load Balancer: 4+ cores, 8GB+ RAM
Database Server: 16+ cores, 64GB+ RAM, 2TB+ SSD
Redis Cluster: 3+ nodes, 8GB+ RAM each
```

---

## Installation

### Step 1: Database Setup

#### SQL Server Installation
```sql
-- Create CSET database
CREATE DATABASE CSET_Enterprise
GO

-- Create application user
CREATE LOGIN cset_app WITH PASSWORD = 'SecurePassword123!'
GO

USE CSET_Enterprise
GO

CREATE USER cset_app FOR LOGIN cset_app
GO

-- Grant permissions
GRANT CONNECT TO cset_app
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO cset_app
GRANT CREATE TABLE TO cset_app
GRANT CREATE VIEW TO cset_app
GRANT CREATE PROCEDURE TO cset_app
GO
```

#### PostgreSQL Installation
```sql
-- Create database and user
CREATE DATABASE cset_enterprise;
CREATE USER cset_app WITH PASSWORD 'SecurePassword123!';
GRANT ALL PRIVILEGES ON DATABASE cset_enterprise TO cset_app;
```

### Step 2: Redis Installation

#### Windows Installation
```powershell
# Download and install Redis for Windows
# Or use Docker
docker run -d --name redis-cset -p 6379:6379 redis:6.2-alpine
```

#### Linux Installation
```bash
# Install Redis
sudo apt update
sudo apt install redis-server

# Configure Redis
sudo nano /etc/redis/redis.conf

# Set memory limit and persistence
maxmemory 2gb
maxmemory-policy allkeys-lru
save 900 1
save 300 10
save 60 10000

# Start Redis
sudo systemctl enable redis-server
sudo systemctl start redis-server
```

### Step 3: Application Installation

#### Windows Installation
```powershell
# Create application directory
New-Item -ItemType Directory -Path "C:\CSET\Enterprise" -Force

# Download CSET Enterprise package
# Extract to C:\CSET\Enterprise

# Configure IIS
Import-Module WebAdministration
New-WebSite -Name "CSET-Enterprise" -Port 443 -PhysicalPath "C:\CSET\Enterprise\wwwroot" -ApplicationPool "CSET-Pool"

# Create application pool
New-WebAppPool -Name "CSET-Pool"
Set-ItemProperty -Path "IIS:\AppPools\CSET-Pool" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
```

#### Linux Installation
```bash
# Create application directory
sudo mkdir -p /opt/cset/enterprise
sudo chown www-data:www-data /opt/cset/enterprise

# Download and extract CSET Enterprise
# Configure Nginx
sudo nano /etc/nginx/sites-available/cset-enterprise

server {
    listen 443 ssl http2;
    server_name cset.yourdomain.com;
    
    ssl_certificate /etc/ssl/certs/cset.crt;
    ssl_certificate_key /etc/ssl/private/cset.key;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
    
    location /hubs/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }
}

# Enable site
sudo ln -s /etc/nginx/sites-available/cset-enterprise /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Configuration

### Step 1: Application Settings

#### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "CSETWeb": "Server=your-db-server;Database=CSET_Enterprise;User Id=cset_app;Password=SecurePassword123!;TrustServerCertificate=true;",
    "Redis": "your-redis-server:6379,password=your-redis-password"
  },
  "ApplicationInsights": {
    "InstrumentationKey": "your-app-insights-key",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true
  },
  "Security": {
    "JwtSecret": "your-super-secure-jwt-secret-key-here",
    "JwtExpirationHours": 24,
    "MfaEnabled": true,
    "PasswordPolicy": {
      "MinLength": 12,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    }
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ],
    "ClientRules": [
      {
        "ClientId": "admin",
        "Period": "1h",
        "Limit": 5000
      }
    ]
  },
  "Caching": {
    "RedisEnabled": true,
    "DefaultExpirationMinutes": 60,
    "MaxMemoryMB": 2048
  },
  "Collaboration": {
    "SignalREnabled": true,
    "MaxConcurrentConnections": 1000,
    "MessageRetentionHours": 24
  },
  "MachineLearning": {
    "Enabled": true,
    "ModelStoragePath": "/opt/cset/ml-models",
    "TrainingDataRetentionDays": 365
  },
  "Notifications": {
    "Email": {
      "SmtpServer": "smtp.yourdomain.com",
      "SmtpPort": 587,
      "Username": "noreply@yourdomain.com",
      "Password": "your-smtp-password",
      "EnableSsl": true
    },
    "Sms": {
      "Provider": "Twilio",
      "AccountSid": "your-twilio-sid",
      "AuthToken": "your-twilio-token",
      "FromNumber": "+1234567890"
    }
  },
  "PerformanceMonitoring": {
    "Enabled": true,
    "MetricsCollectionInterval": 60,
    "AlertThresholds": {
      "ResponseTimeMs": 2000,
      "MemoryUsagePercent": 80,
      "CpuUsagePercent": 70
    }
  }
}
```

### Step 2: Environment Variables
```bash
# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://localhost:5000
export CSET_DB_CONNECTION="Server=your-db-server;Database=CSET_Enterprise;User Id=cset_app;Password=SecurePassword123!;"
export CSET_REDIS_CONNECTION="your-redis-server:6379,password=your-redis-password"
export CSET_JWT_SECRET="your-super-secure-jwt-secret-key-here"
export CSET_APP_INSIGHTS_KEY="your-app-insights-key"
```

---

## Security Setup

### Step 1: SSL/TLS Configuration

#### Generate SSL Certificate
```bash
# Generate self-signed certificate (for testing)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/ssl/private/cset.key \
  -out /etc/ssl/certs/cset.crt \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=cset.yourdomain.com"

# For production, use a certificate from a trusted CA
```

### Step 2: Firewall Configuration

#### Windows Firewall
```powershell
# Allow HTTPS traffic
New-NetFirewallRule -DisplayName "CSET HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Allow database connections
New-NetFirewallRule -DisplayName "CSET Database" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow

# Allow Redis connections
New-NetFirewallRule -DisplayName "CSET Redis" -Direction Inbound -Protocol TCP -LocalPort 6379 -Action Allow
```

#### Linux Firewall (UFW)
```bash
# Allow HTTPS
sudo ufw allow 443/tcp

# Allow database (if on same server)
sudo ufw allow 5432/tcp

# Allow Redis (if on same server)
sudo ufw allow 6379/tcp

# Enable firewall
sudo ufw enable
```

### Step 3: Multi-Factor Authentication Setup

#### Configure MFA Providers
```json
{
  "Mfa": {
    "TOTP": {
      "Enabled": true,
      "Issuer": "CSET Enterprise",
      "WindowSize": 2
    },
    "SMS": {
      "Enabled": true,
      "Provider": "Twilio",
      "AccountSid": "your-twilio-sid",
      "AuthToken": "your-twilio-token"
    },
    "Email": {
      "Enabled": true,
      "SmtpServer": "smtp.yourdomain.com",
      "FromAddress": "mfa@yourdomain.com"
    }
  }
}
```

---

## Performance Monitoring

### Step 1: Application Insights Setup

#### Azure Application Insights
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-app-insights-key",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true,
    "EnableDependencyTrackingTelemetryModule": true,
    "EnableEventCounterCollectionModule": true,
    "EnableRequestTrackingTelemetryModule": true
  }
}
```

#### Custom Performance Counters
```csharp
// Configure custom metrics
services.AddApplicationInsightsTelemetry();
services.AddSingleton<IPerformanceCounterService, PerformanceCounterService>();
```

### Step 2: Health Monitoring

#### Health Check Endpoints
```http
GET /api/health
GET /api/health/detailed
GET /api/health/database
GET /api/health/redis
GET /api/health/external-services
```

#### Monitoring Dashboard
- **Grafana Dashboard** for real-time monitoring
- **Alert Rules** for performance thresholds
- **Log Aggregation** with ELK Stack

---

## Machine Learning Setup

### Step 1: ML Environment Setup

#### Install ML Dependencies
```bash
# Install Python and ML libraries
sudo apt install python3 python3-pip
pip3 install scikit-learn pandas numpy joblib

# Create ML model directory
sudo mkdir -p /opt/cset/ml-models
sudo chown www-data:www-data /opt/cset/ml-models
```

#### Configure ML Settings
```json
{
  "MachineLearning": {
    "Enabled": true,
    "ModelStoragePath": "/opt/cset/ml-models",
    "TrainingDataRetentionDays": 365,
    "ModelRetentionDays": 730,
    "MaxConcurrentTrainingJobs": 2,
    "TrainingTimeoutMinutes": 60,
    "PredictionTimeoutSeconds": 30
  }
}
```

### Step 2: Initial Model Training

#### Train Base Models
```http
POST /api/ml/models/train
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "modelType": "RiskPrediction",
  "trainingData": {
    "startDate": "2023-01-01",
    "endDate": "2023-12-31",
    "frameworks": ["NIST_CSF", "CMMC_2_0"]
  },
  "parameters": {
    "algorithm": "RandomForest",
    "maxDepth": 10,
    "nEstimators": 100
  }
}
```

---

## Real-time Collaboration

### Step 1: SignalR Configuration

#### Configure SignalR
```json
{
  "Collaboration": {
    "SignalREnabled": true,
    "MaxConcurrentConnections": 1000,
    "MessageRetentionHours": 24,
    "ConnectionTimeoutMinutes": 30,
    "EnableDetailedLogging": true
  }
}
```

#### Load Balancer Configuration
```nginx
# Nginx configuration for SignalR
location /hubs/ {
    proxy_pass http://localhost:5000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_read_timeout 86400;
}
```

### Step 2: Collaboration Features

#### Enable Collaboration Features
```json
{
  "Collaboration": {
    "UserPresence": true,
    "LiveEditing": true,
    "Comments": true,
    "ConflictResolution": "KeepLatest",
    "AuditTrail": true
  }
}
```

---

## Caching Configuration

### Step 1: Redis Configuration

#### Redis Cluster Setup
```bash
# Create Redis cluster configuration
cat > redis-cluster.conf << EOF
port 6379
cluster-enabled yes
cluster-config-file nodes.conf
cluster-node-timeout 5000
appendonly yes
maxmemory 2gb
maxmemory-policy allkeys-lru
EOF

# Start Redis cluster
redis-server redis-cluster.conf
```

#### Application Caching
```json
{
  "Caching": {
    "RedisEnabled": true,
    "DefaultExpirationMinutes": 60,
    "MaxMemoryMB": 2048,
    "CacheKeys": {
      "Standards": {
        "ExpirationMinutes": 1440,
        "Priority": "High"
      },
      "Assessments": {
        "ExpirationMinutes": 30,
        "Priority": "Medium"
      },
      "Questions": {
        "ExpirationMinutes": 60,
        "Priority": "Medium"
      }
    }
  }
}
```

---

## Backup and Recovery

### Step 1: Database Backup

#### SQL Server Backup
```sql
-- Create backup job
USE [msdb]
GO

EXEC dbo.sp_add_job
    @job_name = N'CSET_Database_Backup',
    @enabled = 1
GO

EXEC sp_add_jobstep
    @job_name = N'CSET_Database_Backup',
    @step_name = N'Backup Database',
    @subsystem = N'TSQL',
    @command = N'
BACKUP DATABASE [CSET_Enterprise] 
TO DISK = N''C:\Backups\CSET_Enterprise_$(Get-Date -Format "yyyyMMdd_HHmmss").bak''
WITH COMPRESSION, CHECKSUM
GO'
GO

-- Schedule backup job
EXEC sp_add_schedule
    @schedule_name = N'Daily_Backup',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000
GO
```

#### PostgreSQL Backup
```bash
#!/bin/bash
# Backup script for PostgreSQL
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/opt/backups/cset"
mkdir -p $BACKUP_DIR

pg_dump -h localhost -U cset_app -d cset_enterprise \
  --format=custom --compress=9 \
  --file="$BACKUP_DIR/cset_enterprise_$DATE.backup"

# Keep backups for 30 days
find $BACKUP_DIR -name "*.backup" -mtime +30 -delete
```

### Step 2: Application Backup

#### File System Backup
```bash
#!/bin/bash
# Application backup script
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/opt/backups/cset"
APP_DIR="/opt/cset/enterprise"

# Create backup
tar -czf "$BACKUP_DIR/cset_app_$DATE.tar.gz" \
  --exclude="$APP_DIR/logs" \
  --exclude="$APP_DIR/temp" \
  $APP_DIR

# Keep backups for 30 days
find $BACKUP_DIR -name "cset_app_*.tar.gz" -mtime +30 -delete
```

---

## Monitoring and Maintenance

### Step 1: System Monitoring

#### Performance Monitoring
```bash
# Monitor system resources
htop
iotop
nethogs

# Monitor application logs
tail -f /opt/cset/enterprise/logs/app.log
tail -f /opt/cset/enterprise/logs/error.log
```

#### Database Monitoring
```sql
-- Monitor database performance
SELECT 
    DB_NAME(database_id) AS DatabaseName,
    COUNT(*) AS NumberOfConnections,
    SUM(num_reads) AS TotalReads,
    SUM(num_writes) AS TotalWrites
FROM sys.dm_exec_connections
CROSS APPLY sys.dm_exec_sql_text(most_recent_sql_handle)
GROUP BY database_id;
```

### Step 2: Maintenance Tasks

#### Regular Maintenance
```bash
#!/bin/bash
# Weekly maintenance script

# Clean up old logs
find /opt/cset/enterprise/logs -name "*.log" -mtime +30 -delete

# Clean up temporary files
find /opt/cset/enterprise/temp -name "*" -mtime +7 -delete

# Update system packages
apt update && apt upgrade -y

# Restart services
systemctl restart cset-enterprise
systemctl restart redis-server
```

---

## Troubleshooting

### Common Issues

#### Database Connection Issues
```bash
# Check database connectivity
telnet your-db-server 1433

# Check connection string
echo $CSET_DB_CONNECTION

# Test database connection
sqlcmd -S your-db-server -U cset_app -P SecurePassword123! -Q "SELECT 1"
```

#### Redis Connection Issues
```bash
# Check Redis connectivity
redis-cli -h your-redis-server ping

# Check Redis memory usage
redis-cli -h your-redis-server info memory

# Monitor Redis connections
redis-cli -h your-redis-server monitor
```

#### Performance Issues
```bash
# Check application performance
curl -X GET "https://cset.yourdomain.com/api/health/detailed"

# Check system resources
free -h
df -h
top

# Check application logs
tail -f /opt/cset/enterprise/logs/app.log | grep ERROR
```

### Support Resources

#### Documentation
- [CSET Enterprise Documentation](https://docs.cset.gov/enterprise)
- [API Reference](https://docs.cset.gov/api)
- [Troubleshooting Guide](https://docs.cset.gov/troubleshooting)

#### Support Contacts
- **Technical Support**: support@cset.gov
- **Security Issues**: security@cset.gov
- **Emergency**: +1-XXX-XXX-XXXX

#### Monitoring Tools
- **Application Insights**: Azure portal
- **Grafana Dashboard**: http://your-grafana-server
- **ELK Stack**: http://your-elk-server

---

## Security Checklist

### Pre-Deployment
- [ ] SSL/TLS certificate installed
- [ ] Firewall configured
- [ ] Database security configured
- [ ] Application secrets secured
- [ ] MFA enabled
- [ ] Rate limiting configured

### Post-Deployment
- [ ] Security scan completed
- [ ] Penetration testing performed
- [ ] Backup procedures tested
- [ ] Monitoring alerts configured
- [ ] Incident response plan documented
- [ ] User training completed

### Ongoing
- [ ] Regular security updates
- [ ] Vulnerability assessments
- [ ] Access reviews
- [ ] Audit log reviews
- [ ] Backup verification
- [ ] Performance monitoring

---

## Performance Tuning

### Database Optimization
```sql
-- Create indexes for performance
CREATE INDEX IX_Assessments_CreatedDate ON Assessments(CreatedDate);
CREATE INDEX IX_Questions_Category ON Questions(Category);
CREATE INDEX IX_UserActivity_UserId_Date ON UserActivity(UserId, ActivityDate);

-- Update statistics
UPDATE STATISTICS Assessments;
UPDATE STATISTICS Questions;
UPDATE STATISTICS UserActivity;
```

### Application Optimization
```json
{
  "Performance": {
    "EnableResponseCompression": true,
    "EnableCaching": true,
    "MaxConcurrentRequests": 1000,
    "RequestTimeoutSeconds": 30,
    "EnableDetailedLogging": false
  }
}
```

---

This comprehensive deployment guide ensures a secure, scalable, and maintainable CSET Enterprise installation with all enhanced features properly configured and optimized for production use. 