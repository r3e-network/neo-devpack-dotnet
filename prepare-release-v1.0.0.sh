#!/bin/bash

# R3E DevPack v1.0.0 Release Preparation Script

echo "🚀 Preparing R3E DevPack v1.0.0 Release..."

# Create release directory
RELEASE_DIR="./release-v1.0.0"
mkdir -p "$RELEASE_DIR"

# Copy NuGet packages
echo "📦 Copying NuGet packages..."
cp artifacts/*.nupkg "$RELEASE_DIR/"

# Create binary archives (we only have Linux binary built)
echo "📁 Creating binary archives..."
cd artifacts/binaries/linux-x64
tar -czf "../../../$RELEASE_DIR/rncc-linux-x64-v1.0.0.tar.gz" .
cd ../../..

# Copy release notes and changelog
echo "📝 Copying documentation..."
cp RELEASE_NOTES_v1.0.0.md "$RELEASE_DIR/"
cp CHANGELOG.md "$RELEASE_DIR/"

# Create a summary file
echo "📊 Creating release summary..."
cat > "$RELEASE_DIR/RELEASE_SUMMARY.txt" << EOF
R3E DevPack v1.0.0 Release
==========================

Release Date: $(date -u +"%Y-%m-%d %H:%M:%S UTC")

NuGet Packages:
--------------
EOF

ls -1 "$RELEASE_DIR"/*.nupkg | xargs -n1 basename >> "$RELEASE_DIR/RELEASE_SUMMARY.txt"

cat >> "$RELEASE_DIR/RELEASE_SUMMARY.txt" << EOF

Binary Archives:
---------------
- rncc-linux-x64-v1.0.0.tar.gz

Installation:
------------
To install RNCC CLI tool:
  dotnet tool install -g R3E.Compiler.CSharp.Tool --version 1.0.0

To add framework to your project:
  <PackageReference Include="R3E.SmartContract.Framework" Version="1.0.0" />

Quick Start:
-----------
  rncc new MyContract --template=solution
  cd MyContract
  dotnet build
  dotnet test

Documentation:
-------------
- Full release notes: RELEASE_NOTES_v1.0.0.md
- Changelog: CHANGELOG.md
- Online docs: https://r3edevpack.netlify.app

EOF

echo "✅ Release preparation complete!"
echo "📂 Release files are in: $RELEASE_DIR"
echo ""
echo "Next steps:"
echo "1. Review the release files in $RELEASE_DIR"
echo "2. Create a git tag: git tag v1.0.0 -m 'Release v1.0.0'"
echo "3. Push the tag: git push origin v1.0.0"
echo "4. Create GitHub release and upload artifacts from $RELEASE_DIR"
echo "5. Publish packages to NuGet.org"