param(
    [string]$ProjectPath = "CoffeeBreakTimer.App\CoffeeBreakTimer.App.csproj",
    [string]$Framework = "net9.0-windows10.0.19041.0",
    [string]$RuntimeIdentifier = "win10-x64",
    [string]$CertificateSubject = "CN=CoffeeBreakerTimer"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$fullProjectPath = Join-Path $repoRoot $ProjectPath
$certFolder = Join-Path $repoRoot "build\certificates"
$certPath = Join-Path $certFolder "CoffeeBreakerTimer_TestCertificate.cer"

New-Item -ItemType Directory -Force -Path $certFolder | Out-Null

Get-Process CoffeeBreakTimer.App -ErrorAction SilentlyContinue | Stop-Process -Force

$cert = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq $CertificateSubject -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date).AddDays(30) } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

if (-not $cert) {
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $CertificateSubject `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -KeyExportPolicy Exportable `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter (Get-Date).AddYears(3)
}

Export-Certificate -Cert $cert -FilePath $certPath -Force | Out-Null

dotnet publish $fullProjectPath `
    -f $Framework `
    -c Release `
    -p:RuntimeIdentifierOverride=$RuntimeIdentifier `
    -p:WindowsPackageType=MSIX `
    -p:GenerateAppxPackageOnBuild=true `
    -p:WindowsAppSDKSelfContained=true `
    -p:SelfContained=true `
    -p:AppxPackageSigningEnabled=true `
    -p:PackageCertificateThumbprint=$($cert.Thumbprint) `
    -p:AppxBundle=Never `
    -p:PublishProfile=Windows-MSIX-SelfContained

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$packageRoot = Join-Path (Split-Path -Parent $fullProjectPath) "bin\Release\$Framework\$RuntimeIdentifier\AppPackages"

Write-Host ""
Write-Host "MSIX package generated under:"
Write-Host $packageRoot
Write-Host ""
Write-Host "Certificate exported for test/sideload installs:"
Write-Host $certPath
Write-Host ""
Write-Host "Certificate thumbprint:"
Write-Host $cert.Thumbprint
