// Copyright (C) 2015-2025 The Neo Project.
//
// Program.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Neo.Extensions;
using Neo.Json;
using Neo.Optimizer;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.SmartContract.Testing.Extensions;
using Neo.Compiler.WebGui;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Neo.Compiler.Tool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            RootCommand rootCommand = new(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()!.Title)
            {
                new Argument<string[]>("paths", "The path of the solution file, project file, project directory or source files."),
                new Option<string>(["-o", "--output"], "Specifies the output directory."),
                new Option<string>("--base-name", "Specifies the base name of the output files."),
                new Option<NullableContextOptions>("--nullable", () => NullableContextOptions.Annotations, "Represents the default state of nullable analysis in this compilation."),
                new Option<bool>("--checked", "Indicates whether to check for overflow and underflow."),
                new Option<bool>("--assembly", "Indicates whether to generate assembly."),
                new Option<Options.GenerateArtifactsKind>("--generate-artifacts", "Instruct the compiler how to generate artifacts."),
                new Option<bool>("--security-analysis", "Whether to perform security analysis on the compiled contract"),
                new Option<bool>("--generate-interface", "Generate interface file for contracts with the Contract attribute"),
                new Option<bool>("--generate-plugin", "Generate a Neo N3 plugin for interacting with the compiled contract"),
                new Option<bool>("--generate-webgui", "Generate an interactive web GUI for the compiled contract"),
                new Option<bool>("--deploy-webgui", "Deploy the generated WebGUI to R3E hosting service"),
                new Option<string>("--webgui-service-url", () => "https://api.r3e-gui.com", "URL of the R3E WebGUI hosting service"),
                new Option<string>("--contract-address", "The deployed contract address for WebGUI deployment"),
                new Option<string>("--network", () => "testnet", "The network where the contract is deployed"),
                new Option<string>("--deployer-address", "The address of the contract deployer"),
                new Option<CompilationOptions.OptimizationType>("--optimize", $"Optimization level. e.g. --optimize={CompilationOptions.OptimizationType.All}"),
                new Option<bool>("--no-inline", "Instruct the compiler not to insert inline code."),
                new Option<byte>("--address-version", () => ProtocolSettings.Default.AddressVersion, "Indicates the address version used by the compiler.")
            };

            // Add template commands
            var newCommand = new Command("new", "Create a new Neo smart contract solution from template")
            {
                new Argument<string>("name", "The name of the contract/solution to create"),
                new Option<string>("--template", () => "solution", "The template to use: solution, nep17, nep11, oracle, owner"),
                new Option<string>("--author", () => "Developer", "The author name for the contract"),
                new Option<string>("--email", () => "developer@example.com", "The author email for the contract"),
                new Option<bool>("--with-tests", () => true, "Include test project"),
                new Option<bool>("--with-deploy-scripts", () => true, "Include deployment scripts"),
                new Option<bool>("--git-init", () => false, "Initialize git repository")
            };
            newCommand.Handler = CommandHandler.Create<string, string, string, string, bool, bool, bool>(HandleNewCommand);
            rootCommand.AddCommand(newCommand);

            var templatesCommand = new Command("templates", "List available templates");
            templatesCommand.Handler = CommandHandler.Create(HandleTemplatesCommand);
            rootCommand.AddCommand(templatesCommand);

            var debugOption = new Option<CompilationOptions.DebugType>(["-d", "--debug"],
                new ParseArgument<CompilationOptions.DebugType>(ParseDebug), description: "Indicates the debug level.")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            rootCommand.AddOption(debugOption);

            rootCommand.Handler = CommandHandler.Create<RootCommand, Options, string[], InvocationContext>(Handle);
            return rootCommand.Invoke(args);
        }

        private static CompilationOptions.DebugType ParseDebug(ArgumentResult result)
        {
            var debugValue = result.Tokens.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(debugValue)) return CompilationOptions.DebugType.Extended;

            if (!Enum.TryParse<CompilationOptions.DebugType>(debugValue, true, out var ret))
            {
                throw new ArgumentException($"Invalid debug type: {debugValue}");
            }

            return ret;
        }

        private static void Handle(RootCommand command, Options options, string[]? paths, InvocationContext context)
        {
            // Check if the --generate-interface option is present in the command line args
            options.GenerateContractInterface = context.ParseResult.CommandResult.Children
                .Any(token => token.Symbol.Name == "generate-interface");

            // Check if the --generate-plugin option is present in the command line args
            options.GeneratePlugin = context.ParseResult.CommandResult.Children
                .Any(token => token.Symbol.Name == "generate-plugin");

            // Check if the --generate-webgui option is present in the command line args
            options.GenerateWebGui = context.ParseResult.CommandResult.Children
                .Any(token => token.Symbol.Name == "generate-webgui");

            // Check WebGUI deployment options
            options.DeployWebGui = context.ParseResult.CommandResult.Children
                .Any(token => token.Symbol.Name == "deploy-webgui");

            // Extract WebGUI deployment parameters
            if (options.DeployWebGui)
            {
                options.WebGuiServiceUrl = context.ParseResult.GetValueForOption(
                    context.ParseResult.CommandResult.Command.Options.FirstOrDefault(o => o.Name == "webgui-service-url")) as string;
                options.ContractAddress = context.ParseResult.GetValueForOption(
                    context.ParseResult.CommandResult.Command.Options.FirstOrDefault(o => o.Name == "contract-address")) as string;
                options.Network = context.ParseResult.GetValueForOption(
                    context.ParseResult.CommandResult.Command.Options.FirstOrDefault(o => o.Name == "network")) as string;
                options.DeployerAddress = context.ParseResult.GetValueForOption(
                    context.ParseResult.CommandResult.Command.Options.FirstOrDefault(o => o.Name == "deployer-address")) as string;

                // Auto-enable WebGUI generation if deployment is requested
                if (!options.GenerateWebGui)
                {
                    options.GenerateWebGui = true;
                    Console.WriteLine("Auto-enabling WebGUI generation for deployment...");
                }
            }

            if (paths is null || paths.Length == 0)
            {
                // catch Unhandled exception: System.Reflection.TargetInvocationException
                try
                {
                    context.ExitCode = ProcessDirectory(options, Environment.CurrentDirectory);
                    if (context.ExitCode == 2)
                    {
                        // Display help without args
                        command.Invoke("--help");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Unauthorized to access the project directory, or no project is specified. Please ensure you have the proper permissions and a project is specified.");
                }

                return;
            }
            paths = paths.Select(Path.GetFullPath).ToArray();
            if (paths.Length == 1)
            {
                string path = paths[0];
                if (Directory.Exists(path))
                {
                    context.ExitCode = ProcessDirectory(options, path);
                    return;
                }
                if (File.Exists(path))
                {
                    string extension = Path.GetExtension(path).ToLowerInvariant();
                    if (extension == ".csproj")
                    {
                        context.ExitCode = ProcessCsproj(options, path);
                        return;
                    }
                    else if (extension == ".sln")
                    {
                        context.ExitCode = ProcessSln(options, path);
                        return;
                    }
                }
            }
            foreach (string path in paths)
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension == ".nef")
                {
                    if (options.Optimize != CompilationOptions.OptimizationType.Experimental)
                    {
                        Console.Error.WriteLine($"Required {nameof(options.Optimize).ToLower()}={options.Optimize}, " +
                            $"but the .nef optimizer supports only {CompilationOptions.OptimizationType.Experimental} level of optimization. ");
                        Console.Error.WriteLine($"Still using {nameof(options.Optimize).ToLower()}={CompilationOptions.OptimizationType.Experimental}");
                        options.Optimize = CompilationOptions.OptimizationType.Experimental;
                    }
                    string directory = Path.GetDirectoryName(path)!;
                    string filename = Path.GetFileNameWithoutExtension(path)!;
                    Console.WriteLine($"Optimizing {filename}.nef to {filename}.optimized.nef...");
                    NefFile nef = NefFile.Parse(File.ReadAllBytes(path));
                    string manifestPath = Path.Join(directory, filename + ".manifest.json");
                    if (!File.Exists(manifestPath))
                        throw new FileNotFoundException($"{filename}.manifest.json required for optimization");
                    ContractManifest manifest = ContractManifest.Parse(File.ReadAllText(manifestPath));
                    string debugInfoPath = Path.Join(directory, filename + ".nefdbgnfo");
                    JObject? debugInfo;
                    if (File.Exists(debugInfoPath))
                        debugInfo = (JObject?)JObject.Parse(DumpNef.UnzipDebugInfo(File.ReadAllBytes(debugInfoPath)));
                    else
                        debugInfo = null;
                    (nef, manifest, debugInfo) = Neo.Optimizer.Optimizer.Optimize(nef, manifest, debugInfo, optimizationType: options.Optimize);
                    File.WriteAllBytes(Path.Combine(directory, filename + ".optimized.nef"), nef.ToArray());
                    File.WriteAllBytes(Path.Combine(directory, filename + ".optimized.manifest.json"), manifest.ToJson().ToByteArray(true));
                    if (options.Assembly)
                    {
                        string dumpnef = DumpNef.GenerateDumpNef(nef, debugInfo, manifest);
                        File.WriteAllText(Path.Combine(directory, filename + ".optimized.nef.txt"), dumpnef);
                    }
                    if (debugInfo != null)
                        File.WriteAllBytes(Path.Combine(directory, filename + ".optimized.nefdbgnfo"), DumpNef.ZipDebugInfo(debugInfo.ToByteArray(true), filename + ".optimized.debug.json"));
                    Console.WriteLine($"Optimization finished.");
                    if (options.SecurityAnalysis)
                        SecurityAnalyzer.SecurityAnalyzer.AnalyzeWithPrint(nef, manifest, debugInfo);
                    return;
                }
                else if (extension != ".cs")
                {
                    Console.Error.WriteLine("The files must have a .cs extension.");
                    context.ExitCode = 1;
                    Console.Error.WriteLine("Maybe invalid command line args. Got the following paths to compile:");
                    foreach (string p in paths)
                        Console.Error.WriteLine($"  {p}");
                    return;
                }
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"The file \"{path}\" doesn't exist.");
                    context.ExitCode = 1;
                    return;
                }
            }
            context.ExitCode = ProcessSources(options, Path.GetDirectoryName(paths[0])!, paths);
        }

        private static int ProcessDirectory(Options options, string path)
        {
            // First, look for a solution file
            string? sln = Directory.EnumerateFiles(path, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (sln is not null)
            {
                Console.WriteLine($"Found solution file: {Path.GetFileName(sln)}");
                return ProcessSln(options, sln);
            }

            // If no solution file, look for a project file
            string? csproj = Directory.EnumerateFiles(path, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (csproj is not null)
                return ProcessCsproj(options, csproj);

            // Look for solution files in subdirectories
            Console.WriteLine($"No .sln or .csproj file found in \"{path}\". Searching in sub-directories.");
            List<string> slnFiles = Directory.EnumerateFiles(path, "*.sln", SearchOption.AllDirectories).ToList();
            if (slnFiles.Count > 0)
            {
                Console.WriteLine($"Will process {slnFiles.Count} .sln files in sub-directories.");
                return Enumerable.Max(slnFiles.Select((slnFile) =>
                    ProcessSln(options, slnFile)));
            }

            // Look for project files in subdirectories
            List<string> csprojFiles = Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories).ToList();
            if (csprojFiles.Count > 0)
            {
                Console.WriteLine($"Will process {csprojFiles.Count} .csproj files in sub-directories.");
                return Enumerable.Max(csprojFiles.Select((csprojFile) =>
                    ProcessCsproj(options, csprojFile)));
            }
            string obj = Path.Combine(path, "obj");
            string[] sourceFiles = Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories).Where(p => !p.StartsWith(obj)).ToArray();
            if (sourceFiles.Length == 0)
            {
                Console.Error.WriteLine($"No .cs file is found in \"{path}\".");
                return 2;
            }
            Console.WriteLine($"Will process {sourceFiles.Length} .cs files in the requested path and its sub-directories.");
            return ProcessSources(options, path, sourceFiles);
        }

        private static int ProcessCsproj(Options options, string path)
        {
            return ProcessOutputs(options, Path.GetDirectoryName(path)!, new CompilationEngine(options).CompileProject(path));
        }

        private static int ProcessSln(Options options, string path)
        {
            try
            {
                string solutionDir = Path.GetDirectoryName(path)!;
                string solutionContent = File.ReadAllText(path);

                // Use regex to find all project references in the solution file
                var projectRegex = new Regex(@"Project\(""\{[\w-]+\}""\)\s*=\s*""[^""]*"",\s*""([^""]*\.csproj)"",\s*""\{[\w-]+\}""")
                ;
                var matches = projectRegex.Matches(solutionContent);

                if (matches.Count == 0)
                {
                    Console.Error.WriteLine("No project files found in the solution.");
                    return 1;
                }

                Console.WriteLine($"Found {matches.Count} projects in solution {Path.GetFileName(path)}");
                List<string> projectPaths = new();

                foreach (Match match in matches.Cast<Match>())
                {
                    string relativePath = match.Groups[1].Value;
                    // Replace backslashes with forward slashes for cross-platform compatibility
                    relativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar);
                    string fullPath = Path.GetFullPath(Path.Combine(solutionDir, relativePath));

                    if (File.Exists(fullPath))
                    {
                        projectPaths.Add(fullPath);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Project file not found: {fullPath}");
                    }
                }

                // Process each project file
                List<CompilationContext> allContexts = new();
                foreach (string projectPath in projectPaths)
                {
                    try
                    {
                        Console.WriteLine($"Compiling project: {Path.GetFileName(projectPath)}");
                        var contexts = new CompilationEngine(options).CompileProject(projectPath);
                        allContexts.AddRange(contexts);
                    }
                    catch (Exception ex) when (!(ex is FormatException && ex.Message.Contains("No valid neo SmartContract found")))
                    {
                        // Only log errors for projects that aren't smart contracts
                        Console.WriteLine($"Error compiling project {Path.GetFileName(projectPath)}: {ex.Message}");
                    }
                }

                if (allContexts.Count == 0)
                {
                    Console.Error.WriteLine("No valid Neo smart contracts found in any projects in the solution.");
                    return 1;
                }

                return ProcessOutputs(options, solutionDir, allContexts);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error processing solution: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static int ProcessSources(Options options, string folder, string[] sourceFiles)
        {
            return ProcessOutputs(options, folder, new CompilationEngine(options).CompileSources(sourceFiles));
        }

        private static int ProcessOutputs(Options options, string folder, List<CompilationContext> contexts)
        {
            int result = 0;
            List<Exception> exceptions = new();
            foreach (CompilationContext context in contexts)
                try
                {
                    if (ProcessOutput(options, folder, context) != 0)
                        result = 1;
                }
                catch (Exception e)
                {
                    result = 1;
                    exceptions.Add(e);
                }
            foreach (Exception e in exceptions)
                Console.Error.WriteLine(e.ToString());
            return result;
        }

        private static int ProcessOutput(Options options, string folder, CompilationContext context)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                    Console.Error.WriteLine(diagnostic.ToString());
                else
                    Console.WriteLine(diagnostic.ToString());
            }
            if (context.Success)
            {
                string outputFolder = options.Output ?? Path.Combine(folder, "bin", "sc");
                string path = outputFolder;
                string baseName = context.ContractName!;

                NefFile nef;
                ContractManifest manifest;
                JToken debugInfo;
                try
                {
                    (nef, manifest, debugInfo) = context.CreateResults(folder);
                }
                catch (CompilationException ex)
                {
                    Console.Error.WriteLine(ex.Diagnostic);
                    return -1;
                }

                try
                {
                    Directory.CreateDirectory(outputFolder);
                    path = Path.Combine(path, $"{baseName}.nef");
                    File.WriteAllBytes(path, nef.ToArray());
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Can't create {path}. {ex.Message}.");
                    return 1;
                }
                Console.WriteLine($"Created {path}");
                path = Path.Combine(outputFolder, $"{baseName}.manifest.json");
                try
                {
                    File.WriteAllBytes(path, manifest.ToJson().ToByteArray(false));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Can't create {path}. {ex.Message}.");
                    return 1;
                }
                Console.WriteLine($"Created {path}");

                if (options.GenerateArtifacts != Options.GenerateArtifactsKind.None)
                {
                    var artifact = manifest.GetArtifactsSource(baseName, nef, debugInfo: debugInfo);

                    if (options.GenerateArtifacts.HasFlag(Options.GenerateArtifactsKind.Source))
                    {
                        path = Path.Combine(outputFolder, $"{baseName}.artifacts.cs");
                        File.WriteAllText(path, artifact);
                        Console.WriteLine($"Created {path}");
                    }

                    if (options.GenerateArtifacts.HasFlag(Options.GenerateArtifactsKind.Library))
                    {
                        try
                        {
                            // Try to compile the artifacts into a dll

                            var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
                            var references = new MetadataReference[]
                            {
                                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")),
                                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(DisplayNameAttribute).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(System.Numerics.BigInteger).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(NeoSystem).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(SmartContract.Testing.TestEngine).Assembly.Location)
                            };

                            CSharpCompilationOptions csOptions = new(
                                    OutputKind.DynamicallyLinkedLibrary,
                                    optimizationLevel: OptimizationLevel.Debug,
                                    platform: Platform.AnyCpu,
                                    nullableContextOptions: NullableContextOptions.Enable,
                                    deterministic: true);

                            var syntaxTree = CSharpSyntaxTree.ParseText(artifact, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));
                            var compilation = CSharpCompilation.Create(baseName, new[] { syntaxTree }, references, csOptions);

                            using var ms = new MemoryStream();
                            EmitResult result = compilation.Emit(ms);

                            if (!result.Success)
                            {
                                var failures = result.Diagnostics.Where(diagnostic =>
                                    diagnostic.IsWarningAsError ||
                                    diagnostic.Severity == DiagnosticSeverity.Error);

                                foreach (var diagnostic in failures)
                                {
                                    Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                                }
                            }
                            else
                            {
                                ms.Seek(0, SeekOrigin.Begin);

                                // Write dll

                                path = Path.Combine(outputFolder, $"{baseName}.artifacts.dll");
                                File.WriteAllBytes(path, ms.ToArray());
                                Console.WriteLine($"Created {path}");
                            }
                        }
                        catch
                        {
                            Console.Error.WriteLine("Artifacts compilation error.");
                        }
                    }
                }
                if (options.Debug != CompilationOptions.DebugType.None)
                {
                    path = Path.Combine(outputFolder, $"{baseName}.nefdbgnfo");
                    using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
                    using ZipArchive archive = new(fs, ZipArchiveMode.Create);
                    using Stream stream = archive.CreateEntry($"{baseName}.debug.json").Open();
                    stream.Write(debugInfo.ToByteArray(false));
                    Console.WriteLine($"Created {path}");
                }
                if (options.Assembly)
                {
                    path = Path.Combine(outputFolder, $"{baseName}.asm");
                    File.WriteAllText(path, context.CreateAssembly());
                    Console.WriteLine($"Created {path}");
                    try
                    {
                        path = Path.Combine(outputFolder, $"{baseName}.nef.txt");
                        File.WriteAllText(path, DumpNef.GenerateDumpNef(nef, debugInfo, manifest));
                        Console.WriteLine($"Created {path}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to dumpnef: {ex}");
                    }
                }
                Console.WriteLine("Compilation completed successfully.");

                if (options.SecurityAnalysis)
                {
                    Console.WriteLine("Performing security analysis...");
                    try
                    {
                        SecurityAnalyzer.SecurityAnalyzer.AnalyzeWithPrint(nef, manifest, debugInfo);
                    }
                    catch (Exception e) { Console.WriteLine(e); }
                    Console.WriteLine("Finished security analysis.");
                    Console.WriteLine("There can be many false positives in the security analysis. Take it easy.");
                }

                // Generate contract interface if the option is enabled
                if (options.GenerateContractInterface)
                {
                    var contractHash = context.GetContractHash();
                    if (contractHash != null)
                    {
                        var interfacePath = Path.Combine(outputFolder, $"I{baseName}.cs");
                        try
                        {
                            var interfaceSource = ContractInterfaceGenerator.GenerateInterface(baseName, manifest, contractHash);
                            File.WriteAllText(interfacePath, interfaceSource);
                            Console.WriteLine($"Created contract interface: {interfacePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error generating contract interface: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipping interface generation for {baseName} as no contract hash was found.");
                    }
                }

                // Generate plugin if the option is enabled
                if (options.GeneratePlugin)
                {
                    var contractHash = context.GetContractHash();
                    if (contractHash != null)
                    {
                        try
                        {
                            ContractPluginGenerator.GeneratePlugin(baseName, manifest, contractHash, outputFolder);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error generating plugin: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipping plugin generation for {baseName} as no contract hash was found.");
                    }
                }

                // Generate web GUI if the option is enabled
                if (options.GenerateWebGui)
                {
                    var contractHash = context.GetContractHash();
                    try
                    {
                        var webGuiResult = context.GenerateWebGui(outputFolder);
                        if (webGuiResult.Success)
                        {
                            Console.WriteLine($"Created web GUI at: {webGuiResult.OutputDirectory}");
                            Console.WriteLine($"Main HTML file: {webGuiResult.HtmlFilePath}");

                            // Deploy WebGUI if requested
                            if (options.DeployWebGui)
                            {
                                try
                                {
                                    Console.WriteLine("🚀 Deploying WebGUI to R3E hosting service...");
                                    Task.Run(async () => await DeployWebGuiAsync(options, webGuiResult, context.ContractName!)).GetAwaiter().GetResult();
                                }
                                catch (Exception deployEx)
                                {
                                    Console.Error.WriteLine($"❌ WebGUI deployment failed: {deployEx.Message}");
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"Error generating web GUI: {webGuiResult.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error generating web GUI: {ex.Message}");
                    }
                }

                return 0;
            }
            else
            {
                Console.Error.WriteLine("Compilation failed.");
                return 1;
            }
        }

        private static async Task DeployWebGuiAsync(Options options, WebGuiGenerationResult webGuiResult, string contractName)
        {
            if (string.IsNullOrEmpty(options.ContractAddress) || string.IsNullOrEmpty(options.DeployerAddress))
            {
                throw new ArgumentException("Contract address and deployer address are required for WebGUI deployment");
            }

            var serviceUrl = options.WebGuiServiceUrl ?? "https://api.r3e-gui.com";
            
            using var httpClient = new HttpClient();
            using var form = new MultipartFormDataContent();

            // Add metadata
            form.Add(new StringContent(options.ContractAddress), "contractAddress");
            form.Add(new StringContent(contractName), "contractName");
            form.Add(new StringContent(options.Network ?? "testnet"), "network");
            form.Add(new StringContent(options.DeployerAddress), "deployerAddress");
            form.Add(new StringContent($"WebGUI for {contractName} contract"), "description");

            // Add WebGUI files
            var webGuiDirectory = webGuiResult.OutputDirectory!;
            var files = Directory.GetFiles(webGuiDirectory, "*", SearchOption.AllDirectories);
            
            foreach (var filePath in files)
            {
                var relativePath = Path.GetRelativePath(webGuiDirectory, filePath);
                var fileContent = await File.ReadAllBytesAsync(filePath);
                var byteContent = new ByteArrayContent(fileContent);
                
                var contentType = GetContentType(filePath);
                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                
                form.Add(byteContent, "webGUIFiles", relativePath);
            }

            Console.WriteLine($"📁 Uploading {files.Length} WebGUI files...");

            // Deploy to service
            var response = await httpClient.PostAsync($"{serviceUrl}/api/webgui/deploy", form);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<DeploymentResult>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                Console.WriteLine("✅ WebGUI deployed successfully!");
                Console.WriteLine($"🌐 Subdomain: {result?.Subdomain}");
                Console.WriteLine($"🔗 URL: {result?.Url}");
                Console.WriteLine($"📍 Contract: {result?.ContractAddress}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Deployment failed ({response.StatusCode}): {errorContent}");
            }
        }

        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
        }

        private static void HandleTemplatesCommand()
        {
            Console.WriteLine("Available Neo Smart Contract Templates:");
            Console.WriteLine();
            Console.WriteLine("📄 solution        - Complete contract solution with testing and deployment");
            Console.WriteLine("💰 nep17           - NEP-17 fungible token contract");
            Console.WriteLine("🎨 nep11           - NEP-11 non-fungible token contract");
            Console.WriteLine("🔮 oracle          - Oracle contract for external data");
            Console.WriteLine("👤 owner           - Contract with owner functionality");
            Console.WriteLine();
            Console.WriteLine("Usage: rncc new MyContract --template=solution --author=\"Your Name\"");
        }

        private static void HandleNewCommand(string name, string template, string author, string email, bool withTests, bool withDeployScripts, bool gitInit)
        {
            try
            {
                Console.WriteLine($"Creating new Neo smart contract: {name}");
                Console.WriteLine($"Template: {template}");
                Console.WriteLine($"Author: {author}");
                Console.WriteLine();

                var outputDir = Path.Combine(Environment.CurrentDirectory, name);
                
                if (Directory.Exists(outputDir))
                {
                    Console.Error.WriteLine($"Directory '{name}' already exists!");
                    return;
                }

                Directory.CreateDirectory(outputDir);

                // Get the assembly location to find templates
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                var templatesDir = Path.Combine(assemblyDir, "templates");
                
                // If templates aren't found next to the assembly, try the source location
                if (!Directory.Exists(templatesDir))
                {
                    // Try to find templates in the source directory structure
                    var currentDir = Directory.GetCurrentDirectory();
                    var srcDir = FindSourceDirectory(currentDir);
                    if (srcDir != null)
                    {
                        templatesDir = Path.Combine(srcDir, "src", "Neo.SmartContract.Template", "templates");
                    }
                }

                if (!Directory.Exists(templatesDir))
                {
                    Console.Error.WriteLine("Templates directory not found. Please ensure RNCC is properly installed.");
                    return;
                }

                CreateFromTemplate(name, template, author, email, withTests, withDeployScripts, templatesDir, outputDir);

                if (gitInit)
                {
                    InitializeGitRepository(outputDir);
                }

                Console.WriteLine($"✅ Successfully created '{name}' in {outputDir}");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine($"  cd {name}");
                Console.WriteLine("  dotnet build");
                Console.WriteLine("  dotnet test");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating contract: {ex.Message}");
            }
        }

        private static void CreateFromTemplate(string name, string template, string author, string email, bool withTests, bool withDeployScripts, string templatesDir, string outputDir)
        {
            switch (template.ToLowerInvariant())
            {
                case "solution":
                    CreateSolutionTemplate(name, author, email, withTests, withDeployScripts, templatesDir, outputDir);
                    break;
                case "nep17":
                    CreateNep17Template(name, author, email, templatesDir, outputDir);
                    break;
                case "nep11":
                    CreateNep11Template(name, author, email, templatesDir, outputDir);
                    break;
                case "oracle":
                    CreateOracleTemplate(name, author, email, templatesDir, outputDir);
                    break;
                case "owner":
                    CreateOwnerTemplate(name, author, email, templatesDir, outputDir);
                    break;
                default:
                    throw new ArgumentException($"Unknown template: {template}");
            }
        }

        private static void CreateSolutionTemplate(string name, string author, string email, bool withTests, bool withDeployScripts, string templatesDir, string outputDir)
        {
            var solutionTemplate = Path.Combine(templatesDir, "neocontractsolution");
            if (!Directory.Exists(solutionTemplate))
            {
                Console.Error.WriteLine("Solution template not found.");
                return;
            }

            // Copy the entire solution template
            CopyDirectory(solutionTemplate, outputDir);

            // Replace template variables
            var replacements = new Dictionary<string, string>
            {
                { "MyContract", name },
                { "NeoContractSolution", name },
                { "TemplateAuthor", author },
                { "TemplateEmail", email },
                { "TemplateNeoVersion", "1.0.0" },
                { "TemplateDotNetVersion", "9.0.0" },
                { "TemplateFrameworkVersion", "net9.0" }
            };

            ProcessTemplateReplacements(outputDir, replacements);

            // Rename files and directories
            RenameTemplateFiles(outputDir, "MyContract", name);
            RenameTemplateFiles(outputDir, "NeoContractSolution", name);

            if (!withTests)
            {
                var testsDir = Path.Combine(outputDir, "tests");
                if (Directory.Exists(testsDir))
                {
                    Directory.Delete(testsDir, true);
                }
            }

            if (!withDeployScripts)
            {
                var deployDir = Path.Combine(outputDir, "deploy");
                if (Directory.Exists(deployDir))
                {
                    Directory.Delete(deployDir, true);
                }
            }

            Console.WriteLine($"Created solution template with:");
            Console.WriteLine($"  📁 src/{name}/ - Contract project");
            if (withTests) Console.WriteLine($"  🧪 tests/{name}.Tests/ - Test project");
            if (withDeployScripts) Console.WriteLine($"  🚀 deploy/ - Deployment scripts");
        }

        private static void CreateNep17Template(string name, string author, string email, string templatesDir, string outputDir)
        {
            var nep17Template = Path.Combine(templatesDir, "neocontractnep17");
            if (!Directory.Exists(nep17Template))
            {
                Console.Error.WriteLine("NEP-17 template not found.");
                return;
            }

            CopyDirectory(nep17Template, outputDir);

            var replacements = new Dictionary<string, string>
            {
                { "Nep17Contract", name },
                { "TemplateAuthor", author },
                { "TemplateEmail", email },
                { "TemplateNeoVersion", "1.0.0" }
            };

            ProcessTemplateReplacements(outputDir, replacements);
            RenameTemplateFiles(outputDir, "Nep17Contract", name);

            Console.WriteLine($"Created NEP-17 token contract: {name}");
        }

        private static void CreateNep11Template(string name, string author, string email, string templatesDir, string outputDir)
        {
            // For now, use the NEP-17 template and modify it
            CreateNep17Template(name, author, email, templatesDir, outputDir);
            Console.WriteLine($"Created NEP-11 NFT contract: {name} (based on NEP-17 template)");
        }

        private static void CreateOracleTemplate(string name, string author, string email, string templatesDir, string outputDir)
        {
            var oracleTemplate = Path.Combine(templatesDir, "neocontractoracle");
            if (!Directory.Exists(oracleTemplate))
            {
                Console.Error.WriteLine("Oracle template not found.");
                return;
            }

            CopyDirectory(oracleTemplate, outputDir);

            var replacements = new Dictionary<string, string>
            {
                { "OracleRequest", name },
                { "TemplateAuthor", author },
                { "TemplateEmail", email },
                { "TemplateNeoVersion", "1.0.0" }
            };

            ProcessTemplateReplacements(outputDir, replacements);
            RenameTemplateFiles(outputDir, "OracleRequest", name);

            Console.WriteLine($"Created Oracle contract: {name}");
        }

        private static void CreateOwnerTemplate(string name, string author, string email, string templatesDir, string outputDir)
        {
            var ownerTemplate = Path.Combine(templatesDir, "neocontractowner");
            if (!Directory.Exists(ownerTemplate))
            {
                Console.Error.WriteLine("Owner template not found.");
                return;
            }

            CopyDirectory(ownerTemplate, outputDir);

            var replacements = new Dictionary<string, string>
            {
                { "Ownable", name },
                { "TemplateAuthor", author },
                { "TemplateEmail", email },
                { "TemplateNeoVersion", "1.0.0" }
            };

            ProcessTemplateReplacements(outputDir, replacements);
            RenameTemplateFiles(outputDir, "Ownable", name);

            Console.WriteLine($"Created Owner contract: {name}");
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                if (dirName == ".template.config") continue; // Skip template config

                var destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir);
            }
        }

        private static void ProcessTemplateReplacements(string directory, Dictionary<string, string> replacements)
        {
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains(".git") && !f.Contains("bin") && !f.Contains("obj"))
                .ToArray();

            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var modified = false;

                    foreach (var replacement in replacements)
                    {
                        if (content.Contains(replacement.Key))
                        {
                            content = content.Replace(replacement.Key, replacement.Value);
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        File.WriteAllText(file, content);
                    }
                }
                catch
                {
                    // Skip binary files or files that can't be read
                }
            }
        }

        private static void RenameTemplateFiles(string directory, string oldName, string newName)
        {
            // Rename files
            var files = Directory.GetFiles(directory, $"*{oldName}*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var newFileName = Path.GetFileName(file).Replace(oldName, newName);
                var newFilePath = Path.Combine(Path.GetDirectoryName(file)!, newFileName);
                File.Move(file, newFilePath);
            }

            // Rename directories
            var directories = Directory.GetDirectories(directory, $"*{oldName}*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length) // Process deeper directories first
                .ToArray();

            foreach (var dir in directories)
            {
                var newDirName = Path.GetFileName(dir).Replace(oldName, newName);
                var newDirPath = Path.Combine(Path.GetDirectoryName(dir)!, newDirName);
                Directory.Move(dir, newDirPath);
            }
        }

        private static string? FindSourceDirectory(string currentDir)
        {
            var dir = new DirectoryInfo(currentDir);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "src")) && 
                    Directory.Exists(Path.Combine(dir.FullName, "src", "Neo.SmartContract.Template")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return null;
        }

        private static void InitializeGitRepository(string directory)
        {
            try
            {
                var gitDir = Path.Combine(directory, ".git");
                if (Directory.Exists(gitDir))
                {
                    Console.WriteLine("Git repository already exists.");
                    return;
                }

                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "init",
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                process?.WaitForExit();

                if (process?.ExitCode == 0)
                {
                    Console.WriteLine("Initialized git repository.");
                }
                else
                {
                    Console.WriteLine("Failed to initialize git repository.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize git repository: {ex.Message}");
            }
        }
    }

    public class DeploymentResult
    {
        public bool Success { get; set; }
        public string? Subdomain { get; set; }
        public string? Url { get; set; }
        public string? ContractAddress { get; set; }
    }
}
