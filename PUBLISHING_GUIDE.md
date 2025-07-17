# Publishing RNCC as a .NET Tool

This guide explains how to publish RNCC (R3E Neo Contract Compiler) as a .NET tool to NuGet.

## Prerequisites

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org/)
2. **API Key**: Generate an API key from your NuGet account settings
3. **.NET SDK**: Ensure you have .NET 8.0 or later installed

## Package Structure

We publish two packages for user convenience:

1. **R3E.Compiler.CSharp.Tool** - The main package with full name
2. **rncc** - Short alias package for easier installation

Both packages provide the same `rncc` command when installed.

## Publishing Process

### Option 1: Using the Release Script (Recommended)

```bash
# Set your NuGet API key
export NUGET_API_KEY=your-api-key-here

# Run the release script
make release-tool
# or
./release-rncc-tool.sh
```

The script will:
1. Build the tool
2. Run tests
3. Create NuGet packages
4. Test local installation
5. Prompt for publishing confirmation
6. Publish to NuGet.org

### Option 2: Manual Publishing

```bash
# 1. Clean and build
dotnet clean
dotnet build --configuration Release

# 2. Run tests
dotnet test --configuration Release

# 3. Pack the main tool
dotnet pack src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
  --configuration Release \
  --output ./packages

# 4. Pack the short alias (optional)
dotnet pack src/RNCC/RNCC.csproj \
  --configuration Release \
  --output ./packages

# 5. Test local installation
dotnet tool install -g R3E.Compiler.CSharp.Tool \
  --add-source ./packages \
  --version 0.0.4

# 6. Verify it works
rncc --help

# 7. Publish to NuGet
dotnet nuget push packages/R3E.Compiler.CSharp.Tool.0.0.4.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json

dotnet nuget push packages/rncc.0.0.4.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Option 3: GitHub Actions

Push a version tag to trigger automatic publishing:

```bash
git tag v0.0.4
git push origin v0.0.4
```

Or trigger manually from GitHub Actions tab.

## Version Management

Update version in `Directory.Build.props`:

```xml
<Version>0.0.4</Version>
```

This version is used by all packages in the solution.

## Post-Publishing

After publishing:

1. **Verify Installation**:
   ```bash
   # Wait a few minutes for NuGet indexing
   dotnet tool install -g R3E.Compiler.CSharp.Tool
   rncc --help
   ```

2. **Update Documentation**:
   - Update README.md with the latest version
   - Update website installation instructions
   - Create GitHub release notes

3. **Announce Release**:
   - Discord community
   - Twitter/Social media
   - Neo community forums

## Package Metadata

Key metadata in the project file:

```xml
<PropertyGroup>
  <PackageId>R3E.Compiler.CSharp.Tool</PackageId>
  <ToolCommandName>rncc</ToolCommandName>
  <PackAsTool>true</PackAsTool>
  <Description>RNCC - Professional Neo smart contract compiler</Description>
  <PackageProjectUrl>https://r3edevpack.netlify.app</PackageProjectUrl>
  <RepositoryUrl>https://github.com/r3e-network/r3e-devpack-dotnet</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <Authors>R3E Network</Authors>
</PropertyGroup>
```

## Troubleshooting

### Package Already Exists
If you get "Package already exists" error, either:
- Increment the version number
- Use `--skip-duplicate` flag
- Delete the package version from NuGet (if you own it)

### Tool Not Found After Installation
- Ensure `~/.dotnet/tools` is in your PATH
- Run `dotnet tool list -g` to verify installation
- Try closing and reopening your terminal

### Build Errors
- Ensure all submodules are updated: `git submodule update --init --recursive`
- Check .NET SDK version: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

## Security Notes

- Never commit your NuGet API key
- Use GitHub Secrets for CI/CD workflows
- Rotate API keys periodically
- Use scoped API keys when possible

## Support

For issues or questions:
- [GitHub Issues](https://github.com/r3e-network/r3e-devpack-dotnet/issues)
- [Discord Community](https://discord.gg/r3e)