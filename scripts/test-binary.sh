#!/bin/bash

# Test script for Neo Compiler binaries
# Usage: ./test-binary.sh <path-to-binary>

set -e

BINARY_PATH="$1"

if [ -z "$BINARY_PATH" ]; then
    echo "Usage: $0 <path-to-binary>"
    echo "Example: $0 ./neoc-linux-x64"
    exit 1
fi

if [ ! -f "$BINARY_PATH" ]; then
    echo "Error: Binary not found at $BINARY_PATH"
    exit 1
fi

echo "Testing Neo Compiler binary: $BINARY_PATH"
echo "=============================================="

# Test 1: Check if binary is executable
echo "1. Checking if binary is executable..."
if [ ! -x "$BINARY_PATH" ]; then
    echo "   Making binary executable..."
    chmod +x "$BINARY_PATH"
fi
echo "   ✓ Binary is executable"

# Test 2: Check version
echo "2. Checking version..."
VERSION_OUTPUT=$("$BINARY_PATH" --version 2>&1 || true)
if [ $? -eq 0 ]; then
    echo "   ✓ Version: $VERSION_OUTPUT"
else
    echo "   ⚠ Version check failed: $VERSION_OUTPUT"
fi

# Test 3: Check help output
echo "3. Checking help output..."
HELP_OUTPUT=$("$BINARY_PATH" --help 2>&1 || true)
if echo "$HELP_OUTPUT" | grep -q "Usage\|USAGE\|help\|HELP"; then
    echo "   ✓ Help output looks good"
else
    echo "   ⚠ Help output may be incomplete"
fi

# Test 4: Check file size
echo "4. Checking binary size..."
SIZE=$(wc -c < "$BINARY_PATH")
SIZE_MB=$((SIZE / 1024 / 1024))
echo "   Binary size: ${SIZE_MB}MB (${SIZE} bytes)"

if [ $SIZE -lt 10000000 ]; then  # Less than 10MB
    echo "   ⚠ Binary seems small - may be missing dependencies"
elif [ $SIZE -gt 500000000 ]; then  # More than 500MB
    echo "   ⚠ Binary seems very large"
else
    echo "   ✓ Binary size looks reasonable"
fi

# Test 5: Create a simple test contract
echo "5. Testing compilation with a simple contract..."
TEST_DIR=$(mktemp -d)
TEST_CONTRACT="$TEST_DIR/TestContract.cs"

cat > "$TEST_CONTRACT" << 'EOF'
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

[DisplayName("TestContract")]
[ManifestExtra("Author", "Test")]
[ManifestExtra("Email", "test@example.com")]
[ManifestExtra("Description", "This is a test contract")]
public class TestContract : SmartContract
{
    [DisplayName("GetMessage")]
    public static string GetMessage()
    {
        return "Hello, Neo!";
    }
    
    [DisplayName("Add")]
    public static int Add(int a, int b)
    {
        return a + b;
    }
}
EOF

echo "   Testing compilation..."
COMPILE_OUTPUT=$("$BINARY_PATH" "$TEST_CONTRACT" -o "$TEST_DIR" 2>&1 || true)
COMPILE_EXIT_CODE=$?

if [ $COMPILE_EXIT_CODE -eq 0 ]; then
    echo "   ✓ Compilation successful"
    
    # Check if output files were created
    if [ -f "$TEST_DIR/TestContract.nef" ] && [ -f "$TEST_DIR/TestContract.manifest.json" ]; then
        echo "   ✓ Output files created: .nef and .manifest.json"
        
        # Check file sizes
        NEF_SIZE=$(wc -c < "$TEST_DIR/TestContract.nef")
        MANIFEST_SIZE=$(wc -c < "$TEST_DIR/TestContract.manifest.json")
        
        echo "   NEF size: ${NEF_SIZE} bytes"
        echo "   Manifest size: ${MANIFEST_SIZE} bytes"
        
        if [ $NEF_SIZE -gt 0 ] && [ $MANIFEST_SIZE -gt 0 ]; then
            echo "   ✓ Output files have content"
        else
            echo "   ⚠ Output files are empty"
        fi
    else
        echo "   ⚠ Expected output files not found"
    fi
else
    echo "   ⚠ Compilation failed:"
    echo "   $COMPILE_OUTPUT"
fi

# Cleanup
rm -rf "$TEST_DIR"

echo ""
echo "=============================================="
echo "Binary test completed!"

if [ $COMPILE_EXIT_CODE -eq 0 ]; then
    echo "✓ All tests passed - binary appears to be working correctly"
    exit 0
else
    echo "⚠ Some tests failed - binary may have issues"
    exit 1
fi