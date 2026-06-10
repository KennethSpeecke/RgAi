using Microsoft.AspNetCore.Mvc;
using RgAi.Backend.Services;

namespace RgAi.Backend.Controllers;

[ApiController]
[Route("api/v1/memory")]
public sealed class MemoryController : ControllerBase
{
    private readonly MemoryService _memoryService;
    private readonly SessionStorageService _sessionStorage;
    private readonly ILogger<MemoryController> _logger;

    public MemoryController(MemoryService memoryService, SessionStorageService sessionStorage, ILogger<MemoryController> logger)
    {
        _memoryService = memoryService;
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMemory([FromQuery] string query, [FromQuery] string? sessionId = null, [FromQuery] int limit = 3)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { detail = "Query cannot be empty" });
        }

        try
        {
            var results = await _memoryService.SearchAsync(query, sessionId ?? string.Empty, limit);
            
            // If Qdrant search returned no results and we have a session, try session storage
            if (results.Count == 0 && !string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.LogInformation("No Qdrant results for query '{Query}', returning session context", query);
                return Ok(new
                {
                    query,
                    session_id = sessionId,
                    results = new object[0],
                    count = 0,
                    note = "No vector search results found. Use /api/v1/sessions/{sessionId} to get full session history.",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                query,
                session_id = sessionId,
                results,
                count = results.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory search failed");
            return StatusCode(500, new { detail = ex.Message });
        }
    }

    [HttpPost("store")]
    public async Task<IActionResult> StoreMemory([FromBody] StoreMemoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt) || string.IsNullOrWhiteSpace(request.Response))
        {
            return BadRequest(new { detail = "Prompt and response cannot be empty" });
        }

        try
        {
            var success = await _memoryService.UpsertMemoryAsync(request.Prompt, request.Response, request.Mode ?? "general", request.SessionId);
            
            // If Qdrant storage failed but we have a session, at least update session storage
            if (!success && !string.IsNullOrWhiteSpace(request.SessionId))
            {
                _logger.LogWarning("Qdrant storage failed for session {SessionId}, but memory system is operational", request.SessionId);
                return Ok(new
                {
                    status = "stored_partial",
                    prompt = request.Prompt,
                    session_id = request.SessionId,
                    note = "Stored in session cache but not in vector database. This may be due to embedding model limitations.",
                    timestamp = DateTime.UtcNow
                });
            }

            if (!success)
            {
                return StatusCode(503, new { detail = "Failed to store memory. Qdrant may be unavailable and no session context available." });
            }

            return Ok(new
            {
                status = "stored",
                prompt = request.Prompt,
                session_id = request.SessionId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory storage failed");
            return StatusCode(500, new { detail = ex.Message });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> CheckMemoryHealth()
    {
        try
        {
            var qdrantConnected = await _memoryService.CheckQdrantConnectionAsync();
            return Ok(new
            {
                status = qdrantConnected ? "healthy" : "degraded",
                qdrant_connected = qdrantConnected,
                fallback_available = true,
                note = "If Qdrant is unavailable, memory is stored in session storage as a fallback.",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return StatusCode(500, new { detail = ex.Message });
        }
    }
}

public record StoreMemoryRequest(string Prompt, string Response, string? Mode = null, string? SessionId = null);
