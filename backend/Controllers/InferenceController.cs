using Microsoft.AspNetCore.Mvc;
using RgAi.Backend.Models;
using RgAi.Backend.Services;

namespace RgAi.Backend.Controllers;

[ApiController]
[Route("api/v1/inference")]
public sealed class InferenceController : ControllerBase
{
    private readonly InferenceService _inferenceService;

    public InferenceController(InferenceService inferenceService)
    {
        _inferenceService = inferenceService;
    }

    [HttpPost]
    public async Task<IActionResult> PostInference(InferenceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { detail = "Prompt cannot be empty" });
        }

        try
        {
            var result = await _inferenceService.GenerateAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(502, new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = ex.Message });
        }
    }
}
