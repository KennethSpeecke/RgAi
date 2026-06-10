Write-Host "=== RgAi LLM Setup ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Starting Ollama service..." -ForegroundColor Yellow
docker compose up -d llm
Write-Host ""
Write-Host "Waiting for Ollama to be ready (max 60 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
Write-Host ""
Write-Host "Pulling Qwen3-Coder model (this may take 10-15 minutes)..." -ForegroundColor Yellow
docker exec rgai-llm ollama pull qwen3-coder
Write-Host ""
Write-Host "Testing inference..." -ForegroundColor Yellow
docker exec rgai-llm ollama run qwen3-coder "Write a Python hello world function"
Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Cyan
