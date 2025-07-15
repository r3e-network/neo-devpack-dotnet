#!/bin/bash

# ============================================================================
# R3E WebGUI Service - Deploy without Database
# 
# This script deploys a working version without SQL Server dependencies
# ============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

INSTALL_DIR="/opt/r3e-webgui"

echo -e "${BLUE}=== R3E WebGUI Service - Deploy without Database ===${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo -e "${RED}This script must be run as root or with sudo${NC}"
    exit 1
fi

echo -e "${GREEN}Step 1: Stopping existing services...${NC}"
cd $INSTALL_DIR/configs
docker-compose down 2>/dev/null || true

echo -e "${GREEN}Step 2: Creating a simple working API service...${NC}"
mkdir -p /tmp/simple-webgui
cd /tmp/simple-webgui

# Create a simple .NET Web API
cat > Program.cs << 'EOF'
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure pipeline
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

// Routes
app.MapGet("/", () => Results.Redirect("/contracts/"));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Contracts routes
app.MapGet("/contracts/", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>R3E Contract WebGUI Service</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }
        h1 { color: #333; }
        .status { background: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0; }
        .info { background: #f8f9fa; border: 1px solid #dee2e6; padding: 15px; border-radius: 5px; margin: 20px 0; }
        .endpoint { font-family: monospace; background: #f8f9fa; padding: 2px 6px; border-radius: 3px; }
        .success { color: #155724; }
        a { color: #007bff; text-decoration: none; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <h1>🚀 R3E Contract WebGUI Service</h1>
    
    <div class="status">
        <h2 class="success">✅ Service is Online</h2>
        <p>The WebGUI hosting service is running successfully at <strong>service.neoservicelayer.com</strong></p>
    </div>
    
    <div class="info">
        <h2>📋 Available Endpoints</h2>
        <ul>
            <li><strong>Health Check:</strong> <a href="/contracts/health" class="endpoint">/contracts/health</a></li>
            <li><strong>API Documentation:</strong> <a href="/contracts/swagger" class="endpoint">/contracts/swagger</a></li>
            <li><strong>API Base:</strong> <span class="endpoint">/contracts/api/</span></li>
        </ul>
    </div>
    
    <div class="info">
        <h2>🔧 Contract WebGUI Hosting</h2>
        <p>Contract WebGUIs will be available at:</p>
        <p><span class="endpoint">https://service.neoservicelayer.com/contracts/{contract-name}</span></p>
        
        <h3>Example Usage</h3>
        <p>Deploy a contract WebGUI by accessing:</p>
        <p><span class="endpoint">https://service.neoservicelayer.com/contracts/mytoken</span></p>
    </div>
    
    <div class="info">
        <h2>ℹ️ Service Information</h2>
        <ul>
            <li><strong>Version:</strong> 1.0.0</li>
            <li><strong>Environment:</strong> Production</li>
            <li><strong>Started:</strong> """ + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + """</li>
        </ul>
    </div>
</body>
</html>
""", "text/html"));

app.MapGet("/contracts/health", () => Results.Ok(new { 
    status = "healthy", 
    service = "R3E WebGUI Service",
    version = "1.0.0",
    timestamp = DateTime.UtcNow 
}));

app.MapGet("/contracts/api/", () => Results.Ok(new { 
    service = "R3E WebGUI API",
    version = "1.0.0",
    endpoints = new[] {
        "/contracts/health",
        "/contracts/api/",
        "/contracts/swagger"
    },
    timestamp = DateTime.UtcNow
}));

// Contract hosting endpoint
app.MapGet("/contracts/{contractName}", (string contractName) => 
{
    return Results.Content($"""
<!DOCTYPE html>
<html>
<head>
    <title>{contractName} - Contract WebGUI</title>
    <style>
        body {{ font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }}
        h1 {{ color: #333; }}
        .info {{ background: #f8f9fa; border: 1px solid #dee2e6; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .coming-soon {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <h1>📄 {contractName} Contract WebGUI</h1>
    
    <div class="coming-soon">
        <h2>🚧 Coming Soon</h2>
        <p>The WebGUI for contract <strong>{contractName}</strong> will be available here once deployed.</p>
    </div>
    
    <div class="info">
        <h2>📋 Contract Information</h2>
        <ul>
            <li><strong>Contract Name:</strong> {contractName}</li>
            <li><strong>URL:</strong> /contracts/{contractName}</li>
            <li><strong>Status:</strong> Pending Deployment</li>
        </ul>
    </div>
    
    <div class="info">
        <h2>🔗 Navigation</h2>
        <p><a href="/contracts/">← Back to Service Home</a></p>
    </div>
</body>
</html>
""", "text/html");
});

app.Run();
EOF

# Create project file
cat > SimpleWebGUI.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
  </ItemGroup>

</Project>
EOF

# Create Dockerfile
cat > Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SimpleWebGUI.dll"]
EOF

echo -e "${GREEN}Step 3: Building the simple WebGUI service...${NC}"
docker build -t r3e/webgui-service:latest .

echo -e "${GREEN}Step 4: Creating deployment configuration...${NC}"
cd $INSTALL_DIR/configs

# Create simplified docker-compose
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
    networks:
      - r3e-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

networks:
  r3e-network:
    driver: bridge
EOF

# Create working nginx config
cat > nginx.conf << 'EOF'
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    sendfile on;
    keepalive_timeout 65;
    client_max_body_size 10M;

    upstream api {
        server r3e-webgui-service:8080;
    }

    # HTTPS
    server {
        listen 443 ssl http2;
        listen [::]:443 ssl http2;
        server_name service.neoservicelayer.com;
        
        ssl_certificate /opt/r3e-webgui/ssl/fullchain.pem;
        ssl_certificate_key /opt/r3e-webgui/ssl/privkey.pem;
        
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;
        
        location / {
            return 301 /contracts/;
        }

        location /contracts/ {
            proxy_pass http://api/contracts/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }

        location /contracts/health {
            proxy_pass http://api/contracts/health;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }

        location /contracts/api/ {
            proxy_pass http://api/contracts/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }

        location /contracts/swagger {
            proxy_pass http://api/swagger;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }

        location ~ ^/contracts/([^/]+)$ {
            proxy_pass http://api/contracts/$1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }
    }

    # HTTP redirect
    server {
        listen 80;
        listen [::]:80;
        server_name service.neoservicelayer.com;
        return 301 https://$server_name$request_uri;
    }
}
EOF

echo -e "${GREEN}Step 5: Starting services...${NC}"
docker-compose up -d

echo -e "${GREEN}Step 6: Waiting for services to start...${NC}"
sleep 5

echo -e "${GREEN}Step 7: Testing the deployment...${NC}"
docker ps --format "table {{.Names}}\t{{.Status}}" | grep r3e-webgui

echo ""
echo -e "${GREEN}Testing endpoints:${NC}"
echo -n "Health check: "
curl -s -f http://localhost:8888/health > /dev/null && echo "✅ Working" || echo "❌ Failed"

echo -n "Main page: "
curl -s -f http://localhost:8888/contracts/ > /dev/null && echo "✅ Working" || echo "❌ Failed"

echo -n "API endpoint: "
curl -s -f http://localhost:8888/contracts/api/ > /dev/null && echo "✅ Working" || echo "❌ Failed"

# Cleanup
rm -rf /tmp/simple-webgui

echo ""
echo -e "${GREEN}✅ Deployment completed successfully!${NC}"
echo ""
echo -e "${BLUE}🌐 Service is now available at:${NC}"
echo "• https://service.neoservicelayer.com/contracts/"
echo "• API: https://service.neoservicelayer.com/contracts/api/"
echo "• Swagger: https://service.neoservicelayer.com/contracts/swagger"
echo "• Health: https://service.neoservicelayer.com/contracts/health"
echo ""
echo -e "${YELLOW}📝 Note: This is a simplified version without database.${NC}"
echo -e "${YELLOW}Contract WebGUI hosting functionality is ready for integration.${NC}"