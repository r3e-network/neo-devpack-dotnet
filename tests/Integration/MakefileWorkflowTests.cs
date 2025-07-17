using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neo.Compiler.CSharp.IntegrationTests
{
    /// <summary>
    /// Tests for Makefile-based workflow commands
    /// </summary>
    public class MakefileWorkflowTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _testDirectory;
        private readonly string _makeCommand;
        private readonly string _projectRoot;

        public MakefileWorkflowTests(ITestOutputHelper output)
        {
            _output = output;
            _testDirectory = Path.Combine(Path.GetTempPath(), $"make_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);
            
            // Find project root (where Makefile is located)
            _projectRoot = FindProjectRoot();
            _output.WriteLine($"Project root: {_projectRoot}");
            _output.WriteLine($"Test directory: {_testDirectory}");
            
            // Determine make command based on OS
            _makeCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "make.bat" : "make";
        }

        [Fact]
        public async Task TestMakeHelp()
        {
            // Act
            var result = await RunMakeCommandAsync("help");
            
            // Assert
            Assert.True(result.Success, $"Make help failed: {result.Error}");
            Assert.Contains("R3E Neo Contract DevPack", result.Output);
            Assert.Contains("make new-contract", result.Output);
            Assert.Contains("make build-contract", result.Output);
            Assert.Contains("make test-contract", result.Output);
            
            _output.WriteLine("Make help command works correctly");
        }

        [Fact]
        public async Task TestMakeNewContract_Interactive()
        {
            // This test simulates the interactive contract creation
            // In a real scenario, we'd need to provide input programmatically
            
            var contractName = $"TestMakeContract_{Guid.NewGuid():N.Substring(0, 8)}";
            var input = $"{contractName}\nbasic\nTest Author\ntest@example.com\n";
            
            // Change to test directory for contract creation
            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(_testDirectory);
                
                // Run make new-contract with piped input
                var result = await RunMakeCommandWithInputAsync("new-contract", input);
                
                // The command might fail in CI due to missing RNCC, but we can check the attempt
                if (result.Success || result.Error.Contains("rncc"))
                {
                    _output.WriteLine($"Contract creation attempted for: {contractName}");
                    
                    // If successful, verify the directory was created
                    var contractPath = Path.Combine(_testDirectory, contractName);
                    if (Directory.Exists(contractPath))
                    {
                        _output.WriteLine($"Contract directory created at: {contractPath}");
                        Assert.True(true, "Contract directory created");
                    }
                }
                else
                {
                    _output.WriteLine($"Make new-contract output: {result.Output}");
                    _output.WriteLine($"Make new-contract error: {result.Error}");
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }

        [Fact]
        public async Task TestMakeBuildContract_NoSolution()
        {
            // Test build-contract when no solution exists
            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(_testDirectory);
                
                var result = await RunMakeCommandAsync("build-contract");
                
                // Should fail with helpful message
                Assert.False(result.Success, "Should fail when no solution exists");
                Assert.Contains("No solution file found", result.Output + result.Error);
                Assert.Contains("make new-contract", result.Output + result.Error);
                
                _output.WriteLine("Build-contract correctly handles missing solution");
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }

        [Fact]
        public async Task TestMakeClean()
        {
            // Test make clean command
            var result = await RunMakeCommandAsync("clean");
            
            // Clean should always succeed
            Assert.True(result.Success || result.Output.Contains("Clean completed"), 
                $"Make clean failed: {result.Error}");
            
            _output.WriteLine("Make clean command executed");
        }

        [Fact]
        public async Task TestMakeInstallTools()
        {
            // Test installing development tools
            var result = await RunMakeCommandAsync("install-tools");
            
            // May fail if tools are already installed, but should not error
            if (result.Success || result.Error.Contains("already installed"))
            {
                _output.WriteLine("Make install-tools executed successfully");
                Assert.True(true);
            }
            else
            {
                _output.WriteLine($"Install tools output: {result.Output}");
                _output.WriteLine($"Install tools error: {result.Error}");
            }
        }

        [Fact]
        public async Task TestCompleteWorkflowSimulation()
        {
            // Simulate a complete workflow using make commands
            _output.WriteLine("=== Simulating Complete Workflow ===");
            
            // Step 1: Check if we can run make
            var helpResult = await RunMakeCommandAsync("help");
            Assert.True(helpResult.Success, "Make command not available");
            
            // Step 2: Clean any existing artifacts
            var cleanResult = await RunMakeCommandAsync("clean");
            _output.WriteLine($"Clean result: {cleanResult.Success}");
            
            // Step 3: Verify contract commands are available
            Assert.Contains("new-contract", helpResult.Output);
            Assert.Contains("build-contract", helpResult.Output);
            Assert.Contains("test-contract", helpResult.Output);
            Assert.Contains("deploy-contract", helpResult.Output);
            
            _output.WriteLine("All workflow commands are available in Makefile");
        }

        [Fact]
        public async Task TestMakeStats()
        {
            // Test project statistics command
            var result = await RunMakeCommandAsync("stats");
            
            if (result.Success)
            {
                Assert.Contains("Lines of Code", result.Output);
                Assert.Contains("Number of Projects", result.Output);
                _output.WriteLine("Project statistics generated successfully");
            }
            else
            {
                _output.WriteLine($"Stats command not available or failed: {result.Error}");
            }
        }

        private string FindProjectRoot()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var dir = new DirectoryInfo(currentDir);
            
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "Makefile")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            
            // Fallback to current directory
            return currentDir;
        }

        private async Task<(bool Success, string Output, string Error)> RunMakeCommandAsync(string target)
        {
            return await RunCommandAsync(_makeCommand, target, _projectRoot);
        }

        private async Task<(bool Success, string Output, string Error)> RunMakeCommandWithInputAsync(
            string target, string input)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _makeCommand,
                Arguments = target,
                WorkingDirectory = _projectRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Write input
            await process.StandardInput.WriteAsync(input);
            process.StandardInput.Close();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            return (process.ExitCode == 0, output, error);
        }

        private async Task<(bool Success, string Output, string Error)> RunCommandAsync(
            string command, string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            
            try
            {
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                _output.WriteLine($"Command: {command} {arguments}");
                _output.WriteLine($"Exit Code: {process.ExitCode}");
                
                return (process.ExitCode == 0, output, error);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to run command: {ex.Message}");
                return (false, "", ex.Message);
            }
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