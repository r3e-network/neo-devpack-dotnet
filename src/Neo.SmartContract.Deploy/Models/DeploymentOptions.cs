using Neo;
using System.Collections.Generic;

namespace Neo.SmartContract.Deploy.Models;

/// <summary>
/// Deployment options for deploying a smart contract
/// </summary>
public class DeploymentOptions
{
    /// <summary>
    /// Network RPC URL
    /// </summary>
    public string RpcUrl { get; set; } = string.Empty;

    /// <summary>
    /// Network magic number (optional - will be retrieved from RPC if not specified)
    /// </summary>
    public uint? NetworkMagic { get; set; }

    /// <summary>
    /// Deployer account script hash
    /// </summary>
    public UInt160 DeployerAccount { get; set; } = UInt160.Zero;

    /// <summary>
    /// Gas limit for deployment transaction
    /// </summary>
    public long GasLimit { get; set; } = 50_000_000; // 0.5 GAS

    /// <summary>
    /// Whether to wait for transaction confirmation
    /// </summary>
    public bool WaitForConfirmation { get; set; } = true;

    /// <summary>
    /// Number of retries when waiting for confirmation
    /// </summary>
    public int ConfirmationRetries { get; set; } = 30;

    /// <summary>
    /// Delay between confirmation checks in seconds
    /// </summary>
    public int ConfirmationDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Number of blocks before transaction expires (default: 100)
    /// </summary>
    public uint ValidUntilBlockOffset { get; set; } = 100;

    /// <summary>
    /// Default network fee in GAS fractions (default: 0.001 GAS = 1000000)
    /// </summary>
    public long DefaultNetworkFee { get; set; } = 1000000;

    /// <summary>
    /// Whether to use Neo Express for local development
    /// </summary>
    public bool UseNeoExpress { get; set; } = false;

    /// <summary>
    /// Initial parameters to pass to contract constructor
    /// </summary>
    public List<object>? InitialParameters { get; set; }

    /// <summary>
    /// Whether to perform a dry-run without actually deploying
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Whether to verify the contract after deployment
    /// </summary>
    public bool VerifyAfterDeploy { get; set; } = true;

    /// <summary>
    /// Delay in milliseconds before verification (to allow for blockchain propagation)
    /// </summary>
    public int VerificationDelayMs { get; set; } = 5000;

    /// <summary>
    /// Whether to enable automatic rollback on failure
    /// </summary>
    public bool EnableRollback { get; set; } = false;

    /// <summary>
    /// WIF (Wallet Import Format) private key for signing transactions
    /// If provided, will be used instead of loading from wallet file
    /// </summary>
    public string? WifKey { get; set; }
}
