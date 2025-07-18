# Release Checklist - R3E DevPack v1.0.0

Use this checklist to ensure all release steps are completed:

## Pre-Release Preparation ✅

- [x] Update version numbers in all projects to 1.0.0
- [x] Update Directory.Build.props version
- [x] Update Makefile version
- [x] Create comprehensive release notes
- [x] Update CHANGELOG.md
- [x] Update README.md with v1.0.0 information
- [x] Update package descriptions
- [x] Create migration guide
- [x] Create quick start guide
- [x] Create GitHub issue templates
- [x] Build all packages
- [x] Run integration tests
- [x] Create release artifacts directory
- [x] Generate Linux binary
- [x] Create release announcement

## Git Operations ✅

- [x] Stage all changes
- [x] Create release commit
- [x] Create v1.0.0 tag
- [ ] Push commits to origin
- [ ] Push tag to origin

## GitHub Release 🔄

- [ ] Navigate to GitHub releases page
- [ ] Click "Create a new release"
- [ ] Select tag v1.0.0
- [ ] Set release title: "R3E DevPack v1.0.0 - First Major Release"
- [ ] Copy release notes content
- [ ] Upload artifacts:
  - [ ] All .nupkg files from release-v1.0.0/
  - [ ] rncc-linux-x64-v1.0.0.tar.gz
  - [ ] RELEASE_NOTES_v1.0.0.md
  - [ ] CHANGELOG.md
  - [ ] QUICK_START_v1.0.0.md
  - [ ] MIGRATION_GUIDE_v1.0.0.md
- [ ] Mark as latest release
- [ ] Publish release

## NuGet Publishing 🔄

For each package in release-v1.0.0/:

- [ ] R3E.Compiler.CSharp.Tool.1.0.0.nupkg
- [ ] R3E.SmartContract.Framework.1.0.0.nupkg
- [ ] R3E.SmartContract.Testing.1.0.0.nupkg
- [ ] R3E.SmartContract.Deploy.1.0.0.nupkg
- [ ] R3E.WebGUI.Service.1.0.0.nupkg
- [ ] R3E.WebGUI.Deploy.1.0.0.nupkg
- [ ] R3E.SmartContract.Template.1.0.0.nupkg
- [ ] R3E.SmartContract.Analyzer.1.0.0.nupkg
- [ ] R3E.Disassembler.CSharp.1.0.0.nupkg
- [ ] R3E.Compiler.CSharp.1.0.0.nupkg

Command:
```bash
dotnet nuget push [package].nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Documentation Updates 🔄

- [ ] Update documentation site with v1.0.0
- [ ] Add release announcement to site
- [ ] Update installation instructions
- [ ] Update API documentation
- [ ] Add migration guide to docs
- [ ] Add quick start guide to docs

## Community Announcements 🔄

- [ ] Post announcement on Discord
- [ ] Tweet release announcement
- [ ] Update project website
- [ ] Send newsletter (if applicable)
- [ ] Post on Reddit r/NEO
- [ ] Update Neo community forums

## Post-Release Tasks 🔄

- [ ] Monitor GitHub issues for early feedback
- [ ] Check NuGet package downloads
- [ ] Respond to community questions
- [ ] Plan webinar/tutorial for new features
- [ ] Update roadmap for next version
- [ ] Create v1.0.1 milestone for patches

## Verification Steps 🔄

- [ ] Test package installation:
  ```bash
  dotnet tool install -g R3E.Compiler.CSharp.Tool --version 1.0.0
  ```
- [ ] Test template creation:
  ```bash
  rncc new TestV1 --template=solution
  ```
- [ ] Verify documentation links work
- [ ] Check all download links
- [ ] Confirm packages appear on NuGet.org

## Rollback Plan (If Needed)

1. Unlist packages from NuGet (not delete)
2. Mark GitHub release as pre-release
3. Investigate issues
4. Fix problems
5. Release v1.0.1 with fixes

## Success Metrics

Track these after release:
- Package download counts
- GitHub stars/forks
- Community feedback
- Issue reports
- Discord activity

---

**Release Manager**: ___________________
**Date**: January 18, 2025
**Status**: Ready for Release 🚀