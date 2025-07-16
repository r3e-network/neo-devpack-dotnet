# Neo Compiler Binaries

This document describes how to download and use pre-built binaries of the Neo C# Compiler Tool (`neoc`).

## Download

Pre-built binaries are available for multiple platforms from the [GitHub Releases page](https://github.com/neo-project/neo-devpack-dotnet/releases).

### Supported Platforms

| Platform | Architecture | Binary Name | Notes |
|----------|-------------|-------------|-------|
| Windows | x64 | `neoc-win-x64.exe` | Self-contained executable |
| Linux | x64 | `neoc-linux-x64` | Self-contained executable |
| macOS | x64 (Intel) | `neoc-macos-x64` | Self-contained executable |
| macOS | ARM64 (Apple Silicon) | `neoc-macos-arm64` | Self-contained executable |

### Download Instructions

1. Visit the [releases page](https://github.com/neo-project/neo-devpack-dotnet/releases)
2. Find the latest release
3. Download the appropriate binary for your platform from the "Assets" section
4. Make the binary executable (Linux/macOS only)

## Installation

### Windows

1. Download `neoc-win-x64.exe`
2. Place it in a directory of your choice (e.g., `C:\tools\`)
3. Optionally, add the directory to your PATH environment variable
4. Run from command prompt: `neoc-win-x64.exe --help`

### Linux

1. Download `neoc-linux-x64`
2. Make it executable:
   ```bash
   chmod +x neoc-linux-x64
   ```
3. Optionally, move to a directory in your PATH:
   ```bash
   sudo mv neoc-linux-x64 /usr/local/bin/neoc
   ```
4. Run: `./neoc-linux-x64 --help` or `neoc --help` (if in PATH)

### macOS

1. Download the appropriate binary:
   - Intel Macs: `neoc-macos-x64`
   - Apple Silicon Macs: `neoc-macos-arm64`
2. Make it executable:
   ```bash
   chmod +x neoc-macos-x64  # or neoc-macos-arm64
   ```
3. On first run, you may need to allow the unsigned binary:
   ```bash
   sudo xattr -rd com.apple.quarantine neoc-macos-x64
   ```
4. Optionally, move to a directory in your PATH:
   ```bash
   sudo mv neoc-macos-x64 /usr/local/bin/neoc
   ```
5. Run: `./neoc-macos-x64 --help` or `neoc --help` (if in PATH)

## Usage

All binaries have the same command-line interface. Basic usage:

```bash
# Show help
neoc --help

# Compile a smart contract
neoc YourContract.cs

# Compile with specific output directory
neoc YourContract.cs -o ./output

# Compile with debug symbols
neoc YourContract.cs --debug

# Show version
neoc --version
```

## Features

The pre-built binaries include:

- **Self-contained**: No need to install .NET Runtime
- **Single file**: All dependencies bundled into one executable
- **Optimized**: Trimmed and compressed for smaller size
- **Cross-platform**: Native binaries for each platform

## Verification

To verify your download:

1. Check the file size is reasonable (typically 50-150 MB)
2. Run `neoc --version` to see version information
3. Run `neoc --help` to see command options

## Building from Source

If you prefer to build from source or need a platform not listed above:

```bash
# Clone the repository
git clone https://github.com/neo-project/neo-devpack-dotnet.git
cd neo-devpack-dotnet

# Build for your platform (example for Linux x64)
dotnet publish src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ./publish

# The binary will be at ./publish/rncc (Linux/macOS) or ./publish/rncc.exe (Windows)
```

## Supported Runtime Identifiers

For building custom platforms, .NET supports many runtime identifiers:

- Windows: `win-x64`, `win-x86`, `win-arm64`
- Linux: `linux-x64`, `linux-arm64`, `linux-musl-x64`
- macOS: `osx-x64`, `osx-arm64`
- And many more...

## Troubleshooting

### Windows

- **"Windows protected your PC"**: Click "More info" → "Run anyway"
- **Antivirus warnings**: Add the executable to your antivirus whitelist

### Linux

- **Permission denied**: Run `chmod +x neoc-linux-x64`
- **Command not found**: Ensure the binary is in your PATH or use `./neoc-linux-x64`

### macOS

- **"Cannot be opened because it is from an unidentified developer"**: 
  - Run `sudo xattr -rd com.apple.quarantine neoc-macos-x64`
  - Or go to System Preferences → Security & Privacy → Allow anyway
- **Wrong architecture**: Ensure you downloaded the correct version for your Mac (Intel vs Apple Silicon)

## Security

- Binaries are built automatically by GitHub Actions
- All builds are reproducible from the source code
- No code signing is applied (you may see security warnings)
- Always download from official GitHub releases

## License

The Neo Compiler binaries are distributed under the same license as the source code (MIT License).