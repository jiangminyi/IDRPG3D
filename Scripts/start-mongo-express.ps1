param(
    [string]$DatabaseName = "idrpg3d_dev",
    [string]$MongoHost = "127.0.0.1",
    [int]$MongoPort = 27017,
    [int]$WebPort = 8081,
    [string]$BasicAuthUsername = "admin",
    [string]$BasicAuthPassword = "pass",
    [switch]$Writable
)

$ErrorActionPreference = "Stop"

$npx = Get-Command npx.cmd -ErrorAction SilentlyContinue
$npmCache = Join-Path (Resolve-Path "$PSScriptRoot\..") ".cache\npm"

$env:ME_CONFIG_MONGODB_URL = "mongodb://${MongoHost}:${MongoPort}/${DatabaseName}"
$env:ME_CONFIG_MONGODB_ENABLE_ADMIN = "false"
$env:ME_CONFIG_BASICAUTH_ENABLED = "true"
$env:ME_CONFIG_BASICAUTH_USERNAME = $BasicAuthUsername
$env:ME_CONFIG_BASICAUTH_PASSWORD = $BasicAuthPassword

if ($Writable) {
    Remove-Item Env:\ME_CONFIG_OPTIONS_READONLY -ErrorAction SilentlyContinue
}
else {
    $env:ME_CONFIG_OPTIONS_READONLY = "true"
}

Write-Host "Starting mongo-express 1.0.0 for IDRPG3D..."
Write-Host "MongoDB: mongodb://${MongoHost}:${MongoPort}/${DatabaseName}"
Write-Host "Web UI:  http://127.0.0.1:${WebPort}"
if ($BasicAuthUsername) {
    Write-Host "Login:   $BasicAuthUsername / $BasicAuthPassword"
}
Write-Host "Mode:    $(if ($Writable) { 'writable' } else { 'read-only' })"
Write-Host ""

if (-not $npx) {
    throw "npx.cmd was not found. Install Node.js/npm, then run this script again."
}

New-Item -ItemType Directory -Force -Path $npmCache | Out-Null
& $npx.Source -y --cache $npmCache --registry https://registry.npmjs.org/ mongo-express@1.0.0 --port $WebPort
