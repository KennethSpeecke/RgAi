using RgAi.Backend.Models;

namespace RgAi.Backend.Services;

public sealed class InferenceService
{
    private readonly LlmService _llmService;
    private readonly MemoryService _memoryService;
    private readonly SessionStorageService _sessionStorage;
    private readonly BackendSettings _settings;
    private readonly ModelSelectionService _selectionService;
    private readonly ILogger<InferenceService> _logger;

    public InferenceService(
        LlmService llmService,
        MemoryService memoryService,
        SessionStorageService sessionStorage,
        BackendSettings settings,
        ModelSelectionService selectionService,
        ILogger<InferenceService> logger)
    {
        _llmService = llmService;
        _memoryService = memoryService;
        _sessionStorage = sessionStorage;
        _settings = settings;
        _selectionService = selectionService;
        _logger = logger;
    }

    public async Task<InferenceResult> GenerateAsync(InferenceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt cannot be empty.", nameof(request.Prompt));
        }

        var retrievedContext = string.Empty;
        
        // Try Qdrant first (vector search with embeddings)
        if (!string.IsNullOrWhiteSpace(request.SessionId) && await _memoryService.CheckQdrantConnectionAsync())
        {
            try
            {
                var memoryResults = await _memoryService.SearchAsync(request.Prompt, request.SessionId!, _settings.QdrantDefaultLimit);
                if (memoryResults.Any())
                {
                    retrievedContext = BuildRetrievalContext(memoryResults);
                    _logger.LogInformation("Retrieved {Count} memories from Qdrant for session {SessionId}", memoryResults.Count, request.SessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Qdrant search failed, falling back to session storage");
                // Fall through to session storage fallback below
            }
        }

        // Fallback to session storage if Qdrant retrieval failed or no Qdrant connection
        if (string.IsNullOrWhiteSpace(retrievedContext) && !string.IsNullOrWhiteSpace(request.SessionId))
        {
            retrievedContext = _sessionStorage.BuildSessionContext(request.SessionId!);
            if (!string.IsNullOrWhiteSpace(retrievedContext))
            {
                _logger.LogInformation("Retrieved context from session storage for session {SessionId}", request.SessionId);
            }
        }

        var prompt = ComposePrompt(request.Prompt, request.Mode, retrievedContext);
        var response = await _llmService.GenerateAsync(prompt, request.Temperature, request.TopP, request.TopK);

        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            _sessionStorage.AppendHistory(request.SessionId, "user", request.Prompt);
            _sessionStorage.AppendHistory(request.SessionId, "assistant", response);
            
            // Try to store in Qdrant, but don't fail inference if it doesn't work
            try
            {
                var stored = await _memoryService.UpsertMemoryAsync(request.Prompt, response, request.Mode, request.SessionId);
                if (stored)
                {
                    _logger.LogInformation("Memory stored in Qdrant for session {SessionId}", request.SessionId);
                }
                else
                {
                    _logger.LogWarning("Failed to store memory in Qdrant for session {SessionId}, but session storage is updated", request.SessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while storing memory in Qdrant for session {SessionId}, continuing with session storage", request.SessionId);
            }
        }

        return new InferenceResult(response, request.Mode, request.Prompt, _selectionService.GenerateModel, string.IsNullOrWhiteSpace(retrievedContext) ? null : retrievedContext);
    }

    private static string ComposePrompt(string prompt, string mode, string retrievedContext)
    {
        var modeContext = mode.ToLowerInvariant() switch
        {
            "code" => "You are an expert programmer. Generate clean, production-ready code with comments. Format code in markdown code blocks (```language code```).",
            "debug" => "You are a debugging expert. Analyze the issue, explain what's wrong, and provide a solution. Format code in markdown code blocks.",
            "explain" => "You are a code explanation expert. Explain the code in simple terms, breaking down each part. Include what it does and why.",
            _ => "You are a helpful AI assistant. Answer questions clearly and concisely."
        };

        var userPrompt = $"{modeContext}\n\nUser: {prompt}";
        if (string.IsNullOrWhiteSpace(retrievedContext))
        {
            return userPrompt;
        }

        return $"Use the following retrieved memory from prior interactions to help answer the user's request. Only use relevant context and do not invent details.\n\n{retrievedContext}\n\n{userPrompt}";
    }

    private static string BuildRetrievalContext(IReadOnlyList<MemoryResult> results)
    {
        var lines = results.Select((item, index) => $"Memory {index + 1}: Prompt: {item.Prompt}\nResponse: {item.Response}").ToList();
        return string.Join("\n\n", lines);
    }
}
