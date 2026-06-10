#!/bin/bash

# RgAi Docker Helper Commands
# Usage: ./docker-helper.sh [command]

set -e

COMMANDS=(
    "help"
    "status"
    "logs"
    "pull-model"
    "test-backend"
    "backup"
    "restore"
    "clean"
)

help() {
    echo "RgAi Docker Helper"
    echo ""
    echo "Usage: ./docker-helper.sh [command]"
    echo ""
    echo "Commands:"
    echo "  help              - Show this help message"
    echo "  status            - Show service status and ports"
    echo "  logs              - Follow all service logs"
    echo "  pull-model        - Pull qwen3-coder LLM model"
    echo "  test-backend      - Test backend connectivity"
    echo "  backup            - Backup all data to archive"
    echo "  restore [file]    - Restore data from backup"
    echo "  clean             - Clean up Docker volumes and images"
    echo ""
}

status() {
    echo "📊 Service Status:"
    docker compose ps --format "table {{.Service}}\t{{.Status}}\t{{.Ports}}"
    echo ""
    echo "📋 Health Check:"
    echo "  Backend: $(curl -s http://localhost:8000/health | jq -r '.status' 2>/dev/null || echo 'unreachable')"
    echo "  Ollama:  $(curl -s http://localhost:11434/api/tags > /dev/null && echo 'healthy' || echo 'unreachable')"
    echo "  Qdrant:  $(curl -s http://localhost:6333/health > /dev/null && echo 'healthy' || echo 'unreachable')"
}

logs() {
    echo "Following all service logs (Ctrl+C to stop)..."
    docker compose logs -f
}

pull_model() {
    echo "🧠 Pulling qwen3-coder model..."
    docker compose exec llm ollama pull qwen3-coder
    echo "✅ Model pulled successfully"
}

test_backend() {
    echo "🧪 Testing backend..."
    echo ""
    echo "1. Health check:"
    curl -s http://localhost:8000/health | jq . || echo "Failed"
    echo ""
    echo "2. Status:"
    curl -s http://localhost:8000/status | jq . || echo "Failed"
    echo ""
    echo "3. Test inference:"
    curl -s -X POST http://localhost:8000/test-inference \
        -H "Content-Type: application/json" \
        -d '{"prompt": "Write a hello world function in Python"}' | jq . || echo "Failed"
}

backup() {
    FILENAME="rgai-backup-$(date +%Y%m%d_%H%M%S).tar.gz"
    echo "💾 Backing up data to $FILENAME..."
    tar -czf "$FILENAME" Data/
    echo "✅ Backup complete: $FILENAME"
    ls -lh "$FILENAME"
}

restore() {
    if [ -z "$1" ]; then
        echo "❌ Please specify backup file"
        echo "Usage: ./docker-helper.sh restore [filename]"
        echo ""
        echo "Available backups:"
        ls -lh rgai-backup-*.tar.gz 2>/dev/null || echo "No backups found"
        exit 1
    fi
    
    if [ ! -f "$1" ]; then
        echo "❌ File not found: $1"
        exit 1
    fi
    
    echo "🔄 Restoring from $1..."
    tar -xzf "$1"
    echo "✅ Restore complete"
    echo "⚠️  Restart services: docker compose up -d"
}

clean() {
    echo "⚠️  WARNING: This will remove Docker volumes and data!"
    read -p "Are you sure? (yes/no): " -r
    echo
    if [[ $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
        echo "🗑️  Cleaning up..."
        docker compose down -v
        rm -rf Data/*
        mkdir -p Data/{llm/{models,cache},qdrant/{storage,snapshots},backend/{logs,cache},shared/{uploads,metadata}}
        echo "✅ Cleanup complete"
    else
        echo "Cancelled"
    fi
}

# Main
case "${1:-help}" in
    help)
        help
        ;;
    status)
        status
        ;;
    logs)
        logs
        ;;
    pull-model)
        pull_model
        ;;
    test-backend)
        test_backend
        ;;
    backup)
        backup
        ;;
    restore)
        restore "$2"
        ;;
    clean)
        clean
        ;;
    *)
        echo "Unknown command: $1"
        echo ""
        help
        exit 1
        ;;
esac
