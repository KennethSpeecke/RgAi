@echo off
REM RgAi Project Setup & Start (Windows)

echo.
echo 🚀 RgAi Project Setup ^& Start
echo ==============================
echo.

REM Check Docker
docker --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not installed. Please install Docker Desktop.
    exit /b 1
)

echo ✅ Docker is installed

REM Check if Data folder exists
if not exist "Data" (
    echo 📁 Creating Data folder structure...
    mkdir Data\llm\models
    mkdir Data\llm\cache
    mkdir Data\qdrant\storage
    mkdir Data\qdrant\snapshots
    mkdir Data\backend\logs
    mkdir Data\backend\cache
    mkdir Data\shared\uploads
    mkdir Data\shared\metadata
    echo ✅ Data folder created
) else (
    echo ✅ Data folder exists
)

REM Check if .env file exists
if not exist ".env" (
    if exist ".env.example" (
        copy .env.example .env
        echo ✅ Created .env from .env.example (update with your settings)
    )
)

REM Build images
echo.
echo 🔨 Building Docker images...
docker compose build

echo.
echo 🐳 Starting services...
docker compose up -d

echo.
echo ⏳ Waiting for services to be healthy...
timeout /t 10 /nobreak

REM Check status
echo.
echo 📊 Service Status:
docker compose ps

echo.
echo ✨ Setup complete!
echo.
echo 📋 Next Steps:
echo   1. Pull LLM model:  docker compose exec llm ollama pull qwen3-coder
echo   2. Verify backend:  curl http://localhost:8000/health
echo   3. Check UI:        http://localhost:3000
echo   4. View logs:       docker compose logs -f
echo.
echo 📚 Documentation:
echo   - Setup Guide:  SETUP.md
echo   - Data Folder:  Data/README.md
echo.
pause
