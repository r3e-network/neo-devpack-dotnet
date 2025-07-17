# Neo N3 Smart Contract DevPack for .NET (R3E Community Edition)

**Version 0.0.4**

<p align="center">
  <a href="https://github.com/r3e-network/">
    <img src="https://avatars.githubusercontent.com/u/187460041?s=200&v=4" width="250px" alt="r3e-logo">
  </a>
</p>

<p align="center">
  <strong>📚 Documentation:</strong> <a href="https://r3edevpack.netlify.app">https://r3edevpack.netlify.app</a>
</p>

<p align="center">
  <a href="https://github.com/r3e-network/r3e-devpack-dotnet/blob/r3e/LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License">
  </a>
  <a href="https://www.nuget.org/packages/R3E.SmartContract.Framework">
    <img src="https://img.shields.io/nuget/v/R3E.SmartContract.Framework.svg" alt="NuGet Version">
  </a>
  <a href="https://www.nuget.org/packages/R3E.SmartContract.Framework">
    <img src="https://img.shields.io/nuget/dt/R3E.SmartContract.Framework.svg" alt="NuGet Downloads">
  </a>
  <a href="https://github.com/r3e-network/r3e-devpack-dotnet/releases">
    <img src="https://img.shields.io/github/v/release/r3e-network/r3e-devpack-dotnet?include_prereleases" alt="GitHub Release">
  </a>
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4" alt=".NET Version">
</p>

## Overview

This is the R3E Community Edition of the Neo N3 Smart Contract DevPack for .NET - a comprehensive suite of development tools for building smart contracts and decentralized applications (dApps) on the Neo blockchain platform using .NET. This toolkit enables developers to write, compile, test, and deploy Neo smart contracts using C# and other .NET languages.

The R3E Community maintains this fork of the official Neo DevPack with the goal of providing enhanced features and tools for the Neo developer community. All contracts compiled with this DevPack run on the Neo blockchain.

## Available Packages

| Package | Version | Description |
|---------|---------|-------------|
| [R3E.SmartContract.Framework](https://www.nuget.org/packages/R3E.SmartContract.Framework/) | ![NuGet](https://img.shields.io/nuget/v/R3E.SmartContract.Framework.svg) | Core framework for Neo smart contracts (R3E edition) |
| [R3E.SmartContract.Testing](https://www.nuget.org/packages/R3E.SmartContract.Testing/) | ![NuGet](https://img.shields.io/nuget/v/R3E.SmartContract.Testing.svg) | Testing framework for Neo contracts |
| [R3E.Compiler.CSharp](https://www.nuget.org/packages/R3E.Compiler.CSharp/) | ![NuGet](https://img.shields.io/nuget/v/R3E.Compiler.CSharp.svg) | C# to Neo VM bytecode compiler with plugin & WebGUI generation |
| [R3E.Compiler.CSharp.Tool](https://www.nuget.org/packages/R3E.Compiler.CSharp.Tool/) | ![NuGet](https://img.shields.io/nuget/v/R3E.Compiler.CSharp.Tool.svg) | Global CLI tool for compiling Neo contracts |
| [R3E.SmartContract.Analyzer](https://www.nuget.org/packages/R3E.SmartContract.Analyzer/) | ![NuGet](https://img.shields.io/nuget/v/R3E.SmartContract.Analyzer.svg) | Roslyn analyzers for Neo contract development |
| [R3E.SmartContract.Template](https://www.nuget.org/packages/R3E.SmartContract.Template/) | ![NuGet](https://img.shields.io/nuget/v/R3E.SmartContract.Template.svg) | Project templates for Neo contracts |
| [R3E.Disassembler.CSharp](https://www.nuget.org/packages/R3E.Disassembler.CSharp/) | ![NuGet](https://img.shields.io/nuget/v/R3E.Disassembler.CSharp.svg) | Neo VM bytecode disassembler |
| [R3E.SmartContract.Deploy](https://www.nuget.org/packages/R3E.SmartContract.Deploy/) | ![NuGet](https://img.shields.io/nuget/v/R3E.SmartContract.Deploy.svg) | Deployment toolkit for Neo contracts |
| [R3E.WebGUI.Deploy](https://www.nuget.org/packages/R3E.WebGUI.Deploy/) | ![NuGet](https://img.shields.io/nuget/v/R3E.WebGUI.Deploy.svg) | WebGUI deployment and hosting tools |
| [R3E.WebGUI.Service](https://www.nuget.org/packages/R3E.WebGUI.Service/) | ![NuGet](https://img.shields.io/nuget/v/R3E.WebGUI.Service.svg) | WebGUI hosting service |

## Components

The R3E Community Edition Neo DevPack consists of several key components:

### R3E.SmartContract.Framework

The framework provides the necessary libraries and APIs for writing Neo smart contracts in C#. It includes:

- Base classes and interfaces for smart contract development
- Neo blockchain API wrappers
- Standard contract templates (NEP-17, NEP-11, etc.)
- Utilities for common blockchain operations

### R3E.Compiler.CSharp (Neo Contract Compiler - R3E Edition)

A specialized compiler that translates C# code into Neo Virtual Machine (NeoVM) bytecode. Features include:

- Full C# language support for smart contract development
- Optimization for gas efficiency
- Debug information generation
- Source code generation for contract testing
- Contract interface generation
- **WebGUI generation for interactive contract interfaces**
  - Automatic HTML/CSS/JS generation from contract manifest
  - Multiple template support (Standard, NEP-17, NEP-11)
  - Real-time blockchain data monitoring
  - Wallet integration (NeoLine, O3, WalletConnect)
  - Method invocation with gas estimation
- **Neo N3 plugin generation for CLI integration**
  - Complete plugin project generation
  - CLI command wrappers for all contract methods
  - Type-safe parameter handling
  - Integration with neo-cli
- **Available as both a CLI tool and a NuGet library for programmatic compilation**

[![NuGet](https://img.shields.io/nuget/v/R3E.Compiler.CSharp.svg)](https://www.nuget.org/packages/R3E.Compiler.CSharp/)

### R3E.SmartContract.Testing

A testing framework for Neo smart contracts that allows:

- Unit testing of contracts without deployment
- Storage simulation
- Mock native contracts
- Blockchain state simulation
- Gas consumption tracking
- Code coverage analysis

### R3E.Disassembler.CSharp

A tool for disassembling Neo VM bytecode back to readable C# code.

### R3E.SmartContract.Analyzer

Code analyzers and linting tools to help write secure and efficient Neo contracts.

### R3E.SmartContract.Template

Project templates for creating new Neo smart contracts with the proper structure and configurations.

### R3E.SmartContract.Deploy

A comprehensive deployment toolkit that simplifies the process of deploying and updating smart contracts on Neo networks. Features include:

- Simplified API for contract deployment and updates
- Automatic wallet and configuration management
- Support for multiple Neo networks (MainNet, TestNet, local)
- Contract compilation and deployment in one step
- Multi-contract deployment from manifest files
- **Contract update capabilities using Neo N3's _deploy method**
- WIF key support for direct transaction signing
- Integration with Neo Express for local development
- Comprehensive error handling and retry mechanisms
- Deployment record tracking and history
- Health check services for monitoring

### R3E.WebGUI.Deploy

Deployment tools for WebGUI generation and hosting:

- Generate interactive web interfaces from contract manifests
- Deploy WebGUIs to R3E hosting service
- Support for custom templates and themes
- Integration with deployment toolkit

### R3E.WebGUI.Service

Professional WebGUI hosting service for Neo smart contracts:

- **Multiple hosting options**:
  - Path-based hosting: `service.neoservicelayer.com/contracts/{name}`
  - Subdomain-based hosting: `{name}.service.neoservicelayer.com`
- **JSON-based configuration** with signature authentication
- **Plugin upload support** with SHA256 verification
- Real-time contract data monitoring
- RESTful API with comprehensive documentation
- SQL Server backend for scalability
- Docker support for easy deployment
- SSL/TLS support with Let's Encrypt integration
- Rate limiting and security headers
- Health monitoring endpoints

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or later
- [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) (optional but recommended)
- [Make](https://www.gnu.org/software/make/) (optional, for build automation)

### Quick Start with Makefile

This project includes a comprehensive Makefile for easy development:

```bash
# Show all available targets
make help

# Quick development workflow
make dev                # Clean, build, and test
make all               # Complete build pipeline

# Individual operations
make build             # Build all projects
make test              # Run all tests
make pack              # Create NuGet packages
make clean             # Clean build artifacts
```

For Windows users without Make:
```cmd
# Use the batch wrapper
make.bat help
make.bat dev
```

### Installation

#### Install from NuGet

```shell
# Install the core framework
dotnet add package R3E.SmartContract.Framework --version 0.0.4

# Install the testing framework
dotnet add package R3E.SmartContract.Testing --version 0.0.4

# Install the compiler library
dotnet add package R3E.Compiler.CSharp --version 0.0.4

# Install the deployment toolkit
dotnet add package R3E.SmartContract.Deploy --version 0.0.4

# Install the compiler global tool
dotnet tool install -g R3E.Compiler.CSharp.Tool --version 0.0.4

# Install project templates
dotnet new install R3E.SmartContract.Template::0.0.4
```

#### Build from Source

Clone the repository with submodules:

```shell
git clone --recurse-submodules https://github.com/r3e-network/r3e-devpack-dotnet.git
cd neo-devpack-dotnet
```

### Build

```shell
# Using Makefile (recommended)
make build

# Or using dotnet directly
dotnet build
```

### Run Tests

```shell
# Using Makefile (recommended)
make test

# Or using dotnet directly
dotnet test
```

## Usage

### Creating a New Smart Contract

#### Using the Template (Recommended)

```shell
# Create a new Neo smart contract project
dotnet new r3e-contract -n MyContract

# Navigate to the project
cd MyContract

# Build the contract
dotnet build
```

#### Manual Creation

1. Create a new class library project targeting .NET 9.0 or later
2. Add a reference to the R3E.SmartContract.Framework package
3. Create a class that inherits from `SmartContract`
4. Implement your contract logic
5. Compile using the R3E.Compiler.CSharp

Example:

```csharp
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

public class HelloWorldContract : SmartContract
{
    [Safe]
    public static string SayHello(string name)
    {
        return $"Hello, {name}!";
    }
}
```

### Compiling a Smart Contract

The R3E Community Edition compiler translates your C# smart contract into Neo VM bytecode, which can then be deployed to the Neo blockchain. There are several ways to compile your contract:

#### Using the Global Tool (Recommended)

Install the compiler as a global tool:

```shell
dotnet tool install -g R3E.Compiler.CSharp.Tool
```

Then compile your contract:

```shell
# The R3E compiler still compiles to Neo VM bytecode
r3e-compiler path/to/your/contract.csproj
```

#### Using the Library Package

Add the compiler library to your project:

```shell
dotnet add package R3E.Compiler.CSharp
```

Then compile programmatically in your code. See [compiler library usage documentation](docs/compiler-library-usage.md) for details.

#### Basic Compilation (From Source)

```shell
dotnet run --project src/R3E.Compiler.CSharp/R3E.Compiler.CSharp.csproj -- path/to/your/contract.csproj
```

This command will compile your contract and generate the following files in the `bin/sc` directory of your project:
- `YourContract.nef`: The compiled bytecode file that is deployed to the blockchain
- `YourContract.manifest.json`: Contract manifest containing metadata and ABI information

#### Compilation Options

You can customize the compilation process with various options:

```shell
# For bash/zsh (macOS/Linux)
dotnet run --project src/R3E.Compiler.CSharp/R3E.Compiler.CSharp.csproj -- \
    path/to/your/contract.csproj \
    -o output/directory \
    --base-name MyContract \
    --debug \
    --assembly \
    --optimize=All \
    --generate-interface

# For Windows Command Prompt
dotnet run --project src/R3E.Compiler.CSharp/R3E.Compiler.CSharp.csproj -- ^
    path/to/your/contract.csproj ^
    -o output/directory ^
    --base-name MyContract ^
    --debug ^
    --assembly ^
    --optimize=All ^
    --generate-interface
```

#### Working with Single Files or Directories

The compiler can also process individual `.cs` files or entire directories:

```shell
# Compile a single file
dotnet run --project src/R3E.Compiler.CSharp/R3E.Compiler.CSharp.csproj -- path/to/Contract.cs

# Compile all contracts in a directory
dotnet run --project src/R3E.Compiler.CSharp/R3E.Compiler.CSharp.csproj -- path/to/contract/directory
```

#### Compiler Command Reference

The R3E C# compiler supports the following options:

| Option | Description |
|--------|-------------|
| `-o, --output` | Specifies the output directory for compiled files |
| `--base-name` | Specifies the base name of the output files (overrides contract name) |
| `--debug` | Generates debug information (default is `Extended`) |
| `--assembly` | Generates a readable assembly (.asm) file |
| `--generate-artifacts` | Generates additional artifacts for contract interaction (Source, Library, or All) |
| `--generate-interface` | Generates a C# interface file for the contract (useful for type-safe interaction) |
| `--security-analysis` | Performs security analysis on the compiled contract |
| `--optimize` | Specifies the optimization level (None, Basic, All, Experimental) |
| `--no-inline` | Disables method inlining during compilation |
| `--nullable` | Sets the nullable context options (Disable, Enable, Annotations, Warnings) |
| `--checked` | Enables overflow checking for arithmetic operations |
| `--address-version` | Sets the address version for script hash calculations |
| `--generate-webgui` | Generates interactive WebGUI for the contract |
| `--generate-plugin` | Generates Neo CLI plugin for the contract |

### Testing a Smart Contract

The R3E Community Edition includes a comprehensive testing framework specifically designed for Neo smart contracts. Here's how to create unit tests for your contracts:

```csharp
using Neo.SmartContract.Testing;
using Neo.SmartContract.Testing.TestingStandards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Example.SmartContract.MyContract.UnitTests
{
    [TestClass]
    public class MyContractTests : TestBase<MyContract>
    {
        [TestInitialize]
        public void TestSetup()
        {
            // The testing framework automatically compiles and loads your contract
            TestInitialize();
        }

        [TestMethod]
        public void TestMethod()
        {
            // Access contract properties and methods through the Contract property
            var result = Contract.MyMethod("parameter");
            Assert.AreEqual("Expected result", result);

            // Test storage changes after operations
            var storedValue = Storage.Get(Contract.Hash, "myKey");
            Assert.AreEqual("Expected storage value", storedValue);

            // Verify emitted events
            var notifications = Notifications;
            Assert.AreEqual(1, notifications.Count);
            Assert.AreEqual("ExpectedEvent", notifications[0].EventName);
        }
    }
}
```

#### Key Testing Features

1. **TestBase<T> Class**: Provides a base class for contract testing with access to the contract, storage, and notifications.

2. **Automatic Artifact Generation**: The testing framework automatically compiles your contracts and generates testing artifacts.

3. **Direct Contract Interaction**: Access contract properties and methods directly through the strongly-typed `Contract` property.

4. **Storage Simulation**: Test persistent storage operations without deploying to a blockchain.

5. **Event Verification**: Validate that your contract emits the expected events.

6. **Gas Consumption Analysis**: Track and analyze GAS costs of operations:

```csharp
[TestMethod]
public void TestGasConsumption()
{
    // Record initial gas
    var initialGas = Engine.GasConsumed;

    // Execute contract operation
    Contract.ExpensiveOperation();

    // Check gas consumption
    var gasUsed = Engine.GasConsumed - initialGas;
    Console.WriteLine($"Gas used: {gasUsed}");
    Assert.IsTrue(gasUsed < 100_000_000, "Operation used too much gas");
}
```

#### Setting Up the Test Project

1. Create a new test project for your contract
2. Add references to the R3E.SmartContract.Testing package and your contract project
3. Create a test class that inherits from TestBase<T>
4. Implement the TestSetup method to compile and initialize the contract
5. Write test methods for each contract feature or scenario

### Generating WebGUI for Your Contract

The R3E compiler can automatically generate an interactive web interface for your smart contract:

```csharp
using Neo.Compiler;

// Compile and generate WebGUI
var engine = new CompilationEngine();
var contexts = engine.CompileProject("path/to/contract.csproj");

foreach (var context in contexts)
{
    var result = context.GenerateWebGui("./webgui-output", new WebGuiOptions
    {
        RpcEndpoint = "https://test1.neo.coz.io:443",
        NetworkMagic = 894710606, // TestNet
        DarkTheme = true,
        IncludeMethodInvocation = true,
        IncludeEventMonitoring = true,
        IncludeWalletConnection = true
    });
    
    if (result.Success)
    {
        Console.WriteLine($"WebGUI generated at: {result.OutputDirectory}");
    }
}
```

### Generating Neo CLI Plugin

Generate a complete Neo CLI plugin for your contract:

```csharp
using Neo.Compiler;

// Generate plugin after compilation
var manifest = context.CreateManifest();
var contractHash = context.GetContractHash();

ContractPluginGenerator.GeneratePlugin(
    contractName: "MyContract",
    manifest: manifest,
    contractHash: contractHash,
    outputPath: "./plugins"
);
```

### Deploying Your Contract

Use the R3E deployment toolkit for simplified contract deployment:

```csharp
using R3E.SmartContract.Deploy;

// Create deployment toolkit
var toolkit = new DeploymentToolkit()
    .SetNetwork("testnet")
    .SetWifKey("your-wif-key");

// Deploy contract
var result = await toolkit.DeployAsync("path/to/contract.csproj");

if (result.Success)
{
    Console.WriteLine($"Contract deployed at: {result.ContractHash}");
    Console.WriteLine($"Transaction: {result.TransactionHash}");
}
```

### Uploading WebGUI to R3E Service

Deploy your WebGUI to the R3E hosting service for public access:

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

// Prepare deployment request
var request = new
{
    ContractAddress = "0x1234567890abcdef1234567890abcdef12345678",
    ContractName = "MyContract",
    Network = "testnet",
    DeployerAddress = "NXXxXXxXXxXXxXXxXXxXXxXXxXXxXXxXXx",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Signature = "signature-hex",
    PublicKey = "public-key-hex"
};

// Deploy to R3E WebGUI Service
var httpClient = new HttpClient();
var response = await httpClient.PostAsync(
    "https://service.neoservicelayer.com/contracts/api/webgui/deploy-from-manifest",
    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"WebGUI deployed: {result}");
}
```

### Updating Smart Contracts

The R3E Community Edition provides comprehensive support for updating deployed Neo smart contracts. Here's how to implement and execute contract updates:

#### Making Your Contract Updatable

Add an update method to your contract:

```csharp
[DisplayName("update")]
public static bool Update(ByteString nefFile, string manifest, object data)
{
    // Check authorization - only owner can update
    if (!Runtime.CheckWitness(GetOwner()))
    {
        throw new Exception("Only owner can update contract");
    }
    
    // Call ContractManagement.Update to perform the update
    ContractManagement.Update(nefFile, manifest, data);
    return true;
}
```

#### Updating a Contract Using the Deployment Toolkit

```csharp
using Neo.SmartContract.Deploy;

// Create and configure the toolkit
var toolkit = new DeploymentToolkit();
toolkit.SetWifKey("your-wif-key"); // Use the same key that deployed the contract
toolkit.SetNetwork("testnet");

// Update the contract
var result = await toolkit.UpdateAsync(
    contractHash: "0x1234567890abcdef1234567890abcdef12345678",
    path: "path/to/UpdatedContract.cs"
);

if (result.Success)
{
    Console.WriteLine($"Contract updated successfully!");
    Console.WriteLine($"Transaction: {result.TransactionHash}");
}
```

#### Update Best Practices

1. **Test Updates Thoroughly**: Always test on testnet before mainnet
2. **Maintain State Compatibility**: Ensure storage layout remains compatible
3. **Version Your Contracts**: Track contract versions for easier management
4. **Implement Authorization**: Use proper access controls for updates
5. **Document Changes**: Keep a changelog of contract updates

For detailed update documentation, see:
- [Contract Update Guide](docs/UPDATE_GUIDE.md)
- [Update Troubleshooting](docs/CONTRACT_UPDATE_TROUBLESHOOTING.md)
- [Deployment Example with Updates](examples/DeploymentExample/)

## Complete Workflow Example

Here's a complete example demonstrating the full R3E devpack workflow from compilation to deployment with WebGUI:

```csharp
using Neo.Compiler;
using R3E.SmartContract.Deploy;

// Step 1: Compile the contract
var engine = new CompilationEngine();
var contexts = engine.CompileProject("./MyContract/MyContract.csproj");
var context = contexts.First();

// Step 2: Generate plugin
ContractPluginGenerator.GeneratePlugin(
    contractName: context.ContractName,
    manifest: context.CreateManifest(),
    contractHash: context.GetContractHash(),
    outputPath: "./generated/plugins"
);

// Step 3: Generate WebGUI
var webGuiResult = context.GenerateWebGui("./generated/webgui", new WebGuiOptions
{
    RpcEndpoint = "https://test1.neo.coz.io:443",
    NetworkMagic = 894710606,
    DarkTheme = true,
    IncludeMethodInvocation = true,
    IncludeEventMonitoring = true,
    IncludeWalletConnection = true
});

// Step 4: Deploy to blockchain
var toolkit = new DeploymentToolkit()
    .SetNetwork("testnet")
    .SetWifKey("your-wif-key");

var deployResult = await toolkit.DeployAsync(
    nefPath: Path.Combine(context.Options.OutputDirectory, $"{context.ContractName}.nef"),
    manifestPath: Path.Combine(context.Options.OutputDirectory, $"{context.ContractName}.manifest.json")
);

// Step 5: Upload WebGUI to R3E Service
if (deployResult.Success && webGuiResult.Success)
{
    // Sign the deployment message
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var message = $"Deploy WebGUI for contract {deployResult.ContractHash} by {deployerAddress} at {timestamp}";
    
    // ... signing logic ...
    
    var request = new
    {
        ContractAddress = deployResult.ContractHash,
        ContractName = context.ContractName,
        Network = "testnet",
        DeployerAddress = deployerAddress,
        Timestamp = timestamp,
        Signature = signature,
        PublicKey = publicKey
    };
    
    // Deploy to R3E service
    var response = await httpClient.PostAsJsonAsync(
        "https://service.neoservicelayer.com/contracts/api/webgui/deploy-from-manifest",
        request
    );
    
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Console.WriteLine($"✅ WebGUI available at: {result.url}");
    }
}
```

## Examples

The repository includes various example contracts that demonstrate different features and capabilities in the `examples` directory:

| Example | Description |
|---------|-------------|
| [HelloWorld](examples/Example.SmartContract.HelloWorld/) | Basic contract example |
| [NEP-17](examples/Example.SmartContract.NEP17/) | Token standard implementation |
| [NEP-11](examples/Example.SmartContract.NFT/) | NFT standard implementation |
| [Storage](examples/Example.SmartContract.Storage/) | Persistent storage example |
| [Events](examples/Example.SmartContract.Event/) | Event notification example |
| [Contract Calls](examples/Example.SmartContract.ContractCall/) | Inter-contract calls |
| [Exception](examples/Example.SmartContract.Exception/) | Error handling examples |
| [ZKP](examples/Example.SmartContract.ZKP/) | Zero-knowledge proof implementation examples |
| [Inscription](examples/Example.SmartContract.Inscription/) | Blockchain inscription examples |
| [Oracle](examples/Example.SmartContract.Oracle/) | Oracle service interaction examples |
| [Modifier](examples/Example.SmartContract.Modifier/) | Method modifier usage examples |
| [Transfer](examples/Example.SmartContract.Transfer/) | Asset transfer examples |
| [SampleRoyaltyNEP11Token](examples/Example.SmartContract.SampleRoyaltyNEP11Token/) | NFT with royalty feature implementation |

Each example comes with corresponding unit tests that demonstrate how to properly test the contract functionality.

## Docker Deployment

The R3E WebGUI Service can be deployed using automated setup scripts:

```bash
# Quick deployment to service.neoservicelayer.com/contracts
curl -sSL https://raw.githubusercontent.com/r3e-network/r3e-devpack-dotnet/r3e/src/R3E.WebGUI.Service/setup-contracts-service.sh | sudo bash

# The service will be available at:
# - API: https://service.neoservicelayer.com/contracts/api/
# - Swagger: https://service.neoservicelayer.com/contracts/swagger
# - WebGUIs: https://service.neoservicelayer.com/contracts/{contract-name}
```

For local development using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/r3e-network/r3e-devpack-dotnet.git
cd r3e-devpack-dotnet/src/R3E.WebGUI.Service

# Start the service locally
docker-compose up -d

# Local service will be available at:
# - API: http://localhost:8888
# - WebGUIs: http://{contract}.localhost
```

## Documentation

**📚 Full Documentation Website: [https://r3edevpack.netlify.app](https://r3edevpack.netlify.app)**

The documentation website includes:
- Getting Started Guide with Project Templates
- Complete Compiler Reference (RNCC)
- WebGUI Service Documentation
- Plugin Development Guide
- API Reference
- Security Best Practices
- Example Contracts
- Downloads for All Platforms

Additional resources:
- [R3E Community Edition Repository](https://github.com/r3e-network/r3e-devpack-dotnet)
- [Neo Official Documentation](https://docs.neo.org/)
- [Neo Smart Contract Development Guide](https://docs.neo.org/docs/en-us/develop/write/basics.html)
- [Official Neo DevPack](https://github.com/neo-project/neo-devpack-dotnet) (upstream project)
- [R3E WebGUI Service API Documentation](https://service.neoservicelayer.com/contracts/swagger)
- [Deployment Toolkit Documentation](docs/deployment-toolkit.md)
- [WebGUI Generation Guide](docs/webgui-generation.md)
- [WebGUI Service Deployment Checklist](src/R3E.WebGUI.Service/DEPLOYMENT-CHECKLIST.md)

## Contributing

Contributions are welcome! This project includes comprehensive build automation to make contributing easy.

### Quick Start for Contributors

```bash
# Set up development environment
make install-tools
make dev                # Clean, build, and test

# Run full CI pipeline
make ci                 # Clean, restore, build, test, lint, security scan
```

### Development Workflow

```bash
# Individual tasks
make build              # Build all projects
make test               # Run all tests
make coverage           # Generate code coverage report
make format             # Format code
make lint               # Run linters
make security-scan      # Check for vulnerabilities

# Component-specific builds
make compiler-only      # Build only the compiler
make testing-only       # Build only testing framework
make webgui-only        # Build only WebGUI service
```

For detailed contribution guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md).

### Pull Request Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes and run `make ci` to ensure everything passes
4. Commit your changes (`git commit -am 'Add your feature'`)
5. Push to the branch (`git push origin feature/your-feature`)
6. Create a new Pull Request

Please ensure that your code follows the existing coding style and includes appropriate tests and documentation.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [R3E Community](https://github.com/r3e-network/) - A Neo blockchain community
- [NEO Project](https://neo.org/) - Original developers of the Neo DevPack
- [NEO Community](https://neo.org/community)

## About R3E

R3E is a community within the Neo ecosystem dedicated to advancing Neo blockchain development. This DevPack edition represents our contribution to making Neo smart contract development more accessible and feature-rich for developers.
