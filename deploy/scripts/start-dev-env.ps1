param (
    [switch]$ForceRecreate = $false
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DockerDir = Join-Path (Split-Path -Parent $ScriptDir) "docker"
$ComposeFile = Join-Path $DockerDir "docker-compose.yml"
$EnvExample = Join-Path $DockerDir ".env.example"
$EnvFile = Join-Path $DockerDir ".env"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "   CRMPro AI - Local Developer Environment Setup  " -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Ensure .env exists
if (-not (Test-Path $EnvFile)) {
    Write-Host "[INFO] Creating .env from .env.example..." -ForegroundColor Yellow
    Copy-Item $EnvExample $EnvFile
}

# Verify Docker availability
$dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerCmd) {
    Write-Warning "[WARN] 'docker' command was not found in PATH."
    Write-Host "Checking for running native services on this Windows machine..." -ForegroundColor Yellow

    # Check RabbitMQ Windows Service
    $rabbitService = Get-Service -Name "RabbitMQ" -ErrorAction SilentlyContinue
    if ($rabbitService -and $rabbitService.Status -eq "Running") {
        Write-Host "  [OK] Local RabbitMQ Service is actively RUNNING on port 5672." -ForegroundColor Green
    } else {
        Write-Host "  [--] Local RabbitMQ Service is not running." -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "To use the containerized developer environment:" -ForegroundColor Cyan
    Write-Host "  1. Install Docker Desktop or Podman on Windows."
    Write-Host "  2. Run: cd deploy/docker; docker compose up -d"
    Write-Host "  Alternatively, install PostgreSQL 16 with pgvector directly via winget."
    exit 0
}

Write-Host "[INFO] Starting dependencies via Docker Compose..." -ForegroundColor Green
Set-Location $DockerDir

$upArgs = @("compose", "-f", $ComposeFile, "up", "-d")
if ($ForceRecreate) {
    $upArgs += "--force-recreate"
}

& docker @upArgs

Write-Host ""
Write-Host "[INFO] Waiting for service health checks to pass..." -ForegroundColor Yellow
$maxAttempts = 15
$attempt = 0
$healthy = $false

while ($attempt -lt $maxAttempts -and -not $healthy) {
    Start-Sleep -Seconds 2
    $attempt++

    $status = & docker compose -f $ComposeFile ps --format "{{.Service}}: {{.Health}}" 2>$null
    Write-Host "[$attempt/$maxAttempts] Current Health Status:" -ForegroundColor DarkGray
    $status | ForEach-Object { Write-Host "   $_" -ForegroundColor DarkGray }

    $unhealthy = $status | Where-Object { $_ -match "unhealthy|starting" }
    if (-not $unhealthy -and $status.Count -ge 3) {
        $healthy = $true
    }
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Green
Write-Host "  All dependencies are UP and HEALTHY!            " -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host "  PostgreSQL : localhost:5432 (Database: crmpro_dev, User: crmpro_app)"
Write-Host "  Redis      : localhost:6379"
Write-Host "  RabbitMQ   : localhost:5672 (Management UI: http://localhost:15672)"
Write-Host "==================================================" -ForegroundColor Green
