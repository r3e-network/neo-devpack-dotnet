#!/bin/bash

# Script to manually publish all R3E packages to NuGet
# Usage: ./publish-all-packages.sh <NUGET_API_KEY>

set -e

if [ -z "$1" ]; then
    echo "Usage: $0 <NUGET_API_KEY>"
    exit 1
fi

NUGET_API_KEY=$1
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

echo "Building and publishing all R3E packages..."
cd "$PROJECT_ROOT"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf ./nupkgs
mkdir -p ./nupkgs

# Build all projects
echo "Building all projects..."
dotnet build -c Release

# List of all packages to publish
declare -A PACKAGES=(
    ["R3E.Compiler.CSharp"]="src/Neo.Compiler.CSharp/Neo.Compiler.CSharp.csproj"
    ["R3E.Compiler.CSharp.Tool"]="src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj"
    ["R3E.Disassembler.CSharp"]="src/Neo.Disassembler.CSharp/Neo.Disassembler.CSharp.csproj"
    ["R3E.SmartContract.Analyzer"]="src/Neo.SmartContract.Analyzer/Neo.SmartContract.Analyzer.csproj"
    ["R3E.SmartContract.Deploy"]="src/Neo.SmartContract.Deploy/Neo.SmartContract.Deploy.csproj"
    ["R3E.SmartContract.Framework"]="src/Neo.SmartContract.Framework/Neo.SmartContract.Framework.csproj"
    ["R3E.SmartContract.Template"]="src/Neo.SmartContract.Template/Neo.SmartContract.Template.csproj"
    ["R3E.SmartContract.Testing"]="src/Neo.SmartContract.Testing/Neo.SmartContract.Testing.csproj"
    ["R3E.WebGUI.Deploy"]="src/R3E.WebGUI.Deploy/R3E.WebGUI.Deploy.csproj"
    ["R3E.WebGUI.Service"]="src/R3E.WebGUI.Service/R3E.WebGUI.Service.csproj"
)

# Pack each project explicitly
echo "Packing all packages..."
for package_name in "${!PACKAGES[@]}"; do
    project_path="${PACKAGES[$package_name]}"
    echo "Packing $package_name from $project_path..."
    if [ -f "$project_path" ]; then
        dotnet pack "$project_path" -c Release --no-build -o ./nupkgs || echo "Failed to pack $package_name"
    else
        echo "WARNING: Project file not found: $project_path"
    fi
done

# List all generated packages
echo ""
echo "Generated packages:"
ls -la ./nupkgs/R3E*.nupkg

# Publish each package
echo ""
echo "Publishing packages to NuGet..."
SUCCESS_COUNT=0
FAIL_COUNT=0
PUBLISHED_PACKAGES=()
FAILED_PACKAGES=()

for nupkg in ./nupkgs/R3E*.nupkg; do
    if [[ ! "$nupkg" =~ \.snupkg$ ]]; then
        package_basename=$(basename "$nupkg")
        echo "Publishing $package_basename..."
        
        if dotnet nuget push "$nupkg" \
            --source https://api.nuget.org/v3/index.json \
            --api-key "$NUGET_API_KEY" \
            --skip-duplicate; then
            echo "✓ Successfully published $package_basename"
            PUBLISHED_PACKAGES+=("$package_basename")
            ((SUCCESS_COUNT++))
        else
            echo "✗ Failed to publish $package_basename"
            FAILED_PACKAGES+=("$package_basename")
            ((FAIL_COUNT++))
        fi
        echo ""
    fi
done

# Summary
echo "========================================="
echo "Publishing Summary"
echo "========================================="
echo "Total packages processed: $((SUCCESS_COUNT + FAIL_COUNT))"
echo "Successfully published: $SUCCESS_COUNT"
echo "Failed to publish: $FAIL_COUNT"
echo ""

if [ ${#PUBLISHED_PACKAGES[@]} -gt 0 ]; then
    echo "Published packages:"
    for pkg in "${PUBLISHED_PACKAGES[@]}"; do
        echo "  ✓ $pkg"
    done
fi

if [ ${#FAILED_PACKAGES[@]} -gt 0 ]; then
    echo ""
    echo "Failed packages:"
    for pkg in "${FAILED_PACKAGES[@]}"; do
        echo "  ✗ $pkg"
    done
fi

# Check for missing critical packages
echo ""
echo "Checking for critical packages..."
CRITICAL_MISSING=false

for package_name in "${!PACKAGES[@]}"; do
    if ! ls ./nupkgs/${package_name}.*.nupkg >/dev/null 2>&1; then
        echo "ERROR: Critical package missing: $package_name"
        CRITICAL_MISSING=true
    fi
done

if [ "$CRITICAL_MISSING" = true ]; then
    echo ""
    echo "ERROR: Some critical packages were not generated!"
    exit 1
fi

if [ $FAIL_COUNT -gt 0 ]; then
    echo ""
    echo "WARNING: Some packages failed to publish. Please check the errors above."
    exit 1
fi

echo ""
echo "All packages published successfully!"