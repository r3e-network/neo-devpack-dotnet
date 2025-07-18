#!/bin/bash

# Test script to verify release build works locally

echo "🧪 Testing Release Build Process"
echo "================================"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Function to check command result
check_result() {
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ $1 succeeded${NC}"
    else
        echo -e "${RED}✗ $1 failed${NC}"
        exit 1
    fi
}

# Clean previous builds
echo -e "${YELLOW}Cleaning previous builds...${NC}"
rm -rf ./nupkgs ./publish
check_result "Clean"

# Restore
echo -e "${YELLOW}Restoring dependencies...${NC}"
dotnet restore
check_result "Restore"

# Build
echo -e "${YELLOW}Building solution...${NC}"
dotnet build -c Release --no-restore
check_result "Build"

# Pack
echo -e "${YELLOW}Packing NuGet packages...${NC}"
dotnet pack -c Release --no-build -o ./nupkgs
check_result "Pack"

# List packages
echo -e "${YELLOW}Packages created:${NC}"
ls -1 ./nupkgs/*.nupkg | grep -v snupkg | sort

# Count packages
TOTAL=$(ls ./nupkgs/*.nupkg | grep -v snupkg | wc -l)
R3E=$(ls ./nupkgs/R3E*.nupkg 2>/dev/null | grep -v snupkg | wc -l)
NEO=$(ls ./nupkgs/Neo*.nupkg 2>/dev/null | grep -v snupkg | wc -l)

echo ""
echo -e "${YELLOW}Package Summary:${NC}"
echo "Total packages: $TOTAL"
echo "R3E packages: $R3E"
echo "Neo packages: $NEO"

# Test binary build
echo ""
echo -e "${YELLOW}Testing Linux binary build...${NC}"
dotnet publish src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    -o ./publish/linux-x64

if [ -f "./publish/linux-x64/rncc" ]; then
    echo -e "${GREEN}✓ Linux binary created successfully${NC}"
    ls -lh ./publish/linux-x64/rncc
else
    echo -e "${RED}✗ Linux binary not found${NC}"
fi

echo ""
echo -e "${GREEN}✅ Release build test completed!${NC}"