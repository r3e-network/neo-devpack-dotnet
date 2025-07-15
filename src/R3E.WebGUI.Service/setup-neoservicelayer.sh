#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Custom Setup for service.neoservicelayer.com
# 
# This script configures the R3E WebGUI Service for the specific domain setup:
# - Main domain: service.neoservicelayer.com
# - Contract subdomains: *.service.neoservicelayer.com
# - Example: pricefeed.service.neoservicelayer.com
# ============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Fixed configuration
DOMAIN="service.neoservicelayer.com"
EMAIL="jimmy@r3e.network"
INSTALL_DIR="/opt/r3e-webgui"

echo -e "${BLUE}=== R3E WebGUI Service Setup for service.neoservicelayer.com ===${NC}"
echo ""
echo -e "${YELLOW}This will configure the R3E WebGUI Service with:${NC}"
echo -e "${YELLOW}- Domain: $DOMAIN${NC}"
echo -e "${YELLOW}- Wildcard: *.$DOMAIN${NC}"
echo -e "${YELLOW}- Email: $EMAIL${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo -e "${RED}This script must be run as root or with sudo${NC}"
    exit 1
fi

# Download and run the main setup script with custom parameters
echo -e "${GREEN}Downloading setup script...${NC}"
curl -sSL https://raw.githubusercontent.com/r3e-network/r3e-devpack-dotnet/r3e/src/R3E.WebGUI.Service/setup-r3e-webgui.sh -o /tmp/setup-r3e-webgui.sh
chmod +x /tmp/setup-r3e-webgui.sh

# Create a pre-configuration file
echo -e "${GREEN}Creating configuration...${NC}"
mkdir -p $INSTALL_DIR/configs

# Export variables for the setup script
export DEFAULT_BASE_DOMAIN="$DOMAIN"
export DEFAULT_ADMIN_EMAIL="$EMAIL"

# Run the setup script in non-interactive mode
echo -e "${GREEN}Running setup...${NC}"
/tmp/setup-r3e-webgui.sh -y

# Update nginx configuration for the specific domain
echo -e "${GREEN}Updating nginx configuration for $DOMAIN...${NC}"
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

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;

    # Upstream for API
    upstream webgui_api {
        server r3e-webgui-service:8080;
    }

    # Main domain - service.neoservicelayer.com
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;

        # API endpoints
        location /api/ {
            proxy_pass http://webgui_api;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Health check
        location /health {
            proxy_pass http://webgui_api/health;
        }

        # Swagger documentation
        location /swagger {
            proxy_pass http://webgui_api/swagger;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
        }

        # Root - redirect to swagger
        location = / {
            return 301 /swagger;
        }
    }

    # Wildcard subdomains - *.service.neoservicelayer.com
    server {
        listen 80;
        listen [::]:80;
        server_name *.service.neoservicelayer.com;

        # Extract subdomain
        set $subdomain "";
        if ($host ~* ^([^.]+)\.service\.neoservicelayer\.com$) {
            set $subdomain $1;
        }

        # Serve WebGUI files
        root /var/www/webgui/$subdomain;
        index index.html;

        # Try files, fallback to API
        location / {
            try_files $uri $uri/ @backend;
        }

        # Fallback to backend for dynamic content
        location @backend {
            proxy_pass http://webgui_api/subdomain/$uri;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Subdomain $subdomain;
        }

        # API calls from WebGUI
        location /api/ {
            proxy_pass http://webgui_api;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }

    # SSL configuration (to be added after Let's Encrypt setup)
    # include /etc/nginx/conf.d/ssl.conf;
}
EOF

# Update docker-compose environment
echo -e "${GREEN}Updating docker-compose configuration...${NC}"
sed -i "s/BASE_DOMAIN=.*/BASE_DOMAIN=$DOMAIN/g" $INSTALL_DIR/configs/.env
sed -i "s/ADMIN_EMAIL=.*/ADMIN_EMAIL=$EMAIL/g" $INSTALL_DIR/configs/.env

# Add specific configuration for service.neoservicelayer.com
cat >> $INSTALL_DIR/configs/.env << EOF

# NeoServiceLayer specific configuration
ALLOWED_ORIGINS=https://service.neoservicelayer.com,https://*.service.neoservicelayer.com
CORS_ENABLED=true
EOF

# Restart services
echo -e "${GREEN}Restarting services...${NC}"
cd $INSTALL_DIR/configs
docker-compose down
docker-compose up -d

# Show completion message
echo ""
echo -e "${GREEN}✅ Setup complete!${NC}"
echo ""
echo -e "${BLUE}Service URLs:${NC}"
echo "• API: http://service.neoservicelayer.com/api/"
echo "• Swagger: http://service.neoservicelayer.com/swagger"
echo "• Contract WebGUIs: http://{contract}.service.neoservicelayer.com"
echo "• Example: http://pricefeed.service.neoservicelayer.com"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Update DNS records:"
echo "   - A record: service.neoservicelayer.com → $(curl -s ifconfig.me)"
echo "   - CNAME record: *.service.neoservicelayer.com → service.neoservicelayer.com"
echo ""
echo "2. Setup SSL certificates (after DNS propagation):"
echo "   $INSTALL_DIR/scripts/setup-letsencrypt.sh"
echo ""
echo "3. Deploy your first contract WebGUI:"
echo "   Use the API at https://service.neoservicelayer.com/api/webgui/deploy-from-manifest"
echo ""

# Cleanup
rm -f /tmp/setup-r3e-webgui.sh