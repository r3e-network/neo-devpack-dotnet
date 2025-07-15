#!/bin/bash

# Debug script for R3E WebGUI Service

echo "=== R3E WebGUI Service Debug ==="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root or with sudo"
    exit 1
fi

INSTALL_DIR="/opt/r3e-webgui"

echo "1. Checking Docker containers..."
docker ps -a | grep r3e-webgui
echo ""

echo "2. Checking nginx configuration..."
if [ -f "$INSTALL_DIR/configs/nginx.conf" ]; then
    echo "Nginx config exists at: $INSTALL_DIR/configs/nginx.conf"
    echo "First 20 lines:"
    head -20 "$INSTALL_DIR/configs/nginx.conf"
else
    echo "ERROR: Nginx config not found!"
fi
echo ""

echo "3. Checking docker-compose setup..."
if [ -f "$INSTALL_DIR/configs/docker-compose.yml" ]; then
    echo "Docker-compose file exists"
    cd "$INSTALL_DIR/configs"
    docker-compose ps
else
    echo "ERROR: docker-compose.yml not found!"
fi
echo ""

echo "4. Checking nginx logs..."
echo "Last 10 lines of nginx access log:"
docker logs r3e-webgui-nginx --tail 10 2>&1 | grep -v "^$"
echo ""

echo "5. Testing service endpoints..."
echo "Testing http://localhost/"
curl -I http://localhost/ 2>/dev/null | head -5

echo ""
echo "Testing http://localhost/contracts/"
curl -I http://localhost/contracts/ 2>/dev/null | head -5

echo ""
echo "Testing http://localhost:8888/health"
curl -I http://localhost:8888/health 2>/dev/null | head -5

echo ""
echo "6. Checking service logs..."
echo "R3E WebGUI Service logs:"
docker logs r3e-webgui-service --tail 20 2>&1

echo ""
echo "7. Checking port bindings..."
netstat -tlnp | grep -E ":(80|443|8888|1433)" || ss -tlnp | grep -E ":(80|443|8888|1433)"

echo ""
echo "8. Docker compose config check..."
cd "$INSTALL_DIR/configs"
docker-compose config | head -50