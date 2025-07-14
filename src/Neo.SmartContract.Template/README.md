# Neo.SmartContract.Template

Project templates for creating Neo N3 smart contracts with best practices and standard structure.

## Features

- NEP-17 Token template
- NFT (NEP-11) template  
- Basic smart contract template
- Oracle consumer template
- Multi-signature wallet template
- Pre-configured project structure

## Installation

```bash
dotnet new install R3E.SmartContract.Template
```

## Available Templates

### Basic Contract
```bash
dotnet new neocontract -n MyContract
```

### NEP-17 Token
```bash
dotnet new nep17 -n MyToken
```

### NEP-11 NFT
```bash
dotnet new nep11 -n MyNFT
```

### Oracle Consumer
```bash
dotnet new neooracle -n MyOracleContract
```

## Template Structure

Each template includes:
- Pre-configured `.csproj` with required packages
- Contract manifest attributes
- Standard methods for the contract type
- Unit test project setup
- Deployment configuration

## License

MIT License - see the [LICENSE](https://github.com/r3e-network/r3e-devpack-dotnet/blob/master/LICENSE) file for details.