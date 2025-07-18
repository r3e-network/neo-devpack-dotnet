# Quick Start Guide - R3E DevPack v1.0.0

Get started with Neo smart contract development in minutes using R3E DevPack!

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Basic knowledge of C# programming
- (Optional) [Neo Express](https://github.com/neo-project/neo-express) for local testing

## Installation

### 1. Install RNCC CLI Tool

```bash
dotnet tool install -g R3E.Compiler.CSharp.Tool --version 1.0.0
```

Verify installation:
```bash
rncc --version
```

## Your First Smart Contract

### 1. Create a New Project

```bash
# Create a complete solution with contract, tests, and deployment
rncc new HelloContract --template=solution --author="Your Name"
```

### 2. Navigate to Your Project

```bash
cd HelloContract
```

### 3. Explore the Structure

```
HelloContract/
├── HelloContract.sln          # Solution file
├── src/
│   └── HelloContract/         # Contract project
│       ├── HelloContract.cs   # Contract code
│       └── HelloContract.csproj
├── tests/
│   └── HelloContract.Tests/   # Test project
│       ├── HelloContractTests.cs
│       └── HelloContract.Tests.csproj
└── deploy/
    └── Deploy/                # Deployment project
        ├── Program.cs
        └── Deploy.csproj
```

### 4. Build Your Contract

```bash
dotnet build
```

This will:
- Compile your C# code
- Generate NEF and manifest files
- Place them in `deploy/contracts/`

### 5. Run Tests

```bash
dotnet test
```

### 6. View Compiled Contract

```bash
ls deploy/contracts/
# HelloContract.nef
# HelloContract.manifest.json
```

## Creating Different Contract Types

### NEP-17 Token

```bash
rncc new MyToken --template=nep17 --author="Your Name"
cd MyToken
dotnet build
```

### Oracle Contract

```bash
rncc new MyOracle --template=oracle --author="Your Name"
cd MyOracle
dotnet build
```

### Simple Contract (No Solution)

```bash
rncc new SimpleContract --template=contract --author="Your Name"
cd SimpleContract
dotnet build SimpleContract.csproj
```

## Working with Existing Projects

### Add Framework to Existing Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="R3E.SmartContract.Framework" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Compile with RNCC

```bash
rncc MyContract.csproj
```

## Common Commands

### List Available Templates
```bash
rncc templates
```

### Get Help
```bash
rncc --help
rncc new --help
```

### Create with All Options
```bash
rncc new MyProject \
  --template=solution \
  --author="John Doe" \
  --email="john@example.com" \
  --with-tests=true \
  --with-deploy-scripts=true \
  --git-init=true
```

## Testing Your Contract

### Basic Test Structure

```csharp
using Neo.SmartContract.Testing;
using Xunit;

public class MyContractTests : TestBase<MyContract>
{
    [Fact]
    public void Test_Deploy()
    {
        var contract = Deploy();
        Assert.NotNull(contract);
    }
    
    [Fact]
    public void Test_MyMethod()
    {
        var contract = Deploy();
        var result = contract.MyMethod("test");
        Assert.Equal("Hello, test!", result);
    }
}
```

## Deployment

### Local Testing with Neo Express

1. Install Neo Express:
```bash
dotnet tool install -g Neo.Express --version 3.8.1
```

2. Create local blockchain:
```bash
neoxp create
```

3. Deploy contract:
```bash
cd deploy
dotnet run
```

### Testnet Deployment

Update `appsettings.json` in deploy project:
```json
{
  "Network": "testnet",
  "RpcUrl": "https://testnet.rpc.neo.org",
  "WalletPath": "path/to/wallet.json"
}
```

## Best Practices

1. **Always Write Tests** - Use the testing framework to ensure contract correctness
2. **Use Templates** - Start with templates to follow best practices
3. **Version Control** - Use git (add `--git-init` when creating projects)
4. **Security First** - Review security guidelines before mainnet deployment

## Troubleshooting

### RNCC Not Found
Add dotnet tools to PATH:
```bash
export PATH="$HOME/.dotnet/tools:$PATH"
```

### Build Errors
Clean and rebuild:
```bash
dotnet clean
dotnet restore
dotnet build
```

### Missing Dependencies
Ensure .NET 9.0 SDK is installed:
```bash
dotnet --version
```

## Next Steps

1. 📖 Read the [full documentation](https://r3edevpack.netlify.app)
2. 🔍 Explore [example contracts](examples/)
3. 🤝 Join the [R3E Discord](https://discord.gg/r3e)
4. 🚀 Deploy to testnet and start testing!

## Resources

- **Documentation**: https://r3edevpack.netlify.app
- **GitHub**: https://github.com/r3e-network/r3e-devpack-dotnet
- **Neo Docs**: https://docs.neo.org
- **Neo DevPack Original**: https://github.com/neo-project/neo-devpack-dotnet

Welcome to Neo smart contract development with R3E DevPack! 🎉