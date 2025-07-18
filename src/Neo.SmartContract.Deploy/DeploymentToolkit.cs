using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo;
using Neo.Wallets;
using Neo.SmartContract.Deploy.Interfaces;
using Neo.SmartContract.Deploy.Models;
using Neo.SmartContract.Deploy.Services;
using Neo.SmartContract.Deploy.Shared;

namespace Neo.SmartContract.Deploy;

/// <summary>
/// Streamlined deployment toolkit providing a simplified API for Neo smart contract deployment with automatic configuration
/// </summary>
public class DeploymentToolkit : IDisposable
{
    private const string GAS_CONTRACT_HASH = "0xd2a4cff31913016155e38e474a2c06d08be276cf";
    private const decimal GAS_DECIMALS = 100_000_000m;
    private readonly string MAINNET_RPC_URL;
    private readonly string TESTNET_RPC_URL;
    private readonly string LOCAL_RPC_URL;
    private readonly string DEFAULT_RPC_URL;

    private readonly NeoContractToolkit _toolkit;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeploymentToolkit> _logger;
    private readonly SemaphoreSlim _walletLock = new SemaphoreSlim(1, 1);
    private volatile bool _walletLoaded = false;
    private volatile string? _currentNetwork = null;
    private volatile string? _wifKey = null;
    private bool _disposed = false;

    /// <summary>
    /// Create a new DeploymentToolkit instance with automatic configuration
    /// </summary>
    /// <param name="configPath">Optional path to configuration file. Defaults to appsettings.json in current directory</param>
    public DeploymentToolkit(string? configPath = null)
    {
        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        if (!string.IsNullOrEmpty(configPath))
        {
            builder.AddJsonFile(configPath, optional: false);
        }
        else
        {
            builder.AddJsonFile("appsettings.json", optional: true);
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        }

        builder.AddEnvironmentVariables();
        _configuration = builder.Build();

        // Initialize RPC URLs from configuration or use defaults
        MAINNET_RPC_URL = _configuration["Network:MainnetRpcUrl"] ?? "https://rpc10.n3.nspcc.ru:10331";
        TESTNET_RPC_URL = _configuration["Network:TestnetRpcUrl"] ?? "https://testnet1.neo.coz.io:443";
        LOCAL_RPC_URL = _configuration["Network:LocalRpcUrl"] ?? "http://localhost:50012";
        DEFAULT_RPC_URL = _configuration["Network:DefaultRpcUrl"] ?? "http://localhost:10332";

        // Create toolkit with minimal setup
        var toolkitBuilder = NeoContractToolkitBuilder.Create()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(_configuration);
            });

        _toolkit = toolkitBuilder.Build();
        _serviceProvider = toolkitBuilder.ServiceProvider!;
        _logger = _toolkit.GetService<ILogger<DeploymentToolkit>>()!;

        // Auto-load wallet if configured
        _ = LoadWalletIfConfigured();
    }

    /// <summary>
    /// Set the network to use (mainnet, testnet, or custom RPC URL)
    /// </summary>
    /// <param name="network">Network name or RPC URL</param>
    /// <returns>This instance for chaining</returns>
    /// <exception cref="ArgumentException">Thrown when network is invalid</exception>
    public DeploymentToolkit SetNetwork(string network)
    {
        if (string.IsNullOrWhiteSpace(network))
            throw new ArgumentException("Network cannot be null or empty", nameof(network));
        _currentNetwork = network.ToLowerInvariant();

        // Update configuration based on network
        var configSection = _configuration.GetSection("Network");

        switch (_currentNetwork)
        {
            case "mainnet":
                Environment.SetEnvironmentVariable("Network__RpcUrl", MAINNET_RPC_URL);
                Environment.SetEnvironmentVariable("Network__Network", "mainnet");
                break;

            case "testnet":
                Environment.SetEnvironmentVariable("Network__RpcUrl", TESTNET_RPC_URL);
                Environment.SetEnvironmentVariable("Network__Network", "testnet");
                break;

            case "local":
            case "private":
                Environment.SetEnvironmentVariable("Network__RpcUrl", LOCAL_RPC_URL);
                Environment.SetEnvironmentVariable("Network__Network", "private");
                break;

            default:
                // Assume it's a custom RPC URL
                if (network.StartsWith("http"))
                {
                    Environment.SetEnvironmentVariable("Network__RpcUrl", network);
                    Environment.SetEnvironmentVariable("Network__Network", "custom");
                }
                break;
        }

        _logger.LogInformation($"Network set to: {_currentNetwork}");
        return this;
    }

    /// <summary>
    /// Set the WIF (Wallet Import Format) key for signing transactions
    /// </summary>
    /// <param name="wifKey">The WIF private key</param>
    /// <returns>The deployment toolkit instance for chaining</returns>
    /// <exception cref="ArgumentException">Thrown when WIF key is invalid</exception>
    public DeploymentToolkit SetWifKey(string wifKey)
    {
        if (string.IsNullOrWhiteSpace(wifKey))
            throw new ArgumentException("WIF key cannot be null or empty", nameof(wifKey));

        try
        {
            // Validate the WIF key by attempting to create a KeyPair
            var privateKey = Neo.Wallets.Wallet.GetPrivateKeyFromWIF(wifKey);
            var keyPair = new KeyPair(privateKey);
            var account = Neo.SmartContract.Contract.CreateSignatureContract(keyPair.PublicKey).ScriptHash;

            _wifKey = wifKey;
            _walletLoaded = true; // Mark as loaded since we have a WIF key

            _logger.LogInformation($"WIF key set for account: {account.ToAddress(Neo.ProtocolSettings.Default.AddressVersion)}");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid WIF key: {ex.Message}", nameof(wifKey));
        }

        return this;
    }

    /// <summary>
    /// Deploy a contract from source code or project
    /// </summary>
    /// <param name="path">Path to contract project (.csproj) or source file</param>
    /// <param name="initParams">Optional initialization parameters</param>
    /// <returns>Deployment information</returns>
    /// <exception cref="ArgumentException">Thrown when path is invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when file does not exist</exception>
    public async Task<ContractDeploymentInfo> DeployAsync(string path, object[]? initParams = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        await EnsureWalletLoaded();

        if (!File.Exists(path))
            throw new FileNotFoundException($"Contract file not found: {path}", path);

        // Determine if it's a project or source file
        var isProject = path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);

        var compilationOptions = new CompilationOptions
        {
            ProjectPath = isProject ? path : null,
            SourcePath = isProject ? null : path,
            OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "artifacts"),
            ContractName = Path.GetFileNameWithoutExtension(path) ?? "Contract"
        };

        var deploymentOptions = new DeploymentOptions
        {
            DeployerAccount = await GetDeployerAccountAsync(),
            GasLimit = _configuration.GetValue<long>("Deployment:GasLimit", 100_000_000),
            WaitForConfirmation = _configuration.GetValue<bool>("Deployment:WaitForConfirmation", true),
            InitialParameters = initParams?.ToList(),
            WifKey = _wifKey
        };

        _logger.LogInformation($"Deploying {compilationOptions.ContractName}...");

        var result = await _toolkit.CompileAndDeployAsync(compilationOptions, deploymentOptions);

        if (result.Success)
        {
            _logger.LogInformation($"✅ Contract deployed: {result.ContractHash}");
        }
        else
        {
            _logger.LogError($"❌ Deployment failed: {result.ErrorMessage}");
        }

        return result;
    }

    /// <summary>
    /// Deploy a pre-compiled contract from NEF and manifest files
    /// </summary>
    /// <param name="nefPath">Path to NEF file</param>
    /// <param name="manifestPath">Path to manifest file</param>
    /// <param name="initParams">Optional initialization parameters</param>
    /// <returns>Deployment information</returns>
    /// <exception cref="ArgumentException">Thrown when paths are invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when files do not exist</exception>
    public async Task<ContractDeploymentInfo> DeployArtifactsAsync(string nefPath, string manifestPath, object[]? initParams = null)
    {
        if (string.IsNullOrWhiteSpace(nefPath))
            throw new ArgumentException("NEF path cannot be null or empty", nameof(nefPath));

        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("Manifest path cannot be null or empty", nameof(manifestPath));

        if (!File.Exists(nefPath))
            throw new FileNotFoundException($"NEF file not found: {nefPath}", nefPath);

        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Manifest file not found: {manifestPath}", manifestPath);
        await EnsureWalletLoaded();

        var deploymentOptions = new DeploymentOptions
        {
            DeployerAccount = await GetDeployerAccountAsync(),
            GasLimit = _configuration.GetValue<long>("Deployment:GasLimit", 100_000_000),
            WaitForConfirmation = _configuration.GetValue<bool>("Deployment:WaitForConfirmation", true),
            InitialParameters = initParams?.ToList(),
            WifKey = _wifKey
        };

        _logger.LogInformation($"Deploying from artifacts...");

        var result = await _toolkit.DeployFromArtifactsAsync(nefPath, manifestPath, deploymentOptions);

        if (result.Success)
        {
            _logger.LogInformation($"✅ Contract deployed: {result.ContractHash}");
        }
        else
        {
            _logger.LogError($"❌ Deployment failed: {result.ErrorMessage}");
        }

        return result;
    }

    /// <summary>
    /// Call a contract method (read-only)
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="contractHashOrAddress">Contract hash or address</param>
    /// <param name="method">Method name</param>
    /// <param name="args">Method arguments</param>
    /// <returns>Method return value</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    public async Task<T> CallAsync<T>(string contractHashOrAddress, string method, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(contractHashOrAddress))
            throw new ArgumentException("Contract hash or address cannot be null or empty", nameof(contractHashOrAddress));

        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Method name cannot be null or empty", nameof(method));
        var contractHash = ParseContractHash(contractHashOrAddress);
        return await _toolkit.CallContractAsync<T>(contractHash!, method, args).ConfigureAwait(false);
    }

    /// <summary>
    /// Invoke a contract method (state-changing transaction)
    /// </summary>
    /// <param name="contractHashOrAddress">Contract hash or address</param>
    /// <param name="method">Method name</param>
    /// <param name="args">Method arguments</param>
    /// <returns>Transaction hash</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    public async Task<UInt256> InvokeAsync(string contractHashOrAddress, string method, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(contractHashOrAddress))
            throw new ArgumentException("Contract hash or address cannot be null or empty", nameof(contractHashOrAddress));

        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Method name cannot be null or empty", nameof(method));
        await EnsureWalletLoaded();

        // Set deployment options with WIF key if available
        if (!string.IsNullOrEmpty(_wifKey))
        {
            _toolkit.SetDeploymentOptions(new DeploymentOptions { WifKey = _wifKey });
        }

        var contractHash = ParseContractHash(contractHashOrAddress);
        return await _toolkit.InvokeContractAsync(contractHash, method, args);
    }

    /// <summary>
    /// Update an existing contract with a new version
    /// </summary>
    /// <param name="contractHashOrAddress">Contract hash or address to update</param>
    /// <param name="path">Path to new contract project (.csproj) or source file (.cs)</param>
    /// <returns>Update result containing transaction hash and success status</returns>
    /// <remarks>
    /// <para>This method updates a deployed contract by calling ContractManagement.Update directly.</para>
    /// <para>When a contract is updated, Neo automatically calls the contract's _deploy method with update=true.</para>
    /// <para>Requirements:</para>
    /// <list type="bullet">
    /// <item><description>The contract's _deploy method must check authorization when update=true</description></item>
    /// <item><description>The calling account must be authorized to update (typically the contract owner)</description></item>
    /// <item><description>Sufficient GAS balance for the update transaction</description></item>
    /// </list>
    /// <para>Example _deploy method with update authorization:</para>
    /// <code>
    /// [DisplayName("_deploy")]
    /// public static void _deploy(object data, bool update)
    /// {
    ///     if (update)
    ///     {
    ///         if (!Runtime.CheckWitness(GetOwner()))
    ///             throw new Exception("Only owner can update contract");
    ///         // Optional: Perform state migration
    ///         return;
    ///     }
    ///     // Initial deployment logic
    /// }
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when file does not exist</exception>
    public async Task<ContractDeploymentInfo> UpdateAsync(string contractHashOrAddress, string path)
    {
        if (string.IsNullOrWhiteSpace(contractHashOrAddress))
            throw new ArgumentException("Contract hash or address cannot be null or empty", nameof(contractHashOrAddress));

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Contract file not found: {path}", path);
        await EnsureWalletLoaded();

        var contractHash = ParseContractHash(contractHashOrAddress);
        var isProject = path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);

        var compilationOptions = new CompilationOptions
        {
            ProjectPath = isProject ? path : null,
            SourcePath = isProject ? null : path,
            OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "artifacts"),
            ContractName = Path.GetFileNameWithoutExtension(path) ?? "Contract"
        };

        var deploymentOptions = new DeploymentOptions
        {
            DeployerAccount = await GetDeployerAccountAsync(),
            WifKey = _wifKey, // Support WIF key for direct signing
            RpcUrl = GetCurrentRpcUrl(),
            NetworkMagic = GetNetworkMagic(),
            GasLimit = _configuration.GetValue<long>("Deployment:GasLimit", 100_000_000),
            WaitForConfirmation = _configuration.GetValue<bool>("Deployment:WaitForConfirmation", true),
            VerifyAfterDeploy = _configuration.GetValue<bool>("Deployment:VerifyAfterDeploy", true)
        };

        _logger.LogInformation($"Updating contract {contractHash}...");

        // Compile first
        var compiler = _serviceProvider.GetRequiredService<IContractCompiler>();
        var compiled = await compiler.CompileAsync(compilationOptions);

        // Use ContractDeployer directly for better WIF key support
        var deployer = _serviceProvider.GetRequiredService<IContractDeployer>();
        var result = await deployer.UpdateAsync(compiled, contractHash, deploymentOptions);

        if (result.Success)
        {
            _logger.LogInformation($"✅ Contract updated: {result.ContractHash}");
        }
        else
        {
            _logger.LogError($"❌ Update failed: {result.ErrorMessage}");
        }

        return result;
    }

    /// <summary>
    /// Get the default deployer account
    /// </summary>
    /// <returns>Deployer account script hash</returns>
    /// <exception cref="InvalidOperationException">Thrown when no deployer account is configured</exception>
    public async Task<UInt160> GetDeployerAccountAsync()
    {
        if (!string.IsNullOrEmpty(_wifKey))
        {
            // Use WIF key to get account
            var privateKey = Neo.Wallets.Wallet.GetPrivateKeyFromWIF(_wifKey);
            var keyPair = new KeyPair(privateKey);
            return Neo.SmartContract.Contract.CreateSignatureContract(keyPair.PublicKey).ScriptHash;
        }

        await EnsureWalletLoaded();
        var account = _toolkit.GetDeployerAccount();
        if (account == null || account == UInt160.Zero)
            throw new InvalidOperationException("No deployer account configured");
        return account;
    }

    /// <summary>
    /// Get the current balance of an account
    /// </summary>
    /// <param name="address">Account address (null for default deployer)</param>
    /// <returns>GAS balance</returns>
    /// <exception cref="ArgumentException">Thrown when address is invalid</exception>
    public async Task<decimal> GetGasBalanceAsync(string? address = null)
    {
        UInt160 account;
        if (string.IsNullOrEmpty(address))
        {
            account = await GetDeployerAccountAsync();
        }
        else
        {
            account = ParseAddress(address);
        }

        // Call GAS token contract
        var gasHash = UInt160.Parse(GAS_CONTRACT_HASH);
        var balance = await CallAsync<System.Numerics.BigInteger>(gasHash.ToString(), "balanceOf", account);

        // GAS has 8 decimals
        return (decimal)balance / GAS_DECIMALS;
    }

    /// <summary>
    /// Deploy multiple contracts from a manifest file
    /// </summary>
    /// <param name="manifestPath">Path to the deployment manifest JSON file</param>
    /// <returns>Dictionary of contract names to deployment information</returns>
    /// <exception cref="ArgumentException">Thrown when manifestPath is invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when manifest file does not exist</exception>
    public async Task<Dictionary<string, ContractDeploymentInfo>> DeployFromManifestAsync(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("Manifest path cannot be null or empty", nameof(manifestPath));

        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Manifest file not found: {manifestPath}", manifestPath);
        await EnsureWalletLoaded();

        var deploymentOptions = new DeploymentOptions
        {
            DeployerAccount = await GetDeployerAccountAsync(),
            GasLimit = _configuration.GetValue<long>("Deployment:GasLimit", 100_000_000),
            WaitForConfirmation = _configuration.GetValue<bool>("Deployment:WaitForConfirmation", true)
        };

        _logger.LogInformation($"Deploying contracts from manifest: {manifestPath}");

        var result = await _toolkit.DeployFromManifestAsync(manifestPath, deploymentOptions);

        // Convert MultiContractDeploymentResult to simplified dictionary
        var deploymentMap = new Dictionary<string, ContractDeploymentInfo>();

        foreach (var deployment in result.SuccessfulDeployments)
        {
            deploymentMap[deployment.ContractName] = deployment;
        }

        if (result.FailedDeployments.Any())
        {
            var failures = string.Join(", ", result.FailedDeployments.Select(f => $"{f.ContractName}: {f.Reason}"));
            _logger.LogWarning($"Some deployments failed: {failures}");
        }

        return deploymentMap;
    }

    /// <summary>
    /// Check if a contract exists at the given address
    /// </summary>
    /// <param name="contractHashOrAddress">Contract hash or address</param>
    /// <returns>True if contract exists, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when contractHashOrAddress is invalid</exception>
    public async Task<bool> ContractExistsAsync(string contractHashOrAddress)
    {
        if (string.IsNullOrWhiteSpace(contractHashOrAddress))
            throw new ArgumentException("Contract hash or address cannot be null or empty", nameof(contractHashOrAddress));
        var contractHash = ParseAddress(contractHashOrAddress);
        var deployer = _serviceProvider.GetRequiredService<IContractDeployer>();
        var rpcUrl = GetCurrentRpcUrl();

        return await deployer.ContractExistsAsync(contractHash, rpcUrl);
    }


    #region Private Methods

    private async Task LoadWalletIfConfigured()
    {
        await _walletLock.WaitAsync();
        try
        {
            if (_walletLoaded) return;

            var walletPath = _configuration["Wallet:Path"];

            if (!string.IsNullOrEmpty(walletPath))
            {
                _logger.LogInformation($"Auto-loading wallet from configuration...");

                // Try secure loading first
                try
                {
                    var walletManager = _serviceProvider.GetService<IWalletManager>();
                    if (walletManager != null)
                    {
                        await walletManager.LoadWalletSecurelyAsync(walletPath);
                        _walletLoaded = true;
                        _logger.LogInformation($"Wallet loaded successfully using secure credential provider");
                        return;
                    }
                }
                catch (Exception secureEx)
                {
                    _logger.LogDebug($"Secure wallet loading failed: {secureEx.Message}");
                }

                // Fall back to password from config (not recommended)
                var walletPassword = _configuration["Wallet:Password"];
                if (!string.IsNullOrEmpty(walletPassword))
                {
                    _logger.LogWarning("Loading wallet with password from configuration. This is not recommended for production!");
                    await _toolkit.LoadWalletAsync(walletPath, walletPassword);
                    _walletLoaded = true;
                    _logger.LogInformation($"Wallet loaded successfully");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to auto-load wallet: {ex.Message}");
        }
        finally
        {
            _walletLock.Release();
        }
    }

    private async Task EnsureWalletLoaded()
    {
        if (_walletLoaded) return;

        // If WIF key is set, we don't need to load wallet
        if (!string.IsNullOrEmpty(_wifKey))
        {
            _walletLoaded = true;
            return;
        }

        await _walletLock.WaitAsync();
        try
        {
            if (_walletLoaded) return; // Double-check after acquiring lock

            // Try to load from environment variables
            var walletPath = Environment.GetEnvironmentVariable("NEO_WALLET_PATH") ?? _configuration["Wallet:Path"];

            if (!string.IsNullOrEmpty(walletPath))
            {
                var walletManager = _serviceProvider.GetService<IWalletManager>();
                if (walletManager != null)
                {
                    try
                    {
                        await walletManager.LoadWalletSecurelyAsync(walletPath);
                        _walletLoaded = true;
                        _logger.LogInformation($"Wallet loaded securely");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"Secure loading failed: {ex.Message}");
                    }
                }

                // Try with explicit password as fallback
                var walletPassword = Environment.GetEnvironmentVariable("NEO_WALLET_PASSWORD");
                if (!string.IsNullOrEmpty(walletPassword))
                {
                    await _toolkit.LoadWalletAsync(walletPath, walletPassword);
                    _walletLoaded = true;
                    _logger.LogInformation($"Wallet loaded from environment variables");
                    return;
                }
            }

            throw new InvalidOperationException(
                "No wallet loaded. Set NEO_WALLET_PATH environment variable and configure " +
                "a secure credential provider or set NEO_WALLET_PASSWORD environment variable.");
        }
        finally
        {
            _walletLock.Release();
        }
    }

    private UInt160 ParseContractHash(string value)
    {
        // Try to parse as hash first
        if (UInt160.TryParse(value, out var hash))
        {
            return hash;
        }

        // Try to parse as address
        try
        {
            return ParseAddress(value);
        }
        catch
        {
            throw new ArgumentException($"Invalid contract hash or address: {value}");
        }
    }

    private UInt160 ParseAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty", nameof(address));

        // Try to parse as hash first
        if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (UInt160.TryParse(address, out var hash))
                return hash;
        }

        try
        {
            // Try common address versions
            if (address.StartsWith("N")) // N3
                return address.ToScriptHash((byte)53);
            if (address.StartsWith("A")) // Legacy
                return address.ToScriptHash((byte)23);

            // Default to N3
            return address.ToScriptHash((byte)53);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid address format: {address}", nameof(address), ex);
        }
    }

    private string GetCurrentRpcUrl()
    {
        if (!string.IsNullOrEmpty(_currentNetwork))
        {
            var networks = _configuration.GetSection("Network:Networks").Get<Dictionary<string, NetworkConfiguration>>();
            if (networks != null && networks.TryGetValue(_currentNetwork, out var network))
            {
                return network.RpcUrl;
            }
        }

        // Fallback to default RPC URL
        return _configuration["Network:RpcUrl"] ?? DEFAULT_RPC_URL;
    }

    private uint GetNetworkMagic()
    {
        if (!string.IsNullOrEmpty(_currentNetwork))
        {
            var networks = _configuration.GetSection("Network:Networks").Get<Dictionary<string, NetworkConfiguration>>();
            if (networks != null && networks.TryGetValue(_currentNetwork, out var network))
            {
                return network.NetworkMagic;
            }
        }

        // Return network magic based on current network
        return _currentNetwork?.ToLower() switch
        {
            "mainnet" => 860833102,
            "testnet" => 894710606,
            _ => _configuration.GetValue<uint>("Network:NetworkMagic", 894710606) // Default to testnet
        };
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Dispose of the toolkit and its resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (_serviceProvider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                }

                // Clear any cached RPC clients if using RpcClientFactory
                if (_serviceProvider != null)
                {
                    try
                    {
                        var rpcFactory = _serviceProvider.GetService(typeof(IRpcClientFactory)) as RpcClientFactory;
                        rpcFactory?.ClearPool();
                    }
                    catch
                    {
                        // Ignore errors during disposal
                    }
                }

                // Dispose of the semaphore
                _walletLock?.Dispose();
            }

            _disposed = true;
        }
    }

    #endregion
}
