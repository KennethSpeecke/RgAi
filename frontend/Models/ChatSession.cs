namespace frontend.Models;

public sealed record ChatSession(
    string SessionId,
    string Name,
    string? CreatedAt,
    string? UpdatedAt,
    int MessageCount
);
