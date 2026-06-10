# RgAi - RAG with AI

Full-stack containerized application with LLM inference, vector embeddings, and a modern frontend.

## 🏗️ Architecture

```
Frontend (React)              Backend API (FastAPI)         Services
  :3000                         :8000                    
    │                             │                        ┌─────────┐
    └─────────────────────────────┼────────────────────────→│ Ollama  │
                                  │                        │  LLM    │
                                  ├────────────────────────→│ :11434  │
                                  │                        └─────────┘
                                  │                        ┌─────────┐
                                  └────────────────────────→│ Qdrant  │
                                                           │ Vector  │
                                                           │ :6333   │
                                                           └─────────┘
```

## 📦 Services

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| **Frontend** | Node.js 18 Alpine | 3000 | React UI |
| **Backend** | Python 3.11 Slim | 8000 | FastAPI REST API |
| **LLM** | Ollama | 11434 | Code generation, inference |
| **Vector DB** | Qdrant | 6333 | Semantic search, embeddings |

## 🚀 Quick Start

### Option 1: Automated Setup (Recommended)

**macOS/Linux:**
```bash
./setup.sh
```

**Windows:**
```bash
setup.bat
```

### Option 2: Manual Setup

```bash
# 1. Build all images
docker compose build

# 2. Start services
docker compose up -d

# 3. Pull LLM model
docker compose exec llm ollama pull qwen3-coder

# 4. Verify
curl http://localhost:8000/health
```

## 📁 Data Structure

All persistent data stored in `Data/` folder:

```
Data/
├── llm/              # LLM models & cache
├── qdrant/           # Vector database
├── backend/          # Logs & cache
└── shared/           # Uploads & metadata
```

See `Data/README.md` for details.

## 📚 Documentation

- **[SETUP.md](./SETUP.md)** - Complete setup & troubleshooting guide
- **[Data/README.md](./Data/README.md)** - Data folder organization
- **[Backend API Docs](http://localhost:8000/docs)** - Swagger UI (when running)

## 🔗 Service URLs

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:8000 |
| API Docs | http://localhost:8000/docs |
| Ollama | http://localhost:11434 |
| Qdrant | http://localhost:6333 |

## 🛠️ Common Commands

```bash
# View all services
docker compose ps

# View logs
docker compose logs -f

# View specific service logs
docker compose logs -f backend

# Restart services
docker compose restart

# Stop services (keeps data)
docker compose stop

# Remove containers (keeps data)
docker compose down

# Remove everything (deletes data!)
docker compose down -v
```

## 🧠 Using the LLM

### Via CLI

```bash
docker compose exec llm ollama run qwen3-coder "Write a Python hello world"
```

### Via Backend API

```bash
curl -X POST http://localhost:8000/test-inference \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Write a hello world function in Python"}'
```

### Via Backend Python

```python
import requests

response = requests.post(
    "http://localhost:8000/test-inference",
    json={"prompt": "Your prompt here"}
)
print(response.json())
```

## 💾 Backup & Restore

### Backup
```bash
tar -czf rgai-backup-$(date +%Y%m%d_%H%M%S).tar.gz Data/
```

### Restore
```bash
tar -xzf rgai-backup-YYYYMMDD_HHMMSS.tar.gz
docker compose up -d
```

## 📊 System Requirements

- **Docker Desktop** (latest)
- **Disk Space**: 15GB+ (for models & data)
- **RAM**: 4GB+ available
- **CPU**: 2+ cores recommended

## 🔍 Troubleshooting

### Services won't start
```bash
docker compose logs -f
docker system df  # Check disk space
```

### Can't connect to services
```bash
docker compose ps --format "table {{.Service}}\t{{.Status}}"
docker network inspect rgai_network
```

### Out of disk space
```bash
docker system prune -a
docker volume prune
```

See **[SETUP.md](./SETUP.md)** for more troubleshooting.

## 📝 Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
QDRANT_API_KEY=your-key-here
BACKEND_HOST=0.0.0.0
BACKEND_PORT=8000
LLM_MODEL=qwen3-coder
LOG_LEVEL=INFO
```

## 🏭 Project Structure

```
RgAi/
├── backend/              # FastAPI backend
│   ├── main.py           # Entry point
│   ├── requirements.txt   # Dependencies
│   └── Dockerfile        # Container image
├── frontend/             # React frontend
│   ├── src/              # Components
│   ├── public/           # Assets
│   ├── package.json      # Dependencies
│   └── Dockerfile        # Container image
├── Data/                 # All persistent data
│   └── README.md         # Data organization
├── docker-compose.yml    # Service orchestration
├── .env.example          # Environment template
├── setup.sh              # Setup script (macOS/Linux)
├── setup.bat             # Setup script (Windows)
└── SETUP.md              # Detailed setup guide
```

## 🔐 Security Notes

- Change `QDRANT_API_KEY` in `.env` for production
- Don't commit `.env` file to Git
- Use `.env.local` for local-only overrides
- See `SETUP.md` for production deployment tips

## 📖 API Endpoints

### Health & Status

```
GET  /                    # Info
GET  /health              # Health check
GET  /status              # Detailed status
```

### LLM Integration

```
POST /test-inference      # Test LLM inference
```

### Data

```
POST /upload              # Upload file
GET  /files               # List files
GET  /files/{id}          # Get file
```

*(Add more endpoints as you develop)*

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Test locally: `docker compose up`
4. Commit and push

## 📄 License

[Your License Here]

## 🆘 Support

- Check [SETUP.md](./SETUP.md) for troubleshooting
- Review `docker compose logs -f` for errors
- See [Data/README.md](./Data/README.md) for data management

---

**Next Steps:**
1. ✅ Run `./setup.sh` (or `setup.bat` on Windows)
2. 🧠 Pull the LLM model: `docker compose exec llm ollama pull qwen3-coder`
3. 🔍 Verify everything: `curl http://localhost:8000/health`
4. 🚀 Start building!
