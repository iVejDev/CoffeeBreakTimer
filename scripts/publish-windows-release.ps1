param(
    [string]$ProjectPath = "CoffeeBreakTimer.App\CoffeeBreakTimer.App.csproj",
    [string]$Framework = "net9.0-windows10.0.19041.0",
    [string]$RuntimeIdentifier = "win10-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$fullProjectPath = Join-Path $repoRoot $ProjectPath

Get-Process CoffeeBreakTimer.App -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet publish $fullProjectPath `
    -f $Framework `
    -c Release `
    -p:RuntimeIdentifierOverride=$RuntimeIdentifier `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:PublishProfile=Windows-Unpackaged-SelfContained

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$publishDir = Join-Path (Split-Path -Parent $fullProjectPath) "bin\Release\$Framework\$RuntimeIdentifier\publish"
Write-Host ""
Write-Host "Windows release generated:"
Write-Host $publishDir
Write-Host ""
Write-Host "Start file:"
Write-Host (Join-Path $publishDir "CoffeeBreakTimer.App.exe")
