#!/bin/bash
# Simple build script for Netlify to ensure correct directory structure

echo "Build script starting..."
echo "Current directory: $(pwd)"
echo "Directory contents:"
ls -la

echo "Website directory contents:"
ls -la website/

echo "Build complete - website directory should be used as publish directory"