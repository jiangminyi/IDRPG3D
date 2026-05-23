param(
    [string]$PackageId = "MongoDB.Server"
)

$ErrorActionPreference = "Stop"

Write-Host "Installing MongoDB Community Server via winget..."

try {
    winget install --id $PackageId --exact --silent --accept-package-agreements --accept-source-agreements
}
catch {
    Write-Warning "winget installation failed: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "Manual fallback:"
    Write-Host "1. Download MongoDB Community Server MSI:"
    Write-Host "   https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.3.2-signed.msi"
    Write-Host "2. Install it as a Windows service."
    Write-Host "3. Verify with:"
    Write-Host "   Test-NetConnection 127.0.0.1 -Port 27017"
    exit 1
}

Write-Host ""
Write-Host "MongoDB install command finished. Verify service and port:"
Write-Host "Get-Service | Where-Object { `$_.Name -like '*Mongo*' -or `$_.DisplayName -like '*Mongo*' }"
Write-Host "Test-NetConnection 127.0.0.1 -Port 27017"
