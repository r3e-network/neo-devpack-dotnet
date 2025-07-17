# Contributing to R3E Neo Contract DevPack

Thank you for your interest in contributing to the R3E Neo Contract DevPack! This document provides guidelines and instructions for contributors.

## Quick Start for Contributors

The project includes a comprehensive Makefile that automates all common development tasks:

```bash
# Show all available make targets
make help

# Quick development workflow
make dev                # Clean, build, and run unit tests
make ci                 # Full CI build (clean, restore, build, test, lint, security scan)
make all               # Complete build (clean, restore, build, test, pack)
```

## Development Environment Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/) with submodules support
- [Make](https://www.gnu.org/software/make/) (or use `make.bat` on Windows)

### Initial Setup

1. Clone the repository with submodules:
```bash
git clone --recurse-submodules https://github.com/r3e-network/r3e-devpack-dotnet.git
cd r3e-devpack-dotnet
```

2. Install required tools:
```bash
make install-tools
```

3. Build and test:
```bash
make all
```

## Development Workflow

### Common Commands

```bash
# Development
make dev                 # Quick development build
make build              # Build all projects
make test               # Run all tests
make clean              # Clean build artifacts

# Component-specific builds
make compiler-only      # Build only the compiler
make testing-only       # Build only testing framework
make webgui-only        # Build only WebGUI service

# Testing
make unit-tests         # Run unit tests only
make integration-tests  # Run integration tests only
make coverage           # Generate code coverage report
make benchmarks         # Run performance benchmarks

# Code quality
make format             # Format all code
make lint               # Run linters
make security-scan      # Check for security vulnerabilities
```

### Project Structure

```
r3e-devpack-dotnet/
├── src/                           # Source code
│   ├── Neo.Compiler.CSharp/       # RNCC compiler
│   ├── Neo.SmartContract.Framework/ # Contract framework
│   ├── Neo.SmartContract.Testing/ # Testing framework
│   ├── Neo.SmartContract.Deploy/  # Deployment toolkit
│   └── Neo.WebGUI.Service/        # WebGUI hosting service
├── tests/                         # Test projects
├── examples/                      # Example contracts
├── website/                       # Documentation website
├── Makefile                       # Build automation
└── make.bat                       # Windows wrapper
```

## Contributing Guidelines

### Code Style

- Follow existing code style and conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Format code before submitting: `make format`

### Testing

- Write unit tests for all new functionality
- Include integration tests for complex features
- Ensure all tests pass: `make test`
- Check code coverage: `make coverage`

### Security

- Run security scans: `make security-scan`
- Never commit sensitive information (keys, passwords, etc.)
- Follow security best practices for smart contracts

### Documentation

- Update documentation for new features
- Include code examples where appropriate
- Update the website if adding user-facing features

## Submitting Changes

### Pull Request Process

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes
4. Run the full CI build: `make ci`
5. Commit your changes: `git commit -am 'Add your feature'`
6. Push to your fork: `git push origin feature/your-feature`
7. Create a Pull Request

### Commit Message Format

Use conventional commit messages:
```
feat: add new compiler optimization
fix: resolve memory leak in testing framework
docs: update getting started guide
test: add integration tests for WebGUI service
```

### Pull Request Checklist

- [ ] Code follows project style guidelines
- [ ] All tests pass (`make test`)
- [ ] Code coverage is maintained or improved
- [ ] Documentation is updated
- [ ] Security scan passes (`make security-scan`)
- [ ] Commit messages follow conventional format

## Advanced Development

### Building Specific Components

```bash
# Build only what you're working on
make compiler-only      # Just the compiler
make testing-only       # Just testing framework
make deploy-only        # Just deployment toolkit
make webgui-only        # Just WebGUI service
```

### Release Process

```bash
# Create a full release
make release            # Build, test, pack, and tag

# Create platform-specific binaries
make release-binaries   # Build RNCC for all platforms
make release-compiler   # Package compiler binaries
```

### Docker Development

```bash
# Build and run WebGUI service
make docker-build       # Build Docker images
make docker-run         # Run service in Docker
make docker-compose-up  # Start all services
```

### Website Development

```bash
# Work on documentation website
make website            # Build website
make serve-website      # Serve locally on port 8000
make deploy-website     # Deploy to production
```

### Performance Analysis

```bash
# Run performance analysis
make benchmarks         # Run benchmark tests
make profile           # Profile compiler performance
```

## Troubleshooting

### Common Issues

1. **Build fails**: Run `make clean` then `make restore`
2. **Tests fail**: Check if you need to update submodules: `git submodule update --init --recursive`
3. **Missing tools**: Run `make install-tools`
4. **Permission issues**: Ensure you have write permissions in the project directory

### Getting Help

- Check the [documentation website](https://r3edevpack.netlify.app)
- Review existing [GitHub issues](https://github.com/r3e-network/r3e-devpack-dotnet/issues)
- Ask questions in [GitHub discussions](https://github.com/r3e-network/r3e-devpack-dotnet/discussions)

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

## Code of Conduct

Please note that this project is released with a [Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project, you agree to abide by its terms.

## Recognition

All contributors are recognized in our [CONTRIBUTORS.md](CONTRIBUTORS.md) file. Thank you for helping make Neo smart contract development better!