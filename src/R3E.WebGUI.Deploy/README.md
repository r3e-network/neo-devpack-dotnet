# R3E.WebGUI.Deploy

Command-line tool for deploying Neo smart contract WebGUIs to the R3E hosting service. Automatically generates and deploys professional web interfaces from contract manifests.

## Features

- Deploy WebGUIs from contract manifests
- Signature-based authentication
- Plugin upload support
- Multi-wallet integration
- JSON-based configuration
- Subdomain routing support

## Installation

```bash
dotnet tool install -g R3E.WebGUI.Deploy
```

## Usage

### Basic Deployment

```bash
r3e-webgui-deploy \
  --contract-address 0x1234567890abcdef1234567890abcdef12345678 \
  --deployer-address NPvKVTGZapmFWABLsyvfreuqn73jCjJtN5 \
  --network testnet
```

### Full Deployment with Options

```bash
r3e-webgui-deploy \
  --contract-address 0x1234567890abcdef1234567890abcdef12345678 \
  --deployer-address NPvKVTGZapmFWABLsyvfreuqn73jCjJtN5 \
  --network testnet \
  --name "MyToken" \
  --description "A sample NEP-17 token with WebGUI" \
  --service-url http://localhost:8888 \
  --plugin-path ./MyTokenPlugin.zip
```

### Using Project File

```bash
r3e-webgui-deploy \
  --project MyToken.csproj \
  --contract-address 0x1234567890abcdef1234567890abcdef12345678 \
  --deployer-address NPvKVTGZapmFWABLsyvfreuqn73jCjJtN5 \
  --generate-plugin
```

## Configuration

Create a `.r3e-webgui.json` file for default settings:

```json
{
  "serviceUrl": "https://webgui.r3e.network",
  "network": "mainnet",
  "deployerAddress": "NPvKVTGZapmFWABLsyvfreuqn73jCjJtN5"
}
```

## License

MIT License - see the [LICENSE](https://github.com/r3e-network/r3e-devpack-dotnet/blob/master/LICENSE) file for details.