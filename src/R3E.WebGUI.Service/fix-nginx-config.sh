#!/bin/bash

# Fix nginx configuration for R3E WebGUI Service

set -e

INSTALL_DIR="/opt/r3e-webgui"
DOMAIN="service.neoservicelayer.com"

echo "=== Fixing Nginx Configuration ==="

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root or with sudo"
    exit 1
fi

# Stop services
echo "Stopping services..."
cd $INSTALL_DIR/configs
docker-compose down

# Backup existing config
if [ -f "$INSTALL_DIR/configs/nginx.conf" ]; then
    cp "$INSTALL_DIR/configs/nginx.conf" "$INSTALL_DIR/configs/nginx.conf.backup"
    echo "Backed up existing nginx.conf"
fi

# Create correct nginx configuration
echo "Creating nginx configuration..."
cat > $INSTALL_DIR/configs/nginx.conf << 'EOF'
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Logging
    access_log /var/log/nginx/access.log;
    error_log /var/log/nginx/error.log debug;

    # Basic settings
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;
    client_max_body_size 10M;

    # Upstream for WebGUI API
    upstream webgui_api {
        server r3e-webgui-service:8080;
    }

    # Default server
    server {
        listen 80 default_server;
        listen [::]:80 default_server;
        server_name _;

        # Root redirect to /contracts
        location = / {
            return 301 /contracts/;
        }

        # Contracts root
        location = /contracts {
            return 301 /contracts/;
        }

        # Contracts landing page
        location = /contracts/ {
            # For now, just return a simple response
            add_header Content-Type text/html;
            return 200 '<html><body><h1>R3E Contract WebGUI Service</h1><p>API: <a href="/contracts/api/">/contracts/api/</a></p><p>Swagger: <a href="/contracts/swagger">/contracts/swagger</a></p></body></html>';
        }

        # API endpoints
        location /contracts/api/ {
            proxy_pass http://webgui_api/api/;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts;
            proxy_set_header Connection "";
        }

        # Swagger
        location /contracts/swagger {
            proxy_pass http://webgui_api/swagger;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Prefix /contracts;
        }

        # Health check
        location /contracts/health {
            proxy_pass http://webgui_api/health;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
        }

        # Direct health check for testing
        location /health {
            add_header Content-Type text/plain;
            return 200 'OK';
        }

        # Contract WebGUIs
        location ~ ^/contracts/([^/]+)(/.*)?$ {
            set $contract_name $1;
            set $path_remainder $2;
            
            # Skip system endpoints
            if ($contract_name ~ ^(api|swagger|health)$) {
                return 404;
            }
            
            if ($path_remainder = '') {
                set $path_remainder '/';
            }
            
            # For now, return a placeholder
            add_header Content-Type text/html;
            return 200 '<html><body><h1>Contract: $contract_name</h1><p>Path: $path_remainder</p><p>WebGUI will be loaded here</p></body></html>';
        }
    }
}
EOF

# Update docker-compose to mount nginx config correctly
echo "Updating docker-compose configuration..."
cat > $INSTALL_DIR/configs/docker-compose.override.yml << 'EOF'
version: '3.8'

services:
  nginx:
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
    command: [nginx-debug, '-g', 'daemon off;']
EOF

# Start services
echo "Starting services..."
cd $INSTALL_DIR/configs
docker-compose up -d

# Wait for services
echo "Waiting for services to start..."
sleep 5

# Test the service
echo ""
echo "Testing service..."
echo "1. Testing root redirect:"
curl -I http://localhost/ 2>/dev/null | grep -E "HTTP|Location"

echo ""
echo "2. Testing /contracts/:"
curl -s http://localhost/contracts/ | head -5

echo ""
echo "3. Testing health:"
curl -I http://localhost/health 2>/dev/null | grep HTTP

echo ""
echo "4. Checking nginx logs:"
docker logs r3e-webgui-nginx --tail 10

echo ""
echo "Done! The service should now be accessible at:"
echo "- http://localhost/contracts/"
echo "- http://$DOMAIN/contracts/ (once DNS is configured)"