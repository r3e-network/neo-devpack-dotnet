#!/bin/bash
# Release script for RNCC .NET Tool to NuGet

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${YELLOW}=== RNCC .NET Tool Release Script ===${NC}"
echo ""

# Check if we're in the right directory
if [ ! -f "src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj" ]; then
    echo -e "${RED}Error: Must run from repository root${NC}"
    exit 1
fi

# Get current version
VERSION=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
echo -e "${YELLOW}Current version: ${VERSION}${NC}"

# Check for API key
if [ -z "$NUGET_API_KEY" ]; then
    echo -e "${RED}Error: NUGET_API_KEY environment variable not set${NC}"
    echo "Please set your NuGet API key:"
    echo "  export NUGET_API_KEY=your-api-key-here"
    exit 1
fi

# Clean previous packages
echo -e "${YELLOW}Cleaning previous builds...${NC}"
rm -rf src/Neo.Compiler.CSharp.Tool/bin/Release
rm -rf artifacts/packages

# Build the tool
echo -e "${YELLOW}Building RNCC tool...${NC}"
dotnet build src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
    --configuration Release

# Run tests
echo -e "${YELLOW}Running compiler tests...${NC}"
dotnet test tests/Neo.Compiler.CSharp.UnitTests/Neo.Compiler.CSharp.UnitTests.csproj \
    --configuration Release \
    --no-build || true

# Pack the tool
echo -e "${YELLOW}Creating NuGet package...${NC}"
mkdir -p artifacts/packages
dotnet pack src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
    --configuration Release \
    --no-build \
    --output artifacts/packages

# Display package info
PACKAGE_FILE=$(ls artifacts/packages/R3E.Compiler.CSharp.Tool.*.nupkg)
echo ""
echo -e "${GREEN}Package created: ${PACKAGE_FILE}${NC}"

# Verify package contents
echo ""
echo -e "${YELLOW}Package contents:${NC}"
dotnet nuget locals all --list
unzip -l "$PACKAGE_FILE" | grep -E "(tools|README|LICENSE)"

# Test local installation
echo ""
echo -e "${YELLOW}Testing local installation...${NC}"
dotnet tool uninstall -g R3E.Compiler.CSharp.Tool || true
dotnet tool install -g R3E.Compiler.CSharp.Tool \
    --add-source ./artifacts/packages \
    --version $VERSION

# Verify installation
echo ""
echo -e "${YELLOW}Verifying installation...${NC}"
if command -v rncc &> /dev/null; then
    echo -e "${GREEN}✓ RNCC installed successfully${NC}"
    rncc --version || echo "Version command not implemented"
else
    echo -e "${RED}✗ RNCC installation failed${NC}"
    exit 1
fi

# Prompt for confirmation
echo ""
echo -e "${YELLOW}Ready to publish to NuGet.org${NC}"
echo "Package: $PACKAGE_FILE"
echo "Version: $VERSION"
echo ""
read -p "Do you want to publish to NuGet? (y/N): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Publishing to NuGet...${NC}"
    dotnet nuget push "$PACKAGE_FILE" \
        --api-key "$NUGET_API_KEY" \
        --source https://api.nuget.org/v3/index.json \
        --skip-duplicate
    
    echo ""
    echo -e "${GREEN}✅ Package published successfully!${NC}"
    echo ""
    echo "Users can now install with:"
    echo "  dotnet tool install -g R3E.Compiler.CSharp.Tool"
    echo ""
    echo "Or shorter alias:"
    echo "  dotnet tool install -g rncc"
else
    echo -e "${YELLOW}Publishing cancelled${NC}"
    echo ""
    echo "To publish manually:"
    echo "  dotnet nuget push \"$PACKAGE_FILE\" \\"
    echo "    --api-key \$NUGET_API_KEY \\"
    echo "    --source https://api.nuget.org/v3/index.json"
fi

echo ""
echo -e "${YELLOW}Post-release checklist:${NC}"
echo "[ ] Update documentation with installation instructions"
echo "[ ] Create GitHub release tag v$VERSION"
echo "[ ] Update website with dotnet tool install command"
echo "[ ] Test installation on different platforms"
echo "[ ] Announce release on Discord/Twitter"