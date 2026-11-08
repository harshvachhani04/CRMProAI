<#
.SYNOPSIS
    Stops local developer environment dependencies for CRMPro AI.
#>

[CmdletBinding()]
param (
    [switch]$RemoveVolumes = $false
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DockerDir = Join-Path (Split-Path -Parent $ScriptDir) "docker"
$ComposeFile = Join-Path $DockerDir "docker-compose.yml"

$dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerCmd) {
    Write-Warning "[WARN] 'docker' command was not found in PATH."
    exit 0
}

Write-Host "[INFO] Stopping dependencies via Docker Compose..." -ForegroundColor Yellow
$downArgs = @("compose", "-f", $ComposeFile, "down")
if ($RemoveVolumes) {
    $downArgs += "-v"
    Write-Warning "Removing named volumes (crmpro_postgres_data, crmpro_redis_data, crmpro_rabbitmq_data)..."
}

& docker @downArgs
Write-Host "[OK] Dependencies stopped." -ForegroundColor Green
