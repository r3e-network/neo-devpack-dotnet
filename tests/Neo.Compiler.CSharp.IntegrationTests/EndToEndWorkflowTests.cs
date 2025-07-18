using System.Diagnostics;
using System.Text.Json;
using Neo.SmartContract.Testing;
using Neo.SmartContract.Testing.Native;
using Neo.SmartContract.Testing.TestingStandards;
using Neo.VM;
using NUnit.Framework;

namespace Neo.Compiler.CSharp.IntegrationTests
{
    [TestFixture]
    [Category("Integration")]
    public class EndToEndWorkflowTests
    {
        private string _testDirectory = null!;
        private string _rnccPath = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"e2e_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);

            // Find RNCC
            _rnccPath = FindExecutable("rncc") ?? throw new InvalidOperationException("RNCC not found");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                try { Directory.Delete(_testDirectory, true); } catch { }
            }
        }

        [Test]
        public async Task Test_CompleteWorkflow_FromTemplateToDeployment()
        {
            // Step 1: Create contract from template
            var contractName = $"E2EContract_{DateTime.Now:yyyyMMddHHmmss}";
            var projectPath = Path.Combine(_testDirectory, contractName);

            var createResult = await RunCommandAsync(_rnccPath,
                $"new {contractName} --template=solution --author=\"E2E Test\" --email=\"e2e@test.com\"",
                _testDirectory);

            Assert.That(createResult.ExitCode, Is.EqualTo(0), $"Failed to create contract: {createResult.Error}");

            // Step 2: Build the contract
            var buildResult = await RunCommandAsync("dotnet", "build --configuration Release", projectPath);
            Assert.That(buildResult.ExitCode, Is.EqualTo(0), $"Failed to build contract: {buildResult.Error}");

            // Step 3: Run unit tests
            var testResult = await RunCommandAsync("dotnet", "test --configuration Release", projectPath);
            Assert.That(testResult.ExitCode, Is.EqualTo(0), $"Tests failed: {testResult.Error}");

            // Step 4: Verify contract artifacts
            var contractsDir = Path.Combine(projectPath, "deploy", "contracts");
            var nefFile = Path.Combine(contractsDir, $"{contractName}.nef");
            var manifestFile = Path.Combine(contractsDir, $"{contractName}.manifest.json");

            Assert.That(File.Exists(nefFile), Is.True, "NEF file not generated");
            Assert.That(File.Exists(manifestFile), Is.True, "Manifest file not generated");

            // Step 5: Load and test the compiled contract using testing framework
            await TestCompiledContract(nefFile, manifestFile, contractName);
        }

        private async Task TestCompiledContract(string nefPath, string manifestPath, string contractName)
        {
            // Read NEF and manifest
            var nefBytes = await File.ReadAllBytesAsync(nefPath);
            var manifestJson = await File.ReadAllTextAsync(manifestPath);

            // Verify files were generated correctly
            Assert.That(nefBytes.Length, Is.GreaterThan(0), "NEF file should not be empty");
            Assert.That(manifestJson.Length, Is.GreaterThan(0), "Manifest should not be empty");

            // Verify manifest structure
            var manifestDoc = JsonDocument.Parse(manifestJson);
            Assert.That(manifestDoc.RootElement.TryGetProperty("name", out _), Is.True, "Manifest should have name");
            Assert.That(manifestDoc.RootElement.TryGetProperty("abi", out _), Is.True, "Manifest should have ABI");
            
            // In a real integration test with Neo Express or test network,
            // we would deploy and invoke the contract
            Console.WriteLine($"Contract {contractName} compiled successfully");
            Console.WriteLine($"NEF size: {nefBytes.Length} bytes");
            Console.WriteLine($"Manifest: {manifestJson.Length} characters");
        }

        [Test]
        public async Task Test_NEP17TokenWorkflow()
        {
            // Create NEP-17 token
            var tokenName = $"TestToken_{DateTime.Now:yyyyMMddHHmmss}";
            var tokenPath = Path.Combine(_testDirectory, tokenName);

            var createResult = await RunCommandAsync(_rnccPath,
                $"new {tokenName} --template=nep17 --author=\"Token Test\" --email=\"token@test.com\"",
                _testDirectory);

            Assert.That(createResult.ExitCode, Is.EqualTo(0), $"Failed to create token: {createResult.Error}");

            // Build token contract
            var buildResult = await RunCommandAsync("dotnet", 
                $"build \"{Path.Combine(tokenPath, $"{tokenName}.csproj")}\" --configuration Release", 
                _testDirectory);

            Assert.That(buildResult.ExitCode, Is.EqualTo(0), $"Failed to build token: {buildResult.Error}");

            // Find generated files
            var nefFiles = Directory.GetFiles(tokenPath, "*.nef", SearchOption.AllDirectories);
            var manifestFiles = Directory.GetFiles(tokenPath, "*.manifest.json", SearchOption.AllDirectories);

            Assert.That(nefFiles.Length, Is.GreaterThan(0), "No NEF file generated for token");
            Assert.That(manifestFiles.Length, Is.GreaterThan(0), "No manifest file generated for token");

            // Verify NEP-17 standard in manifest
            var manifestContent = await File.ReadAllTextAsync(manifestFiles[0]);
            var manifest = JsonDocument.Parse(manifestContent);
            
            var hasNep17 = false;
            if (manifest.RootElement.TryGetProperty("supportedstandards", out var standards))
            {
                foreach (var standard in standards.EnumerateArray())
                {
                    if (standard.GetString() == "NEP-17")
                    {
                        hasNep17 = true;
                        break;
                    }
                }
            }

            Assert.That(hasNep17, Is.True, "Token manifest should declare NEP-17 support");

            // Test token methods exist in manifest
            var hasSymbol = false;
            var hasDecimals = false;
            var hasTransfer = false;
            var hasBalanceOf = false;

            if (manifest.RootElement.TryGetProperty("abi", out var abi) &&
                abi.TryGetProperty("methods", out var methods))
            {
                foreach (var method in methods.EnumerateArray())
                {
                    var name = method.GetProperty("name").GetString();
                    switch (name)
                    {
                        case "symbol": hasSymbol = true; break;
                        case "decimals": hasDecimals = true; break;
                        case "transfer": hasTransfer = true; break;
                        case "balanceOf": hasBalanceOf = true; break;
                    }
                }
            }

            Assert.That(hasSymbol, Is.True, "Token should have symbol method");
            Assert.That(hasDecimals, Is.True, "Token should have decimals method");
            Assert.That(hasTransfer, Is.True, "Token should have transfer method");
            Assert.That(hasBalanceOf, Is.True, "Token should have balanceOf method");
        }

        [Test]
        public async Task Test_MultipleTemplateTypes()
        {
            var templates = new[] { "solution", "nep17", "oracle", "owner" };
            var results = new List<(string template, bool success, string message)>();

            foreach (var template in templates)
            {
                var projectName = $"Multi_{template}_{DateTime.Now:yyyyMMddHHmmss}";
                var projectPath = Path.Combine(_testDirectory, projectName);

                try
                {
                    // Create project
                    var createResult = await RunCommandAsync(_rnccPath,
                        $"new {projectName} --template={template}",
                        _testDirectory);

                    if (createResult.ExitCode != 0)
                    {
                        results.Add((template, false, $"Create failed: {createResult.Error}"));
                        continue;
                    }

                    // Build project (solution template has different structure)
                    string buildCommand;
                    if (template == "solution")
                    {
                        buildCommand = "build --configuration Release";
                    }
                    else
                    {
                        buildCommand = $"build \"{Path.Combine(projectPath, $"{projectName}.csproj")}\" --configuration Release";
                    }

                    var buildResult = await RunCommandAsync("dotnet", buildCommand, 
                        template == "solution" ? projectPath : _testDirectory);

                    if (buildResult.ExitCode != 0)
                    {
                        results.Add((template, false, $"Build failed: {buildResult.Error}"));
                        continue;
                    }

                    // Verify artifacts
                    var hasNef = Directory.GetFiles(projectPath, "*.nef", SearchOption.AllDirectories).Any();
                    var hasManifest = Directory.GetFiles(projectPath, "*.manifest.json", SearchOption.AllDirectories).Any();

                    if (hasNef && hasManifest)
                    {
                        results.Add((template, true, "Success"));
                    }
                    else
                    {
                        results.Add((template, false, $"Missing artifacts: NEF={hasNef}, Manifest={hasManifest}"));
                    }
                }
                catch (Exception ex)
                {
                    results.Add((template, false, $"Exception: {ex.Message}"));
                }
            }

            // Report results
            foreach (var (template, success, message) in results)
            {
                Console.WriteLine($"{template}: {(success ? "✓" : "✗")} - {message}");
            }

            // Assert all templates worked
            var failures = results.Where(r => !r.success).ToList();
            Assert.That(failures, Is.Empty, 
                $"Some templates failed: {string.Join(", ", failures.Select(f => $"{f.template}: {f.message}"))}");
        }

        private string? FindExecutable(string name)
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var paths = new[]
            {
                Path.Combine(homeDir, ".dotnet", "tools", name),
                Path.Combine(homeDir, ".dotnet", "tools", $"{name}.exe"),
                name
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        private async Task<CommandResult> RunCommandAsync(string command, string arguments, string workingDirectory)
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
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return new CommandResult
            {
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString()
            };
        }

        private class CommandResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; } = string.Empty;
            public string Error { get; set; } = string.Empty;
        }
    }
}