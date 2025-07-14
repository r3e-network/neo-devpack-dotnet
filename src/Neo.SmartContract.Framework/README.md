# Neo.SmartContract.Framework

The core framework for developing Neo N3 smart contracts in C#. This library provides all the essential APIs and attributes needed to write smart contracts for the Neo blockchain.

## Features

- Complete Neo N3 smart contract API
- Native contract interfaces (NEO, GAS, Oracle, etc.)
- NEP standard implementations (NEP-17, NEP-11)
- Storage, Runtime, and Crypto APIs
- Contract upgrade and update functionality
- Event and notification support

## Installation

```bash
dotnet add package R3E.SmartContract.Framework
```

## Quick Start

```csharp
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

[DisplayName("HelloWorld")]
[ManifestExtra("Author", "Neo Community")]
public class HelloWorld : SmartContract
{
    public static string SayHello(string name)
    {
        return $"Hello, {name}!";
    }
}
```

## Core APIs

- **Runtime**: Contract execution context and blockchain data
- **Storage**: Persistent contract storage
- **Crypto**: Cryptographic operations
- **Contract**: Inter-contract calls
- **Native Contracts**: NEO, GAS, Oracle, Policy, etc.

## Documentation

For detailed documentation, visit the [Neo Smart Contract Development Guide](https://docs.neo.org/docs/n3/develop/write/basics).

## License

MIT License - see the [LICENSE](https://github.com/r3e-network/r3e-devpack-dotnet/blob/master/LICENSE) file for details.