#!/bin/bash

# ============================================================================
# R3E WebGUI Service Fix Script V2
# 
# This script fixes all known issues with the WebGUI service deployment
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

echo -e "${BLUE}=== R3E WebGUI Service Fix Script V2 ===${NC}"
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
        return 0
    else
        echo -e "${RED}✗ $1 failed${NC}"
        return 1
    fi
}

echo -e "${GREEN}Step 1: Stopping all containers...${NC}"
cd $INSTALL_DIR/configs
docker-compose down || true
check_success "Containers stopped"

echo -e "${GREEN}Step 2: Backing up configurations...${NC}"
cp nginx.conf nginx.conf.backup-$(date +%Y%m%d-%H%M%S) 2>/dev/null || true
cp docker-compose.yml docker-compose.yml.backup-$(date +%Y%m%d-%H%M%S) 2>/dev/null || true
check_success "Configurations backed up"

echo -e "${GREEN}Step 3: Creating fixed nginx configuration...${NC}"
cat > $INSTALL_DIR/configs/nginx.conf << 'EOF'
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
        
        # SSL configuration
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;
        
        # Root redirect to contracts
        location = / {
            return 301 /contracts/;
        }

        location = /contracts {
            return 301 /contracts/;
        }

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

        location /contracts/api/ {
            proxy_pass http://webgui_api/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Forwarded-Prefix /contracts;
            proxy_read_timeout 300s;
            proxy_connect_timeout 75s;
        }

        location /contracts/swagger {
            proxy_pass http://webgui_api/swagger;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Forwarded-Prefix /contracts;
        }

        location /contracts/health {
            proxy_pass http://webgui_api/health;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
        }

        location ~ ^/contracts/([^/]+)(/.*)?$ {
            set $contract_name $1;
            set $path_remainder $2;
            
            if ($contract_name ~ ^(api|swagger|health)$) {
                return 404;
            }
            
            if ($path_remainder = '') {
                set $path_remainder '/';
            }
            
            root /var/www/webgui;
            try_files /$contract_name$path_remainder @contract_proxy;
        }

        location @contract_proxy {
            proxy_pass http://webgui_api/subdomain$path_remainder;
            proxy_set_header Host $contract_name.service.neoservicelayer.com;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Forwarded-Prefix /contracts/$contract_name;
            proxy_set_header X-Contract-Name $contract_name;
        }

        location /contracts/assets/ {
            alias /var/www/webgui/assets/;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }

    # HTTP server - redirect to HTTPS
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;
        return 301 https://$server_name$request_uri;
    }
}
EOF
check_success "Nginx configuration created"

echo -e "${GREEN}Step 4: Creating fixed docker-compose.yml...${NC}"
cat > $INSTALL_DIR/configs/docker-compose.yml << 'EOF'
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
      - ../data:/var/www/webgui:ro
    depends_on:
      - r3e-webgui-service
    networks:
      - r3e-network

  r3e-webgui-service:
    image: r3enetwork/webgui-service:latest
    container_name: r3e-webgui-service
    restart: always
    ports:
      - "8888:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=r3e-webgui-sqlserver;Database=R3EWebGUI;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true
      - ASPNETCORE_PATHBASE=/contracts
      - R3EWebGUI__BasePath=/contracts
      - R3EWebGUI__PublicUrl=https://service.neoservicelayer.com
    depends_on:
      - r3e-webgui-sqlserver
    networks:
      - r3e-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  r3e-webgui-sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: r3e-webgui-sqlserver
    restart: always
    ports:
      - "${SQL_PORT:-1433}:1433"
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD}
      - MSSQL_PID=Express
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - r3e-network
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "${DB_PASSWORD}", "-Q", "SELECT 1", "-C"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 30s

volumes:
  sqlserver-data:

networks:
  r3e-network:
    driver: bridge
EOF
check_success "Docker-compose configuration created"

echo -e "${GREEN}Step 5: Ensuring .env file has required variables...${NC}"
if ! grep -q "DB_PASSWORD" $INSTALL_DIR/configs/.env 2>/dev/null; then
    echo "DB_PASSWORD=$(openssl rand -base64 32)" >> $INSTALL_DIR/configs/.env
fi
check_success "Environment variables configured"

echo -e "${GREEN}Step 6: Creating/verifying SSL certificates...${NC}"
if [ ! -f "$INSTALL_DIR/ssl/fullchain.pem" ] || [ ! -f "$INSTALL_DIR/ssl/privkey.pem" ]; then
    echo -e "${YELLOW}SSL certificates not found. Attempting to copy from Let's Encrypt...${NC}"
    if [ -f "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" ]; then
        mkdir -p $INSTALL_DIR/ssl
        cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $INSTALL_DIR/ssl/
        cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $INSTALL_DIR/ssl/
        check_success "SSL certificates copied"
    else
        echo -e "${RED}No SSL certificates found. Service will run HTTP only.${NC}"
        # Modify nginx.conf to remove SSL configuration
        sed -i '/listen 443 ssl/,/^    }/d' $INSTALL_DIR/configs/nginx.conf
        sed -i 's/return 301 https/return 301 http/g' $INSTALL_DIR/configs/nginx.conf
    fi
else
    check_success "SSL certificates verified"
fi

echo -e "${GREEN}Step 7: Starting services...${NC}"
cd $INSTALL_DIR/configs
docker-compose up -d
check_success "Services started"

echo -e "${GREEN}Step 8: Waiting for services to initialize...${NC}"
echo -n "Waiting for services to be ready"
for i in {1..30}; do
    if docker exec r3e-webgui-service curl -s -f http://localhost:8080/health > /dev/null 2>&1; then
        echo ""
        check_success "Services ready"
        break
    fi
    echo -n "."
    sleep 2
done
echo ""

echo -e "${GREEN}Step 9: Verifying service status...${NC}"
echo ""
echo "Container Status:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep r3e-webgui || echo "No containers found"

echo ""
echo -e "${GREEN}Step 10: Testing endpoints...${NC}"

# Test internal health check
echo -n "API Health (internal): "
if docker exec r3e-webgui-service curl -s -f http://localhost:8080/health > /dev/null 2>&1; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
fi

# Test external endpoints
echo -n "HTTP redirect (port 80): "
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -L http://localhost/ 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ] || [ "$HTTP_CODE" = "302" ]; then
    echo -e "${GREEN}OK ($HTTP_CODE)${NC}"
else
    echo -e "${RED}FAILED ($HTTP_CODE)${NC}"
fi

echo -n "HTTPS (port 443): "
HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k https://localhost/contracts/ 2>/dev/null || echo "000")
if [ "$HTTPS_CODE" = "200" ]; then
    echo -e "${GREEN}OK ($HTTPS_CODE)${NC}"
else
    echo -e "${YELLOW}Status: $HTTPS_CODE${NC}"
fi

echo -n "API endpoint (8888): "
API_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8888/health 2>/dev/null || echo "000")
if [ "$API_CODE" = "200" ]; then
    echo -e "${GREEN}OK ($API_CODE)${NC}"
else
    echo -e "${RED}FAILED ($API_CODE)${NC}"
fi

echo ""
echo -e "${GREEN}Step 11: Checking for errors...${NC}"
echo "Recent nginx errors:"
docker logs r3e-webgui-nginx 2>&1 | grep -i error | tail -3 || echo "No errors found"

echo ""
echo -e "${GREEN}✅ Fix script completed!${NC}"
echo ""

# Determine if HTTPS is working
if [ "$HTTPS_CODE" = "200" ]; then
    PROTOCOL="https"
else
    PROTOCOL="http"
fi

echo -e "${BLUE}Service endpoints:${NC}"
echo "• Main: $PROTOCOL://service.neoservicelayer.com/contracts/"
echo "• API: $PROTOCOL://service.neoservicelayer.com/contracts/api/"
echo "• Swagger: $PROTOCOL://service.neoservicelayer.com/contracts/swagger"
echo "• Health: $PROTOCOL://service.neoservicelayer.com/contracts/health"
echo ""

if [ "$PROTOCOL" = "http" ]; then
    echo -e "${YELLOW}Note: Running in HTTP mode. To enable HTTPS:${NC}"
    echo "1. Ensure DNS points to this server"
    echo "2. Run: certbot certonly --standalone -d $DOMAIN"
    echo "3. Run this fix script again"
fi

echo ""
echo -e "${YELLOW}Useful commands:${NC}"
echo "• View logs: docker logs -f r3e-webgui-nginx"
echo "• Check status: docker ps | grep r3e-webgui"
echo "• Restart: cd $INSTALL_DIR/configs && docker-compose restart"
echo "• Stop: cd $INSTALL_DIR/configs && docker-compose down"