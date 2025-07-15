# R3E WebGUI Service Deployment Checklist

## Pre-Deployment Requirements

- [ ] Root or sudo access on target server
- [ ] Domain: service.neoservicelayer.com
- [ ] Server IP address for DNS configuration
- [ ] Email: jimmy@r3e.network (for SSL certificates)

## Deployment Steps

### 1. Connect to Server
```bash
ssh root@your-server-ip
```

### 2. Run Setup Script
Based on review, use the hybrid approach (setup-contracts-service.sh):
```bash
curl -sSL https://raw.githubusercontent.com/r3e-network/r3e-devpack-dotnet/r3e/src/R3E.WebGUI.Service/setup-contracts-service.sh | sudo bash
```

This will:
- Install Docker and Docker Compose
- Set up SQL Server database
- Configure nginx reverse proxy
- Create systemd services
- Configure firewall rules
- Set up the service at `/contracts` path

### 3. Configure DNS
Add the following DNS record:
- A Record: `service.neoservicelayer.com` → `YOUR_SERVER_IP`

Wait for DNS propagation (usually 5-30 minutes).

### 4. Set up SSL Certificate
After DNS is configured:
```bash
/opt/r3e-webgui/scripts/setup-letsencrypt.sh
```

### 5. Verify Deployment
Run the check script:
```bash
/opt/r3e-webgui/scripts/check-service.sh
```

## Service URLs After Deployment

- Landing Page: https://service.neoservicelayer.com/contracts
- API Documentation: https://service.neoservicelayer.com/contracts/swagger
- API Endpoints: https://service.neoservicelayer.com/contracts/api/
- Health Check: https://service.neoservicelayer.com/contracts/health
- Contract WebGUIs: https://service.neoservicelayer.com/contracts/{contract-name}

## Post-Deployment

### Deploy a Contract WebGUI
```bash
/opt/r3e-webgui/scripts/deploy-contract-webgui.sh <contract-address> <contract-name> [network]
```

Example:
```bash
/opt/r3e-webgui/scripts/deploy-contract-webgui.sh 0x123... mytoken mainnet
```

### Service Management Commands
- View logs: `/opt/r3e-webgui/scripts/logs.sh`
- Check status: `/opt/r3e-webgui/scripts/status.sh`
- Restart: `/opt/r3e-webgui/scripts/restart.sh`
- Update: `/opt/r3e-webgui/scripts/update.sh`

## Troubleshooting

If issues arise:
1. Run debug script: `sudo /opt/r3e-webgui/scripts/debug-service.sh`
2. Check nginx config: `sudo /opt/r3e-webgui/scripts/fix-nginx-config.sh`
3. View container logs: `docker logs r3e-webgui-service`
4. Check nginx logs: `docker logs r3e-webgui-nginx`

## Security Notes

- Firewall configured to allow ports 80, 443, and 22
- SQL Server is containerized and not exposed externally
- API service runs on internal Docker network
- SSL/TLS will be configured via Let's Encrypt