#!/usr/bin/env python3
"""
Ollama Inference Verification Script
Tests the Ollama LLM service to ensure it's working correctly.
"""

import requests
import json
import sys

OLLAMA_BASE_URL = "http://localhost:11434"
MODEL = "qwen3-coder"

def check_ollama_service():
    """Check if Ollama service is running"""
    try:
        response = requests.get(f"{OLLAMA_BASE_URL}/api/tags", timeout=5)
        return response.status_code == 200
    except requests.exceptions.ConnectionError:
        return False

def list_models():
    """List available models"""
    try:
        response = requests.get(f"{OLLAMA_BASE_URL}/api/tags")
        data = response.json()
        models = data.get("models", [])
        return [m["name"] for m in models]
    except Exception as e:
        print(f"Error listing models: {e}")
        return []

def run_inference(prompt):
    """Run inference with the specified model"""
    url = f"{OLLAMA_BASE_URL}/api/generate"
    payload = {
        "model": MODEL,
        "prompt": prompt,
        "stream": False,
    }
    
    try:
        response = requests.post(url, json=payload, timeout=300)
        if response.status_code == 200:
            result = response.json()
            return result.get("response", "")
        else:
            return f"Error: {response.status_code}"
    except requests.exceptions.Timeout:
        return "Error: Request timed out (model may still be loading)"
    except Exception as e:
        return f"Error: {str(e)}"

def main():
    print("=== Ollama Inference Verification ===\n")
    
    # Check service
    print("1. Checking Ollama service...")
    if not check_ollama_service():
        print("✗ Ollama service is not running at http://localhost:11434")
        print("  Start it with: docker compose up -d llm")
        sys.exit(1)
    print("✓ Ollama service is running\n")
    
    # List models
    print("2. Available models:")
    models = list_models()
    if not models:
        print("✗ No models found")
        sys.exit(1)
    for model in models:
        print(f"  - {model}")
    
    if MODEL not in models:
        print(f"\n✗ {MODEL} not found. Pull it with:")
        print(f"  docker exec rgai-llm ollama pull {MODEL}")
        sys.exit(1)
    print(f"\n✓ {MODEL} is available\n")
    
    # Run inference
    print(f"3. Testing inference with {MODEL}...")
    test_prompt = "Write a Python function that returns the sum of two numbers. Only show the code."
    print(f"   Prompt: {test_prompt}\n")
    
    print("   Response:")
    response = run_inference(test_prompt)
    print(f"   {response}\n")
    
    print("✓ Inference verification complete!")
    print(f"\nTo use the API programmatically:")
    print(f"  POST {OLLAMA_BASE_URL}/api/generate")
    print(f"  Payload: {{'model': '{MODEL}', 'prompt': 'your prompt here', 'stream': false}}")

if __name__ == "__main__":
    main()
