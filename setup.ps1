#!/usr/bin/env powershell
#
# RgAi Windows Setup Script
# Creates D: drive directories and starts Docker containers
#
# Run: .\setup.ps1

param(
    [switch]$SkipBuild = $false,
    [switch]$SkipModel = $false
)

Write-Host ""
Write-Host "🚀 RgAi Windows Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as admin (for folder creation)
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "⚠️  This script should be run as Administrator for folder creation" -ForegroundColor Yellow
    Write-Host "   Right-click PowerShell → Run as Administrator" -ForegroundColor Yellow
    Write-Host ""
}

# Check Docker
Write-Host "1️⃣  Checking Docker..." -ForegroundColor Cyan
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker is installed: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Please install Docker Desktop" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Create D: drive directories
Write-Host "2️⃣  Creating D: drive directory structure..." -ForegroundColor Cyan

$directories = @(
    "D:\Docker\volumes\llm_data",
    "D:\Docker\volumes\qdrant_data",
    "D:\Docker\volumes\backend_logs",
    "D:\Docker\volumes\backend_cache",
    "D:\Docker\volumes\shared_uploads"
)

foreach ($dir in $directories) {
    if (Test-Path $dir) {
        Write-Host "   ✅ $dir (exists)" -ForegroundColor Green
    } else {
        try {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "   ✅ $dir (created)" -ForegroundColor Green
        } catch {
            Write-Host "   ❌ Failed to create $dir" -ForegroundColor Red
            Write-Host "   $_" -ForegroundColor Red
            if ($isAdmin) {
                exit 1
            }
        }
    }
}

Write-Host ""

# Check .env file
Write-Host "3️⃣  Checking environment configuration..." -ForegroundColor Cyan

if (Test-Path ".env") {
    Write-Host "   ✅ .env file exists" -ForegroundColor Green
} else {
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Host "   ✅ Created .env from .env.example" -ForegroundColor Green
        Write-Host "   📝 Edit .env to configure settings" -ForegroundColor Yellow
    } else {
        Write-Host "   ⚠️  .env.example not found" -ForegroundColor Yellow
    }
}

Write-Host ""

# Build images
if (-not $SkipBuild) {
    Write-Host "4️⃣  Building Docker images..." -ForegroundColor Cyan
    docker compose build --quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Images built successfully" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Build failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "4️⃣  Skipping build (use -SkipBuild `$false to rebuild)" -ForegroundColor Yellow
}

Write-Host ""

# Start services
Write-Host "5️⃣  Starting services..." -ForegroundColor Cyan
docker compose up -d
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Services started" -ForegroundColor Green
} else {
    Write-Host "   ❌ Failed to start services" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Wait and show status
Write-Host "⏳ Waiting for services to be healthy..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "📊 Service Status:" -ForegroundColor Cyan
docker compose ps --format "table {{.Service}}\t{{.Status}}\t{{.Ports}}"

Write-Host ""

# Pull model (optional)
if (-not $SkipModel) {
    $pullModel = Read-Host "Pull qwen3-coder LLM model now? (Y/n)"
    if ($pullModel -ne 'n' -and $pullModel -ne 'N') {
        Write-Host ""
        Write-Host "🧠 Pulling qwen3-coder model (this may take several minutes)..." -ForegroundColor Cyan
        docker compose exec llm ollama pull qwen3-coder
        Write-Host "✅ Model pulled successfully" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "✨ Setup Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Access your services:" -ForegroundColor Cyan
Write-Host "   Frontend:    http://localhost:3000" -ForegroundColor White
Write-Host "   Backend API: http://localhost:8000" -ForegroundColor White
Write-Host "   API Docs:    http://localhost:8000/docs" -ForegroundColor White
Write-Host "   Ollama LLM:  http://localhost:11434" -ForegroundColor White
Write-Host "   Qdrant DB:   http://localhost:6333" -ForegroundColor White
Write-Host ""

Write-Host "💾 Data Location:" -ForegroundColor Cyan
Write-Host "   All data saves to: D:\Docker\volumes\" -ForegroundColor White
Write-Host ""

Write-Host "📚 Documentation:" -ForegroundColor Cyan
Write-Host "   Setup Guide:        SETUP.md" -ForegroundColor White
Write-Host "   Windows D: Guide:   WINDOWS_D_DRIVE_SETUP.md" -ForegroundColor White
Write-Host "   Quick Reference:    QUICK_REFERENCE.md" -ForegroundColor White
Write-Host ""

Write-Host "🛠️  Common Commands:" -ForegroundColor Cyan
Write-Host "   docker compose ps                     # View status" -ForegroundColor White
Write-Host "   docker compose logs -f                # View logs" -ForegroundColor White
Write-Host "   docker compose stop                   # Stop services" -ForegroundColor White
Write-Host "   docker compose restart                # Restart services" -ForegroundColor White
Write-Host ""

Write-Host "✅ You're all set! Data is being saved to D:\Docker\volumes\" -ForegroundColor Green
