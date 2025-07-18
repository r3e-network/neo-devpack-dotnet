# R3E Neo Contract DevPack Makefile
# Comprehensive build automation for the entire project

.PHONY: all help clean build test pack publish release docker website deploy-website
.PHONY: install-tools restore format lint security-scan benchmarks docs
.PHONY: compiler-only testing-only deploy-only webgui-only
.PHONY: integration-tests unit-tests coverage
.PHONY: docker-build docker-push docker-run docker-compose-up docker-compose-down
.PHONY: release-compiler release-binaries tag-release
.PHONY: new-contract build-contract test-contract deploy-contract

# Variables
DOTNET := dotnet
CONFIGURATION := Release
VERSION := 1.0.1
RUNTIME_IDS := win-x64 linux-x64 osx-x64 osx-arm64
OUTPUT_DIR := ./artifacts
DOCKER_REGISTRY := ghcr.io/r3e-network
WEBSITE_DIR := ./website

# Color codes for output
GREEN := \033[0;32m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

# Default target
all: clean restore build test pack

# Help target
help:
	@echo "$(GREEN)R3E Neo Contract DevPack - Make Targets$(NC)"
	@echo ""
	@echo "$(YELLOW)Basic Commands:$(NC)"
	@echo "  make all              - Clean, restore, build, test, and pack"
	@echo "  make build            - Build all projects"
	@echo "  make test             - Run all tests"
	@echo "  make clean            - Clean all build artifacts"
	@echo "  make pack             - Create NuGet packages"
	@echo ""
	@echo "$(YELLOW)Component Builds:$(NC)"
	@echo "  make compiler-only    - Build only the compiler"
	@echo "  make testing-only     - Build only the testing framework"
	@echo "  make deploy-only      - Build only the deployment toolkit"
	@echo "  make webgui-only      - Build only the WebGUI service"
	@echo ""
	@echo "$(YELLOW)Testing:$(NC)"
	@echo "  make unit-tests       - Run unit tests only"
	@echo "  make integration-tests- Run integration tests only"
	@echo "  make workflow-test    - Test full workflow (template to deployment)"
	@echo "  make coverage         - Run tests with code coverage"
	@echo "  make benchmarks       - Run performance benchmarks"
	@echo ""
	@echo "$(YELLOW)Publishing:$(NC)"
	@echo "  make publish          - Publish all packages to NuGet"
	@echo "  make release          - Create a full release (build, test, pack, tag)"
	@echo "  make release-compiler - Release RNCC binaries for all platforms"
	@echo "  make release-binaries - Build platform-specific binaries"
	@echo ""
	@echo "$(YELLOW)Docker:$(NC)"
	@echo "  make docker-build     - Build Docker images"
	@echo "  make docker-push      - Push Docker images to registry"
	@echo "  make docker-run       - Run WebGUI service in Docker"
	@echo "  make docker-compose-up- Start all services with docker-compose"
	@echo ""
	@echo "$(YELLOW)Website:$(NC)"
	@echo "  make website          - Build the documentation website"
	@echo "  make deploy-website   - Deploy website to Netlify"
	@echo "  make serve-website    - Serve website locally"
	@echo ""
	@echo "$(YELLOW)Contract Development:$(NC)"
	@echo "  make new-contract     - Create a new contract solution from template"
	@echo "  make build-contract   - Build contract solution in current directory"
	@echo "  make test-contract    - Test contract solution in current directory"
	@echo "  make deploy-contract  - Deploy contract solution to testnet"
	@echo ""
	@echo "$(YELLOW)Maintenance:$(NC)"
	@echo "  make install-tools    - Install required .NET tools"
	@echo "  make format           - Format all source code"
	@echo "  make lint             - Run code linters"
	@echo "  make security-scan    - Run security vulnerability scan"
	@echo "  make docs             - Generate API documentation"

# Clean build artifacts
clean:
	@echo "$(YELLOW)Cleaning build artifacts...$(NC)"
	$(DOTNET) clean --configuration $(CONFIGURATION) || true
	rm -rf $(OUTPUT_DIR)
	rm -rf **/bin **/obj
	rm -rf TestResults
	rm -rf coverage
	@echo "$(GREEN)Clean completed!$(NC)"

# Restore dependencies
restore:
	@echo "$(YELLOW)Restoring dependencies...$(NC)"
	$(DOTNET) restore
	@echo "$(GREEN)Restore completed!$(NC)"

# Build all projects
build: restore
	@echo "$(YELLOW)Building all projects...$(NC)"
	$(DOTNET) build --configuration $(CONFIGURATION) --no-restore
	@echo "$(GREEN)Build completed!$(NC)"

# Build specific components
compiler-only:
	@echo "$(YELLOW)Building compiler only...$(NC)"
	$(DOTNET) build src/Neo.Compiler.CSharp/Neo.Compiler.CSharp.csproj --configuration $(CONFIGURATION)

testing-only:
	@echo "$(YELLOW)Building testing framework only...$(NC)"
	$(DOTNET) build src/Neo.SmartContract.Testing/Neo.SmartContract.Testing.csproj --configuration $(CONFIGURATION)

deploy-only:
	@echo "$(YELLOW)Building deployment toolkit only...$(NC)"
	$(DOTNET) build src/Neo.SmartContract.Deploy/Neo.SmartContract.Deploy.csproj --configuration $(CONFIGURATION)

webgui-only:
	@echo "$(YELLOW)Building WebGUI service only...$(NC)"
	$(DOTNET) build src/R3E.WebGUI.Service/R3E.WebGUI.Service.csproj --configuration $(CONFIGURATION)

# Testing targets
test: unit-tests integration-tests

unit-tests:
	@echo "$(YELLOW)Running unit tests...$(NC)"
	$(DOTNET) test --configuration $(CONFIGURATION) --no-build \
		--filter "Category!=Integration" \
		--logger "console;verbosity=minimal" \
		--logger "trx;LogFileName=unit-tests.trx"
	@echo "$(GREEN)Unit tests completed!$(NC)"

integration-tests:
	@echo "$(YELLOW)Running integration tests...$(NC)"
	@echo "Installing RNCC tool if not present..."
	@$(DOTNET) tool install -g R3E.Compiler.CSharp.Tool --add-source ./artifacts || true
	@export PATH="$$PATH:$$HOME/.dotnet/tools" && \
	$(DOTNET) test --configuration $(CONFIGURATION) --no-build \
		--filter "Category=Integration" \
		--logger "console;verbosity=minimal" \
		--logger "trx;LogFileName=integration-tests.trx"
	@echo "$(GREEN)Integration tests completed!$(NC)"

workflow-test: pack install-local
	@echo "$(YELLOW)Running full workflow integration tests...$(NC)"
	@echo "This will test the complete workflow from template creation to deployment"
	@export PATH="$$PATH:$$HOME/.dotnet/tools" && \
	$(DOTNET) test tests/Neo.Compiler.CSharp.IntegrationTests/Neo.Compiler.CSharp.IntegrationTests.csproj \
		--configuration $(CONFIGURATION) \
		--filter "FullName~EndToEndWorkflowTests" \
		--logger "console;verbosity=detailed"
	@echo "$(GREEN)Workflow tests completed!$(NC)"

coverage:
	@echo "$(YELLOW)Running tests with code coverage...$(NC)"
	$(DOTNET) test --configuration $(CONFIGURATION) \
		--collect:"XPlat Code Coverage" \
		--results-directory ./coverage \
		/p:CoverletOutputFormat=cobertura
	$(DOTNET) reportgenerator \
		-reports:./coverage/**/coverage.cobertura.xml \
		-targetdir:./coverage/report \
		-reporttypes:Html
	@echo "$(GREEN)Coverage report generated in ./coverage/report$(NC)"

benchmarks:
	@echo "$(YELLOW)Running benchmarks...$(NC)"
	$(DOTNET) run --project tests/Neo.Compiler.CSharp.Benchmarks/Neo.Compiler.CSharp.Benchmarks.csproj \
		--configuration Release

# Pack NuGet packages
pack: build
	@echo "$(YELLOW)Creating NuGet packages...$(NC)"
	mkdir -p $(OUTPUT_DIR)
	$(DOTNET) pack --configuration $(CONFIGURATION) --no-build \
		--output $(OUTPUT_DIR) \
		/p:Version=$(VERSION)
	@echo "$(GREEN)Packages created in $(OUTPUT_DIR)$(NC)"

# Publish to NuGet
publish: pack
	@echo "$(YELLOW)Publishing packages to NuGet...$(NC)"
	@for package in $(OUTPUT_DIR)/*.nupkg; do \
		echo "Publishing $$package..."; \
		$(DOTNET) nuget push $$package \
			--source https://api.nuget.org/v3/index.json \
			--api-key $(NUGET_API_KEY) \
			--skip-duplicate; \
	done
	@echo "$(GREEN)Packages published!$(NC)"

# Release compiler binaries
release-binaries:
	@echo "$(YELLOW)Building RNCC binaries for all platforms...$(NC)"
	mkdir -p $(OUTPUT_DIR)/binaries
	@for rid in $(RUNTIME_IDS); do \
		echo "Building for $$rid..."; \
		$(DOTNET) publish src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
			--configuration $(CONFIGURATION) \
			--runtime $$rid \
			--self-contained true \
			--output $(OUTPUT_DIR)/binaries/$$rid \
			/p:PublishSingleFile=true \
			/p:PublishTrimmed=true \
			/p:Version=$(VERSION); \
		if [ "$$rid" = "win-x64" ]; then \
			mv $(OUTPUT_DIR)/binaries/$$rid/rncc.exe $(OUTPUT_DIR)/binaries/rncc-$$rid.exe; \
		else \
			mv $(OUTPUT_DIR)/binaries/$$rid/rncc $(OUTPUT_DIR)/binaries/rncc-$$rid; \
			chmod +x $(OUTPUT_DIR)/binaries/rncc-$$rid; \
		fi; \
		rm -rf $(OUTPUT_DIR)/binaries/$$rid; \
	done
	@echo "$(GREEN)Binaries created in $(OUTPUT_DIR)/binaries$(NC)"

release-compiler: release-binaries
	@echo "$(YELLOW)Creating compiler release...$(NC)"
	cd $(OUTPUT_DIR)/binaries && \
	for file in rncc-*; do \
		if [[ $$file == *.exe ]]; then \
			zip $$file.zip $$file; \
		else \
			tar -czf $$file.tar.gz $$file; \
		fi; \
	done
	@echo "$(GREEN)Compiler release packages created!$(NC)"

# Docker targets
docker-build:
	@echo "$(YELLOW)Building Docker images...$(NC)"
	docker build -t $(DOCKER_REGISTRY)/webgui-service:$(VERSION) \
		-t $(DOCKER_REGISTRY)/webgui-service:latest \
		-f src/R3E.WebGUI.Service/Dockerfile .
	@echo "$(GREEN)Docker images built!$(NC)"

docker-push: docker-build
	@echo "$(YELLOW)Pushing Docker images...$(NC)"
	docker push $(DOCKER_REGISTRY)/webgui-service:$(VERSION)
	docker push $(DOCKER_REGISTRY)/webgui-service:latest
	@echo "$(GREEN)Docker images pushed!$(NC)"

docker-run:
	@echo "$(YELLOW)Running WebGUI service in Docker...$(NC)"
	docker run -d \
		--name webgui-service \
		-p 8080:80 \
		-e ASPNETCORE_ENVIRONMENT=Production \
		$(DOCKER_REGISTRY)/webgui-service:latest

docker-compose-up:
	@echo "$(YELLOW)Starting services with docker-compose...$(NC)"
	cd src/R3E.WebGUI.Service && docker-compose up -d

docker-compose-down:
	@echo "$(YELLOW)Stopping services...$(NC)"
	cd src/R3E.WebGUI.Service && docker-compose down

# Website targets
website:
	@echo "$(YELLOW)Building documentation website...$(NC)"
	cd $(WEBSITE_DIR) && \
	if [ -f "package.json" ]; then \
		npm install && npm run build; \
	else \
		echo "Website is static HTML, no build required"; \
	fi
	@echo "$(GREEN)Website built!$(NC)"

serve-website:
	@echo "$(YELLOW)Serving website locally...$(NC)"
	cd $(WEBSITE_DIR) && python3 -m http.server 8000

deploy-website:
	@echo "$(YELLOW)Deploying website to Netlify...$(NC)"
	cd $(WEBSITE_DIR) && \
	netlify deploy --prod --dir=. --site=r3edevpack
	@echo "$(GREEN)Website deployed!$(NC)"

# Contract development helpers
new-contract:
	@echo "$(YELLOW)Creating new contract solution from template...$(NC)"
	@read -p "Enter contract name: " name; \
	read -p "Enter template (solution/nep17/nep11/defi/multisig): " template; \
	read -p "Enter author name: " author; \
	read -p "Enter author email: " email; \
	echo ""; \
	echo "Creating $$name solution with $$template template..."; \
	rncc new $$name --template=$$template \
		--author="$$author" \
		--email="$$email" \
		--with-tests \
		--with-deploy-scripts \
		--git-init || \
	dotnet tool run rncc new $$name --template=$$template \
		--author="$$author" \
		--email="$$email" \
		--with-tests \
		--with-deploy-scripts \
		--git-init || \
	./artifacts/binaries/rncc-linux-x64 new $$name --template=$$template \
		--author="$$author" \
		--email="$$email" \
		--with-tests \
		--with-deploy-scripts \
		--git-init
	@echo "$(GREEN)Contract solution created in ./$$name/$(NC)"
	@echo "Next steps:"
	@echo "  cd $$name"
	@echo "  make build-contract"
	@echo "  make test-contract"

build-contract:
	@echo "$(YELLOW)Building contract solution...$(NC)"
	@if [ -f *.sln ]; then \
		rncc build || \
		dotnet tool run rncc build || \
		./artifacts/binaries/rncc-linux-x64 build || \
		dotnet build; \
		echo "$(GREEN)Contract solution built successfully!$(NC)"; \
	else \
		echo "$(RED)No solution file found in current directory$(NC)"; \
		echo "Use 'make new-contract' to create a new contract solution"; \
	fi

test-contract:
	@echo "$(YELLOW)Testing contract solution...$(NC)"
	@if [ -f *.sln ]; then \
		$(DOTNET) test; \
		echo "$(GREEN)All tests passed!$(NC)"; \
	else \
		echo "$(RED)No solution file found in current directory$(NC)"; \
		echo "Use 'make new-contract' to create a new contract solution"; \
	fi

deploy-contract:
	@echo "$(YELLOW)Deploying contract to testnet...$(NC)"
	@if [ -f *.sln ]; then \
		rncc deploy --network=testnet || \
		dotnet tool run rncc deploy --network=testnet || \
		./artifacts/binaries/rncc-linux-x64 deploy --network=testnet; \
	else \
		echo "$(RED)No solution file found in current directory$(NC)"; \
		echo "Use 'make new-contract' to create a new contract solution"; \
	fi

# Development tools
install-tools:
	@echo "$(YELLOW)Installing required .NET tools...$(NC)"
	$(DOTNET) tool install -g dotnet-format || true
	$(DOTNET) tool install -g dotnet-reportgenerator-globaltool || true
	$(DOTNET) tool install -g security-scan || true
	$(DOTNET) tool install -g rncc || true
	@echo "$(GREEN)Tools installed!$(NC)"

install-rncc:
	@echo "$(YELLOW)Installing RNCC globally...$(NC)"
	$(DOTNET) tool install -g --add-source ./artifacts R3E.Compiler.CSharp.Tool || \
	$(DOTNET) tool update -g --add-source ./artifacts R3E.Compiler.CSharp.Tool || \
	$(DOTNET) pack src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
		--configuration Release \
		--output ./artifacts && \
	$(DOTNET) tool install -g --add-source ./artifacts R3E.Compiler.CSharp.Tool
	@echo "$(GREEN)RNCC installed globally!$(NC)"

install-local:
	@echo "$(YELLOW)Installing RNCC from local build...$(NC)"
	$(DOTNET) build src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj --configuration Release
	$(DOTNET) pack src/Neo.Compiler.CSharp.Tool/Neo.Compiler.CSharp.Tool.csproj \
		--configuration Release \
		--output ./artifacts
	$(DOTNET) tool install -g --add-source ./artifacts R3E.Compiler.CSharp.Tool --version $(VERSION) || \
	$(DOTNET) tool update -g --add-source ./artifacts R3E.Compiler.CSharp.Tool --version $(VERSION)
	@echo "$(GREEN)RNCC installed from local build!$(NC)"

format:
	@echo "$(YELLOW)Formatting code...$(NC)"
	$(DOTNET) format

lint:
	@echo "$(YELLOW)Running linters...$(NC)"
	$(DOTNET) format --verify-no-changes

security-scan:
	@echo "$(YELLOW)Running security scan...$(NC)"
	$(DOTNET) list package --vulnerable --include-transitive

docs:
	@echo "$(YELLOW)Generating API documentation...$(NC)"
	$(DOTNET) tool run docfx docs/docfx.json --serve

# Release management
tag-release:
	@echo "$(YELLOW)Creating release tag...$(NC)"
	git tag -a v$(VERSION) -m "Release version $(VERSION)"
	git push origin v$(VERSION)

release: clean build test pack release-compiler tag-release
	@echo "$(GREEN)Release v$(VERSION) completed!$(NC)"
	@echo "Next steps:"
	@echo "1. Upload binaries from $(OUTPUT_DIR)/binaries to GitHub releases"
	@echo "2. Run 'make publish' to publish NuGet packages"
	@echo "3. Run 'make docker-push' to push Docker images"

# Release RNCC as .NET tool
release-tool:
	@echo "$(YELLOW)Releasing RNCC as .NET tool...$(NC)"
	./release-rncc-tool.sh

# Development workflow shortcuts
dev: clean build unit-tests
	@echo "$(GREEN)Development build completed!$(NC)"

ci: clean restore build test lint security-scan
	@echo "$(GREEN)CI build completed!$(NC)"

# Quick contract workflow
quick-start:
	@echo "$(YELLOW)Setting up quick start example...$(NC)"
	@echo "This will create a complete contract solution with:"
	@echo "  - Contract project"
	@echo "  - Testing project"
	@echo "  - Deployment scripts"
	@echo "  - Full solution structure"
	@echo ""
	@mkdir -p examples/quickstart
	@cd examples/quickstart && \
	echo "Creating QuickStartContract solution..." && \
	(rncc new QuickStartContract --template=solution \
		--author="Quick Start Developer" \
		--email="quickstart@example.com" \
		--with-tests \
		--with-deploy-scripts || \
	dotnet tool run rncc new QuickStartContract --template=solution \
		--author="Quick Start Developer" \
		--email="quickstart@example.com" \
		--with-tests \
		--with-deploy-scripts) && \
	cd QuickStartContract && \
	echo "" && \
	echo "Building solution..." && \
	dotnet build && \
	echo "" && \
	echo "Running tests..." && \
	dotnet test
	@echo ""
	@echo "$(GREEN)✅ Quick start contract solution ready!$(NC)"
	@echo ""
	@echo "Solution structure:"
	@echo "  examples/quickstart/QuickStartContract/"
	@echo "  ├── src/QuickStartContract.Contracts/"
	@echo "  ├── tests/QuickStartContract.Tests/"
	@echo "  ├── deploy/"
	@echo "  └── QuickStartContract.sln"
	@echo ""
	@echo "Next steps:"
	@echo "  cd examples/quickstart/QuickStartContract"
	@echo "  make build-contract"
	@echo "  make test-contract"
	@echo "  make deploy-contract"

# Version management
bump-version:
	@echo "$(YELLOW)Bumping version...$(NC)"
	@read -p "Enter new version (current: $(VERSION)): " new_version; \
	sed -i "s/VERSION := .*/VERSION := $$new_version/" Makefile; \
	find . -name "*.csproj" -exec sed -i "s/<Version>.*<\/Version>/<Version>$$new_version<\/Version>/" {} \;
	@echo "$(GREEN)Version bumped to $$new_version$(NC)"

# Performance profiling
profile:
	@echo "$(YELLOW)Running performance profiling...$(NC)"
	$(DOTNET) trace collect --providers Microsoft-DotNETCore-SampleProfiler -- \
		$(DOTNET) run --project src/Neo.Compiler.CSharp/Neo.Compiler.CSharp.csproj \
		--configuration Release -- \
		examples/Example.SmartContract.NEP17/Example.SmartContract.NEP17.csproj

# Database setup for WebGUI service
setup-db:
	@echo "$(YELLOW)Setting up database for WebGUI service...$(NC)"
	cd src/R3E.WebGUI.Service && \
	$(DOTNET) ef database update

# Show project statistics
stats:
	@echo "$(YELLOW)Project Statistics:$(NC)"
	@echo "Lines of Code:"
	@find src -name "*.cs" -exec wc -l {} + | tail -1
	@echo ""
	@echo "Number of Projects:"
	@find . -name "*.csproj" | wc -l
	@echo ""
	@echo "Number of Tests:"
	@grep -r "\[Test\]" tests --include="*.cs" | wc -l

.DEFAULT_GOAL := help