#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Build and Deploy Script (Fixed)
# 
# This script builds the WebGUI service with corrected Docker context
# ============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
DOMAIN="service.neoservicelayer.com"
INSTALL_DIR="/opt/r3e-webgui"
REPO_URL="https://github.com/r3e-network/r3e-devpack-dotnet.git"
BRANCH="r3e"

echo -e "${BLUE}=== R3E WebGUI Service - Build and Deploy (Fixed) ===${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo -e "${RED}This script must be run as root or with sudo${NC}"
    exit 1
fi

# Function to check command success
check_success() {
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ $1${NC}"
    else
        echo -e "${RED}✗ $1 failed${NC}"
        exit 1
    fi
}

echo -e "${GREEN}Step 1: Cloning repository if needed...${NC}"
if [ ! -d "/tmp/r3e-devpack-dotnet" ]; then
    cd /tmp
    git clone -b $BRANCH $REPO_URL
    check_success "Repository clone"
else
    echo "Repository already exists, using existing clone"
fi

echo -e "${GREEN}Step 2: Creating fixed Dockerfile...${NC}"
cd /tmp/r3e-devpack-dotnet/src/R3E.WebGUI.Service

# Create a fixed Dockerfile
cat > Dockerfile.fixed << 'EOF'
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file
COPY ["R3E.WebGUI.Service.csproj", "./"]
RUN dotnet restore "R3E.WebGUI.Service.csproj"

# Copy everything else
COPY . .

# Build
RUN dotnet build "R3E.WebGUI.Service.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "R3E.WebGUI.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directories
RUN mkdir -p /app/webgui-storage && \
    mkdir -p /app/logs && \
    mkdir -p /app/Templates && \
    mkdir -p /app/wwwroot

# Copy published app
COPY --from=publish /app/publish .

# Copy templates if they exist
COPY Templates/ /app/Templates/ || true

# Set up environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "R3E.WebGUI.Service.dll"]
EOF
check_success "Dockerfile created"

echo -e "${GREEN}Step 3: Building the WebGUI service...${NC}"
docker build -f Dockerfile.fixed -t r3e/webgui-service:latest .
check_success "Docker build"

echo -e "${GREEN}Step 4: Stopping existing services...${NC}"
cd $INSTALL_DIR/configs
docker-compose down 2>/dev/null || true
check_success "Services stopped"

echo -e "${GREEN}Step 5: Creating production docker-compose...${NC}"
cat > docker-compose.yml << 'EOF'
version: '3.8'

services:
  r3e-webgui-nginx:
    image: nginx:alpine
    container_name: r3e-webgui-nginx
    restart: always
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ../ssl:/opt/r3e-webgui/ssl:ro
      - webgui-static:/var/www/webgui:ro
    depends_on:
      - r3e-webgui-service
    networks:
      - r3e-network

  r3e-webgui-service:
    image: r3e/webgui-service:latest
    container_name: r3e-webgui-service
    restart: always
    ports:
      - "8888:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=r3e-webgui-sqlserver;Database=R3EWebGUI;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true;Encrypt=false
      - BasePath=/contracts
      - PublicUrl=https://service.neoservicelayer.com
      - CorsSettings__AllowedOrigins=https://service.neoservicelayer.com
      - JwtSettings__Secret=${JWT_SECRET}
      - AuthSettings__RequireSignature=false
      - Logging__LogLevel__Default=Information
      - Logging__LogLevel__Microsoft.AspNetCore=Warning
    depends_on:
      - r3e-webgui-sqlserver
    volumes:
      - webgui-static:/app/wwwroot
      - webgui-storage:/app/webgui-storage
      - ../logs:/app/logs
    networks:
      - r3e-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 60s

  r3e-webgui-sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: r3e-webgui-sqlserver
    restart: always
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD}
      - MSSQL_PID=Express
      - MSSQL_MEMORY_LIMIT_MB=2048
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - r3e-network
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "${DB_PASSWORD}", "-Q", "SELECT 1", "-C", "-l", "10"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 60s

volumes:
  sqlserver-data:
  webgui-static:
  webgui-storage:

networks:
  r3e-network:
    driver: bridge
EOF
check_success "Docker-compose created"

echo -e "${GREEN}Step 6: Ensuring environment variables...${NC}"
if [ ! -f ".env" ]; then
    touch .env
fi

# Generate secure passwords if not exist
if ! grep -q "DB_PASSWORD" .env; then
    # Generate a SQL-safe password (alphanumeric + some special chars)
    DB_PASS=$(openssl rand -base64 32 | tr -d '=' | tr -d '/' | tr -d '+' | tr -d '"' | tr -d "'" | cut -c1-25)
    echo "DB_PASSWORD=Pass${DB_PASS}123!" >> .env
fi
if ! grep -q "JWT_SECRET" .env; then
    echo "JWT_SECRET=$(openssl rand -base64 64)" >> .env
fi
check_success "Environment configured"

echo -e "${GREEN}Step 7: Creating production nginx configuration...${NC}"
cat > nginx.conf << 'EOF'
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Logging
    access_log /var/log/nginx/access.log;
    error_log /var/log/nginx/error.log;

    # Performance
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;
    client_max_body_size 10M;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;

    # Upstream for API
    upstream webgui_api {
        server r3e-webgui-service:8080;
    }

    # HTTPS server
    server {
        listen 443 ssl http2;
        listen [::]:443 ssl http2;
        server_name service.neoservicelayer.com;
        
        ssl_certificate /opt/r3e-webgui/ssl/fullchain.pem;
        ssl_certificate_key /opt/r3e-webgui/ssl/privkey.pem;
        
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;
        
        # Root redirect
        location = / {
            return 301 /contracts/;
        }

        location = /contracts {
            return 301 /contracts/;
        }

        # Main contracts page
        location = /contracts/ {
            proxy_pass http://webgui_api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Forwarded-Prefix /contracts;
            proxy_read_timeout 300s;
            proxy_connect_timeout 75s;
        }

        # API endpoints with rate limiting
        location /contracts/api/ {
            limit_req zone=api_limit burst=20 nodelay;
            
            proxy_pass http://webgui_api/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Forwarded-Prefix /contracts;
            proxy_read_timeout 300s;
            proxy_connect_timeout 75s;
        }

        # Swagger
        location ~ ^/contracts/swagger(.*)$ {
            proxy_pass http://webgui_api/swagger$1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }

        # Health check
        location /contracts/health {
            proxy_pass http://webgui_api/health;
            access_log off;
        }

        # Contract WebGUIs
        location ~ ^/contracts/([^/]+)(/.*)?$ {
            set $contract_name $1;
            set $path_remainder $2;
            
            if ($contract_name ~ ^(api|swagger|health)$) {
                return 404;
            }
            
            if ($path_remainder = '') {
                set $path_remainder '/';
            }
            
            # Try static files first
            root /var/www/webgui;
            try_files /$contract_name$path_remainder @contract_proxy;
        }

        location @contract_proxy {
            proxy_pass http://webgui_api/contracts/$contract_name$path_remainder;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Contract-Name $contract_name;
        }

        # Static assets
        location /contracts/assets/ {
            alias /var/www/webgui/assets/;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }

    # HTTP redirect to HTTPS
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;
        return 301 https://$server_name$request_uri;
    }
}
EOF
check_success "Nginx configuration created"

echo -e "${GREEN}Step 8: Starting services...${NC}"
docker-compose up -d
check_success "Services started"

echo -e "${GREEN}Step 9: Waiting for services to initialize...${NC}"
echo -n "Waiting for SQL Server"
for i in {1..60}; do
    if docker exec r3e-webgui-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(grep DB_PASSWORD .env | cut -d= -f2)" -Q "SELECT 1" -C &>/dev/null; then
        echo ""
        check_success "SQL Server ready"
        break
    fi
    echo -n "."
    sleep 2
done

echo -n "Waiting for API service"
for i in {1..60}; do
    if curl -s -f http://localhost:8888/health > /dev/null 2>&1; then
        echo ""
        check_success "API service ready"
        break
    fi
    echo -n "."
    sleep 2
done

echo -e "${GREEN}Step 10: Final verification...${NC}"
echo ""
docker ps --format "table {{.Names}}\t{{.Status}}" | grep r3e-webgui

echo ""
echo -e "${GREEN}Testing endpoints:${NC}"
echo -n "Health: "
curl -s http://localhost:8888/health || echo "Failed"
echo ""

echo ""
echo -e "${GREEN}✅ Deployment completed!${NC}"
echo ""
echo -e "${BLUE}Service is now available at:${NC}"
echo "• https://service.neoservicelayer.com/contracts/"
echo "• API: https://service.neoservicelayer.com/contracts/api/"
echo "• Swagger: https://service.neoservicelayer.com/contracts/swagger"
echo ""
echo -e "${YELLOW}View logs:${NC}"
echo "• docker logs -f r3e-webgui-service"
echo "• docker logs -f r3e-webgui-nginx"