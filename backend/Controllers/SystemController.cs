using Microsoft.AspNetCore.Mvc;
using RgAi.Backend.Models;
using RgAi.Backend.Services;

namespace RgAi.Backend.Controllers;

[ApiController]
public sealed class SystemController : ControllerBase
{
    private readonly LlmService _llmService;
    private readonly MemoryService _memoryService;
    private readonly BackendSettings _settings;
    private readonly ModelSelectionService _selectionService;

    public SystemController(LlmService llmService, MemoryService memoryService, BackendSettings settings, ModelSelectionService selectionService)
    {
        _llmService = llmService;
        _memoryService = memoryService;
        _settings = settings;
        _selectionService = selectionService;
    }

    [HttpGet("/")]
    public IActionResult GetRoot()
    {
        return Ok(new
        {
            message = "RgAi Backend API",
            status = "operational",
            version = "1.0.0",
            gpu_enabled = true,
            llm = "Ollama with GPU acceleration",
            endpoints = new
            {
                health = "/health",
                status = "/status",
                inference = "/api/v1/inference",
                docs = "/swagger"
            }
        });
    }

    [HttpGet("/health")]
    public async Task<IActionResult> GetHealthAsync()
    {
        var llmConnected = await _llmService.CheckConnectionAsync();
        return Ok(new { status = llmConnected ? "healthy" : "degraded", service = "rgai-backend", llm_connected = llmConnected });
    }

    [HttpGet("/status")]
    public async Task<IActionResult> GetStatusAsync()
    {
        var llmConnected = await _llmService.CheckConnectionAsync();
        var qdrantConnected = await _memoryService.CheckQdrantConnectionAsync();
        var models = llmConnected ? await _llmService.GetAvailableModelsAsync() : Array.Empty<string>();

        return Ok(new
        {
            service = "rgai-backend",
            status = "operational",
            version = "1.0.0",
            config = new
            {
                llm = $"{_settings.LlmHost}:{_settings.LlmPort}",
                qdrant = $"{_settings.QdrantHost}:{_settings.QdrantPort}",
                data_dir = _settings.DataDirectory,
                gpu_enabled = true
            },
            llm_status = new
            {
                connected = llmConnected,
                available_models = models,
                default_model = _settings.DefaultLlmModel,
                active_model = _selectionService.GenerateModel
            },
            qdrant_status = new
            {
                connected = qdrantConnected,
                collection = _settings.QdrantCollection
            }
        });
    }

    [HttpPost("/api/v1/system/warmup")]
    public async Task<IActionResult> WarmUpAsync()
    {
        var warmed = await _llmService.WarmUpAsync();
        return warmed
            ? Ok(new { warmed = true })
            : StatusCode(503, new { detail = "LLM warmup failed. Please wait a moment and try again." });
    }

    [HttpGet("/api/v1/logs")]
    public IActionResult GetLogs()
    {
        var logPath = Path.Combine(_settings.LogsDirectory, "api.log");
        var lines = System.IO.File.Exists(logPath)
            ? System.IO.File.ReadAllLines(logPath).ToList()
            : new List<string>();
        return Ok(new { logs = lines, total = lines.Count });
    }

    [HttpGet("/api/v1/ollama/info")]
    public async Task<IActionResult> GetOllamaInfoAsync()
    {
        var info = await _llmService.GetModelInfoAsync(_settings.DefaultLlmModel);
        if (info is null)
        {
            return StatusCode(503, new { detail = "Ollama info endpoint is unavailable." });
        }

        return Ok(info);
    }
}
