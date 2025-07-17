#!/bin/bash
# Script to recreate tags to trigger GitHub Actions

echo "Recreating tags to trigger GitHub Actions workflow..."
echo ""

# Function to recreate a tag
recreate_tag() {
    local tag=$1
    echo "Recreating tag: $tag"
    
    # Delete local tag
    git tag -d "$tag" 2>/dev/null
    
    # Delete remote tag
    git push origin :refs/tags/"$tag" 2>/dev/null
    
    # Create new tag
    git tag "$tag"
    
    # Push new tag
    git push origin "$tag"
    
    echo "Tag $tag recreated and pushed"
    echo ""
}

# Ask for confirmation
read -p "This will delete and recreate the release tags. Continue? (y/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    # Recreate the v1.0.0-rncc tag (matches v* pattern)
    recreate_tag "v1.0.0-rncc"
    
    echo "Done! Check https://github.com/[your-username]/r3e-devpack-dotnet/actions"
    echo "The workflow should now be running."
else
    echo "Cancelled."
fi