# Installation Guide for R3E DevPack v1.0.2

## Quick Start

### 1. Install the CLI Tool
```bash
dotnet tool install -g R3E.Compiler.CSharp.Tool --version 1.0.2
```

### 2. Install Project Templates
```bash
dotnet new install R3E.SmartContract.Template::1.0.2
```

### 3. Create Your First Project
```bash
# Create a complete solution (NEW in v1.0.2!)
dotnet new solution -n MySmartContract
cd MySmartContract

# Or create individual contracts
dotnet new neocontractnep17 -n MyToken
dotnet new neocontractowner -n MyOwnableContract
dotnet new neocontractoracle -n MyOracleContract
```

## Verify Installation

### Check CLI Tool
```bash
rncc --version
# Should output: rncc 1.0.2
```

### List Available Templates
```bash
dotnet new list neo
# Should show:
# - Neo Smart Contract Solution (solution)
# - Neo Smart Contract - NEP-17 (neocontractnep17)
# - Neo Smart Contract - Oracle (neocontractoracle)
# - Neo Smart Contract - Owner (neocontractowner)
```

## Project Structure (Solution Template)

When you create a solution with `dotnet new solution -n MyProject`, you get:

```
MyProject/
├── MyProject.sln
├── src/
│   └── MyContract/
│       ├── MyContract.cs
│       └── MyContract.csproj
├── tests/
│   └── MyContract.Tests/
│       ├── MyContract.Tests.csproj
│       └── MyContractTests.cs
├── deploy/
│   └── Deploy/
│       ├── Deploy.csproj
│       ├── Program.cs
│       ├── DeploymentExamples.cs
│       ├── appsettings.json
│       └── wallets/
│           └── README.md
├── AddContract.ps1
├── add-contract.sh
├── deploy.ps1
├── deploy.sh
├── update.ps1
└── update.sh
```

## Building and Testing

### Build the Solution
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Compile Contract
```bash
rncc compile src/MyContract/MyContract.csproj
```

## Deployment

### Configure Network
Edit `deploy/Deploy/appsettings.json`:
```json
{
  "Network": {
    "Name": "testnet",
    "RpcUrl": "https://testnet1.neo.coz.io:443"
  }
}
```

### Run Deployment
```bash
cd deploy/Deploy
dotnet run
```

## Upgrading from Previous Versions

### From v1.0.1
```bash
# Update CLI tool
dotnet tool update -g R3E.Compiler.CSharp.Tool

# Reinstall templates
dotnet new uninstall R3E.SmartContract.Template
dotnet new install R3E.SmartContract.Template::1.0.2
```

### Update Package References
In your `.csproj` files, update to v1.0.2:
```xml
<PackageReference Include="R3E.SmartContract.Framework" Version="1.0.2" />
<PackageReference Include="R3E.SmartContract.Testing" Version="1.0.2" />
<PackageReference Include="R3E.SmartContract.Deploy" Version="1.0.2" />
```

## New Features in v1.0.2

### Azure Key Vault Support
```json
// appsettings.json
{
  "Security": {
    "KeyVaultUrl": "https://your-vault.vault.azure.net/",
    "KeyVaultAuthMethod": "DefaultAzureCredential",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id"
  }
}
```

### Configurable RPC Endpoints
```json
// appsettings.json
{
  "Network": {
    "MainnetRpcUrl": "https://custom-mainnet-rpc.com",
    "TestnetRpcUrl": "https://custom-testnet-rpc.com",
    "LocalRpcUrl": "http://localhost:50012"
  }
}
```

## Troubleshooting

### Solution Template Not Showing
If `dotnet new solution` doesn't work:
1. Ensure you have v1.0.2 installed: `dotnet new uninstall R3E.SmartContract.Template && dotnet new install R3E.SmartContract.Template::1.0.2`
2. Clear template cache: `dotnet new cache --clear`
3. List templates: `dotnet new list`

### Build Errors
1. Ensure .NET 9.0 SDK is installed: `dotnet --version`
2. Restore packages: `dotnet restore`
3. Clean and rebuild: `dotnet clean && dotnet build`

## Getting Help

- Documentation: https://r3edevpack.netlify.app
- GitHub Issues: https://github.com/r3e-network/r3e-devpack-dotnet/issues
- Community: Join the R3E Discord

## Next Steps

1. Read the [full documentation](https://r3edevpack.netlify.app)
2. Explore the [example contracts](https://github.com/r3e-network/r3e-devpack-dotnet/tree/r3e/examples)
3. Join the community and share your projects!