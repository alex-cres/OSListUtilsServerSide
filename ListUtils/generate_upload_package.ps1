# Publishes the library for linux-x64 (the ODC container runtime) and packages
# it into ExternalLibrary.zip ready for upload to ODC Portal.
#
# ODC Portal limit: the ZIP must not exceed 90 MB.
#
# Usage (from the repo root or the ListUtils/ folder):
#   .\ListUtils\generate_upload_package.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectDir = $PSScriptRoot
$repoRoot   = Split-Path $projectDir -Parent
$publishDir = Join-Path $projectDir "bin\Release\net10.0\linux-x64\publish"
$zipPath    = Join-Path $repoRoot "ExternalLibrary.zip"
$limitMB    = 90

Write-Host "Publishing for linux-x64..." -ForegroundColor Cyan
Push-Location $projectDir
try {
    dotnet publish -c Release -r linux-x64 --no-self-contained
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }
} finally {
    Pop-Location
}

Write-Host "Packaging $publishDir -> $zipPath ..." -ForegroundColor Cyan
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath)

$sizeMB = [Math]::Round((Get-Item $zipPath).Length / 1MB, 1)
Write-Host "Package size: $sizeMB MB" -ForegroundColor Cyan
if ($sizeMB -gt $limitMB) {
    throw "Package exceeds ODC Portal limit of $limitMB MB ($sizeMB MB). Remove unnecessary native binaries before uploading."
}
Write-Host "Done: $zipPath ($sizeMB MB)" -ForegroundColor Green
