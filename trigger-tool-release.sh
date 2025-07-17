#!/bin/bash
# Trigger RNCC tool release via GitHub Actions

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${YELLOW}=== RNCC Tool Release Trigger ===${NC}"
echo ""

# Get current version
VERSION=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
echo -e "${YELLOW}Current version: ${VERSION}${NC}"

# Check if we're on the r3e branch
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" != "r3e" ]; then
    echo -e "${RED}Warning: Not on r3e branch (current: $CURRENT_BRANCH)${NC}"
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Check for uncommitted changes
if [ -n "$(git status --porcelain)" ]; then
    echo -e "${RED}Error: Uncommitted changes detected${NC}"
    echo "Please commit or stash your changes before releasing"
    exit 1
fi

# Show release options
echo ""
echo -e "${YELLOW}Release Options:${NC}"
echo "1. Create version tag (v${VERSION}) - Triggers automatic NuGet publish"
echo "2. Create tool-specific tag (tool-v${VERSION}) - Tool release only"
echo "3. Manual trigger via GitHub Actions (no tag)"
echo ""
read -p "Select option (1-3): " -n 1 -r OPTION
echo ""

case $OPTION in
    1)
        TAG="v${VERSION}"
        echo -e "${YELLOW}Creating version tag: ${TAG}${NC}"
        ;;
    2)
        TAG="tool-v${VERSION}"
        echo -e "${YELLOW}Creating tool-specific tag: ${TAG}${NC}"
        ;;
    3)
        echo -e "${YELLOW}Manual trigger selected${NC}"
        echo "Please go to:"
        echo "https://github.com/r3e-network/r3e-devpack-dotnet/actions/workflows/publish-tool.yml"
        echo "Click 'Run workflow' and enter version: ${VERSION}"
        echo "Set 'Publish to NuGet' to 'yes'"
        exit 0
        ;;
    *)
        echo -e "${RED}Invalid option${NC}"
        exit 1
        ;;
esac

# Confirm tag creation
echo ""
echo -e "${YELLOW}This will create and push tag: ${TAG}${NC}"
echo "This triggers automatic publishing to NuGet.org"
read -p "Continue? (y/N): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Cancelled"
    exit 0
fi

# Create and push tag
echo -e "${YELLOW}Creating tag...${NC}"
git tag -a "$TAG" -m "Release RNCC Tool ${VERSION}"

echo -e "${YELLOW}Pushing tag to origin...${NC}"
git push origin "$TAG"

echo ""
echo -e "${GREEN}✅ Release triggered successfully!${NC}"
echo ""
echo "Next steps:"
echo "1. Monitor the GitHub Actions workflow:"
echo "   https://github.com/r3e-network/r3e-devpack-dotnet/actions"
echo ""
echo "2. Once published, verify installation:"
echo "   dotnet tool install -g R3E.Compiler.CSharp.Tool"
echo "   rncc --version"
echo ""
echo "3. Package will be available at:"
echo "   https://www.nuget.org/packages/R3E.Compiler.CSharp.Tool/"