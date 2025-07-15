#!/bin/bash

echo "=== Quick Service Check ==="
echo ""

# Check containers
echo "1. Docker containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# Check nginx config mount
echo "2. Checking nginx config mount:"
docker exec r3e-webgui-nginx cat /etc/nginx/nginx.conf 2>/dev/null | head -20 || echo "Failed to read nginx config from container"
echo ""

# Test endpoints
echo "3. Testing endpoints:"
echo -n "GET http://localhost/ -> "
curl -s -o /dev/null -w "%{http_code}" http://localhost/
echo ""

echo -n "GET http://localhost/contracts/ -> "
curl -s -o /dev/null -w "%{http_code}" http://localhost/contracts/
echo ""

echo -n "GET http://localhost:8888/health -> "
curl -s -o /dev/null -w "%{http_code}" http://localhost:8888/health
echo ""

# Check if API service is running
echo ""
echo "4. API Service check:"
docker logs r3e-webgui-service --tail 5 2>&1 | grep -v "^$"

echo ""
echo "5. Nginx error log:"
docker exec r3e-webgui-nginx tail -5 /var/log/nginx/error.log 2>/dev/null || echo "No error log available"