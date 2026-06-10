#!/bin/bash

set -e

echo "🚀 RgAi Project Setup & Start"
echo "=============================="
echo ""

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker Desktop."
    exit 1
fi

echo "✅ Docker is installed"

# Check if Data folder exists
if [ ! -d "Data" ]; then
    echo "📁 Creating Data folder structure..."
    mkdir -p Data/{llm/{models,cache},qdrant/{storage,snapshots},backend/{logs,cache},shared/{uploads,metadata}}
    echo "✅ Data folder created"
else
    echo "✅ Data folder exists"
fi

# Check if .env file exists
if [ ! -f ".env" ]; then
    if [ -f ".env.example" ]; then
        cp .env.example .env
        echo "✅ Created .env from .env.example (update with your settings)"
    fi
fi

# Build images
echo ""
echo "🔨 Building Docker images..."
docker compose build --quiet

echo ""
echo "🐳 Starting services..."
docker compose up -d

echo ""
echo "⏳ Waiting for services to be healthy (this may take a minute)..."
sleep 10

# Check health
echo ""
echo "📊 Service Status:"
docker compose ps --format "table {{.Service}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "✨ Setup complete!"
echo ""
echo "📋 Next Steps:"
echo "  1. Pull LLM model:  docker compose exec llm ollama pull qwen3-coder"
echo "  2. Verify backend:  curl http://localhost:8000/health"
echo "  3. Check UI:        http://localhost:3000"
echo "  4. View logs:       docker compose logs -f"
echo ""
echo "📚 Documentation:"
echo "  - Setup Guide:  SETUP.md"
echo "  - Data Folder:  Data/README.md"
echo ""
