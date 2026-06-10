# RgAi - RAG with AI

Full-stack containerized application with LLM inference, vector embeddings, and semantic search capabilities.

## Technology Stack

### Backend
- **.NET 10.0** - ASP.NET Core runtime
- **C#** - Primary language
- **Multi-stage Docker build** - Optimized image size

### Frontend
- **.NET 10.0** - ASP.NET Core runtime
- **C#** - Primary language
- **Multi-stage Docker build** - Optimized image size

### Infrastructure & Services
- **Docker Compose** - Service orchestration
- **Ollama** - LLM inference engine (latest)
- **Qdrant** - Vector database (latest)
- **NVIDIA CUDA** - GPU acceleration (optional)

## Architecture

```
Frontend (.NET)               Backend (.NET)              External Services
Port 3000 (HTTP)             Port 8000 (HTTP)
    │                            │
    └────────────────────────────┤
                                 │
                    ┌────────────┼────────────┐
                    │            │            │
                    ▼            ▼            ▼
              Ollama LLM    Qdrant VectorDB  External APIs
              Port 11434    Ports 6333/6334
              (GPU Ready)   (Vector Storage)
```

## System Requirements

- **Docker Desktop** (latest) with Compose v2
- **Disk Space**: 15GB+ (for LLM models and persistent data)
- **RAM**: 8GB minimum (4GB available for containers)
- **CPU**: 2+ cores recommended
- **GPU** (Optional): NVIDIA GPU with CUDA support for accelerated inference

## Quick Start

### Prerequisites
1. Copy environment template:
```bash
cp .env.example .env.local
```

2. Update `.env.local` with your configuration (API keys, ports, etc.)

### Build and Run

```bash
# Build all services
docker compose build

# Start all services
docker compose up -d

# Verify services are running
docker compose ps
```

### First-Time Setup

```bash
# Pull LLM model (example: qwen3-coder)
docker compose exec llm ollama pull qwen3-coder

# Verify backend health
curl http://localhost:8000/health

# Check frontend
open http://localhost:3000
```

## Services

| Service | Container | Port(s) | Purpose |
|---------|-----------|---------|---------|
| **Backend** | rgai-backend | 8000 | ASP.NET Core API |
| **Frontend** | rgai-frontend | 3000 | ASP.NET Core Web UI |
| **LLM** | rgai-llm | 11434 | Ollama inference engine |
| **Vector DB** | rgai-qdrant | 6333, 6334 | Qdrant vector database |

## Data Persistence

All persistent data is stored in the `Data/` directory (not tracked in Git):

```
Data/
├── llm/
│   ├── models/          LLM model files (~7GB per model)
│   └── cache/           Model cache and temporary files
├── qdrant/
│   ├── storage/         Vector database collections
│   └── snapshots/       Database backup snapshots
├── backend/
│   ├── logs/            Application logs
│   └── cache/           Application cache
└── shared/
    ├── uploads/         User-uploaded files
    └── metadata/        Configuration and metadata files
```

## Environment Variables

Create `.env.local` from `.env.example`. Key variables:

```env
# Database
QDRANT_API_KEY=your-secure-api-key

# Backend
BACKEND_HOST=0.0.0.0
BACKEND_PORT=8000
DEBUG=false

# Frontend
REACT_APP_API_URL=http://localhost:8000
REACT_APP_API_WS_URL=ws://localhost:8000

# LLM
LLM_HOST=llm
LLM_PORT=11434
LLM_MODEL=qwen3-coder
LLM_TIMEOUT=300

# Logging
LOG_LEVEL=INFO
LOG_DIR=./Data/backend/logs

# Storage
UPLOADS_MAX_SIZE=52428800
DATA_RETENTION_DAYS=30
```

## Common Operations

### View Service Status
```bash
docker compose ps
docker compose ps --format "table {{.Service}}\t{{.Status}}\t{{.Ports}}"
```

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

### Restart Services
```bash
# All services
docker compose restart

# Specific service
docker compose restart backend
```

### Stop Services (Preserves Data)
```bash
docker compose stop
```

### Remove Services (Deletes Containers, Preserves Data)
```bash
docker compose down
```

### Full Cleanup (Deletes Everything including Volumes)
```bash
docker compose down -v
```

## LLM Model Management

### Pull Models
```bash
# Pull specific model
docker compose exec llm ollama pull qwen3-coder

# Pull another model
docker compose exec llm ollama pull mistral
```

### List Available Models
```bash
docker compose exec llm ollama list
```

### Test Model Inference
```bash
docker compose exec llm ollama run qwen3-coder "Your prompt here"
```

## API Endpoints

### Health & Status
- `GET /` - Service info
- `GET /health` - Health check
- `GET /status` - Detailed status

### Backend Operations
- API documentation available at: `http://localhost:8000/docs` (when Swagger is enabled)

## Backup and Restore

### Backup Data
```bash
tar -czf rgai-backup-$(date +%Y%m%d_%H%M%S).tar.gz Data/
```

### Restore Data
```bash
# Stop services first
docker compose down

# Extract backup
tar -xzf rgai-backup-YYYYMMDD_HHMMSS.tar.gz

# Start services
docker compose up -d
```

## Troubleshooting

### Services Won't Start
```bash
# Check compose logs
docker compose logs

# Check disk space
docker system df

# Check Docker daemon
docker ps
```

### Port Already in Use
```bash
# Find process using port
lsof -i :3000
lsof -i :8000
lsof -i :11434

# Change port in docker-compose.yml or .env.local
```

### Out of Disk Space
```bash
docker system df
docker system prune -a
docker volume prune
```

### Cannot Connect to Services
```bash
# Check network
docker network ls
docker network inspect rgai_network

# Check container networking
docker inspect rgai-backend
docker inspect rgai-frontend
```

### GPU Not Detected
```bash
# Verify NVIDIA Docker runtime
docker run --rm --gpus all nvidia/cuda:11.0-runtime nvidia-smi

# Check compose logs for GPU errors
docker compose logs llm
```

## Project Structure

```
.
├── backend/                    C# ASP.NET Core backend
│   ├── Dockerfile             Multi-stage build
│   └── *.csproj               Project configuration
├── frontend/                   C# ASP.NET Core frontend
│   ├── Dockerfile             Multi-stage build
│   └── *.csproj               Project configuration
├── Data/                       Persistent data (not in Git)
│   ├── llm/                   LLM models and cache
│   ├── qdrant/                Vector database storage
│   ├── backend/               Backend logs and cache
│   └── shared/                Shared uploads and metadata
├── docker-compose.yml         Service orchestration
├── .env.example               Environment template
├── .env.local                 Local configuration (not in Git)
├── .dockerignore              Docker build exclusions
├── .gitignore                 Git exclusions
└── README.md                  This file
```

## Development Workflow

### Local Development
1. Set environment in `.env.local`
2. Start services: `docker compose up -d`
3. Modify code in `backend/` or `frontend/` directories
4. Rebuild specific service: `docker compose build backend`
5. Restart service: `docker compose restart backend`

### Adding Dependencies
**Backend (.NET)**: Edit `backend.csproj` and rebuild
```bash
docker compose build backend
docker compose up -d backend
```

**Frontend (.NET)**: Edit `frontend.csproj` and rebuild
```bash
docker compose build frontend
docker compose up -d frontend
```

## Performance Tuning

### CPU/Memory Limits
Edit `docker-compose.yml` to add resource limits:
```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

### GPU Configuration
Ollama service includes NVIDIA GPU support. Ensure:
- NVIDIA Docker runtime installed
- NVIDIA drivers installed on host
- `docker-compose.yml` GPU section configured

## Security Considerations

- Keep `QDRANT_API_KEY` secure and unique
- Do not commit `.env.local` to version control
- Use strong credentials for production
- Restrict network access to services as needed
- Consider using a reverse proxy (nginx) in production
- Enable HTTPS/TLS for production deployments

## Production Deployment

For production, consider:
- Using a managed Qdrant instance
- Implementing proper logging and monitoring
- Setting up automated backups
- Using environment-specific configurations
- Deploying to Kubernetes or container orchestration platform
- Adding rate limiting and authentication to APIs
- Using a CDN for frontend assets
- Implementing health checks and auto-recovery

## Contributing

1. Clone the repository
2. Create a feature branch
3. Make changes in `backend/` or `frontend/` directories
4. Test locally: `docker compose up`
5. Commit and push changes
6. Open pull request

## License

[License: MIT](https://opensource.org)

This project is licensed under the MIT License.

## Support

For issues or questions:
- Check service logs: `docker compose logs -f`
- Review error messages in `Data/backend/logs/`
- Verify all services are healthy: `docker compose ps`
- Check disk space and resource availability
