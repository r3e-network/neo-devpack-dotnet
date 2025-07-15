#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Setup for service.neoservicelayer.com/contracts
# 
# This script configures the R3E WebGUI Service:
# - Domain: service.neoservicelayer.com
# - Service Root: service.neoservicelayer.com/contracts
# - API: service.neoservicelayer.com/contracts/api/
# - Contract WebGUIs: service.neoservicelayer.com/contracts/{contract-name}
# - Swagger: service.neoservicelayer.com/contracts/swagger
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
SERVICE_PATH="/contracts"

echo -e "${BLUE}=== R3E Contract WebGUI Service Setup ===${NC}"
echo ""
echo -e "${YELLOW}This will configure the R3E WebGUI Service at:${NC}"
echo -e "${YELLOW}- Domain: $DOMAIN${NC}"
echo -e "${YELLOW}- Service Path: $DOMAIN$SERVICE_PATH${NC}"
echo -e "${YELLOW}- API: $DOMAIN$SERVICE_PATH/api/${NC}"
echo -e "${YELLOW}- Swagger: $DOMAIN$SERVICE_PATH/swagger${NC}"
echo -e "${YELLOW}- Contract WebGUIs: $DOMAIN$SERVICE_PATH/{contract-name}${NC}"
echo -e "${YELLOW}- Admin Email: $EMAIL${NC}"
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
echo -e "${GREEN}Running base setup...${NC}"
/tmp/setup-r3e-webgui.sh -y

# Update nginx configuration for /contracts path
echo -e "${GREEN}Configuring nginx for /contracts path...${NC}"
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

    # Upstream for WebGUI API service
    upstream webgui_api {
        server r3e-webgui-service:8080;
    }

    # Main server - service.neoservicelayer.com
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;

        # Root location - could serve a landing page or redirect
        location = / {
            # Redirect to contracts service
            return 301 /contracts/;
        }

        # Contracts service root
        location = /contracts {
            return 301 /contracts/;
        }

        location = /contracts/ {
            # Landing page for contracts service
            proxy_pass http://webgui_api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts;
        }

        # API endpoints under /contracts/api/
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

        # Individual contract WebGUIs - /contracts/{contract-name}
        location ~ ^/contracts/([^/]+)(/.*)?$ {
            set $contract_name $1;
            set $path_remainder $2;
            
            # Skip API, swagger, and health endpoints
            if ($contract_name ~ ^(api|swagger|health)$) {
                return 404;
            }
            
            # Default path if not specified
            if ($path_remainder = '') {
                set $path_remainder '/';
            }
            
            # Check if this is a static file request
            root /var/www/webgui;
            try_files /$contract_name$path_remainder @contract_proxy;
        }

        # Proxy to contract WebGUI backend
        location @contract_proxy {
            # The backend expects subdomain-style routing, so we simulate it
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

    # SSL configuration (to be added after Let's Encrypt setup)
    # server {
    #     listen 443 ssl http2;
    #     listen [::]:443 ssl http2;
    #     server_name service.neoservicelayer.com;
    #     
    #     ssl_certificate /opt/r3e-webgui/ssl/fullchain.pem;
    #     ssl_certificate_key /opt/r3e-webgui/ssl/privkey.pem;
    #     
    #     # Include all location blocks from above
    # }
}
EOF

# Update environment configuration
echo -e "${GREEN}Updating environment configuration...${NC}"
cat >> $INSTALL_DIR/configs/.env << EOF

# Path configuration for service.neoservicelayer.com/contracts
SERVICE_BASE_PATH=/contracts
PUBLIC_BASE_URL=https://service.neoservicelayer.com/contracts
ASPNETCORE_PATHBASE=/contracts
R3EWebGUI__BasePath=/contracts
R3EWebGUI__PublicUrl=https://service.neoservicelayer.com
ALLOWED_ORIGINS=https://service.neoservicelayer.com
SWAGGER_BASE_PATH=/contracts
EOF

# Create deployment helper script
echo -e "${GREEN}Creating deployment helper script...${NC}"
cat > $INSTALL_DIR/scripts/deploy-contract-webgui.sh << 'EOF'
#!/bin/bash

# R3E Contract WebGUI Deployment Helper

if [ $# -lt 2 ]; then
    echo "Usage: $0 <contract-address> <contract-name> [network]"
    echo "Example: $0 0x1234567890abcdef1234567890abcdef12345678 mytoken mainnet"
    echo "Network options: mainnet, testnet (default: mainnet)"
    exit 1
fi

CONTRACT_ADDRESS=$1
CONTRACT_NAME=$2
NETWORK=${3:-mainnet}
BASE_URL="http://localhost:8888/api"

echo "Deploying Contract WebGUI..."
echo "================================"
echo "Contract Name: $CONTRACT_NAME"
echo "Contract Address: $CONTRACT_ADDRESS"
echo "Network: $NETWORK"
echo "WebGUI URL: https://service.neoservicelayer.com/contracts/$CONTRACT_NAME"
echo ""

# Note: In production, you'll need to sign this request properly
TIMESTAMP=$(date +%s)

# Create deployment request
REQUEST_JSON=$(cat <<JSON
{
  "contractAddress": "$CONTRACT_ADDRESS",
  "contractName": "$CONTRACT_NAME",
  "network": "$NETWORK",
  "deployerAddress": "YOUR_NEO_ADDRESS",
  "timestamp": $TIMESTAMP,
  "signature": "SIGNATURE_HERE",
  "publicKey": "PUBLIC_KEY_HERE",
  "description": "Contract WebGUI for $CONTRACT_NAME"
}
JSON
)

echo "Sending deployment request..."
curl -X POST "$BASE_URL/webgui/deploy-from-manifest" \
  -H "Content-Type: application/json" \
  -d "$REQUEST_JSON" \
  -w "\n"

echo ""
echo "After successful deployment, the WebGUI will be available at:"
echo "https://service.neoservicelayer.com/contracts/$CONTRACT_NAME"
EOF

chmod +x $INSTALL_DIR/scripts/deploy-contract-webgui.sh

# Create a simple index page for /contracts
echo -e "${GREEN}Creating contracts landing page...${NC}"
mkdir -p $INSTALL_DIR/data/landing
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
        <h2>Deployed Contracts</h2>
        <p>Contract WebGUIs are available at: <code>/contracts/{contract-name}</code></p>
        <p>Example: <code>/contracts/mytoken</code></p>
    </div>
    
    <div class="info">
        <h2>Deploy a Contract WebGUI</h2>
        <p>Use the API endpoint: <code>POST /contracts/api/webgui/deploy-from-manifest</code></p>
        <p>See the <a href="/contracts/swagger">API documentation</a> for details.</p>
    </div>
</body>
</html>
EOF

# Restart services
echo -e "${GREEN}Restarting services...${NC}"
cd $INSTALL_DIR/configs
docker-compose down
docker-compose up -d

# Wait for services to start
echo -e "${GREEN}Waiting for services to start...${NC}"
sleep 10

# Show completion message
echo ""
echo -e "${GREEN}✅ R3E Contract WebGUI Service Setup Complete!${NC}"
echo ""
echo -e "${BLUE}Service Information:${NC}"
echo "• Domain: $DOMAIN"
echo "• Service Root: https://$DOMAIN/contracts"
echo "• API Base: https://$DOMAIN/contracts/api/"
echo "• API Docs: https://$DOMAIN/contracts/swagger"
echo "• Health Check: https://$DOMAIN/contracts/health"
echo ""
echo -e "${BLUE}Contract WebGUI URLs:${NC}"
echo "• Pattern: https://$DOMAIN/contracts/{contract-name}"
echo "• Example: https://$DOMAIN/contracts/mytoken"
echo ""
echo -e "${YELLOW}Required DNS Configuration:${NC}"
echo "• A Record: $DOMAIN → $(curl -s ifconfig.me)"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Configure DNS (if not already done)"
echo "2. Setup SSL certificate:"
echo "   $INSTALL_DIR/scripts/setup-letsencrypt.sh"
echo ""
echo "3. Deploy a contract WebGUI:"
echo "   $INSTALL_DIR/scripts/deploy-contract-webgui.sh <address> <name> [network]"
echo ""
echo -e "${BLUE}Service Management:${NC}"
echo "• View logs: $INSTALL_DIR/scripts/logs.sh"
echo "• Check status: $INSTALL_DIR/scripts/status.sh"
echo "• Restart: $INSTALL_DIR/scripts/restart.sh"
echo "• Update: $INSTALL_DIR/scripts/update.sh"
echo ""

# Cleanup
rm -f /tmp/setup-r3e-webgui.sh