# R3E Neo Contract Compiler (RNCC) Usage Guide

## Overview
RNCC is the R3E Neo Contract Compiler for compiling C# smart contracts for the Neo blockchain.

## Basic Usage

```bash
# Compile a single contract file
./rncc-linux-x64 MyContract.cs

# Compile a project
./rncc-linux-x64 MyProject.csproj

# Compile all contracts in a directory
./rncc-linux-x64 /path/to/contracts/

# Show all available options
./rncc-linux-x64 --help
```

## Common Examples

### 1. Basic Contract Compilation
```bash
./rncc-linux-x64 HelloWorld.cs
```
This generates:
- `bin/sc/HelloWorld.nef` - The compiled contract
- `bin/sc/HelloWorld.manifest.json` - Contract manifest

### 2. Compile with Debug Information
```bash
./rncc-linux-x64 MyContract.cs -d Extended
```

### 3. Compile with Optimization
```bash
./rncc-linux-x64 MyContract.cs --optimize=All
```

### 4. Custom Output Directory
```bash
./rncc-linux-x64 MyContract.cs -o ./build
```

### 5. Generate All Artifacts
```bash
./rncc-linux-x64 MyContract.cs --generate-artifacts=All --assembly
```
This generates:
- NEF and manifest files
- Assembly files (.asm, .nef.txt)
- Source code artifacts

### 6. Generate WebGUI
```bash
./rncc-linux-x64 MyContract.cs --generate-webgui
```

### 7. Full Production Build
```bash
./rncc-linux-x64 MyContract.cs \
  --optimize=All \
  -d Extended \
  --generate-artifacts=All \
  --security-analysis \
  --generate-interface \
  --assembly
```

## Command-Line Options

### Input
- **paths** (positional) - Path to .cs, .csproj, .sln file or directory

### Output Options
- **-o, --output** - Output directory (default: bin/sc)
- **--base-name** - Base name for output files

### Compilation Options
- **--optimize** - Optimization level: None, Basic, Experimental, All
- **--checked** - Check for overflow/underflow
- **--no-inline** - Disable inline code
- **--nullable** - Nullable analysis mode

### Debug Options
- **-d, --debug** - Debug level: None, Strict, Extended

### Generation Options
- **--assembly** - Generate assembly files
- **--generate-artifacts** - Artifact generation: None, Source, Library, All
- **--security-analysis** - Perform security analysis
- **--generate-interface** - Generate interface file
- **--generate-plugin** - Generate Neo N3 plugin
- **--generate-webgui** - Generate interactive web GUI

### WebGUI Deployment
- **--deploy-webgui** - Deploy WebGUI to R3E service
- **--contract-address** - Deployed contract address
- **--network** - Network: testnet, mainnet
- **--deployer-address** - Deployer's address

## Example Contract

Create a simple `HelloWorld.cs`:

```csharp
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;

namespace HelloWorld
{
    public class HelloWorld : SmartContract
    {
        public static string SayHello(string name)
        {
            return $"Hello, {name}!";
        }
    }
}
```

Compile it:
```bash
./rncc-linux-x64 HelloWorld.cs
```

## Troubleshooting

If you see "No .cs file is found", make sure to:
1. Provide a path to a contract file: `./rncc-linux-x64 path/to/contract.cs`
2. Run from a directory containing contracts
3. Use `--help` to see all options

## Platform-Specific Binaries

- **Windows**: `rncc-win-x64.exe`
- **Linux**: `rncc-linux-x64`
- **macOS Intel**: `rncc-macos-x64`
- **macOS Apple Silicon**: `rncc-macos-arm64`

All binaries are self-contained and don't require .NET Runtime installation.