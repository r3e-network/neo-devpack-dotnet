#!/bin/bash
# Clean testing artifacts to ensure fresh generation

echo "Cleaning testing artifacts..."
rm -rf tests/TestingArtifacts/*.cs
echo "Artifacts cleaned. Tests will regenerate them on next run."