#!/bin/bash
# Integration test script for contract workflow
# Tests the complete flow: create -> build -> test -> deploy

set -e  # Exit on error

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${YELLOW}=== R3E DevPack Contract Workflow Integration Test ===${NC}"
echo ""

# Create test directory
TEST_DIR="./test-workflow-$(date +%s)"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

echo -e "${YELLOW}Test directory: $(pwd)${NC}"
echo ""

# Function to run command and check result
run_command() {
    local description="$1"
    local command="$2"
    
    echo -e "${YELLOW}>>> $description${NC}"
    echo "Command: $command"
    
    if eval "$command"; then
        echo -e "${GREEN}✓ Success${NC}"
        echo ""
        return 0
    else
        echo -e "${RED}✗ Failed${NC}"
        echo ""
        return 1
    fi
}

# Test 1: Create a basic contract solution
echo -e "${YELLOW}=== Test 1: Create Basic Contract Solution ===${NC}"
run_command "Creating contract solution" \
    "echo -e 'TestContract\nbasic\nTest Author\ntest@example.com' | make -C .. new-contract"

# Verify solution structure
echo -e "${YELLOW}Verifying solution structure...${NC}"
if [ -d "TestContract" ]; then
    echo -e "${GREEN}✓ Contract directory created${NC}"
    
    # Check for expected files
    if [ -f "TestContract/TestContract.sln" ]; then
        echo -e "${GREEN}✓ Solution file exists${NC}"
    else
        echo -e "${RED}✗ Solution file not found${NC}"
        exit 1
    fi
    
    if [ -d "TestContract/src" ] && [ -d "TestContract/tests" ]; then
        echo -e "${GREEN}✓ Project structure is correct${NC}"
    else
        echo -e "${RED}✗ Project structure is incorrect${NC}"
        exit 1
    fi
else
    echo -e "${RED}✗ Contract directory not created${NC}"
    exit 1
fi

cd TestContract

# Test 2: Build the contract solution
echo ""
echo -e "${YELLOW}=== Test 2: Build Contract Solution ===${NC}"
run_command "Building contract" "make -C ../.. build-contract"

# Test 3: Run tests
echo -e "${YELLOW}=== Test 3: Run Contract Tests ===${NC}"
run_command "Running tests" "make -C ../.. test-contract"

# Test 4: Create NEP-17 token contract
echo -e "${YELLOW}=== Test 4: Create NEP-17 Token Contract ===${NC}"
cd ..
run_command "Creating NEP-17 token" \
    "echo -e 'MyToken\nnep17\nTest Author\ntest@example.com' | make -C .. new-contract"

if [ -d "MyToken" ]; then
    cd MyToken
    run_command "Building NEP-17 token" "dotnet build"
    run_command "Testing NEP-17 token" "dotnet test"
    cd ..
fi

# Test 5: Create full solution template
echo -e "${YELLOW}=== Test 5: Create Full Solution Template ===${NC}"
run_command "Creating full solution" \
    "echo -e 'FullSolution\nsolution\nTest Author\ntest@example.com' | make -C .. new-contract"

if [ -d "FullSolution" ]; then
    echo -e "${YELLOW}Verifying full solution structure...${NC}"
    
    # Check deployment scripts
    if [ -d "FullSolution/deploy" ]; then
        echo -e "${GREEN}✓ Deployment directory exists${NC}"
    else
        echo -e "${RED}✗ Deployment directory not found${NC}"
    fi
    
    # Check for multiple projects
    project_count=$(find FullSolution -name "*.csproj" | wc -l)
    if [ "$project_count" -ge 2 ]; then
        echo -e "${GREEN}✓ Multiple projects found ($project_count)${NC}"
    else
        echo -e "${RED}✗ Expected multiple projects, found $project_count${NC}"
    fi
fi

# Test 6: Quick start workflow
echo ""
echo -e "${YELLOW}=== Test 6: Quick Start Workflow ===${NC}"
cd ..
run_command "Running quick start" "make quick-start"

# Summary
echo ""
echo -e "${YELLOW}=== Test Summary ===${NC}"
echo -e "${GREEN}✓ Contract creation workflow tested${NC}"
echo -e "${GREEN}✓ Build process verified${NC}"
echo -e "${GREEN}✓ Test execution confirmed${NC}"
echo -e "${GREEN}✓ Multiple templates validated${NC}"
echo -e "${GREEN}✓ Solution structure verified${NC}"

# Cleanup
cd ..
echo ""
echo -e "${YELLOW}Cleaning up test directory...${NC}"
rm -rf "$TEST_DIR"

echo ""
echo -e "${GREEN}=== All workflow tests passed! ===${NC}"