# R3E DevPack v1.0.2 Release Notes

## 🎉 Release Highlights

R3E DevPack v1.0.2 brings significant improvements focused on production readiness, complete template support, and enhanced security features.

## 📦 Installation

### Install Templates
```bash
dotnet new install R3E.SmartContract.Template::1.0.2
```

### Install CLI Tool
```bash
dotnet tool install -g R3E.Compiler.CSharp.Tool --version 1.0.2
```

## 🆕 What's New

### Solution Template Support
- **Fixed**: Solution template now properly appears in `dotnet new` list
- **Command**: `dotnet new solution -n MyProject` creates a complete Neo smart contract solution
- **Structure**: Includes contract project, test project, and deployment project

### Production-Ready Improvements
- ✅ Removed all TODO comments and placeholder implementations
- ✅ Implemented Azure Key Vault integration for secure credential management
- ✅ Added proper Neo address to script hash conversion
- ✅ Made all RPC URLs configurable via settings

### Security Enhancements
- **Azure Key Vault**: Full integration with multiple authentication methods
  - Managed Identity
  - Client Secret
  - Environment credentials
  - Default Azure credentials
- **Secure Credential Provider**: Production-ready implementation for managing sensitive data

### Code Quality
- Fixed test stability issues with artifact generation
- Improved error handling throughout the codebase
- Enhanced logging for better debugging
- Removed all hardcoded values in favor of configuration

## 🐛 Bug Fixes

- Fixed `ConcurrentSet.Add` compilation error in test cleanup
- Resolved test failures due to artifact generation timing
- Fixed missing solution template in package distribution
- Corrected debug info handling in security analyzers

## 📋 Breaking Changes

None - This release maintains backward compatibility with v1.0.1

## 🔧 Technical Details

### Updated Dependencies
- Azure.Security.KeyVault.Secrets: 4.7.0
- Azure.Identity: 1.14.0 (updated from 1.13.3)

### Template Changes
- Added `.template.config/template.json` for solution template
- Set shortName to `solution` for easier usage
- Configured proper post-actions for NuGet restore

## 📚 Usage Examples

### Create a New Solution
```bash
# Install the template
dotnet new install R3E.SmartContract.Template::1.0.2

# Create a new solution
dotnet new solution -n MySmartContract

# Navigate to the solution
cd MySmartContract

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Use Azure Key Vault
```json
// appsettings.json
{
  "Security": {
    "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
    "KeyVaultAuthMethod": "DefaultAzureCredential"
  }
}
```

### Configure RPC URLs
```json
// appsettings.json
{
  "Network": {
    "MainnetRpcUrl": "https://rpc10.n3.nspcc.ru:10331",
    "TestnetRpcUrl": "https://testnet1.neo.coz.io:443",
    "LocalRpcUrl": "http://localhost:50012",
    "DefaultRpcUrl": "http://localhost:10332"
  }
}
```

## 🚀 Next Steps

1. **Update your projects**: Run `dotnet tool update -g R3E.Compiler.CSharp.Tool`
2. **Try the solution template**: Create a full project structure with one command
3. **Explore security features**: Implement Key Vault integration for production deployments

## 📊 Package Versions

All packages have been updated to v1.0.2:
- R3E.SmartContract.Framework
- R3E.SmartContract.Template
- R3E.Compiler.CSharp
- R3E.Compiler.CSharp.Tool
- R3E.SmartContract.Testing
- R3E.SmartContract.Deploy
- R3E.Disassembler.CSharp
- R3E.SmartContract.Analyzer
- R3E.WebGUI.Service
- R3E.WebGUI.Deploy

## 🙏 Acknowledgments

Thanks to all contributors and the Neo community for their continued support!

---

For more information, visit: https://github.com/r3e-network/r3e-devpack-dotnet