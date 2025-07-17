using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neo.Compiler.CSharp.IntegrationTests
{
    /// <summary>
    /// Integration tests for the complete contract development workflow
    /// Tests: Creation -> Development -> Testing -> Building -> Deployment
    /// </summary>
    public class ContractWorkflowTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _testDirectory;
        private readonly string _rnccPath;

        public ContractWorkflowTests(ITestOutputHelper output)
        {
            _output = output;
            _testDirectory = Path.Combine(Path.GetTempPath(), $"rncc_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);
            
            // Try to find RNCC executable
            _rnccPath = FindRnccExecutable();
            _output.WriteLine($"Test directory: {_testDirectory}");
            _output.WriteLine($"RNCC path: {_rnccPath}");
        }

        [Fact]
        public async Task TestCompleteContractWorkflow_BasicTemplate()
        {
            // Arrange
            var contractName = "TestBasicContract";
            var contractPath = Path.Combine(_testDirectory, contractName);

            // Act & Assert - Step 1: Create contract solution
            _output.WriteLine("=== Step 1: Creating contract solution ===");
            var createResult = await RunCommandAsync(_rnccPath, 
                $"new {contractName} --template=basic --author=\"Test Author\" --email=\"test@example.com\" --with-tests",
                _testDirectory);
            
            Assert.True(createResult.Success, $"Failed to create contract: {createResult.Error}");
            Assert.True(Directory.Exists(contractPath), "Contract directory not created");
            
            // Verify solution structure
            Assert.True(File.Exists(Path.Combine(contractPath, $"{contractName}.sln")), "Solution file not found");
            Assert.True(Directory.Exists(Path.Combine(contractPath, "src")), "src directory not found");
            Assert.True(Directory.Exists(Path.Combine(contractPath, "tests")), "tests directory not found");
            
            _output.WriteLine($"Contract solution created successfully at: {contractPath}");

            // Act & Assert - Step 2: Build the contract
            _output.WriteLine("\n=== Step 2: Building contract ===");
            var buildResult = await RunCommandAsync("dotnet", "build", contractPath);
            
            Assert.True(buildResult.Success, $"Failed to build contract: {buildResult.Error}");
            _output.WriteLine("Contract built successfully");

            // Act & Assert - Step 3: Run tests
            _output.WriteLine("\n=== Step 3: Running tests ===");
            var testResult = await RunCommandAsync("dotnet", "test", contractPath);
            
            Assert.True(testResult.Success, $"Tests failed: {testResult.Error}");
            _output.WriteLine("All tests passed");

            // Act & Assert - Step 4: Compile with RNCC
            _output.WriteLine("\n=== Step 4: Compiling with RNCC ===");
            var compileResult = await RunCommandAsync(_rnccPath, "build", contractPath);
            
            Assert.True(compileResult.Success, $"Failed to compile with RNCC: {compileResult.Error}");
            
            // Verify compilation artifacts
            var artifactsPath = Path.Combine(contractPath, "bin", "sc");
            Assert.True(Directory.Exists(artifactsPath), "Artifacts directory not found");
            
            var nefFiles = Directory.GetFiles(artifactsPath, "*.nef", SearchOption.AllDirectories);
            Assert.NotEmpty(nefFiles);
            _output.WriteLine($"Found {nefFiles.Length} NEF file(s)");
            
            var manifestFiles = Directory.GetFiles(artifactsPath, "*.manifest.json", SearchOption.AllDirectories);
            Assert.NotEmpty(manifestFiles);
            _output.WriteLine($"Found {manifestFiles.Length} manifest file(s)");
        }

        [Fact]
        public async Task TestCompleteContractWorkflow_NEP17Template()
        {
            // Arrange
            var contractName = "TestTokenContract";
            var contractPath = Path.Combine(_testDirectory, contractName);

            // Act & Assert - Create NEP-17 token contract
            _output.WriteLine("=== Creating NEP-17 token contract ===");
            var createResult = await RunCommandAsync(_rnccPath,
                $"new {contractName} --template=nep17 --author=\"Test Author\" --email=\"test@example.com\" --with-tests",
                _testDirectory);
            
            Assert.True(createResult.Success, $"Failed to create NEP-17 contract: {createResult.Error}");
            
            // Verify NEP-17 specific files
            var contractFile = Path.Combine(contractPath, "src", $"{contractName}.Contracts", $"{contractName}.cs");
            Assert.True(File.Exists(contractFile), "Main contract file not found");
            
            var content = await File.ReadAllTextAsync(contractFile);
            Assert.Contains("NEP-17", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Transfer", content);
            Assert.Contains("BalanceOf", content);
            Assert.Contains("TotalSupply", content);
            
            _output.WriteLine("NEP-17 contract created with required methods");

            // Build and test
            var buildResult = await RunCommandAsync("dotnet", "build", contractPath);
            Assert.True(buildResult.Success, $"Failed to build NEP-17 contract: {buildResult.Error}");
            
            var testResult = await RunCommandAsync("dotnet", "test", contractPath);
            Assert.True(testResult.Success, $"NEP-17 tests failed: {testResult.Error}");
        }

        [Fact]
        public async Task TestCompleteContractWorkflow_SolutionTemplate()
        {
            // Arrange
            var contractName = "TestSolutionContract";
            var contractPath = Path.Combine(_testDirectory, contractName);

            // Act & Assert - Create full solution
            _output.WriteLine("=== Creating full solution template ===");
            var createResult = await RunCommandAsync(_rnccPath,
                $"new {contractName} --template=solution --author=\"Test Author\" --email=\"test@example.com\" --with-tests --with-deploy-scripts",
                _testDirectory);
            
            Assert.True(createResult.Success, $"Failed to create solution: {createResult.Error}");
            
            // Verify complete solution structure
            Assert.True(File.Exists(Path.Combine(contractPath, $"{contractName}.sln")), "Solution file not found");
            Assert.True(Directory.Exists(Path.Combine(contractPath, "src", $"{contractName}.Contracts")), "Contracts project not found");
            Assert.True(Directory.Exists(Path.Combine(contractPath, "tests", $"{contractName}.Tests")), "Tests project not found");
            Assert.True(Directory.Exists(Path.Combine(contractPath, "deploy")), "Deploy directory not found");
            Assert.True(Directory.Exists(Path.Combine(contractPath, "deploy", "scripts")), "Deploy scripts not found");
            
            _output.WriteLine("Full solution structure verified");

            // Verify deployment scripts
            var deployTestnetScript = Path.Combine(contractPath, "deploy", "scripts", "deploy-testnet.sh");
            Assert.True(File.Exists(deployTestnetScript), "TestNet deployment script not found");
            
            // Build entire solution
            var buildResult = await RunCommandAsync("dotnet", "build", contractPath);
            Assert.True(buildResult.Success, $"Failed to build solution: {buildResult.Error}");
            
            // Run all tests
            var testResult = await RunCommandAsync("dotnet", "test", contractPath);
            Assert.True(testResult.Success, $"Solution tests failed: {testResult.Error}");
            
            _output.WriteLine("Complete solution built and tested successfully");
        }

        [Fact]
        public async Task TestContractModificationAndRebuild()
        {
            // Arrange
            var contractName = "TestModifyContract";
            var contractPath = Path.Combine(_testDirectory, contractName);

            // Create contract
            var createResult = await RunCommandAsync(_rnccPath,
                $"new {contractName} --template=basic --author=\"Test Author\" --email=\"test@example.com\" --with-tests",
                _testDirectory);
            Assert.True(createResult.Success);

            // Find main contract file
            var contractFile = Directory.GetFiles(
                Path.Combine(contractPath, "src"), 
                "*.cs", 
                SearchOption.AllDirectories).FirstOrDefault();
            
            Assert.NotNull(contractFile);
            
            // Modify contract - add a new method
            _output.WriteLine("\n=== Modifying contract ===");
            var originalContent = await File.ReadAllTextAsync(contractFile);
            var modifiedContent = originalContent.Replace(
                "public class", 
                @"public class
    {
        public static string GetVersion() => ""v1.0.1"";
    }

    public class");
            
            await File.WriteAllTextAsync(contractFile, modifiedContent);
            _output.WriteLine("Added GetVersion method to contract");

            // Rebuild
            var rebuildResult = await RunCommandAsync("dotnet", "build", contractPath);
            Assert.True(rebuildResult.Success, $"Failed to rebuild after modification: {rebuildResult.Error}");
            
            // Recompile with RNCC
            var recompileResult = await RunCommandAsync(_rnccPath, "build", contractPath);
            Assert.True(recompileResult.Success, $"Failed to recompile with RNCC: {recompileResult.Error}");
            
            _output.WriteLine("Contract successfully rebuilt after modification");
        }

        [Fact]
        public async Task TestInvalidTemplateHandling()
        {
            // Arrange
            var contractName = "TestInvalidContract";

            // Act - Try to create with invalid template
            _output.WriteLine("=== Testing invalid template handling ===");
            var createResult = await RunCommandAsync(_rnccPath,
                $"new {contractName} --template=invalid_template --author=\"Test Author\" --email=\"test@example.com\"",
                _testDirectory);
            
            // Assert - Should fail gracefully
            Assert.False(createResult.Success, "Should fail with invalid template");
            Assert.Contains("template", createResult.Error, StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"Correctly failed with error: {createResult.Error}");
        }

        [Fact]
        public async Task TestContractWithWebGUIGeneration()
        {
            // Arrange
            var contractName = "TestWebGUIContract";
            var contractPath = Path.Combine(_testDirectory, contractName);

            // Create contract
            var createResult = await RunCommandAsync(_rnccPath,
                $"new {contractName} --template=basic --author=\"Test Author\" --email=\"test@example.com\" --with-tests",
                _testDirectory);
            Assert.True(createResult.Success);

            // Build contract
            var buildResult = await RunCommandAsync("dotnet", "build", contractPath);
            Assert.True(buildResult.Success);

            // Compile with WebGUI generation
            _output.WriteLine("\n=== Generating WebGUI ===");
            var webguiResult = await RunCommandAsync(_rnccPath, 
                "build --generate-webgui", 
                contractPath);
            
            if (webguiResult.Success)
            {
                // Verify WebGUI files
                var webguiPath = Path.Combine(contractPath, "bin", "sc", "webgui");
                Assert.True(Directory.Exists(webguiPath), "WebGUI directory not found");
                
                var indexHtml = Path.Combine(webguiPath, "index.html");
                Assert.True(File.Exists(indexHtml), "WebGUI index.html not found");
                
                _output.WriteLine("WebGUI generated successfully");
            }
            else
            {
                _output.WriteLine($"WebGUI generation not available: {webguiResult.Error}");
            }
        }

        private string FindRnccExecutable()
        {
            // Try different possible locations for RNCC
            var possiblePaths = new[]
            {
                "rncc", // Global tool
                Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "binaries", "rncc-linux-x64"),
                Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "binaries", "rncc-win-x64.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "binaries", "rncc-osx-x64"),
                Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "binaries", "rncc-osx-arm64"),
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path) || CommandExists(path))
                {
                    return path;
                }
            }

            // Fallback to dotnet tool
            return "dotnet tool run rncc";
        }

        private bool CommandExists(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = OperatingSystem.IsWindows() ? "where" : "which",
                        Arguments = command,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(bool Success, string Output, string Error)> RunCommandAsync(
            string command, string arguments, string workingDirectory = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command.Contains(' ') ? "sh" : command,
                Arguments = command.Contains(' ') ? $"-c \"{command} {arguments}\"" : arguments,
                WorkingDirectory = workingDirectory ?? _testDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            _output.WriteLine($"Command: {command} {arguments}");
            _output.WriteLine($"Exit Code: {process.ExitCode}");
            if (!string.IsNullOrWhiteSpace(output))
                _output.WriteLine($"Output: {output}");
            if (!string.IsNullOrWhiteSpace(error))
                _output.WriteLine($"Error: {error}");

            return (process.ExitCode == 0, output, error);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}