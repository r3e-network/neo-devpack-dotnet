#!/bin/bash

# ============================================================================
# R3E WebGUI Service Fix Script
# 
# This script fixes common issues with the WebGUI service deployment
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

echo -e "${BLUE}=== R3E WebGUI Service Fix Script ===${NC}"
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
        return 1
    fi
}

echo -e "${GREEN}Step 1: Stopping problematic nginx container...${NC}"
docker stop r3e-webgui-nginx 2>/dev/null || true
docker rm r3e-webgui-nginx 2>/dev/null || true
check_success "Nginx container stopped"

echo -e "${GREEN}Step 2: Backing up current configuration...${NC}"
cp $INSTALL_DIR/configs/nginx.conf $INSTALL_DIR/configs/nginx.conf.backup-$(date +%Y%m%d-%H%M%S)
check_success "Configuration backed up"

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

    # HTTPS server (if SSL certificates exist)
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

echo -e "${GREEN}Step 4: Testing nginx configuration...${NC}"
docker run --rm -v $INSTALL_DIR/configs/nginx.conf:/etc/nginx/nginx.conf:ro -v $INSTALL_DIR/ssl:/opt/r3e-webgui/ssl:ro nginx nginx -t
check_success "Nginx configuration valid"

echo -e "${GREEN}Step 5: Updating docker-compose configuration...${NC}"
# Ensure nginx container has correct port mappings
cd $INSTALL_DIR/configs
if ! grep -q "80:80" docker-compose.yml; then
    echo -e "${YELLOW}Updating docker-compose.yml to expose ports 80 and 443...${NC}"
    # Create a temporary file with the fix
    cat > docker-compose-fix.yml << 'EOF'
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
    mv docker-compose.yml docker-compose.yml.backup
    mv docker-compose-fix.yml docker-compose.yml
fi
check_success "Docker-compose configuration updated"

echo -e "${GREEN}Step 6: Restarting all services...${NC}"
docker-compose down
docker-compose up -d
check_success "Services restarted"

echo -e "${GREEN}Step 7: Waiting for services to initialize...${NC}"
sleep 10

echo -e "${GREEN}Step 8: Checking service status...${NC}"
echo ""
echo "Container Status:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep r3e-webgui

echo ""
echo -e "${GREEN}Step 9: Testing connectivity...${NC}"
echo -n "HTTP redirect (port 80): "
curl -s -o /dev/null -w "%{http_code}" -L http://localhost/ || echo "Failed"
echo ""

echo -n "HTTPS (port 443): "
curl -s -o /dev/null -w "%{http_code}" -k https://localhost/contracts/ || echo "Failed"
echo ""

echo -n "API Health check: "
curl -s -o /dev/null -w "%{http_code}" http://localhost:8888/health || echo "Failed"
echo ""

echo -e "${GREEN}Step 10: Checking logs for errors...${NC}"
echo "Nginx errors (if any):"
docker logs r3e-webgui-nginx 2>&1 | grep -i error | tail -5 || echo "No errors found"
echo ""

echo -e "${GREEN}✅ Fix script completed!${NC}"
echo ""
echo -e "${BLUE}Service should now be accessible at:${NC}"
echo "• https://service.neoservicelayer.com/contracts/"
echo "• API: https://service.neoservicelayer.com/contracts/api/"
echo "• Swagger: https://service.neoservicelayer.com/contracts/swagger"
echo ""
echo -e "${YELLOW}If you still have issues, run:${NC}"
echo "• Check logs: docker logs r3e-webgui-nginx"
echo "• Check status: docker ps | grep r3e-webgui"
echo "• Run diagnostics: curl -sSL https://raw.githubusercontent.com/r3e-network/r3e-devpack-dotnet/r3e/src/R3E.WebGUI.Service/diagnose-service.sh | sudo bash"