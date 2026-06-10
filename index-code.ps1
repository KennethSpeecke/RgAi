#!/usr/bin/env pwsh
# Index Code Repository into RgAI Knowledge Base
# Usage: ./index-code.ps1 -RepoPath "C:\path\to\repo" -SessionId "optional-session-id"

param(
    [Parameter(Mandatory=$true)]
    [string]$RepoPath,
    
    [Parameter(Mandatory=$false)]
    [string]$SessionId,
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 8000
)

if (-not (Test-Path $RepoPath)) {
    Write-Host "ERROR: Repository path not found: $RepoPath" -ForegroundColor Red
    exit 1
}

$backendUrl = "http://localhost:$Port"

# Test backend connectivity
try {
    $testResp = Invoke-WebRequest -Uri "$backendUrl/api/v1/sessions" -Method GET -ErrorAction Stop
    Write-Host "✓ Backend is running at $backendUrl" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Cannot connect to backend at $backendUrl" -ForegroundColor Red
    Write-Host "Make sure the backend is running: docker compose up -d" -ForegroundColor Yellow
    exit 1
}

# Prepare index request
$requestBody = @{
    repositoryPath = $RepoPath
    sessionId = if ($SessionId) { $SessionId } else { $null }
} | ConvertTo-Json

Write-Host "Indexing repository: $RepoPath" -ForegroundColor Cyan
Write-Host "Backend: $backendUrl" -ForegroundColor Cyan
Write-Host ""

# Send indexing request
try {
    $response = Invoke-WebRequest `
        -Uri "$backendUrl/api/v1/code/index" `
        -Method POST `
        -Body $requestBody `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Indexing Complete!" -ForegroundColor Green
    Write-Host "  Files Indexed: $($result.filesIndexed)" -ForegroundColor Green
    Write-Host "  Chunks Created: $($result.chunksCreated)" -ForegroundColor Green
    Write-Host "  Message: $($result.message)" -ForegroundColor Green
    Write-Host "  Timestamp: $($result.timestamp)" -ForegroundColor Gray
    
} catch {
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() }
        Write-Host "ERROR: Indexing failed" -ForegroundColor Red
        Write-Host $errorResponse -ForegroundColor Red
    } else {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    exit 1
}

Write-Host ""
Write-Host "Next: Use code search in your inference requests!" -ForegroundColor Cyan
Write-Host "Example:" -ForegroundColor Yellow
Write-Host '  {"prompt":"how should I implement authentication?","mode":"code","useCodeSearch":true}' -ForegroundColor Gray
