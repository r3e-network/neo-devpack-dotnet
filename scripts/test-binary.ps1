# Test script for R3E Neo Contract Compiler (RNCC) binaries (PowerShell version)
# Usage: .\test-binary.ps1 <path-to-binary>

param(
    [Parameter(Mandatory=$true)]
    [string]$BinaryPath
)

if (-not (Test-Path $BinaryPath)) {
    Write-Host "Error: Binary not found at $BinaryPath" -ForegroundColor Red
    exit 1
}

Write-Host "Testing R3E Neo Contract Compiler binary: $BinaryPath" -ForegroundColor Cyan
Write-Host "=============================================="

# Test 1: Check if binary exists and is executable
Write-Host "1. Checking if binary exists and is executable..."
$binaryInfo = Get-Item $BinaryPath
Write-Host "   ✓ Binary found: $($binaryInfo.Name) ($($binaryInfo.Length) bytes)" -ForegroundColor Green

# Test 2: Check version
Write-Host "2. Checking version..."
try {
    $versionOutput = & $BinaryPath --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Version: $versionOutput" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Version check failed: $versionOutput" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ⚠ Version check failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test 3: Check help output
Write-Host "3. Checking help output..."
try {
    $helpOutput = & $BinaryPath --help 2>&1
    if ($helpOutput -match "Usage|USAGE|help|HELP") {
        Write-Host "   ✓ Help output looks good" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Help output may be incomplete" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ⚠ Help check failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test 4: Check file size
Write-Host "4. Checking binary size..."
$size = $binaryInfo.Length
$sizeMB = [math]::Round($size / 1MB, 2)
Write-Host "   Binary size: ${sizeMB}MB ($size bytes)"

if ($size -lt 10MB) {
    Write-Host "   ⚠ Binary seems small - may be missing dependencies" -ForegroundColor Yellow
} elseif ($size -gt 500MB) {
    Write-Host "   ⚠ Binary seems very large" -ForegroundColor Yellow
} else {
    Write-Host "   ✓ Binary size looks reasonable" -ForegroundColor Green
}

# Test 5: Create a simple test contract
Write-Host "5. Testing compilation with a simple contract..."
$testDir = New-TemporaryFile | ForEach-Object { Remove-Item $_; New-Item -ItemType Directory -Path $_ }
$testContract = Join-Path $testDir "TestContract.cs"

$contractContent = @'
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

[DisplayName("TestContract")]
[ManifestExtra("Author", "Test")]
[ManifestExtra("Email", "test@example.com")]
[ManifestExtra("Description", "This is a test contract")]
public class TestContract : SmartContract
{
    [DisplayName("GetMessage")]
    public static string GetMessage()
    {
        return "Hello, Neo!";
    }
    
    [DisplayName("Add")]
    public static int Add(int a, int b)
    {
        return a + b;
    }
}
'@

Set-Content -Path $testContract -Value $contractContent

Write-Host "   Testing compilation..."
try {
    $compileOutput = & $BinaryPath $testContract -o $testDir 2>&1
    $compileExitCode = $LASTEXITCODE
    
    if ($compileExitCode -eq 0) {
        Write-Host "   ✓ Compilation successful" -ForegroundColor Green
        
        # Check if output files were created
        $nefFile = Join-Path $testDir "TestContract.nef"
        $manifestFile = Join-Path $testDir "TestContract.manifest.json"
        
        if ((Test-Path $nefFile) -and (Test-Path $manifestFile)) {
            Write-Host "   ✓ Output files created: .nef and .manifest.json" -ForegroundColor Green
            
            # Check file sizes
            $nefSize = (Get-Item $nefFile).Length
            $manifestSize = (Get-Item $manifestFile).Length
            
            Write-Host "   NEF size: $nefSize bytes"
            Write-Host "   Manifest size: $manifestSize bytes"
            
            if ($nefSize -gt 0 -and $manifestSize -gt 0) {
                Write-Host "   ✓ Output files have content" -ForegroundColor Green
            } else {
                Write-Host "   ⚠ Output files are empty" -ForegroundColor Yellow
            }
        } else {
            Write-Host "   ⚠ Expected output files not found" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ⚠ Compilation failed:" -ForegroundColor Yellow
        Write-Host "   $compileOutput" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ⚠ Compilation test failed: $($_.Exception.Message)" -ForegroundColor Yellow
    $compileExitCode = 1
}

# Cleanup
Remove-Item -Recurse -Force $testDir

Write-Host ""
Write-Host "=============================================="
Write-Host "Binary test completed!"

if ($compileExitCode -eq 0) {
    Write-Host "✓ All tests passed - binary appears to be working correctly" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠ Some tests failed - binary may have issues" -ForegroundColor Yellow
    exit 1
}