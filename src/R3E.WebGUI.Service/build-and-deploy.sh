#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Build and Deploy Script
# 
# This script builds the actual WebGUI service from source and deploys it
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

echo -e "${BLUE}=== R3E WebGUI Service - Build and Deploy ===${NC}"
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

echo -e "${GREEN}Step 1: Installing prerequisites...${NC}"
# Install .NET SDK if not present
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET 8.0 SDK..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x ./dotnet-install.sh
    ./dotnet-install.sh --version latest
    rm dotnet-install.sh
    export PATH=$PATH:$HOME/.dotnet
    check_success ".NET SDK installation"
else
    echo ".NET SDK already installed"
fi

# Install Git if not present
if ! command -v git &> /dev/null; then
    apt-get update && apt-get install -y git
    check_success "Git installation"
fi

echo -e "${GREEN}Step 2: Cloning repository...${NC}"
cd /tmp
rm -rf r3e-devpack-dotnet
git clone -b $BRANCH $REPO_URL
check_success "Repository clone"

echo -e "${GREEN}Step 3: Building the WebGUI service...${NC}"
cd /tmp/r3e-devpack-dotnet/src/R3E.WebGUI.Service

# Build the Docker image
docker build -t r3e/webgui-service:latest .
check_success "Docker build"

echo -e "${GREEN}Step 4: Stopping existing services...${NC}"
cd $INSTALL_DIR/configs
docker-compose down 2>/dev/null || true
check_success "Services stopped"

echo -e "${GREEN}Step 5: Updating docker-compose configuration...${NC}"
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
      - ConnectionStrings__DefaultConnection=Server=r3e-webgui-sqlserver;Database=R3EWebGUI;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true
      - ASPNETCORE_PATHBASE=/contracts
      - BasePath=/contracts
      - PublicUrl=https://service.neoservicelayer.com
      - CorsSettings__AllowedOrigins=https://service.neoservicelayer.com
      - JwtSettings__Secret=${JWT_SECRET:-$(openssl rand -base64 32)}
      - AuthSettings__RequireSignature=false
    depends_on:
      - r3e-webgui-sqlserver
    volumes:
      - webgui-static:/app/wwwroot
      - ../logs:/app/logs
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
  webgui-static:

networks:
  r3e-network:
    driver: bridge
EOF
check_success "Docker-compose updated"

echo -e "${GREEN}Step 6: Ensuring environment variables...${NC}"
if [ ! -f ".env" ]; then
    touch .env
fi
if ! grep -q "DB_PASSWORD" .env; then
    echo "DB_PASSWORD=$(openssl rand -base64 32 | tr -d '=' | tr -d '/' | tr -d '+')" >> .env
fi
if ! grep -q "JWT_SECRET" .env; then
    echo "JWT_SECRET=$(openssl rand -base64 32)" >> .env
fi
check_success "Environment configured"

echo -e "${GREEN}Step 7: Updating nginx configuration...${NC}"
# Copy the nginx configuration from the repo
cp /tmp/r3e-devpack-dotnet/src/R3E.WebGUI.Service/nginx-service-neoservicelayer.conf ./nginx.conf
# Fix any hardcoded domains
sed -i 's/\$DOMAIN/service.neoservicelayer.com/g' ./nginx.conf
check_success "Nginx configuration updated"

echo -e "${GREEN}Step 8: Starting services...${NC}"
docker-compose up -d
check_success "Services started"

echo -e "${GREEN}Step 9: Waiting for database initialization...${NC}"
echo -n "Waiting for SQL Server to be ready"
for i in {1..60}; do
    if docker exec r3e-webgui-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(grep DB_PASSWORD .env | cut -d= -f2)" -Q "SELECT 1" -C &>/dev/null; then
        echo ""
        check_success "SQL Server ready"
        break
    fi
    echo -n "."
    sleep 2
done

echo -e "${GREEN}Step 10: Waiting for API service to be ready...${NC}"
echo -n "Waiting for API service"
for i in {1..30}; do
    if curl -s -f http://localhost:8888/health > /dev/null 2>&1; then
        echo ""
        check_success "API service ready"
        break
    fi
    echo -n "."
    sleep 2
done

echo -e "${GREEN}Step 11: Verifying deployment...${NC}"
echo ""
echo "Container Status:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep r3e-webgui

echo ""
echo -e "${GREEN}Step 12: Testing endpoints...${NC}"

echo -n "Health check: "
curl -s http://localhost:8888/health || echo "Failed"
echo ""

echo -n "HTTPS main page: "
HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k https://localhost/contracts/ 2>/dev/null || echo "000")
echo "$HTTPS_CODE"

echo -n "API endpoint: "
API_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k https://localhost/contracts/api/ 2>/dev/null || echo "000")
echo "$API_CODE"

# Cleanup
rm -rf /tmp/r3e-devpack-dotnet

echo ""
echo -e "${GREEN}✅ Build and deployment completed!${NC}"
echo ""
echo -e "${BLUE}Service endpoints:${NC}"
echo "• Main: https://service.neoservicelayer.com/contracts/"
echo "• API: https://service.neoservicelayer.com/contracts/api/"
echo "• Swagger: https://service.neoservicelayer.com/contracts/swagger"
echo "• Health: https://service.neoservicelayer.com/contracts/health"
echo ""
echo -e "${YELLOW}To view logs:${NC}"
echo "• API logs: docker logs -f r3e-webgui-service"
echo "• Nginx logs: docker logs -f r3e-webgui-nginx"
echo "• DB logs: docker logs -f r3e-webgui-sqlserver"