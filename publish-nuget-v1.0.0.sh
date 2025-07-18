#!/bin/bash

# R3E DevPack v1.0.0 NuGet Publishing Script
# This script publishes all v1.0.0 packages to NuGet.org

set -e  # Exit on error

# Configuration
RELEASE_DIR="./release-v1.0.0"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"
VERSION="1.0.0"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if API key is provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: NuGet API key not provided${NC}"
    echo "Usage: $0 <NUGET_API_KEY>"
    exit 1
fi

NUGET_API_KEY=$1

# Packages to publish (in dependency order)
PACKAGES=(
    "Neo.Extensions.${VERSION}.nupkg"
    "Neo.Json.${VERSION}.nupkg"
    "Neo.IO.${VERSION}.nupkg"
    "Neo.VM.${VERSION}.nupkg"
    "Neo.Cryptography.BLS12_381.${VERSION}.nupkg"
    "Neo.${VERSION}.nupkg"
    "R3E.SmartContract.Framework.${VERSION}.nupkg"
    "R3E.Disassembler.CSharp.${VERSION}.nupkg"
    "R3E.SmartContract.Analyzer.${VERSION}.nupkg"
    "R3E.SmartContract.Testing.${VERSION}.nupkg"
    "R3E.Compiler.CSharp.${VERSION}.nupkg"
    "R3E.SmartContract.Deploy.${VERSION}.nupkg"
    "R3E.SmartContract.Template.${VERSION}.nupkg"
    "R3E.WebGUI.Service.${VERSION}.nupkg"
    "R3E.WebGUI.Deploy.${VERSION}.nupkg"
    "R3E.Compiler.CSharp.Tool.${VERSION}.nupkg"
)

echo -e "${YELLOW}🚀 R3E DevPack v${VERSION} NuGet Publishing${NC}"
echo "========================================"
echo "Publishing ${#PACKAGES[@]} packages to NuGet.org"
echo ""

# Check if release directory exists
if [ ! -d "$RELEASE_DIR" ]; then
    echo -e "${RED}Error: Release directory not found: $RELEASE_DIR${NC}"
    exit 1
fi

cd "$RELEASE_DIR"

# Track results
SUCCESS_COUNT=0
FAILED_COUNT=0
FAILED_PACKAGES=()

# Publish each package
for PACKAGE in "${PACKAGES[@]}"; do
    if [ -f "$PACKAGE" ]; then
        echo -e "${YELLOW}📦 Publishing $PACKAGE...${NC}"
        
        if dotnet nuget push "$PACKAGE" \
            --api-key "$NUGET_API_KEY" \
            --source "$NUGET_SOURCE" \
            --skip-duplicate; then
            echo -e "${GREEN}✅ Successfully published $PACKAGE${NC}"
            ((SUCCESS_COUNT++))
        else
            echo -e "${RED}❌ Failed to publish $PACKAGE${NC}"
            ((FAILED_COUNT++))
            FAILED_PACKAGES+=("$PACKAGE")
        fi
        
        echo ""
        
        # Small delay between publishes
        sleep 2
    else
        echo -e "${RED}⚠️  Package not found: $PACKAGE${NC}"
        ((FAILED_COUNT++))
        FAILED_PACKAGES+=("$PACKAGE")
    fi
done

# Summary
echo "========================================"
echo -e "${YELLOW}📊 Publishing Summary${NC}"
echo "========================================"
echo -e "${GREEN}✅ Successful: $SUCCESS_COUNT packages${NC}"
echo -e "${RED}❌ Failed: $FAILED_COUNT packages${NC}"

if [ $FAILED_COUNT -gt 0 ]; then
    echo ""
    echo -e "${RED}Failed packages:${NC}"
    for FAILED in "${FAILED_PACKAGES[@]}"; do
        echo "  - $FAILED"
    done
fi

echo ""

# Verification steps
echo -e "${YELLOW}🔍 Next Steps:${NC}"
echo "1. Verify packages on https://www.nuget.org/profiles/R3E"
echo "2. Test installation:"
echo "   dotnet tool install -g R3E.Compiler.CSharp.Tool --version ${VERSION}"
echo "3. Monitor for any issues in the first 24 hours"
echo ""

if [ $FAILED_COUNT -eq 0 ]; then
    echo -e "${GREEN}🎉 All packages published successfully!${NC}"
    exit 0
else
    echo -e "${RED}⚠️  Some packages failed to publish. Please check and retry.${NC}"
    exit 1
fi