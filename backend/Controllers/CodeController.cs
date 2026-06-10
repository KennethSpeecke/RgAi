using Microsoft.AspNetCore.Mvc;
using RgAi.Backend.Services;

namespace RgAi.Backend.Controllers;

[ApiController]
[Route("api/v1/code")]
public sealed class CodeController : ControllerBase
{
    private readonly CodeIndexingService _codeService;
    private readonly ILogger<CodeController> _logger;

    public CodeController(CodeIndexingService codeService, ILogger<CodeController> logger)
    {
        _codeService = codeService;
        _logger = logger;
    }

    [HttpPost("index")]
    public async Task<IActionResult> IndexRepository([FromBody] IndexRepositoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryPath))
        {
            return BadRequest(new { detail = "RepositoryPath is required" });
        }

        if (!Directory.Exists(request.RepositoryPath))
        {
            return BadRequest(new { detail = $"Repository path does not exist: {request.RepositoryPath}" });
        }

        try
        {
            _logger.LogInformation("Starting code indexing for {Path}", request.RepositoryPath);
            var (filesIndexed, chunksCreated, message) = await _codeService.IndexRepositoryAsync(
                request.RepositoryPath,
                request.SessionId);

            return Ok(new
            {
                filesIndexed,
                chunksCreated,
                message,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code indexing failed");
            return StatusCode(500, new { detail = ex.Message });
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchCode([FromBody] SearchCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new { detail = "Query is required" });
        }

        try
        {
            var results = await _codeService.SearchCodeAsync(request.Query, request.Limit ?? 5);
            return Ok(new
            {
                query = request.Query,
                results = results,
                count = results.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code search failed");
            return StatusCode(500, new { detail = ex.Message });
        }
    }
}

public record IndexRepositoryRequest(string RepositoryPath, string? SessionId = null);
public record SearchCodeRequest(string Query, int? Limit = null);
