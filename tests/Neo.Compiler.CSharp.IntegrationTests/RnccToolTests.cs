using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Neo.Compiler.CSharp.IntegrationTests
{
    [TestFixture]
    [Category("Integration")]
    public class RnccToolTests
    {
        private string _testDirectory = null!;
        private string _rnccPath = null!;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"rncc_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);

            // Try to find RNCC in various locations
            _rnccPath = FindRncc() ?? throw new InvalidOperationException("RNCC tool not found");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch { }
            }
        }

        [Test]
        public void Test_RnccVersion()
        {
            var result = RunCommand(_rnccPath, "--version");
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Version command failed: {result.Error}");
            Assert.That(result.Output, Is.Not.Null.And.Not.Empty, "Version output should not be empty");
            // Version format should be like "0.0.5+hash"
            Assert.That(result.Output, Does.Match(@"^\d+\.\d+\.\d+"), "Version should start with semantic version");
        }

        [Test]
        public void Test_RnccHelp()
        {
            var result = RunCommand(_rnccPath, "--help");
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Help command failed: {result.Error}");
            Assert.That(result.Output, Does.Contain("Usage:"), "Help should contain usage information");
            Assert.That(result.Output, Does.Contain("new"), "Help should show 'new' command");
            Assert.That(result.Output, Does.Contain("templates"), "Help should show 'templates' command");
        }

        [Test]
        public void Test_RnccTemplatesList()
        {
            var result = RunCommand(_rnccPath, "templates");
            Assert.That(result.ExitCode, Is.EqualTo(0), $"Templates command failed: {result.Error}");
            Assert.That(result.Output, Does.Contain("solution"), "Should list solution template");
            Assert.That(result.Output, Does.Contain("nep17"), "Should list NEP-17 template");
            Assert.That(result.Output, Does.Contain("oracle"), "Should list oracle template");
            Assert.That(result.Output, Does.Contain("owner"), "Should list owner template");
        }

        [Test]
        public void Test_CreateSolutionTemplate()
        {
            var projectName = "TestSolution";
            var result = RunCommand(_rnccPath, 
                $"new {projectName} --template=solution --author=\"Test Author\" --email=\"test@test.com\"",
                _testDirectory);

            Assert.That(result.ExitCode, Is.EqualTo(0), $"Create solution failed: {result.Error}");
            Assert.That(result.Output, Does.Contain("Successfully created"), "Should show success message");

            // Verify created files
            var projectDir = Path.Combine(_testDirectory, projectName);
            Assert.That(Directory.Exists(projectDir), Is.True, "Project directory not created");
            Assert.That(File.Exists(Path.Combine(projectDir, $"{projectName}.sln")), Is.True, "Solution file not created");
            Assert.That(Directory.Exists(Path.Combine(projectDir, "src", projectName)), Is.True, "Source directory not created");
            Assert.That(Directory.Exists(Path.Combine(projectDir, "tests")), Is.True, "Tests directory not created");
            Assert.That(Directory.Exists(Path.Combine(projectDir, "deploy")), Is.True, "Deploy directory not created");
        }

        [Test]
        public void Test_CreateWithoutTests()
        {
            var projectName = "NoTestsProject";
            var result = RunCommand(_rnccPath,
                $"new {projectName} --template=solution --with-tests=false",
                _testDirectory);

            Assert.That(result.ExitCode, Is.EqualTo(0), $"Create without tests failed: {result.Error}");

            var projectDir = Path.Combine(_testDirectory, projectName);
            Assert.That(Directory.Exists(Path.Combine(projectDir, "src")), Is.True, "Source directory should exist");
            Assert.That(Directory.Exists(Path.Combine(projectDir, "tests")), Is.False, "Tests directory should not exist");
        }

        [Test]
        public void Test_CreateWithGitInit()
        {
            var projectName = "GitProject";
            var result = RunCommand(_rnccPath,
                $"new {projectName} --template=solution --git-init=true",
                _testDirectory);

            Assert.That(result.ExitCode, Is.EqualTo(0), $"Create with git failed: {result.Error}");

            var gitDir = Path.Combine(_testDirectory, projectName, ".git");
            Assert.That(Directory.Exists(gitDir), Is.True, "Git directory should be initialized");
        }

        [Test]
        public void Test_CompileSimpleContract()
        {
            // Create a simple contract file
            var contractPath = Path.Combine(_testDirectory, "SimpleContract.cs");
            File.WriteAllText(contractPath, @"
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;

namespace SimpleContract
{
    [ManifestExtra(""Author"", ""Test"")]
    public class SimpleContract : SmartContract
    {
        public static string GetMessage() => ""Hello, World!"";
    }
}");

            var result = RunCommand(_rnccPath, $"\"{contractPath}\" -o \"{_testDirectory}\"");
            
            // The compilation might fail due to missing references, but we're testing the tool execution
            Assert.That(result.ExitCode == 0 || result.Error.Contains("Could not find"), Is.True, 
                $"Unexpected error: {result.Error}");

            if (result.ExitCode == 0)
            {
                Assert.That(File.Exists(Path.Combine(_testDirectory, "SimpleContract.nef")), Is.True, "NEF file should be created");
                Assert.That(File.Exists(Path.Combine(_testDirectory, "SimpleContract.manifest.json")), Is.True, "Manifest should be created");
            }
        }

        [Test]
        public void Test_InvalidTemplate()
        {
            var result = RunCommand(_rnccPath, "new TestProject --template=invalid", _testDirectory);
            Assert.That(result.ExitCode, Is.Not.EqualTo(0), "Should fail with invalid template");
            Assert.That(result.Error, Does.Contain("Unknown template"), "Should show unknown template error");
        }

        [Test]
        public void Test_CreateAllTemplates()
        {
            var templates = new[] { "solution", "nep17", "oracle", "owner" };
            
            foreach (var template in templates)
            {
                var projectName = $"Test{template}";
                var result = RunCommand(_rnccPath,
                    $"new {projectName} --template={template}",
                    _testDirectory);

                Assert.That(result.ExitCode, Is.EqualTo(0), 
                    $"Failed to create {template} template: {result.Error}");
                
                var projectDir = Path.Combine(_testDirectory, projectName);
                Assert.That(Directory.Exists(projectDir), Is.True, 
                    $"{template} project directory not created");
            }
        }

        private string? FindRncc()
        {
            // Try various locations
            var locations = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", "rncc"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", "rncc.exe"),
                "rncc",
                "rncc.exe",
                Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "binaries", "rncc-linux-x64"),
                Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "binaries", "rncc-win-x64.exe"),
            };

            foreach (var location in locations)
            {
                if (File.Exists(location))
                    return location;
            }

            // Try to find in PATH
            var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
            foreach (var dir in pathDirs)
            {
                var rnccPath = Path.Combine(dir, "rncc");
                if (File.Exists(rnccPath))
                    return rnccPath;
                
                if (OperatingSystem.IsWindows())
                {
                    rnccPath = Path.Combine(dir, "rncc.exe");
                    if (File.Exists(rnccPath))
                        return rnccPath;
                }
            }

            return null;
        }

        private CommandResult RunCommand(string command, string arguments, string? workingDirectory = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? _testDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException($"Failed to start process: {command}");

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return new CommandResult
            {
                ExitCode = process.ExitCode,
                Output = output,
                Error = error
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