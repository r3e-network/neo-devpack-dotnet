#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Custom Setup for service.neoservicelayer.com
# 
# This script configures the R3E WebGUI Service for path-based routing:
# - Main domain: service.neoservicelayer.com
# - API: service.neoservicelayer.com/api/
# - Contracts: service.neoservicelayer.com/contracts/{contract-name}
# - Example: service.neoservicelayer.com/contracts/pricefeed
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
echo -e "${YELLOW}- API Path: $DOMAIN/api/${NC}"
echo -e "${YELLOW}- Contracts Path: $DOMAIN/contracts/{name}${NC}"
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

# Update nginx configuration for path-based routing
echo -e "${GREEN}Updating nginx configuration for path-based routing...${NC}"
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

    # Main server - service.neoservicelayer.com
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;

        # Root location - serve a landing page or redirect
        location = / {
            return 301 /api/swagger;
        }

        # API endpoints
        location /api/ {
            proxy_pass http://webgui_api/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Path /api;
        }

        # Swagger documentation
        location /swagger {
            proxy_pass http://webgui_api/swagger;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Health check
        location /health {
            proxy_pass http://webgui_api/health;
        }

        # Contract WebGUIs - /contracts/{contract-name}
        location ~ ^/contracts/([^/]+)(.*)$ {
            set $contract_name $1;
            set $path_remainder $2;
            
            # Check if requesting a static file
            root /var/www/webgui;
            
            # Try to serve static files first
            try_files /contracts/$contract_name$path_remainder @contract_backend;
        }

        # Fallback to backend for contract WebGUI
        location @contract_backend {
            # Rewrite to subdomain-style request for backend
            proxy_pass http://webgui_api/subdomain$path_remainder;
            proxy_set_header Host $contract_name.service.neoservicelayer.com;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Contract-Name $contract_name;
            proxy_set_header X-Original-Path /contracts/$contract_name$path_remainder;
        }

        # Static assets for all contracts
        location /assets/ {
            root /var/www/webgui;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }

    # SSL configuration (to be added after Let's Encrypt setup)
    # server {
    #     listen 443 ssl http2;
    #     listen [::]:443 ssl http2;
    #     server_name service.neoservicelayer.com;
    #     
    #     ssl_certificate /opt/r3e-webgui/ssl/fullchain.pem;
    #     ssl_certificate_key /opt/r3e-webgui/ssl/privkey.pem;
    #     
    #     # Copy all location blocks from above
    # }
}
EOF

# Update docker-compose environment for path-based routing
echo -e "${GREEN}Updating docker-compose configuration...${NC}"
sed -i "s/BASE_DOMAIN=.*/BASE_DOMAIN=$DOMAIN/g" $INSTALL_DIR/configs/.env
sed -i "s/ADMIN_EMAIL=.*/ADMIN_EMAIL=$EMAIL/g" $INSTALL_DIR/configs/.env

# Add specific configuration for path-based routing
cat >> $INSTALL_DIR/configs/.env << EOF

# Path-based routing configuration
ROUTING_MODE=path
BASE_PATH=/contracts
ALLOWED_ORIGINS=https://service.neoservicelayer.com
CORS_ENABLED=true
PUBLIC_URL=https://service.neoservicelayer.com
EOF

# Update the docker-compose file to support path-based routing
echo -e "${GREEN}Updating docker-compose for path-based routing...${NC}"
cat >> $INSTALL_DIR/configs/docker-compose.yml << EOF

    # Additional environment variables for path-based routing
    environment:
      - ASPNETCORE_PATHBASE=/api
      - R3EWebGUI__RoutingMode=path
      - R3EWebGUI__BasePath=/contracts
      - R3EWebGUI__PublicUrl=https://service.neoservicelayer.com
EOF

# Create a helper script for contract deployment
echo -e "${GREEN}Creating contract deployment helper script...${NC}"
cat > $INSTALL_DIR/scripts/deploy-contract.sh << 'EOF'
#!/bin/bash

# Helper script to deploy contracts with path-based URLs

if [ $# -lt 2 ]; then
    echo "Usage: $0 <contract-address> <contract-name>"
    echo "Example: $0 0x1234567890abcdef1234567890abcdef12345678 pricefeed"
    exit 1
fi

CONTRACT_ADDRESS=$1
CONTRACT_NAME=$2
DOMAIN="service.neoservicelayer.com"

echo "Deploying contract WebGUI..."
echo "Contract: $CONTRACT_NAME"
echo "Address: $CONTRACT_ADDRESS"
echo "URL will be: https://$DOMAIN/contracts/$CONTRACT_NAME"

# Create deployment request
TIMESTAMP=$(date +%s)
REQUEST_JSON=$(cat <<JSON
{
  "contractAddress": "$CONTRACT_ADDRESS",
  "contractName": "$CONTRACT_NAME",
  "network": "mainnet",
  "deployerAddress": "YOUR_DEPLOYER_ADDRESS",
  "timestamp": $TIMESTAMP,
  "signature": "YOUR_SIGNATURE",
  "publicKey": "YOUR_PUBLIC_KEY"
}
JSON
)

# Send deployment request
curl -X POST "http://localhost:8888/api/webgui/deploy-from-manifest" \
  -H "Content-Type: application/json" \
  -d "$REQUEST_JSON"

echo ""
echo "After deployment, access your contract at:"
echo "https://$DOMAIN/contracts/$CONTRACT_NAME"
EOF

chmod +x $INSTALL_DIR/scripts/deploy-contract.sh

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
echo "• API Documentation: http://$DOMAIN/api/swagger"
echo "• API Endpoints: http://$DOMAIN/api/"
echo "• Health Check: http://$DOMAIN/health"
echo "• Contract WebGUIs: http://$DOMAIN/contracts/{contract-name}"
echo "• Example: http://$DOMAIN/contracts/pricefeed"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Update DNS records:"
echo "   - A record: $DOMAIN → $(curl -s ifconfig.me)"
echo ""
echo "2. Setup SSL certificate (after DNS propagation):"
echo "   $INSTALL_DIR/scripts/setup-letsencrypt.sh"
echo ""
echo "3. Deploy your first contract WebGUI:"
echo "   $INSTALL_DIR/scripts/deploy-contract.sh <contract-address> <contract-name>"
echo "   Example: $INSTALL_DIR/scripts/deploy-contract.sh 0x123... pricefeed"
echo ""
echo -e "${BLUE}API Usage Example:${NC}"
echo 'curl -X POST "https://'$DOMAIN'/api/webgui/deploy-from-manifest" \'
echo '  -H "Content-Type: application/json" \'
echo '  -d @deployment-request.json'
echo ""

# Cleanup
rm -f /tmp/setup-r3e-webgui.sh