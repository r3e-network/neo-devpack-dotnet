#!/bin/bash

# Emergency fix script to get the service running

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

INSTALL_DIR="/opt/r3e-webgui"

echo -e "${BLUE}=== Emergency Fix for R3E WebGUI Service ===${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "This script must be run as root or with sudo"
    exit 1
fi

cd $INSTALL_DIR/configs

echo -e "${GREEN}Creating working docker-compose configuration...${NC}"
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
      - ../data:/var/www/webgui:ro
    networks:
      - r3e-network

  r3e-webgui-service:
    # Using a simple web server as placeholder until the actual service is available
    image: nginx:alpine
    container_name: r3e-webgui-service
    restart: always
    ports:
      - "8888:80"
    volumes:
      - ./api-mock:/usr/share/nginx/html:ro
    networks:
      - r3e-network
    command: |
      sh -c '
      echo "server {
        listen 80;
        location /health {
          return 200 \"OK\";
          add_header Content-Type text/plain;
        }
        location / {
          return 200 \"WebGUI API Service - Coming Soon\";
          add_header Content-Type text/plain;
        }
      }" > /etc/nginx/conf.d/default.conf && nginx -g "daemon off;"'

  r3e-webgui-sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: r3e-webgui-sqlserver
    restart: always
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD:-StrongPassword123!}
      - MSSQL_PID=Express
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - r3e-network

volumes:
  sqlserver-data:

networks:
  r3e-network:
    driver: bridge
EOF

echo -e "${GREEN}Creating simple nginx configuration...${NC}"
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

    # Basic settings
    sendfile on;
    keepalive_timeout 65;

    # HTTPS server
    server {
        listen 443 ssl http2;
        listen [::]:443 ssl http2;
        server_name service.neoservicelayer.com;
        
        ssl_certificate /opt/r3e-webgui/ssl/fullchain.pem;
        ssl_certificate_key /opt/r3e-webgui/ssl/privkey.pem;
        
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        
        location / {
            return 301 /contracts/;
        }

        location /contracts/ {
            return 200 '<!DOCTYPE html>
<html>
<head>
    <title>R3E Contract WebGUI Service</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }
        h1 { color: #333; }
        .info { background: #f0f0f0; padding: 15px; border-radius: 5px; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>R3E Contract WebGUI Service</h1>
    <div class="info">
        <h2>Service Status</h2>
        <p>The WebGUI service is being deployed. Please check back soon.</p>
    </div>
    <div class="info">
        <h2>Coming Features</h2>
        <ul>
            <li>Contract WebGUI hosting at /contracts/{name}</li>
            <li>API endpoints at /contracts/api/</li>
            <li>Swagger documentation at /contracts/swagger</li>
        </ul>
    </div>
</body>
</html>';
            add_header Content-Type text/html;
        }

        location /contracts/health {
            return 200 "OK";
            add_header Content-Type text/plain;
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

echo -e "${GREEN}Stopping any existing containers...${NC}"
docker-compose down 2>/dev/null || true

echo -e "${GREEN}Starting services...${NC}"
docker-compose up -d

echo -e "${GREEN}Waiting for services to start...${NC}"
sleep 5

echo -e "${GREEN}Checking service status...${NC}"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep r3e-webgui

echo ""
echo -e "${GREEN}Testing endpoints...${NC}"
echo -n "HTTP redirect: "
curl -s -o /dev/null -w "%{http_code}" http://localhost/ || echo "Failed"
echo ""

echo -n "HTTPS: "
curl -s -o /dev/null -w "%{http_code}" -k https://localhost/contracts/ || echo "Failed"
echo ""

echo -n "Health check: "
curl -s -o /dev/null -w "%{http_code}" -k https://localhost/contracts/health || echo "Failed"
echo ""

echo ""
echo -e "${GREEN}✅ Emergency fix applied!${NC}"
echo ""
echo -e "${BLUE}The service should now be accessible at:${NC}"
echo "• https://service.neoservicelayer.com/contracts/"
echo ""
echo -e "${YELLOW}Note: This is a temporary setup. The actual WebGUI API service needs to be deployed.${NC}"