namespace frontend.Models;

public sealed record InferenceRequest
{
    public string Prompt { get; init; } = string.Empty;
    public string Mode { get; init; } = "chat";
    public decimal Temperature { get; init; } = 0.7m;
    public int MaxTokens { get; init; } = 2048;
    public string SessionId { get; init; } = string.Empty;
}
