# Neo.Disassembler.CSharp

A powerful disassembler for Neo smart contracts that converts compiled NEF (Neo Executable Format) files back into human-readable C# code representations.

## Features

- Disassemble NEF files to understand contract bytecode
- Generate C# code representation from compiled contracts
- Support for all Neo VM opcodes
- Helpful for debugging and contract analysis
- Command-line tool for easy integration

## Installation

```bash
dotnet add package R3E.Disassembler.CSharp
```

## Usage

### As a Library

```csharp
using Neo.Disassembler.CSharp;

// Load and disassemble a NEF file
var nefFile = NefFile.Parse(File.ReadAllBytes("contract.nef"));
var disassembler = new Disassembler();
var result = disassembler.Disassemble(nefFile);

Console.WriteLine(result.CSharpCode);
```

### As a Command-Line Tool

```bash
neo-disassemble contract.nef -o contract.disasm.cs
```

## License

MIT License - see the [LICENSE](https://github.com/r3e-network/r3e-devpack-dotnet/blob/master/LICENSE) file for details.