# RgAi - Windows Docker Desktop Setup (D: Drive)

## ✅ Yes - Data Will Save to D: Drive

All container data will automatically persist to your D: drive in organized folders. **No data goes to C: drive.**

## 📁 D: Drive Storage Structure

```
D:\Docker\volumes\
├── llm_data/                 ← Ollama models (~7GB qwen3-coder)
├── qdrant_data/              ← Vector database
├── backend_logs/             ← Application logs
├── backend_cache/            ← Cache data
└── shared_uploads/           ← User uploads
```

All managed automatically by Docker containers.

## 🚀 Step-by-Step Setup

### Step 1: Create D: Drive Directories

Run this in PowerShell (Admin):

```powershell
# Create directory structure on D: drive
mkdir D:\Docker\volumes\llm_data -Force
mkdir D:\Docker\volumes\qdrant_data -Force
mkdir D:\Docker\volumes\backend_logs -Force
mkdir D:\Docker\volumes\backend_cache -Force
mkdir D:\Docker\volumes\shared_uploads -Force

# Verify they exist
dir D:\Docker\volumes\
```

### Step 2: Restart Docker Desktop

**This is critical** for Docker to recognize the D: drive paths:

1. Right-click Docker Desktop in system tray
2. Click **Quit Docker Desktop**
3. Wait 10 seconds
4. Relaunch Docker Desktop
5. Wait for "Docker is running" message

### Step 3: Start Containers

From PowerShell in your project directory:

```powershell
# Build all images
docker compose build

# Start all services
docker compose up -d

# Verify they're running
docker compose ps
```

### Step 4: Pull LLM Model

```powershell
# This downloads to D:\Docker\volumes\llm_data
docker compose exec llm ollama pull qwen3-coder

# Verify it's on D: drive
dir D:\Docker\volumes\llm_data
```

### Step 5: Verify Everything

```powershell
# Check health
curl http://localhost:8000/health

# Test backend
curl http://localhost:8000/status
```

## 🌐 Access Your Services

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:8000 |
| API Docs | http://localhost:8000/docs |
| Ollama LLM | http://localhost:11434 |
| Qdrant DB | http://localhost:6333 |

## 💾 Verify Data Saving to D: Drive

Check that data is actually being saved:

```powershell
# Check what's on D: drive
dir D:\Docker\volumes\ -Recurse

# Monitor live
Get-ChildItem D:\Docker\volumes\ -Recurse | Select-Object FullName, Length
```

## 📋 Common PowerShell Commands

### View Status
```powershell
docker compose ps
docker compose ps --format "table {{.Service}}\t{{.Status}}\t{{.Ports}}"
```

### View Logs
```powershell
docker compose logs -f
docker compose logs -f backend
docker compose logs -f llm
```

### Restart Services
```powershell
docker compose restart
docker compose restart backend
```

### Stop Services (keeps data on D:)
```powershell
docker compose stop
```

### Remove Containers (data stays on D:)
```powershell
docker compose down
```

### Start Again (data intact from D:)
```powershell
docker compose up -d
```

## 🔄 Backup/Restore Data

### Backup All Data (to Desktop)
```powershell
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "rgai-backup-$timestamp.zip"

# Compress D: drive volumes
Compress-Archive -Path D:\Docker\volumes\ -DestinationPath $backupFile -Force

Write-Host "Backup saved: $backupFile"
```

### Restore from Backup
```powershell
# Stop containers first
docker compose stop

# Extract backup
Expand-Archive -Path rgai-backup-20240101_120000.zip -DestinationPath D:\Docker\volumes\ -Force

# Start containers
docker compose up -d
```

## 🧠 Using the LLM

### Run via Command Line
```powershell
docker compose exec llm ollama run qwen3-coder "Write a Python hello world"
```

### Test via Backend API
```powershell
$prompt = "Write a Python function that adds two numbers"

$payload = @{
    prompt = $prompt
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:8000/test-inference" `
    -Method Post `
    -Body $payload `
    -ContentType "application/json" | Select-Object -ExpandProperty Content
```

### List Available Models
```powershell
docker compose exec llm ollama list
```

## ⚙️ Configuration

### Edit Environment Variables

1. Copy the template:
```powershell
Copy-Item .env.example .env
```

2. Edit `.env` with your editor:
```powershell
# Using Notepad
notepad .env

# Or VS Code
code .env
```

3. Set these values:
```
QDRANT_API_KEY=your-secure-key-here
BACKEND_HOST=0.0.0.0
BACKEND_PORT=8000
LLM_MODEL=qwen3-coder
LOG_LEVEL=INFO
```

## 🔍 Troubleshooting

### Services won't start?

Check Docker is running and D: drive paths exist:

```powershell
# Check Docker status
docker ps

# Check D: drive exists
Test-Path D:\Docker\volumes\

# View error logs
docker compose logs -f
```

### Out of D: drive space?

```powershell
# Check space usage
Get-ChildItem D:\Docker\volumes\ -Recurse | Measure-Object -Property Length -Sum
```

### Can't connect to services?

```powershell
# Check containers are healthy
docker compose ps

# Test connection
Test-NetConnection -ComputerName localhost -Port 8000
Test-NetConnection -ComputerName localhost -Port 3000
```

### Reset Everything

```powershell
# Stop and remove containers (keeps D: drive data)
docker compose down

# Remove containers only
docker compose down -v

# Delete all D: drive data
Remove-Item D:\Docker\volumes\* -Recurse -Force

# Recreate folders
mkdir D:\Docker\volumes\llm_data -Force
mkdir D:\Docker\volumes\qdrant_data -Force
mkdir D:\Docker\volumes\backend_logs -Force
mkdir D:\Docker\volumes\backend_cache -Force
mkdir D:\Docker\volumes\shared_uploads -Force

# Start fresh
docker compose build
docker compose up -d
```

## 📊 Disk Space Estimates

| Component | Size | Location |
|-----------|------|----------|
| LLM Model (qwen3-coder) | ~7GB | `D:\Docker\volumes\llm_data` |
| Vector Database | Variable | `D:\Docker\volumes\qdrant_data` |
| Logs & Cache | 100-500MB | `D:\Docker\volumes\backend_*` |
| Uploads | Variable | `D:\Docker\volumes\shared_uploads` |
| **Total** | **~8-15GB** | **D: drive** |

## ✨ What's Happening

1. **Docker Desktop** runs containers in isolation
2. **D: drive paths** are mounted into containers as volumes
3. **All data written** by containers saves to D: drive
4. **No data on C:** drive (except Docker Desktop installation)
5. **Data persists** when containers restart

## 🎯 Next Steps

1. **Create D: drive directories** (Step 1)
2. **Restart Docker Desktop** (Step 2)
3. **Start containers** (Step 3)
4. **Pull LLM model** (Step 4)
5. **Verify it works** (Step 5)

That's it! Everything then runs automatically with data on D: drive.

## ❓ FAQ

**Q: Will data persist if I restart my computer?**
A: Yes, all data on D: drive persists across restarts.

**Q: Can I access the files from D: drive?**
A: Yes, browse to `D:\Docker\volumes\` anytime.

**Q: What if I want to move data later?**
A: Stop containers, copy `D:\Docker\volumes\` to new location, update docker-compose.yml paths.

**Q: Can I use a different drive?**
A: Yes, replace `D:\` with your drive letter. Both NTFS and ReFS work.

**Q: How much space do I need on D:?**
A: At least 15GB free (7GB models + 5GB database + 3GB logs).

## 📞 Support

- Check `docker compose logs -f` for errors
- Verify D: drive paths exist and are accessible
- Ensure Docker Desktop is running
- Check available disk space: `dir D:\`

---

**Ready to go!** Follow Steps 1-5 above to get started.
