namespace RgAi.Backend.Models;

public sealed class BackendSettings
{
    public string DataDirectory { get; init; } = null!;
    public string LogsDirectory => Path.Combine(DataDirectory, "logs");
    public string CacheDirectory => Path.Combine(DataDirectory, "cache");
    public string UploadsDirectory => Path.Combine(DataDirectory, "uploads");
    public string LlmHost { get; init; } = null!;
    public string LlmPort { get; init; } = null!;
    public string QdrantHost { get; init; } = null!;
    public string QdrantPort { get; init; } = null!;
    public string QdrantApiKey { get; init; } = null!;
    public string DefaultLlmModel { get; init; } = null!;
    public string QdrantCollection { get; init; } = null!;
    public string QdrantDistance { get; init; } = null!;
    public int QdrantDefaultLimit { get; init; }
    public int MaxSessionContextTurns { get; init; }
    public int MaxSessionHistoryLength { get; init; }

    public string LlmUrl => $"http://{LlmHost}:{LlmPort}";
    public string QdrantUrl => $"http://{QdrantHost}:{QdrantPort}";

    private BackendSettings()
    {
    }

    public static BackendSettings Load()
    {
        var settings = new BackendSettings
        {
            DataDirectory = GetEnv("DATA_DIR", Path.Combine(AppContext.BaseDirectory, "data")),
            LlmHost = GetEnv("LLM_HOST", "localhost"),
            LlmPort = GetEnv("LLM_PORT", "11434"),
            QdrantHost = GetEnv("QDRANT_HOST", "localhost"),
            QdrantPort = GetEnv("QDRANT_PORT", "6333"),
            QdrantApiKey = GetEnv("QDRANT_API_KEY", string.Empty),
            DefaultLlmModel = GetEnv("LLM_MODEL", "qwen3-coder"),
            QdrantCollection = GetEnv("QDRANT_COLLECTION", "rgai_memory"),
            QdrantDistance = GetEnv("QDRANT_DISTANCE", "Cosine"),
            QdrantDefaultLimit = int.TryParse(GetEnv("QDRANT_DEFAULT_LIMIT", "3"), out var qlimit) ? qlimit : 3,
            MaxSessionContextTurns = int.TryParse(GetEnv("MAX_SESSION_CONTEXT_TURNS", "6"), out var contextTurns) ? contextTurns : 6,
            MaxSessionHistoryLength = int.TryParse(GetEnv("MAX_SESSION_HISTORY_LENGTH", "120"), out var historyLength) ? historyLength : 120,
        };

        settings.EnsureDirectories();
        return settings;
    }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(CacheDirectory);
        Directory.CreateDirectory(UploadsDirectory);
    }

    private static string GetEnv(string name, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }
}
