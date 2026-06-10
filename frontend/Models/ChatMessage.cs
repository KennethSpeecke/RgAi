namespace frontend.Models;

public sealed record ChatMessage(string Role, string Content, DateTime Timestamp)
{
    public ChatMessage(string role, string content)
        : this(role, content, DateTime.UtcNow)
    {
    }
}
