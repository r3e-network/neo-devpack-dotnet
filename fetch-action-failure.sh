#!/bin/bash

# Script to help fetch GitHub Action failure details
# This provides commands to check the latest workflow runs

echo "GitHub Action Failure Analysis Helper"
echo "===================================="
echo ""
echo "To check the latest workflow runs, use these commands:"
echo ""
echo "1. List recent workflow runs:"
echo "   gh run list --repo r3e-network/r3e-devpack-dotnet --limit 5"
echo ""
echo "2. View details of a specific run (replace RUN_ID):"
echo "   gh run view RUN_ID --repo r3e-network/r3e-devpack-dotnet"
echo ""
echo "3. View logs of a failed run (replace RUN_ID):"
echo "   gh run view RUN_ID --repo r3e-network/r3e-devpack-dotnet --log-failed"
echo ""
echo "4. Download logs for detailed analysis (replace RUN_ID):"
echo "   gh run download RUN_ID --repo r3e-network/r3e-devpack-dotnet"
echo ""
echo "5. Re-run a failed workflow (replace RUN_ID):"
echo "   gh run rerun RUN_ID --repo r3e-network/r3e-devpack-dotnet"
echo ""
echo "6. Watch a running workflow (replace RUN_ID):"
echo "   gh run watch RUN_ID --repo r3e-network/r3e-devpack-dotnet"
echo ""

# Check if gh CLI is available
if command -v gh &> /dev/null; then
    echo "GitHub CLI is installed. Fetching recent runs..."
    echo ""
    gh run list --repo r3e-network/r3e-devpack-dotnet --limit 10
else
    echo "GitHub CLI (gh) is not installed."
    echo "Install it with: sudo apt install gh (Ubuntu) or brew install gh (macOS)"
    echo ""
    echo "Or check manually at:"
    echo "https://github.com/r3e-network/r3e-devpack-dotnet/actions"
fi