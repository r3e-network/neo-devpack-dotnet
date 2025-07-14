# Neo.SmartContract.Analyzer

Roslyn-based code analyzer for Neo smart contracts that provides compile-time checks and warnings to ensure contract safety and best practices.

## Features

- Compile-time detection of unsafe patterns
- Best practice enforcement for Neo smart contracts
- Integration with Visual Studio and other IDEs
- Customizable rule severity levels
- Automatic code fixes for common issues

## Installation

```bash
dotnet add package R3E.SmartContract.Analyzer
```

## Analyzer Rules

- **NEO001**: Warn about non-deterministic operations
- **NEO002**: Detect unsafe external calls
- **NEO003**: Check for proper storage key usage
- **NEO004**: Validate event parameter types
- **NEO005**: Ensure proper BigInteger usage
- **NEO006**: Detect potential reentrancy issues

## Usage

The analyzer automatically runs during compilation when included in your project. Configure rules in your `.editorconfig`:

```ini
[*.cs]
# Neo Smart Contract Analyzer Rules
dotnet_diagnostic.NEO001.severity = warning
dotnet_diagnostic.NEO002.severity = error
```

## License

MIT License - see the [LICENSE](https://github.com/r3e-network/r3e-devpack-dotnet/blob/master/LICENSE) file for details.