namespace RgAi.Backend.Models;

public sealed record SessionCreateRequest(string? SessionId = null, string? Name = null);
public sealed record SessionRenameRequest(string Name);
public sealed record SessionMetadata(string SessionId, string Name, DateTime? CreatedAt, DateTime? UpdatedAt, int MessageCount);
public sealed record SessionHistoryItem(string Role, string Content, string Timestamp);
public sealed class SessionHistoryEntry
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}
public sealed class SessionIndexEntry
{
    public string SessionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public sealed record SessionResponse(SessionMetadata Session, IReadOnlyList<SessionHistoryItem> Messages);
