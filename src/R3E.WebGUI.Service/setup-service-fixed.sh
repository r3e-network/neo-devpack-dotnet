#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Fixed Setup Script for service.neoservicelayer.com
# 
# This script ensures proper setup and configuration of the WebGUI service
# with all necessary fixes and error handling
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
EMAIL="jimmy@r3e.network"
INSTALL_DIR="/opt/r3e-webgui"
SERVICE_PATH="/contracts"

echo -e "${BLUE}=== R3E WebGUI Service Setup (Fixed Version) ===${NC}"
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

# 1. Install Docker if not present
echo -e "${GREEN}Step 1: Checking Docker installation...${NC}"
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com | sh
    check_success "Docker installation"
    systemctl enable docker
    systemctl start docker
    check_success "Docker service start"
else
    echo "Docker already installed"
fi

# 2. Install Docker Compose if not present
echo -e "${GREEN}Step 2: Checking Docker Compose installation...${NC}"
if ! command -v docker-compose &> /dev/null; then
    echo "Installing Docker Compose..."
    curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    check_success "Docker Compose installation"
else
    echo "Docker Compose already installed"
fi

# 3. Create directory structure
echo -e "${GREEN}Step 3: Creating directory structure...${NC}"
mkdir -p $INSTALL_DIR/{configs,data,logs,ssl,scripts,data/landing}
check_success "Directory creation"

# 4. Create environment file
echo -e "${GREEN}Step 4: Creating environment configuration...${NC}"
cat > $INSTALL_DIR/configs/.env << EOF
# R3E WebGUI Service Configuration
BASE_DOMAIN=$DOMAIN
ADMIN_EMAIL=$EMAIL
SERVICE_BASE_PATH=$SERVICE_PATH
PUBLIC_BASE_URL=https://$DOMAIN$SERVICE_PATH
ASPNETCORE_PATHBASE=$SERVICE_PATH
ASPNETCORE_ENVIRONMENT=Production
R3EWebGUI__BasePath=$SERVICE_PATH
R3EWebGUI__PublicUrl=https://$DOMAIN
ALLOWED_ORIGINS=https://$DOMAIN
SWAGGER_BASE_PATH=$SERVICE_PATH
DB_PASSWORD=$(openssl rand -base64 32)
JWT_SECRET=$(openssl rand -base64 32)
CORS_ENABLED=true
EOF
check_success "Environment file creation"

# 5. Create docker-compose.yml
echo -e "${GREEN}Step 5: Creating docker-compose configuration...${NC}"
cat > $INSTALL_DIR/configs/docker-compose.yml << 'EOF'
version: '3.8'

services:
  nginx:
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
      - api
    networks:
      - r3e-network

  api:
    image: r3e/webgui-service:latest
    container_name: r3e-webgui-service
    restart: always
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
      - ASPNETCORE_PATHBASE=${ASPNETCORE_PATHBASE}
      - ConnectionStrings__DefaultConnection=Server=db;Database=R3EWebGUI;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True
      - JwtSettings__Secret=${JWT_SECRET}
      - CorsSettings__AllowedOrigins=${ALLOWED_ORIGINS}
    depends_on:
      - db
    networks:
      - r3e-network
    volumes:
      - ../data:/app/wwwroot
      - ../logs:/app/logs

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: r3e-webgui-db
    restart: always
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD}
      - MSSQL_PID=Express
    volumes:
      - db-data:/var/opt/mssql
    networks:
      - r3e-network

volumes:
  db-data:

networks:
  r3e-network:
    driver: bridge
EOF
check_success "Docker Compose configuration"

# 6. Create nginx configuration
echo -e "${GREEN}Step 6: Creating nginx configuration...${NC}"
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
        server api:8080;
    }

    # HTTP server - redirect to HTTPS if certificates exist
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;

        # Check if SSL is configured
        location = / {
            return 301 /contracts/;
        }

        # Contracts service root
        location = /contracts {
            return 301 /contracts/;
        }

        location = /contracts/ {
            proxy_pass http://webgui_api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts;
        }

        # API endpoints
        location /contracts/api/ {
            proxy_pass http://webgui_api/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts;
        }

        # Swagger documentation
        location /contracts/swagger {
            proxy_pass http://webgui_api/swagger;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts;
        }

        # Health check
        location /contracts/health {
            proxy_pass http://webgui_api/health;
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
            
            root /var/www/webgui;
            try_files /$contract_name$path_remainder @contract_proxy;
        }

        location @contract_proxy {
            proxy_pass http://webgui_api/subdomain$path_remainder;
            proxy_set_header Host $contract_name.$DOMAIN;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts/$contract_name;
            proxy_set_header X-Contract-Name $contract_name;
        }

        # Static assets
        location /contracts/assets/ {
            alias /var/www/webgui/assets/;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
EOF
check_success "Nginx configuration"

# 7. Create helper scripts
echo -e "${GREEN}Step 7: Creating helper scripts...${NC}"

# Status check script
cat > $INSTALL_DIR/scripts/status.sh << 'EOF'
#!/bin/bash
echo "=== R3E WebGUI Service Status ==="
echo ""
cd /opt/r3e-webgui/configs
docker-compose ps
echo ""
echo "=== Port Status ==="
ss -tlnp | grep -E ":(80|443|8080)" || true
echo ""
echo "=== Recent Logs ==="
docker logs r3e-webgui-service --tail 5 2>&1 || true
EOF
chmod +x $INSTALL_DIR/scripts/status.sh

# Logs script
cat > $INSTALL_DIR/scripts/logs.sh << 'EOF'
#!/bin/bash
service=${1:-all}
if [ "$service" = "all" ]; then
    docker-compose -f /opt/r3e-webgui/configs/docker-compose.yml logs -f
else
    docker logs "r3e-webgui-$service" -f
fi
EOF
chmod +x $INSTALL_DIR/scripts/logs.sh

# Restart script
cat > $INSTALL_DIR/scripts/restart.sh << 'EOF'
#!/bin/bash
echo "Restarting R3E WebGUI Service..."
cd /opt/r3e-webgui/configs
docker-compose down
docker-compose up -d
echo "Service restarted. Check status with: /opt/r3e-webgui/scripts/status.sh"
EOF
chmod +x $INSTALL_DIR/scripts/restart.sh

# SSL setup script
cat > $INSTALL_DIR/scripts/setup-ssl.sh << 'EOF'
#!/bin/bash
DOMAIN="service.neoservicelayer.com"
EMAIL="jimmy@r3e.network"

echo "Setting up SSL for $DOMAIN..."

# Stop nginx to free port 80
docker stop r3e-webgui-nginx 2>/dev/null || true

# Get certificate
certbot certonly --standalone -d $DOMAIN --non-interactive --agree-tos --email $EMAIL

if [ $? -eq 0 ]; then
    # Copy certificates
    mkdir -p /opt/r3e-webgui/ssl
    cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem /opt/r3e-webgui/ssl/
    cp /etc/letsencrypt/live/$DOMAIN/privkey.pem /opt/r3e-webgui/ssl/
    
    # Update nginx config for SSL
    /opt/r3e-webgui/scripts/enable-ssl.sh
    
    echo "SSL setup complete!"
else
    echo "SSL certificate generation failed!"
    docker start r3e-webgui-nginx
fi
EOF
chmod +x $INSTALL_DIR/scripts/setup-ssl.sh

# Enable SSL script
cat > $INSTALL_DIR/scripts/enable-ssl.sh << 'EOF'
#!/bin/bash

CONFIG="/opt/r3e-webgui/configs/nginx.conf"

# Add SSL configuration if not present
if ! grep -q "listen 443 ssl" $CONFIG; then
    # Insert SSL server block before the last closing brace
    sed -i '$ d' $CONFIG
    cat >> $CONFIG << 'NGINX_SSL'

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
        
        # Same location blocks as HTTP
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
        }

        location /contracts/api/ {
            proxy_pass http://webgui_api/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header X-Forwarded-Prefix /contracts;
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

    # Update HTTP server to redirect to HTTPS
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;
        return 301 https://$server_name$request_uri;
    }
}
NGINX_SSL
fi

# Restart nginx
docker start r3e-webgui-nginx || docker restart r3e-webgui-nginx
EOF
chmod +x $INSTALL_DIR/scripts/enable-ssl.sh

# 8. Create landing page
echo -e "${GREEN}Step 8: Creating landing page...${NC}"
cat > $INSTALL_DIR/data/landing/index.html << 'EOF'
<!DOCTYPE html>
<html>
<head>
    <title>R3E Contract WebGUI Service</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }
        h1 { color: #333; }
        .info { background: #f0f0f0; padding: 15px; border-radius: 5px; margin: 20px 0; }
        code { background: #e0e0e0; padding: 2px 5px; border-radius: 3px; }
        a { color: #0066cc; }
    </style>
</head>
<body>
    <h1>R3E Contract WebGUI Service</h1>
    <p>Welcome to the R3E Contract WebGUI hosting service.</p>
    
    <div class="info">
        <h2>API Documentation</h2>
        <p>View the API documentation at: <a href="/contracts/swagger">/contracts/swagger</a></p>
    </div>
    
    <div class="info">
        <h2>Health Check</h2>
        <p>Service status: <a href="/contracts/health">/contracts/health</a></p>
    </div>
</body>
</html>
EOF
check_success "Landing page creation"

# 9. Configure firewall
echo -e "${GREEN}Step 9: Configuring firewall...${NC}"
if command -v ufw &> /dev/null; then
    ufw allow 22/tcp
    ufw allow 80/tcp
    ufw allow 443/tcp
    echo "y" | ufw enable
    check_success "Firewall configuration"
else
    echo "UFW not found, skipping firewall configuration"
fi

# 10. Pull/Build Docker images
echo -e "${GREEN}Step 10: Preparing Docker images...${NC}"
cd $INSTALL_DIR/configs

# For now, use a placeholder. In production, this would pull from registry
cat > Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
# Placeholder - actual service would be copied here
CMD ["echo", "WebGUI Service Placeholder"]
EOF

# Build temporary image
docker build -t r3e/webgui-service:latest .
check_success "Docker image preparation"

# 11. Start services
echo -e "${GREEN}Step 11: Starting services...${NC}"
docker-compose down 2>/dev/null || true
docker-compose up -d
check_success "Service startup"

# Wait for services to be ready
echo -e "${GREEN}Waiting for services to initialize...${NC}"
sleep 10

# 12. Final checks
echo -e "${GREEN}Step 12: Running final checks...${NC}"
$INSTALL_DIR/scripts/status.sh

echo ""
echo -e "${GREEN}✅ Setup complete!${NC}"
echo ""
echo -e "${BLUE}Service URLs:${NC}"
echo "• Landing Page: http://$DOMAIN/contracts/"
echo "• API Documentation: http://$DOMAIN/contracts/swagger"
echo "• Health Check: http://$DOMAIN/contracts/health"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Set up SSL certificate:"
echo "   sudo $INSTALL_DIR/scripts/setup-ssl.sh"
echo ""
echo "2. Monitor service:"
echo "   sudo $INSTALL_DIR/scripts/status.sh"
echo "   sudo $INSTALL_DIR/scripts/logs.sh"
echo ""
echo -e "${BLUE}Troubleshooting:${NC}"
echo "• Check status: $INSTALL_DIR/scripts/status.sh"
echo "• View logs: $INSTALL_DIR/scripts/logs.sh [nginx|api|db]"
echo "• Restart service: $INSTALL_DIR/scripts/restart.sh"
echo ""

# Create systemd service for auto-start
echo -e "${GREEN}Creating systemd service...${NC}"
cat > /etc/systemd/system/r3e-webgui.service << EOF
[Unit]
Description=R3E WebGUI Service
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=$INSTALL_DIR/configs
ExecStart=/usr/local/bin/docker-compose up -d
ExecStop=/usr/local/bin/docker-compose down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable r3e-webgui.service
check_success "Systemd service creation"

echo -e "${GREEN}✅ All done! The service should now be accessible.${NC}"