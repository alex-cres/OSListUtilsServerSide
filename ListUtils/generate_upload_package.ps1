#!/usr/bin/env pwsh
# Publishes the ListUtils ODC external library and creates the upload ZIP.

$ErrorActionPreference = 'Stop'
$publishDir = Join-Path $PSScriptRoot 'bin\publish'
$zipPath = Join-Path $PSScriptRoot 'ExternalLibrary.zip'

Write-Host '--- Publishing ListUtils (linux-x64) ---'
dotnet publish (Join-Path $PSScriptRoot 'ListUtils.csproj') `
    -c Release -r linux-x64 --self-contained false `
    -o $publishDir

if (Test-Path $zipPath) { Remove-Item $zipPath }

Write-Host '--- Creating ZIP ---'
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath

$sizeMB = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "ZIP size: $sizeMB MB"
if ($sizeMB -gt 90) {
    Write-Error "ZIP exceeds 90 MB ODC limit ($sizeMB MB). Reduce dependencies."
    exit 1
}
Write-Host "--- Done: $zipPath ---"
