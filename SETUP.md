# RgAi Project - Docker Setup Guide

⚠️ **Windows Users with D: Drive:** See **[WINDOWS_D_DRIVE_SETUP.md](./WINDOWS_D_DRIVE_SETUP.md)** for step-by-step instructions to save all data to D: drive instead.

## Project Structure

```
RgAi/
├── Data/                      # All persistent data (DO NOT commit to git)
│   ├── llm/                  # LLM models and cache
│   │   ├── models/           # Downloaded models (7GB+)
│   │   └── cache/            # Model cache files
│   ├── qdrant/               # Vector database
│   │   ├── storage/          # Vector collections
│   │   └── snapshots/        # Database backups
│   ├── backend/              # Backend data
│   │   ├── logs/             # Application logs
│   │   └── cache/            # Application cache
│   ├── shared/               # Shared data
│   │   ├── uploads/          # User uploads
│   │   └── metadata/         # Configuration files
│   └── README.md             # Data folder documentation
│
├── backend/                  # FastAPI Backend
│   ├── main.py               # Main application
│   ├── requirements.txt       # Python dependencies
│   └── Dockerfile            # Backend container
│
├── frontend/                 # React Frontend
│   ├── public/               # Static assets
│   ├── src/                  # React components
│   ├── package.json          # Node dependencies
│   └── Dockerfile            # Frontend container
│
├── docker-compose.yml        # Service orchestration
├── .dockerignore             # Docker build exclusions
├── .env.example              # Environment template
├── SETUP.md                  # This file
└── verify-llm.py             # Verification script
```

## Data Persistence

All Docker volumes are **bind-mounted** to the `Data/` folder:
- ✅ Data persists when containers stop/restart
- ✅ Data is accessible from your host machine
- ✅ Easy backups: just zip the `Data/` folder
- ✅ No external storage needed

See `Data/README.md` for detailed folder descriptions.

## Prerequisites

1. **Docker Desktop** installed and running
2. **Docker Compose** (included with Docker Desktop)
3. **At least 15GB free disk space** (for LLM models + data)
4. **4GB+ RAM** available

## Quick Start

### 1. Prepare Environment

```bash
# Copy example environment file
cp .env.example .env

# Edit .env and set your API keys (optional)
# nano .env
```

### 2. Start All Services

```bash
# Build and start all containers
docker compose up -d

# Monitor startup
docker compose logs -f
```

This will:
- Start Ollama LLM service (port 11434)
- Start Qdrant vector database (port 6333)
- Start FastAPI backend (port 8000)
- Start React frontend (port 3000)
- Create and mount all data volumes to `Data/` folder

### 3. Verify Services

```bash
# Check all containers are running
docker compose ps

# Check service health
docker compose ps --format "table {{.Service}}\t{{.Status}}"
```

### 4. Pull LLM Model

```bash
# Pull qwen3-coder model (first time ~7GB download)
docker compose exec llm ollama pull qwen3-coder

# List available models
docker compose exec llm ollama list
```

### 5. Test Everything

```bash
# Run verification script
python verify-llm.py

# Or test via backend
curl http://localhost:8000/health
curl http://localhost:8000/status
```

## Service Endpoints

| Service | URL | Purpose |
|---------|-----|---------|
| Frontend | http://localhost:3000 | React UI |
| Backend API | http://localhost:8000 | REST API |
| Backend Docs | http://localhost:8000/docs | Swagger UI |
| Ollama LLM | http://localhost:11434 | LLM API |
| Qdrant Vector DB | http://localhost:6333 | Vector database |

## Common Commands

### View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f backend
docker compose logs -f llm
docker compose logs -f qdrant
docker compose logs -f frontend
```

### Stop Services

```bash
# Stop (keeps data)
docker compose stop

# Stop and remove containers (keeps data)
docker compose down

# Stop and remove everything including volumes
docker compose down -v
```

### Restart Services

```bash
# Restart all
docker compose restart

# Restart specific service
docker compose restart backend
```

### Rebuild Services

```bash
# Rebuild all images
docker compose build

# Rebuild specific service
docker compose build backend
docker compose build frontend
```

### Access Logs Directly

```bash
# Backend logs folder
ls -la Data/backend/logs/

# View log files
tail -f Data/backend/logs/*.log
```

## Data Management

### Backup Data

```bash
# Create backup
tar -czf rgai-backup-$(date +%Y%m%d_%H%M%S).tar.gz Data/

# List backups
ls -lh rgai-backup-*.tar.gz
```

### Restore Data

```bash
# Extract backup
tar -xzf rgai-backup-YYYYMMDD_HHMMSS.tar.gz

# Restart services
docker compose up -d
```

### Clean Up Data

```bash
# Remove logs only
rm -rf Data/backend/logs/*
mkdir -p Data/backend/logs

# Remove cache only
rm -rf Data/backend/cache/*
mkdir -p Data/backend/cache

# Remove everything (WARNING: deletes all data!)
rm -rf Data/*
docker compose down -v
```

## Troubleshooting

### Container won't start

1. Check logs: `docker compose logs -f <service>`
2. Verify disk space: `docker system df`
3. Restart Docker Desktop
4. Check port availability: `lsof -i :3000` (macOS/Linux)

### LLM model not found

```bash
# Re-pull the model
docker compose exec llm ollama pull qwen3-coder

# List available models
docker compose exec llm ollama list
```

### Cannot connect to services

```bash
# Check if services are healthy
docker compose ps --format "table {{.Service}}\t{{.Status}}"

# Inspect network
docker network inspect rgai_network
```

### Out of disk space

```bash
# Clean up Docker system
docker system prune -a

# Remove unused volumes
docker volume prune

# Check space usage
docker system df
```

## Environment Variables

Edit `.env` file to configure:

```bash
# Qdrant API Key
QDRANT_API_KEY=your-secure-key

# Backend
BACKEND_HOST=0.0.0.0
BACKEND_PORT=8000

# LLM Model
LLM_MODEL=qwen3-coder

# Logging
LOG_LEVEL=INFO
```

## Production Deployment

For production:

1. Use external managed services (not local containers)
2. Set `restart: always` in docker-compose.yml
3. Configure proper logging and monitoring
4. Use strong API keys in environment
5. Set up automated backups for Data folder
6. Use a reverse proxy (nginx) for frontend
7. Enable SSL/TLS certificates

## Next Steps

1. ✅ Review `Data/README.md` for folder organization
2. 🚀 Start services and verify they work
3. 📝 Check backend API docs at http://localhost:8000/docs
4. 🎨 Build your frontend components
5. 🧠 Integrate with LLM via backend API

## Support

For issues or questions:
- Check `docker compose logs -f` for detailed errors
- Review service-specific documentation
- See `Data/README.md` for data organization details
