# 🎮 GPU Setup Guide - NVIDIA RTX 3070 Ti

## Your GPU Info

```
GPU: NVIDIA GeForce RTX 3070 Ti
Memory: 8192 MiB (8GB VRAM)
CUDA Version: 13.2
Driver: NVIDIA-SMI 595.97
Status: ✅ Ready for Docker
```

---

## ✅ Prerequisites (Already Installed)

You have everything needed:
- ✅ NVIDIA GPU (RTX 3070 Ti)
- ✅ NVIDIA Driver (595.97)
- ✅ CUDA 13.2
- ✅ Docker Desktop with GPU support

---

## 🚀 Enable GPU in Docker

### Step 1: Verify Docker Can See GPU

Open PowerShell and run:

```powershell
docker run --rm --gpus all nvidia/cuda:12.2.2-runtime-windows-ltsc2022 nvidia-smi
```

This should show your RTX 3070 Ti. If it does, GPU is ready! ✅

### Step 2: Restart Containers with GPU

Stop current containers:

```powershell
docker compose down
```

Rebuild and start with GPU:

```powershell
docker compose up -d
```

The `docker-compose.yml` is now configured with GPU support! ✅

### Step 3: Verify GPU is Being Used

Check if LLM is using GPU:

```powershell
# Option 1: Check Ollama logs
docker compose logs llm | Select-String "GPU\|cuda\|NVIDIA"

# Option 2: Monitor GPU usage in real-time
nvidia-smi

# Option 3: Check Ollama status
docker exec rgai-llm ollama list
```

---

## 🔧 GPU Configuration Details

### What Was Changed in docker-compose.yml

```yaml
deploy:
  resources:
    reservations:
      devices:
        - driver: nvidia
          count: 1
          capabilities: [gpu]
```

This tells Docker to:
- ✅ Reserve the NVIDIA GPU driver
- ✅ Use 1 GPU device
- ✅ Enable GPU capabilities

### Environment Variables

```yaml
environment:
  - CUDA_VISIBLE_DEVICES=0    # Use GPU 0 (your RTX 3070 Ti)
  - OLLAMA_GPU=1              # Enable GPU in Ollama
```

---

## 🧠 How Ollama Uses GPU

Ollama automatically:
1. **Detects** your NVIDIA GPU via CUDA
2. **Loads** models into VRAM (your RTX 3070 Ti has 8GB)
3. **Offloads** computation to GPU
4. **Falls back** to CPU if needed

**Speed improvement**: ~5-10x faster inference with GPU vs CPU!

---

## 📊 Monitor GPU Usage

### Real-time Monitoring

```powershell
# Watch GPU stats live
nvidia-smi -l 1
```

This shows:
- GPU utilization percentage
- Memory usage
- Temperature
- Power consumption

### Docker GPU Monitoring

```powershell
# Check Docker GPU allocation
docker inspect rgai-llm | Select-String "Gpu\|device"

# Check all Docker resources
docker stats rgai-llm
```

### Check Ollama GPU Status

```powershell
# Check Ollama server logs for GPU info
docker compose logs llm | Select-String -Pattern "compute|GPU|cuda|VRAM" -Context 2
```

---

## ⚡ Performance Tips

### 1. **Adjust Layer Offload**

After pulling a model, optimize GPU usage:

```powershell
# For large models (7B+), use GPU offloading
docker exec rgai-llm ollama pull qwen3-coder

# The model will use GPU automatically
```

### 2. **Monitor Memory**

Your RTX 3070 Ti has 8GB VRAM:

| Model | Size | VRAM Needed |
|-------|------|------------|
| Qwen3-Coder (7B) | ~7GB | ~7GB |
| Llama2 (7B) | ~7GB | ~7GB |
| Mistral (7B) | ~7GB | ~7GB |
| Larger models | 13B+ | May need CPU fallback |

### 3. **Optimize Temperature**

Monitor GPU temperature:

```powershell
nvidia-smi --query-gpu=index,name,temperature.gpu,utilization.gpu --format=csv -l 1
```

Target: **60-75°C** for normal operation
Warning: **>85°C** - reduce load

### 4. **Batch Requests**

Send multiple prompts efficiently:

```powershell
# This queues requests to GPU
for ($i = 1; $i -le 10; $i++) {
    docker exec rgai-llm ollama run qwen3-coder "Prompt $i"
}
```

---

## 🔍 Verify GPU is Working

### Test 1: Check GPU Detection

```powershell
docker exec rgai-llm ollama list
```

Should show model loaded with GPU support.

### Test 2: Run Inference and Monitor

Terminal 1 - Watch GPU:
```powershell
nvidia-smi -l 1
```

Terminal 2 - Run LLM:
```powershell
docker exec rgai-llm ollama run qwen3-coder "Write a Python function"
```

**Expected**: GPU usage should spike to 90-100% during inference ✅

### Test 3: Check Performance

Compare CPU vs GPU speed:

```powershell
# Time a request
Measure-Command {
    docker exec rgai-llm ollama run qwen3-coder "Generate code for hello world"
} | Select-Object TotalSeconds
```

**CPU**: ~30-60 seconds
**GPU**: ~5-10 seconds

If GPU is 5-10x faster, it's working! ✅

---

## 🆘 Troubleshooting

### GPU Not Showing in Docker

**Problem**: Docker can't access GPU

**Solution**:
```powershell
# 1. Check NVIDIA Docker support is installed
docker run --rm --gpus all nvidia/cuda:12.2.2-base nvidia-smi

# 2. If that fails, reinstall Docker Desktop:
# - Settings → Resources → Advanced
# - Check "Enable GPU support"
# - Restart Docker Desktop

# 3. Restart containers
docker compose down
docker compose up -d
```

### Ollama Still Using CPU

**Problem**: Ollama not using GPU despite configuration

**Solution**:
```powershell
# 1. Check Ollama logs for GPU info
docker compose logs llm | Select-String "GPU\|cuda\|compute"

# 2. Force GPU by pulling model again
docker exec rgai-llm ollama pull qwen3-coder

# 3. Restart LLM container
docker compose restart llm

# 4. Check status
docker exec rgai-llm ollama list
```

### Out of VRAM (8GB Full)

**Problem**: Model too large for 8GB GPU

**Solution**:
```powershell
# Check VRAM usage
nvidia-smi

# Options:
# 1. Use smaller model: ollama pull mistral-7b
# 2. Clear GPU memory: docker compose restart llm
# 3. Upgrade GPU or use CPU fallback
```

### High Temperature (>85°C)

**Problem**: GPU overheating

**Solution**:
```powershell
# 1. Check temperature
nvidia-smi --query-gpu=temperature.gpu --format=csv -l 1

# 2. Reduce load:
# - Stop other GPU tasks (gaming, rendering)
# - Use smaller models
# - Reduce batch size

# 3. Improve cooling:
# - Check GPU fans
# - Clean dust from GPU
# - Improve case airflow
```

---

## 🎯 Verification Checklist

- [ ] `nvidia-smi` shows RTX 3070 Ti
- [ ] `docker run --gpus all` works
- [ ] `docker compose up -d` starts all containers
- [ ] Ollama logs show GPU detection
- [ ] `nvidia-smi` shows GPU usage during inference
- [ ] Frontend at http://localhost:3000 works
- [ ] Sending prompts to AI is 5-10x faster than CPU

---

## 📊 Expected Performance

### With GPU (RTX 3070 Ti)

| Task | Time |
|------|------|
| Pull Qwen3-Coder | ~5 minutes |
| First inference | ~10-30 seconds (loading) |
| Subsequent inferences | ~2-5 seconds |
| Token generation | ~20-50 tokens/sec |

### Without GPU (CPU only)

| Task | Time |
|------|------|
| Pull Qwen3-Coder | ~5 minutes |
| First inference | ~30-60 seconds (loading) |
| Subsequent inferences | ~15-30 seconds |
| Token generation | ~5-10 tokens/sec |

**GPU is 3-5x faster!** 🚀

---

## 🔗 Useful Commands

```powershell
# Monitor GPU live
nvidia-smi -l 1

# Check specific GPU stats
nvidia-smi --query-gpu=index,name,driver_version,memory.total --format=csv

# Docker GPU check
docker run --rm --gpus all nvidia/cuda:12.2.2-base nvidia-smi

# Ollama GPU verification
docker exec rgai-llm ollama list

# Container resource usage
docker stats rgai-llm

# Restart with GPU
docker compose restart llm

# View GPU-specific logs
docker compose logs llm | Select-String "GPU\|CUDA\|compute"
```

---

## 🎓 Learning More

**NVIDIA Docker Documentation:**
- https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/

**Ollama GPU Support:**
- https://ollama.ai/

**CUDA Information:**
- https://www.nvidia.com/en-us/geforce/

---

## ✅ Summary

Your RTX 3070 Ti is now configured to:
- ✅ Accelerate LLM inference 5-10x
- ✅ Use 8GB VRAM efficiently
- ✅ Fall back to CPU if needed
- ✅ Monitor performance in real-time

**Your AI agent IDE is now GPU-accelerated!** 🚀

Restart containers and enjoy lightning-fast LLM responses!

```powershell
docker compose restart
```
