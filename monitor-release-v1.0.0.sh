#!/bin/bash

# R3E DevPack v1.0.0 Post-Release Monitoring Script
# Helps track the success of the release

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

VERSION="1.0.0"

echo -e "${BLUE}📊 R3E DevPack v${VERSION} Release Monitoring${NC}"
echo "============================================"
echo ""

# Function to check NuGet package
check_nuget_package() {
    local PACKAGE_ID=$1
    echo -e "${YELLOW}Checking $PACKAGE_ID...${NC}"
    
    # This would normally make an API call to NuGet
    # For now, we'll just show the URL to check
    echo "  📦 Check at: https://www.nuget.org/packages/$PACKAGE_ID/$VERSION"
}

# Check key packages
echo -e "${BLUE}🔍 NuGet Package Status:${NC}"
check_nuget_package "R3E.Compiler.CSharp.Tool"
check_nuget_package "R3E.SmartContract.Framework"
check_nuget_package "R3E.SmartContract.Testing"
check_nuget_package "R3E.SmartContract.Deploy"
echo ""

# GitHub Release checks
echo -e "${BLUE}🔍 GitHub Release Status:${NC}"
echo "  📄 Release: https://github.com/r3e-network/r3e-devpack-dotnet/releases/tag/v$VERSION"
echo "  🏷️  Tag: https://github.com/r3e-network/r3e-devpack-dotnet/tree/v$VERSION"
echo ""

# Test commands
echo -e "${BLUE}🧪 Test Commands:${NC}"
echo "Test RNCC installation:"
echo "  dotnet tool uninstall -g R3E.Compiler.CSharp.Tool || true"
echo "  dotnet tool install -g R3E.Compiler.CSharp.Tool --version $VERSION"
echo "  rncc --version"
echo ""

echo "Test template creation:"
echo "  rncc new TestRelease --template=solution"
echo "  cd TestRelease && dotnet build"
echo ""

# Community channels
echo -e "${BLUE}💬 Community Monitoring:${NC}"
echo "  Discord: Check #announcements and #support channels"
echo "  GitHub: Monitor https://github.com/r3e-network/r3e-devpack-dotnet/issues"
echo "  Twitter: Track mentions of #R3EDevPack"
echo ""

# Metrics to track
echo -e "${BLUE}📈 Key Metrics to Track (24-48 hours):${NC}"
echo "  - NuGet download counts"
echo "  - GitHub stars/forks increase"
echo "  - Issue reports (especially critical bugs)"
echo "  - Community feedback"
echo "  - Social media engagement"
echo ""

# Quick fixes reference
echo -e "${BLUE}🔧 Common Quick Fixes:${NC}"
echo "1. If package not found on NuGet:"
echo "   - Wait 15-30 minutes for indexing"
echo "   - Check if package was uploaded correctly"
echo ""
echo "2. If installation fails:"
echo "   - Clear NuGet cache: dotnet nuget locals all --clear"
echo "   - Try with explicit source: --source https://api.nuget.org/v3/index.json"
echo ""
echo "3. If template issues:"
echo "   - Check template package was included"
echo "   - Verify template files in package"
echo ""

echo -e "${GREEN}✅ Monitoring script complete!${NC}"
echo "Run this periodically in the first 48 hours after release."