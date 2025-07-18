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
    public class FullWorkflowIntegrationTests
    {
        private string _testDirectory = null!;
        private string _rnccPath = null!;
        private string _neoExpressPath = null!;
        private string _projectName = null!;
        private string _projectPath = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Create a unique test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), $"rncc_integration_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);

            // Find RNCC tool path
            _rnccPath = FindExecutable("rncc") ?? throw new InvalidOperationException("RNCC tool not found. Please install it first.");
            
            // Find Neo Express path (optional - tests will skip if not found)
            _neoExpressPath = FindExecutable("neoxp") ?? FindExecutable("neo-express");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Cleanup test directory
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Test]
        [Order(1)]
        public async Task Test_01_CreateContractSolution()
        {
            // Arrange
            _projectName = $"TestContract_{DateTime.Now:yyyyMMddHHmmss}";
            _projectPath = Path.Combine(_testDirectory, _projectName);

            // Act - Create new contract solution
            var result = await RunCommandAsync(_rnccPath, 
                $"new {_projectName} --template=solution --author=\"Integration Test\" --email=\"test@example.com\"", 
                _testDirectory);

            // Assert
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Failed to create contract solution: {result.Error}");
            Assert.That(Directory.Exists(_projectPath), Is.True, "Project directory was not created");
            
            // Verify solution structure
            Assert.That(File.Exists(Path.Combine(_projectPath, $"{_projectName}.sln")), Is.True, "Solution file not found");
            Assert.That(Directory.Exists(Path.Combine(_projectPath, "src", _projectName)), Is.True, "Contract project directory not found");
            Assert.That(Directory.Exists(Path.Combine(_projectPath, "tests", $"{_projectName}.Tests")), Is.True, "Test project directory not found");
            Assert.That(Directory.Exists(Path.Combine(_projectPath, "deploy", "Deploy")), Is.True, "Deploy project directory not found");
        }

        [Test]
        [Order(2)]
        public async Task Test_02_BuildContractSolution()
        {
            // Act - Build the solution
            var result = await RunCommandAsync("dotnet", "build", _projectPath);

            // Assert
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Failed to build solution: {result.Error}");
            
            // Verify compiled contract files
            var contractsDir = Path.Combine(_projectPath, "deploy", "contracts");
            Assert.That(Directory.Exists(contractsDir), Is.True, "Contracts directory not created");
            
            var nefFile = Path.Combine(contractsDir, $"{_projectName}.nef");
            var manifestFile = Path.Combine(contractsDir, $"{_projectName}.manifest.json");
            Assert.That(File.Exists(nefFile), Is.True, "NEF file not generated");
            Assert.That(File.Exists(manifestFile), Is.True, "Manifest file not generated");
        }

        [Test]
        [Order(3)]
        public async Task Test_03_RunContractTests()
        {
            // Act - Run tests
            var result = await RunCommandAsync("dotnet", "test", _projectPath);

            // Assert
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Tests failed: {result.Error}");
            Assert.That(result.Output, Does.Contain("Passed"), "No tests passed");
        }

        [Test]
        [Order(4)]
        public async Task Test_04_InitializeNeoExpress()
        {
            if (string.IsNullOrEmpty(_neoExpressPath))
            {
                Assert.Ignore("Neo Express not installed - skipping deployment tests");
                return;
            }

            // Act - Initialize Neo Express
            var expressConfigPath = Path.Combine(_projectPath, "default.neo-express");
            var result = await RunCommandAsync(_neoExpressPath, "create", _projectPath);

            // Assert
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Failed to initialize Neo Express: {result.Error}");
            Assert.That(File.Exists(expressConfigPath), Is.True, "Neo Express config file not created");
        }

        [Test]
        [Order(5)]
        public async Task Test_05_CreateWalletAndTransferGas()
        {
            if (string.IsNullOrEmpty(_neoExpressPath))
            {
                Assert.Ignore("Neo Express not installed - skipping deployment tests");
                return;
            }

            // Act - Create a wallet
            var walletPath = Path.Combine(_projectPath, "deployer.wallet.json");
            var createWalletResult = await RunCommandAsync(_neoExpressPath, 
                $"wallet create deployer --force", 
                _projectPath);

            Assert.That(createWalletResult.ExitCode, Is.EqualTo(0), $"Failed to create wallet: {createWalletResult.Error}");

            // Transfer GAS to the wallet
            var transferResult = await RunCommandAsync(_neoExpressPath,
                "transfer 1000 GAS genesis deployer",
                _projectPath);

            Assert.That(transferResult.ExitCode, Is.EqualTo(0), $"Failed to transfer GAS: {transferResult.Error}");
        }

        [Test]
        [Order(6)]
        public async Task Test_06_DeployContract()
        {
            if (string.IsNullOrEmpty(_neoExpressPath))
            {
                Assert.Ignore("Neo Express not installed - skipping deployment tests");
                return;
            }

            // Arrange
            var nefFile = Path.Combine(_projectPath, "deploy", "contracts", $"{_projectName}.nef");
            var manifestFile = Path.Combine(_projectPath, "deploy", "contracts", $"{_projectName}.manifest.json");

            // Act - Deploy contract
            var deployResult = await RunCommandAsync(_neoExpressPath,
                $"contract deploy {nefFile} deployer",
                _projectPath);

            // Assert
            Assert.That(deployResult.ExitCode, Is.EqualTo(0), $"Failed to deploy contract: {deployResult.Error}");
            Assert.That(deployResult.Output, Does.Contain("Contract deployed"), "Contract deployment message not found");
            
            // Extract contract hash from output
            var contractHash = ExtractContractHash(deployResult.Output);
            Assert.That(contractHash, Is.Not.Null.And.Not.Empty, "Could not extract contract hash");
            
            // Store contract hash for later tests
            File.WriteAllText(Path.Combine(_projectPath, "contract.hash"), contractHash);
        }

        [Test]
        [Order(7)]
        public async Task Test_07_InvokeContractMethods()
        {
            if (string.IsNullOrEmpty(_neoExpressPath))
            {
                Assert.Ignore("Neo Express not installed - skipping deployment tests");
                return;
            }

            // Arrange
            var contractHash = File.ReadAllText(Path.Combine(_projectPath, "contract.hash")).Trim();

            // Test 1: Get name
            var getNameResult = await RunCommandAsync(_neoExpressPath,
                $"contract invoke {contractHash} getName deployer",
                _projectPath);

            Assert.That(getNameResult.ExitCode, Is.EqualTo(0), $"Failed to invoke getName: {getNameResult.Error}");
            Assert.That(getNameResult.Output, Does.Contain(_projectName), "Contract name not returned correctly");

            // Test 2: Store data
            var storeDataResult = await RunCommandAsync(_neoExpressPath,
                $"contract invoke {contractHash} storeData \"test-key\" \"test-value\" deployer",
                _projectPath);

            Assert.That(storeDataResult.ExitCode, Is.EqualTo(0), $"Failed to invoke storeData: {storeDataResult.Error}");

            // Test 3: Get data
            var getDataResult = await RunCommandAsync(_neoExpressPath,
                $"contract invoke {contractHash} getData \"test-key\" deployer",
                _projectPath);

            Assert.That(getDataResult.ExitCode, Is.EqualTo(0), $"Failed to invoke getData: {getDataResult.Error}");
            Assert.That(getDataResult.Output, Does.Contain("test-value"), "Stored data not retrieved correctly");
        }

        [Test]
        [Order(8)]
        public async Task Test_08_TestNEP17Template()
        {
            // Create NEP-17 token contract
            var nep17Name = $"TestToken_{DateTime.Now:yyyyMMddHHmmss}";
            var nep17Path = Path.Combine(_testDirectory, nep17Name);

            var createResult = await RunCommandAsync(_rnccPath,
                $"new {nep17Name} --template=nep17 --author=\"Integration Test\" --email=\"test@example.com\"",
                _testDirectory);

            Assert.That(createResult.ExitCode, Is.EqualTo(0), $"Failed to create NEP-17 contract: {createResult.Error}");

            // Build NEP-17 contract
            var buildResult = await RunCommandAsync("dotnet", $"build {Path.Combine(nep17Path, $"{nep17Name}.csproj")}", _testDirectory);
            Assert.That(buildResult.ExitCode, Is.EqualTo(0), $"Failed to build NEP-17 contract: {buildResult.Error}");

            // Verify NEP-17 specific files
            var nefFile = Directory.GetFiles(nep17Path, "*.nef", SearchOption.AllDirectories).FirstOrDefault();
            Assert.That(nefFile, Is.Not.Null, "NEP-17 NEF file not generated");

            var manifestFile = Directory.GetFiles(nep17Path, "*.manifest.json", SearchOption.AllDirectories).FirstOrDefault();
            Assert.That(manifestFile, Is.Not.Null, "NEP-17 manifest file not generated");

            // Verify manifest contains NEP-17 standard
            var manifestContent = File.ReadAllText(manifestFile!);
            Assert.That(manifestContent, Does.Contain("NEP-17"), "Manifest does not contain NEP-17 standard");
        }

        [Test]
        [Order(9)]
        public async Task Test_09_TestOracleTemplate()
        {
            // Create Oracle contract
            var oracleName = $"TestOracle_{DateTime.Now:yyyyMMddHHmmss}";
            var oraclePath = Path.Combine(_testDirectory, oracleName);

            var createResult = await RunCommandAsync(_rnccPath,
                $"new {oracleName} --template=oracle --author=\"Integration Test\" --email=\"test@example.com\"",
                _testDirectory);

            Assert.That(createResult.ExitCode, Is.EqualTo(0), $"Failed to create Oracle contract: {createResult.Error}");

            // Build Oracle contract
            var buildResult = await RunCommandAsync("dotnet", $"build {Path.Combine(oraclePath, $"{oracleName}.csproj")}", _testDirectory);
            Assert.That(buildResult.ExitCode, Is.EqualTo(0), $"Failed to build Oracle contract: {buildResult.Error}");
        }

        [Test]
        [Order(10)]
        public async Task Test_10_TestOwnerTemplate()
        {
            // Create Owner contract
            var ownerName = $"TestOwner_{DateTime.Now:yyyyMMddHHmmss}";
            var ownerPath = Path.Combine(_testDirectory, ownerName);

            var createResult = await RunCommandAsync(_rnccPath,
                $"new {ownerName} --template=owner --author=\"Integration Test\" --email=\"test@example.com\"",
                _testDirectory);

            Assert.That(createResult.ExitCode, Is.EqualTo(0), $"Failed to create Owner contract: {createResult.Error}");

            // Build Owner contract
            var buildResult = await RunCommandAsync("dotnet", $"build {Path.Combine(ownerPath, $"{ownerName}.csproj")}", _testDirectory);
            Assert.That(buildResult.ExitCode, Is.EqualTo(0), $"Failed to build Owner contract: {buildResult.Error}");
        }

        private string? FindExecutable(string name)
        {
            // Check in PATH
            var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
            foreach (var dir in pathDirs)
            {
                var fullPath = Path.Combine(dir, name);
                if (File.Exists(fullPath))
                    return fullPath;
                
                // Check with .exe extension on Windows
                if (OperatingSystem.IsWindows())
                {
                    fullPath = Path.Combine(dir, $"{name}.exe");
                    if (File.Exists(fullPath))
                        return fullPath;
                }
            }

            // Check .NET tools path
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dotnetToolsPath = Path.Combine(homeDir, ".dotnet", "tools", name);
            if (File.Exists(dotnetToolsPath))
                return dotnetToolsPath;

            if (OperatingSystem.IsWindows())
            {
                dotnetToolsPath = Path.Combine(homeDir, ".dotnet", "tools", $"{name}.exe");
                if (File.Exists(dotnetToolsPath))
                    return dotnetToolsPath;
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

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
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

        private string ExtractContractHash(string output)
        {
            // Look for contract hash pattern in output
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Contract deployed") || line.Contains("Contract hash"))
                {
                    // Extract hash pattern (0x followed by 40 hex characters)
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"0x[a-fA-F0-9]{40}");
                    if (match.Success)
                        return match.Value;
                }
            }
            return string.Empty;
        }

        private class CommandResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; } = string.Empty;
            public string Error { get; set; } = string.Empty;
        }
    }
}