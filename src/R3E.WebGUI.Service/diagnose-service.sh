#!/bin/bash

# R3E WebGUI Service Diagnostic Script

echo "=== R3E WebGUI Service Diagnostics ==="
echo "Date: $(date)"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check function
check() {
    if eval "$2"; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "${RED}✗${NC} $1"
        return 1
    fi
}

echo "1. System Checks:"
check "Running as root/sudo" "[ $EUID -eq 0 ]"
check "Docker installed" "command -v docker &>/dev/null"
check "Docker running" "systemctl is-active docker &>/dev/null"
check "Docker Compose installed" "command -v docker-compose &>/dev/null"
echo ""

echo "2. Directory Structure:"
check "/opt/r3e-webgui exists" "[ -d /opt/r3e-webgui ]"
check "configs directory exists" "[ -d /opt/r3e-webgui/configs ]"
check "nginx.conf exists" "[ -f /opt/r3e-webgui/configs/nginx.conf ]"
check "docker-compose.yml exists" "[ -f /opt/r3e-webgui/configs/docker-compose.yml ]"
check ".env file exists" "[ -f /opt/r3e-webgui/configs/.env ]"
echo ""

echo "3. Docker Containers:"
echo "Running containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep r3e-webgui || echo "No r3e-webgui containers running"
echo ""
echo "All containers (including stopped):"
docker ps -a --format "table {{.Names}}\t{{.Status}}" | grep r3e-webgui || echo "No r3e-webgui containers found"
echo ""

echo "4. Port Availability:"
echo "Listening ports:"
ss -tlnp | grep -E ":(80|443|8080|8888)" || echo "No expected ports listening"
echo ""

echo "5. Nginx Configuration Test:"
if docker ps | grep -q r3e-webgui-nginx; then
    docker exec r3e-webgui-nginx nginx -t 2>&1
else
    echo "Nginx container not running"
fi
echo ""

echo "6. SSL Certificate Check:"
if [ -f /opt/r3e-webgui/ssl/fullchain.pem ]; then
    echo "SSL certificate found:"
    openssl x509 -in /opt/r3e-webgui/ssl/fullchain.pem -noout -subject -dates
else
    echo "No SSL certificate found at /opt/r3e-webgui/ssl/fullchain.pem"
fi
echo ""

echo "7. Recent Container Logs:"
echo "=== Nginx logs (last 10 lines) ==="
docker logs r3e-webgui-nginx --tail 10 2>&1 || echo "Cannot get nginx logs"
echo ""
echo "=== API Service logs (last 10 lines) ==="
docker logs r3e-webgui-service --tail 10 2>&1 || echo "Cannot get service logs"
echo ""

echo "8. Connectivity Tests:"
echo "Testing localhost connections:"
echo -n "HTTP (80): "
curl -s -o /dev/null -w "%{http_code}" -m 5 http://localhost/ || echo "Failed"
echo ""
echo -n "HTTPS (443): "
curl -s -o /dev/null -w "%{http_code}" -m 5 -k https://localhost/ || echo "Failed"
echo ""
echo -n "API (8888): "
curl -s -o /dev/null -w "%{http_code}" -m 5 http://localhost:8888/health || echo "Failed"
echo ""

echo "9. Firewall Status:"
if command -v ufw &>/dev/null; then
    ufw status | grep -E "(80|443)" || echo "No firewall rules for web ports"
else
    echo "UFW not installed"
fi
echo ""

echo "10. DNS Resolution:"
echo -n "Resolving service.neoservicelayer.com: "
dig +short service.neoservicelayer.com || nslookup service.neoservicelayer.com | grep Address | tail -1
echo ""

echo "11. Environment Variables:"
if [ -f /opt/r3e-webgui/configs/.env ]; then
    echo "Environment file contents (sensitive data hidden):"
    grep -E "^[A-Z]" /opt/r3e-webgui/configs/.env | sed 's/=.*/=***/'
else
    echo "No .env file found"
fi
echo ""

echo "12. Recommended Actions:"
if ! docker ps | grep -q r3e-webgui; then
    echo -e "${YELLOW}• No containers running. Try: cd /opt/r3e-webgui/configs && docker-compose up -d${NC}"
fi
if ! [ -f /opt/r3e-webgui/ssl/fullchain.pem ]; then
    echo -e "${YELLOW}• SSL not configured. Run: /opt/r3e-webgui/scripts/setup-ssl.sh${NC}"
fi
if ! ss -tlnp | grep -q ":80"; then
    echo -e "${YELLOW}• Port 80 not listening. Check nginx container status${NC}"
fi

echo ""
echo "=== End of Diagnostics ==="