# Contract Development with R3E DevPack

This guide explains how to use the Makefile for Neo smart contract development with R3E DevPack.

## Creating a New Contract Solution

The R3E DevPack uses a solution-based approach where each smart contract project includes:
- **Contract Project** - The actual smart contract code
- **Testing Project** - Comprehensive unit and integration tests
- **Deployment Scripts** - Scripts for deploying to different networks
- **Solution File** - Ties everything together

### Using Make Commands

```bash
# Create a new contract solution interactively
make new-contract

# This will prompt you for:
# - Contract name
# - Template type (solution/nep17/nep11/defi/multisig)
# - Author name
# - Author email
```

### Available Templates

- **solution** - Complete solution with all components (recommended)
- **nep17** - Fungible token standard implementation
- **nep11** - Non-fungible token (NFT) implementation
- **defi** - DeFi protocol with staking and rewards
- **multisig** - Multi-signature wallet contract

### Example

```bash
$ make new-contract
Creating new contract solution from template...
Enter contract name: MyToken
Enter template (solution/nep17/nep11/defi/multisig): nep17
Enter author name: John Doe
Enter author email: john@example.com

Creating MyToken solution with nep17 template...
✅ Contract solution created in ./MyToken/

Next steps:
  cd MyToken
  make build-contract
  make test-contract
```

## Building Your Contract Solution

Once you have a contract solution, you can build it using:

```bash
# Navigate to your contract directory
cd MyToken

# Build the entire solution
make build-contract
```

This command will:
1. Check for a solution file (.sln)
2. Use RNCC to compile all contracts in the solution
3. Generate NEF files and manifests
4. Build all projects including tests

## Testing Your Contract

Run comprehensive tests for your contract:

```bash
# Run all tests in the solution
make test-contract
```

This will:
- Run unit tests
- Run integration tests
- Show test results and coverage

## Deploying Your Contract

Deploy your contract to Neo TestNet:

```bash
# Deploy to testnet
make deploy-contract
```

For MainNet deployment, you'll need to configure your deployment scripts in the `deploy/` directory.

## Solution Structure

When you create a new contract solution, you get:

```
MyContract/
├── src/
│   └── MyContract.Contracts/
│       ├── MyContract.cs              # Main contract file
│       ├── MyContract.Storage.cs      # Storage management
│       ├── MyContract.Events.cs       # Event definitions
│       └── MyContract.Contracts.csproj
├── tests/
│   └── MyContract.Tests/
│       ├── MyContractTests.cs         # Unit tests
│       ├── IntegrationTests.cs        # Integration tests
│       └── MyContract.Tests.csproj
├── deploy/
│   ├── scripts/
│   │   ├── deploy-testnet.sh          # TestNet deployment
│   │   └── deploy-mainnet.sh          # MainNet deployment
│   └── config/
│       └── deployment-config.json      # Deployment configuration
├── MyContract.sln                     # Solution file
├── nuget.config                       # NuGet configuration
├── .gitignore                         # Git ignore rules
└── README.md                          # Project documentation
```

## Quick Start Example

To see a complete example:

```bash
# Create a full example with all steps
make quick-start
```

This will:
1. Create a new contract solution
2. Build it
3. Run tests
4. Show you the complete structure

## Workflow Summary

1. **Create**: `make new-contract` - Create a new solution from template
2. **Build**: `make build-contract` - Compile the entire solution
3. **Test**: `make test-contract` - Run all tests
4. **Deploy**: `make deploy-contract` - Deploy to blockchain

## Tips

- Always use the solution template for production contracts
- Write tests for all contract functionality
- Use the deployment scripts for consistent deployments
- Keep your contract, tests, and deployment scripts in sync

## Advanced Usage

For more advanced scenarios:

```bash
# Build with specific configuration
cd MyContract
dotnet build --configuration Release

# Run only unit tests
dotnet test --filter Category=Unit

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage"

# Custom deployment
rncc deploy --network=mainnet --config=deploy/config/mainnet.json
```

## Troubleshooting

### "No solution file found"
Make sure you're in the contract directory that contains a .sln file.

### Build errors
1. Check that all NuGet packages are restored: `dotnet restore`
2. Ensure you have the correct .NET SDK version: `dotnet --version`
3. Verify the Neo.SmartContract.Framework version in your .csproj files

### Test failures
1. Review the test output for specific errors
2. Check if your contract logic matches test expectations
3. Ensure test data is properly initialized

## Next Steps

- Explore the [example contracts](examples/) in the repository
- Read the [compiler documentation](website/docs/compiler-reference.html)
- Learn about [WebGUI generation](website/docs/webgui-service.html)
- Join the community discussions