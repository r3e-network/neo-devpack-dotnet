# RNCC - R3E Neo Contract Compiler

[![NuGet](https://img.shields.io/nuget/v/R3E.Compiler.CSharp.Tool.svg)](https://www.nuget.org/packages/R3E.Compiler.CSharp.Tool/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/r3e-network/r3e-devpack-dotnet/blob/r3e/LICENSE)

RNCC (R3E Neo Contract Compiler) is a professional command-line tool for compiling Neo smart contracts with advanced features and comprehensive solution templates.

## Installation

Install RNCC as a global .NET tool:

```bash
dotnet tool install -g R3E.Compiler.CSharp.Tool
```

After installation, the `rncc` command will be available globally.

## Quick Start

### Create a New Contract Solution

```bash
# Create a complete contract solution from template
rncc new MyContract --template=solution --author="Your Name" --email="your@email.com"

# Navigate to your contract
cd MyContract

# Build the solution
rncc build

# Run tests
dotnet test
```

### Compile a Contract

```bash
# Compile a C# file
rncc MyContract.cs

# Compile a project
rncc MyContract.csproj

# Compile a solution
rncc MyContract.sln

# Compile with optimization
rncc MyContract.cs --optimize=All
```

## Features

- 🚀 **Solution Templates** - Create complete contract solutions with testing and deployment projects
- 🔧 **Advanced Compilation** - Optimizations, security analysis, and debug information
- 🌐 **WebGUI Generation** - Automatically generate web interfaces for your contracts
- 🔌 **Plugin Generation** - Create Neo N3 plugins from your contracts
- 📊 **Security Analysis** - Built-in vulnerability scanning
- 🎯 **Multiple Targets** - Compile files, projects, solutions, or entire directories

## Command Overview

### Creating New Contracts

```bash
rncc new <name> [options]

Options:
  --template <template>    Template type: solution, basic, nep17, nep11, defi, multisig
  --author <name>          Author name
  --email <email>          Author email
  --with-tests            Include test project
  --with-deploy-scripts   Include deployment scripts
  --git-init              Initialize git repository
```

### Building Contracts

```bash
rncc build [options]           # Build solution in current directory
rncc <path> [options]          # Compile specific file/project/solution

Options:
  -o, --output <path>          Output directory
  --optimize <level>           Optimization: None, Basic, Experimental, All
  --debug <level>              Debug info: None, Strict, Extended
  --generate-webgui            Generate interactive WebGUI
  --generate-plugin            Generate Neo N3 plugin
  --security-analysis          Run security analysis
  --generate-artifacts <mode>  Generate artifacts: Source, Library, All
```

### Deployment

```bash
rncc deploy [options]

Options:
  --network <network>          Target network: testnet, mainnet
  --config <path>              Deployment configuration file
```

## Templates

### Solution Template (Recommended)
Creates a complete solution with:
- Contract project
- Testing project  
- Deployment scripts
- Documentation

```bash
rncc new MyDApp --template=solution
```

### NEP-17 Token
Creates a fungible token contract:

```bash
rncc new MyToken --template=nep17 --token-name="My Token" --token-symbol="MYT"
```

### NEP-11 NFT
Creates a non-fungible token contract:

```bash
rncc new MyNFT --template=nep11
```

## Advanced Usage

### WebGUI Generation

Generate an interactive web interface for your contract:

```bash
rncc MyContract.cs --generate-webgui --network=testnet
```

### Security Analysis

Run comprehensive security checks:

```bash
rncc MyContract.cs --security-analysis --optimize=All
```

### Plugin Generation

Generate a Neo N3 plugin:

```bash
rncc MyContract.cs --generate-plugin
```

## Output Files

After compilation, you'll find:
- `*.nef` - Compiled contract bytecode
- `*.manifest.json` - Contract manifest with ABI
- `*.nefdbgnfo` - Debug information (if enabled)
- `*.asm` - Assembly listing (if requested)
- `webgui/` - Generated web interface (if enabled)

## Documentation

- [Full Documentation](https://r3edevpack.netlify.app)
- [Getting Started Guide](https://r3edevpack.netlify.app/docs/getting-started.html)
- [Compiler Reference](https://r3edevpack.netlify.app/docs/compiler-reference.html)
- [GitHub Repository](https://github.com/r3e-network/r3e-devpack-dotnet)

## Support

- [GitHub Issues](https://github.com/r3e-network/r3e-devpack-dotnet/issues)
- [Discord Community](https://discord.gg/r3e)

## License

MIT License - see [LICENSE](https://github.com/r3e-network/r3e-devpack-dotnet/blob/r3e/LICENSE) for details.