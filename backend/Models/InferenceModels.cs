namespace RgAi.Backend.Models;

public sealed record InferenceRequest(string Prompt, string Mode = "chat", decimal Temperature = 0.7m, int MaxTokens = 2048, decimal TopP = 0.9m, int TopK = 40, string? SessionId = null);
public sealed record InferenceResult(string Response, string Mode, string Prompt, string Model, string? RetrievedContext);
