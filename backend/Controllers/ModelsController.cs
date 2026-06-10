using Microsoft.AspNetCore.Mvc;
using RgAi.Backend.Models;
using RgAi.Backend.Services;

namespace RgAi.Backend.Controllers;

[ApiController]
[Route("api/v1/models")]
public sealed class ModelsController : ControllerBase
{
    private readonly LlmService _llmService;
    private readonly ModelSelectionService _selectionService;

    public ModelsController(LlmService llmService, ModelSelectionService selectionService)
    {
        _llmService = llmService;
        _selectionService = selectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableModels()
    {
        var models = await _llmService.GetAvailableModelsAsync();
        return Ok(new { models = models.Select(name => new { name }), total = models.Count });
    }

    [HttpPost("pull")]
    public async Task<IActionResult> PullModel(ModelOperationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { detail = "Model name is required." });
        }

        var (success, message) = await _llmService.PullModelAsync(request.Name);
        if (!success)
        {
            return StatusCode(502, new { detail = message });
        }

        return Ok(new { status = "success", model = request.Name, message = message });
    }

    [HttpGet("active")]
    public IActionResult GetActiveModel()
    {
        return Ok(new { active_model = _selectionService.GenerateModel });
    }

    [HttpPost("load")]
    public async Task<IActionResult> LoadModel(ModelOperationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { detail = "Model name is required." });
        }

        var models = await _llmService.GetAvailableModelsAsync();
        if (!models.Contains(request.Name, StringComparer.OrdinalIgnoreCase))
        {
            return NotFound(new { detail = $"Model '{request.Name}' is not available." });
        }

        _selectionService.SetGenerateModel(request.Name);
        _selectionService.SetEmbeddingModel("qwen-embedding"); //Todo - make this configurable
        
        return Ok(new { status = "success", active_model = _selectionService.GenerateModel });
    }

    [HttpDelete("{modelName}")]
    public async Task<IActionResult> DeleteModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return BadRequest(new { detail = "Model name is required." });
        }

        var (success, message) = await _llmService.DeleteModelAsync(modelName);
        if (!success)
        {
            return StatusCode(502, new { detail = message });
        }

        return Ok(new { status = "success", model = modelName, message = message });
    }
}
