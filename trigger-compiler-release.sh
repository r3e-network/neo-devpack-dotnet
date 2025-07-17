#!/bin/bash
# Script to help trigger the compiler binary release workflow

echo "R3E Neo Contract Compiler (RNCC) Release Trigger"
echo "================================================"
echo ""
echo "Since the GitHub Actions workflow may not have triggered automatically,"
echo "you can manually trigger it through the GitHub web interface:"
echo ""
echo "1. Go to: https://github.com/[your-username]/r3e-devpack-dotnet/actions"
echo "2. Click on 'Build Compiler Binaries' workflow"
echo "3. Click 'Run workflow' button"
echo "4. Select branch: r3e"
echo "5. Enter version: v1.0.0-rncc (or leave blank for default)"
echo "6. Click 'Run workflow'"
echo ""
echo "Alternatively, if you have GitHub CLI installed elsewhere, run:"
echo "gh workflow run build-compiler-binaries.yml --ref r3e -f version=v1.0.0-rncc"
echo ""
echo "Current tags that should trigger the workflow:"
git tag -l | grep -E "(v1.0.0-rncc|rncc-v1.0.0)" | while read tag; do
    echo "  - $tag"
done
echo ""
echo "The workflow is configured to trigger on:"
echo "  - Tags matching: v*"
echo "  - Tags matching: compiler-v*"
echo "  - Tags matching: rncc-v*"
echo "  - Manual workflow_dispatch"