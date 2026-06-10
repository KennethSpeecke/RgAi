using Microsoft.AspNetCore.Mvc;
using RgAi.Backend.Models;
using RgAi.Backend.Services;

namespace RgAi.Backend.Controllers;

[ApiController]
[Route("api/v1/sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly SessionStorageService _storage;

    public SessionsController(SessionStorageService storage)
    {
        _storage = storage;
    }

    [HttpGet]
    public IActionResult GetSessions()
    {
        var sessions = _storage.GetSessions();
        return Ok(new { sessions, total = sessions.Count });
    }

    [HttpGet("{sessionId}")]
    public IActionResult GetSession(string sessionId)
    {
        var session = _storage.GetSession(sessionId);
        return Ok(session);
    }

    [HttpPost]
    public IActionResult CreateSession(SessionCreateRequest request)
    {
        var session = _storage.CreateSession(request.SessionId, request.Name);
        return Ok(new { session });
    }

    [HttpPut("{sessionId}")]
    public IActionResult RenameSession(string sessionId, SessionRenameRequest request)
    {
        if (!_storage.RenameSession(sessionId, request.Name))
        {
            return NotFound(new { detail = "Session not found" });
        }

        var session = _storage.GetSession(sessionId).Session;
        return Ok(new { session });
    }

    [HttpDelete("{sessionId}")]
    public IActionResult DeleteSession(string sessionId)
    {
        if (!_storage.DeleteSession(sessionId))
        {
            return NotFound(new { detail = "Session not found" });
        }

        return Ok(new { status = "deleted", session_id = sessionId });
    }
}
