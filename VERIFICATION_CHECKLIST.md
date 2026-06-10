# ✅ Setup Verification Checklist

## Pre-Setup Checklist

Before you start, verify:

- [ ] Docker Desktop is installed and running
- [ ] You have at least 15GB free on D: drive
- [ ] You have at least 4GB available RAM
- [ ] You have admin access to create folders on D:
- [ ] Project files are on your machine

---

## Setup Execution Checklist

### Step 1: Create D: Drive Directories

- [ ] Open PowerShell as Administrator
- [ ] Copy and paste the 5 mkdir commands
- [ ] All 5 folders created on D: drive:
  - [ ] `D:\Docker\volumes\llm_data\`
  - [ ] `D:\Docker\volumes\qdrant_data\`
  - [ ] `D:\Docker\volumes\backend_logs\`
  - [ ] `D:\Docker\volumes\backend_cache\`
  - [ ] `D:\Docker\volumes\shared_uploads\`

### Step 2: Restart Docker Desktop

- [ ] Right-click Docker icon in system tray
- [ ] Click "Quit Docker Desktop"
- [ ] Wait 10 seconds
- [ ] Relaunch Docker Desktop
- [ ] Wait for "Docker is running" confirmation
- [ ] Docker has recognized D: drive paths

### Step 3: Build and Start Containers

- [ ] Open PowerShell in project folder
- [ ] Run: `docker compose build` (wait for completion)
- [ ] Run: `docker compose up -d`
- [ ] All 4 containers started:
  - [ ] `rgai-llm` (Ollama)
  - [ ] `rgai-qdrant` (Vector DB)
  - [ ] `rgai-backend` (FastAPI)
  - [ ] `rgai-frontend` (React)

### Step 4: Pull LLM Model

- [ ] Run: `docker compose exec llm ollama pull qwen3-coder`
- [ ] Wait for download to complete (~7GB)
- [ ] Model successfully pulled

### Step 5: Verify Everything

- [ ] Run: `curl http://localhost:8000/health`
- [ ] Response: `{"status":"healthy",...}`
- [ ] All services responding correctly

---

## Verification Tests

### Test 1: Services Running

```powershell
docker compose ps
```

Expected output:
```
CONTAINER ID        NAMES                STATUS              PORTS
xxxxxxx             rgai-llm             Up (healthy)        0.0.0.0:11434->11434/tcp
xxxxxxx             rgai-qdrant          Up (healthy)        0.0.0.0:6333->6333/tcp, 0.0.0.0:6334->6334/tcp
xxxxxxx             rgai-backend         Up                  0.0.0.0:8000->8000/tcp
xxxxxxx             rgai-frontend        Up                  0.0.0.0:3000->3000/tcp
```

- [ ] All 4 containers showing "Up"
- [ ] No containers in "Exit" state

### Test 2: D: Drive Data

```powershell
dir D:\Docker\volumes\
```

Expected:
```
Directory: D:\Docker\volumes

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
d-----         1/15/2024  2:30 PM                backend_cache
d-----         1/15/2024  2:30 PM                backend_logs
d-----         1/15/2024  2:30 PM                llm_data
d-----         1/15/2024  2:30 PM                qdrant_data
d-----         1/15/2024  2:30 PM                shared_uploads
```

- [ ] All 5 folders exist on D: drive
- [ ] Folders have recent timestamps
- [ ] llm_data folder contains model files (after Step 4)

### Test 3: Backend API

```powershell
curl http://localhost:8000/health
```

Expected:
```json
{"status":"healthy","service":"rgai-backend"}
```

- [ ] Connection successful
- [ ] Response contains "healthy"

### Test 4: Backend Status

```powershell
curl http://localhost:8000/status
```

Expected:
```json
{
  "service":"rgai-backend",
  "status":"operational",
  "version":"1.0.0",
  "config":{
    "llm":"llm:11434",
    "qdrant":"qdrant:6333",
    "data_dir":"/app/data"
  }
}
```

- [ ] Service is operational
- [ ] LLM and Qdrant connection info present

### Test 5: Frontend Access

```powershell
Start-Process http://localhost:3000
```

Expected:
- [ ] Browser opens to React application
- [ ] Frontend loads without errors

### Test 6: Ollama LLM

```powershell
docker compose exec llm ollama list
```

Expected:
```
NAME              ID              SIZE       MODIFIED
qwen3-coder:latest  abc123def...    7.1 GB    2024-01-15
```

- [ ] qwen3-coder model listed
- [ ] Size shows ~7GB
- [ ] Model is available

### Test 7: File Sizes on D: Drive

```powershell
Get-ChildItem D:\Docker\volumes\ -Recurse | Measure-Object -Property Length -Sum
```

Expected:
- [ ] llm_data: ~7000 MB (7GB)
- [ ] qdrant_data: ~100-500 MB
- [ ] backend_logs: some small log files
- [ ] Total should be 7+ GB

### Test 8: Container Logs

```powershell
docker compose logs -f
```

Expected:
- [ ] No error messages
- [ ] LLM service responding
- [ ] Qdrant service responding
- [ ] Backend healthy checks passing

---

## Troubleshooting Verification

### If Tests Fail

**Test 2 Failed (D: drive folders missing)?**
- [ ] Are you running PowerShell as Administrator?
- [ ] Does D: drive have 15GB+ free space?
- [ ] Did you restart Docker Desktop after creating folders?

**Tests 3-4 Failed (Backend not responding)?**
- [ ] Check containers are running: `docker compose ps`
- [ ] Check logs: `docker compose logs -f backend`
- [ ] Restart: `docker compose restart backend`

**Test 6 Failed (Model not found)?**
- [ ] Pull model again: `docker compose exec llm ollama pull qwen3-coder`
- [ ] Check space: `Get-Volume -DriveLetter D`
- [ ] View download progress: `docker compose logs -f llm`

**Test 7 Failed (No files on D: drive)?**
- [ ] Check D: drive is accessible: `Test-Path D:\Docker\volumes\`
- [ ] Check mount paths: `docker inspect rgai-llm`
- [ ] Restart Docker Desktop (critical!)

---

## Post-Verification

After all tests pass:

- [ ] Docker Desktop is running
- [ ] All 4 containers are healthy
- [ ] D: drive has data folders with files
- [ ] Backend API responding
- [ ] Frontend loads
- [ ] LLM model available
- [ ] Data writing to D: drive

---

## Access Points

Once verified, you can access:

- [ ] Frontend: http://localhost:3000
- [ ] Backend API: http://localhost:8000
- [ ] API Docs: http://localhost:8000/docs
- [ ] Ollama: http://localhost:11434
- [ ] Qdrant: http://localhost:6333
- [ ] D: Drive: `D:\Docker\volumes\`

---

## Regular Monitoring

### Daily

```powershell
# Check services are healthy
docker compose ps

# Check D: drive space
Get-Volume -DriveLetter D

# View recent logs
docker compose logs -f --tail 20
```

### Weekly

```powershell
# Check total D: drive usage
[math]::Round((Get-ChildItem D:\Docker\volumes\ -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB, 2)

# Backup data
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Compress-Archive -Path D:\Docker\volumes\ -DestinationPath "Desktop\rgai-backup-$timestamp.zip"
```

### Monthly

```powershell
# Clean up old logs
Remove-Item D:\Docker\volumes\backend_logs\* -Recurse -Force

# Clean old cache
Remove-Item D:\Docker\volumes\backend_cache\* -Recurse -Force

# Verify integrity
docker system df
docker system prune -a --force
```

---

## Success Criteria

All of the following must be true:

1. ✅ All 4 containers running and healthy
2. ✅ All 5 data folders exist on D: drive
3. ✅ Data files present in D: drive folders
4. ✅ Backend API responding
5. ✅ Frontend accessible
6. ✅ LLM model available
7. ✅ No errors in logs
8. ✅ D: drive has sufficient space

---

## Troubleshooting Quick Reference

| Issue | Solution |
|-------|----------|
| Containers won't start | Restart Docker Desktop |
| Can't reach localhost:8000 | Check `docker compose ps` |
| No files on D: drive | Restart Docker Desktop |
| D: drive not found | Check path: `Test-Path D:\` |
| Out of space | Check `Get-Volume -DriveLetter D` |
| Model not found | Run `docker compose exec llm ollama pull qwen3-coder` |
| Services unhealthy | Check logs: `docker compose logs -f` |

---

## When Ready

Once you've verified all checks, you can:

- ✅ Add more LLM models
- ✅ Deploy the application
- ✅ Scale services
- ✅ Set up backups
- ✅ Configure monitoring
- ✅ Start development

---

## Documentation for Reference

| Document | Use Case |
|----------|----------|
| `WINDOWS_D_DRIVE_COMPLETE_ANSWER.md` | Full setup guide |
| `VISUAL_OVERVIEW.md` | Architecture & flow |
| `QUICK_REFERENCE.md` | Common commands |
| `WINDOWS_D_DRIVE_SETUP.md` | Detailed troubleshooting |

---

## Next Steps

1. ✅ Complete all checklist items
2. ✅ Run all verification tests
3. ✅ Confirm all tests pass
4. ✅ Start using your application
5. ✅ Set up regular backups

**Once verified, your setup is complete and ready for production use!** 🚀
