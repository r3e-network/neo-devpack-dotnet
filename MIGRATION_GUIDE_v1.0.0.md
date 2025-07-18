# Migration Guide: R3E DevPack 0.x to 1.0.0

This guide helps you migrate your Neo smart contract projects from R3E DevPack 0.x versions to the new 1.0.0 release.

## Overview of Changes

### Major Improvements in v1.0.0
- New RNCC CLI tool with project scaffolding
- Template-based project generation
- Enhanced testing framework
- Improved deployment toolkit
- Better error messages and diagnostics

### Breaking Changes
- None! Version 1.0.0 maintains full backward compatibility with 0.x versions

## Migration Steps

### 1. Update Package References

Update all R3E package references in your `.csproj` files:

```xml
<!-- Old -->
<PackageReference Include="R3E.SmartContract.Framework" Version="0.0.5" />

<!-- New -->
<PackageReference Include="R3E.SmartContract.Framework" Version="1.0.0" />
```

### 2. Install the New RNCC CLI Tool

Uninstall old version (if installed):
```bash
dotnet tool uninstall -g R3E.Compiler.CSharp.Tool
```

Install new version:
```bash
dotnet tool install -g R3E.Compiler.CSharp.Tool --version 1.0.0
```

### 3. Update Build Scripts

If you have custom build scripts using `nccs` or direct compiler invocation, update them to use `rncc`:

```bash
# Old
nccs MyContract.csproj

# New
rncc MyContract.csproj
```

### 4. Take Advantage of New Features

#### Use Templates for New Contracts
```bash
# Create a complete solution with tests and deployment
rncc new MyNewContract --template=solution --author="Your Name"
```

#### Add dotnet-tools.json for Team Development
Create `.config/dotnet-tools.json` in your project root:
```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "r3e.compiler.csharp.tool": {
      "version": "1.0.0",
      "commands": ["rncc"]
    }
  }
}
```

Then team members can run:
```bash
dotnet tool restore
```

### 5. Update Test Projects

If using the testing framework, you may want to update your test base classes:

```csharp
// Ensure your test contract inherits from the correct interface
public abstract class MyContract : Neo.SmartContract.Testing.SmartContract, 
                                  Neo.SmartContract.Testing.IContractInfo
{
    // Contract methods
}
```

### 6. Review Deployment Scripts

The deployment toolkit has new features. Review your deployment scripts to take advantage of:
- Environment-specific configurations
- Multi-contract deployment support
- Improved error handling

## Common Issues and Solutions

### Issue: "rncc: command not found"
**Solution**: Ensure `$HOME/.dotnet/tools` is in your PATH:
```bash
export PATH="$HOME/.dotnet/tools:$PATH"
```

### Issue: Build errors after update
**Solution**: Clean and rebuild:
```bash
dotnet clean
dotnet restore
dotnet build
```

### Issue: Template types not recognized
**Solution**: The testing framework now requires explicit interface implementation. Update your test contract definitions.

## New Features to Explore

### 1. Project Templates
- `solution` - Complete project structure
- `nep17` - NEP-17 token template
- `oracle` - Oracle contract template
- `owner` - Ownable contract template

### 2. Enhanced CLI Commands
```bash
# List available templates
rncc templates

# Create with options
rncc new MyToken --template=nep17 --with-tests --git-init

# Get help
rncc --help
```

### 3. WebGUI Generation
RNCC now supports WebGUI generation for your contracts. Use the `-webgui` flag during compilation.

## Rollback Instructions

If you need to rollback to 0.x version:

1. Update package references back to 0.0.5
2. Reinstall old CLI tool version
3. Clean and rebuild your projects

## Getting Help

- **Documentation**: https://r3edevpack.netlify.app
- **GitHub Issues**: https://github.com/r3e-network/r3e-devpack-dotnet/issues
- **Discord**: Join R3E Network Discord for support

## Summary

The migration to v1.0.0 is straightforward:
1. ✅ Update package versions
2. ✅ Install new RNCC CLI tool
3. ✅ Update any build scripts
4. ✅ Optionally adopt new features

Welcome to R3E DevPack v1.0.0! 🚀