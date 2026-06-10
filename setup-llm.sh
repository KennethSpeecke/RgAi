#!/bin/bash

echo "=== RgAi LLM Setup ==="
echo ""

# Start Ollama service
echo "Starting Ollama service..."
docker compose up -d llm

# Wait for Ollama to be ready
echo "Waiting for Ollama to be ready..."
max_attempts=30
attempt=0
while [ $attempt -lt $max_attempts ]; do
  if docker exec rgai-llm curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
    echo "✓ Ollama is ready"
    break
  fi
  attempt=$((attempt + 1))
  echo "Attempt $attempt/$max_attempts - waiting..."
  sleep 2
done

if [ $attempt -eq $max_attempts ]; then
  echo "✗ Ollama failed to start"
  exit 1
fi

echo ""
echo "Pulling Qwen3-Coder model (this may take several minutes)..."
docker exec rgai-llm ollama pull qwen3-coder

echo ""
echo "✓ Model pulled successfully"
echo ""
echo "Testing inference..."
docker exec rgai-llm ollama run qwen3-coder "Write a hello world function in Python"

echo ""
echo "=== Setup Complete ==="
echo "Ollama is running at http://localhost:11434"
echo "Available models can be listed with: docker exec rgai-llm ollama list"
echo "To run the LLM: docker exec rgai-llm ollama run qwen3-coder"
