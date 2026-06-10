using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RgAi.Backend.Models;

namespace RgAi.Backend.Services;

public sealed class CodeIndexingService
{
    private readonly LlmService _llmService;
    private readonly BackendSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<CodeIndexingService> _logger;

    private static readonly string[] CodeExtensions = 
    { 
        ".cs", ".py", ".js", ".ts", ".java", ".go", ".rs", ".cpp", ".c", ".h", 
        ".jsx", ".tsx", ".vue", ".html", ".css", ".sql", ".yaml", ".yml", ".json",
        ".xml", ".md", ".txt", ".sh", ".bash", ".gradle", ".maven", ".tf"
    };

    private static readonly string[] IgnorePaths = 
    { 
        "node_modules", ".git", "bin", "obj", "dist", "build", ".venv", "__pycache__",
        ".next", ".nuxt", "coverage", ".idea", ".vscode", "target", "packages"
    };

    private const string CodeCollectionName = "code_chunks";

    public CodeIndexingService(
        LlmService llmService, 
        BackendSettings settings,
        IHttpClientFactory httpClientFactory,
        JsonSerializerOptions jsonOptions,
        ILogger<CodeIndexingService> logger)
    {
        _llmService = llmService;
        _settings = settings;
        _httpClientFactory = httpClientFactory;
        _jsonOptions = jsonOptions;
        _logger = logger;
    }

    public async Task<(int FilesIndexed, int ChunksCreated, string Message)> IndexRepositoryAsync(
        string repoPath, 
        string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(repoPath))
        {
            return (0, 0, $"Repository path not found: {repoPath}");
        }

        try
        {
            var allFiles = GetCodeFiles(repoPath);
            _logger.LogInformation("Found {FileCount} code files to index", allFiles.Count);

            int filesProcessed = 0;
            int chunksCreated = 0;

            foreach (var filePath in allFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var chunks = await IndexFileAsync(filePath, sessionId, cancellationToken);
                    chunksCreated += chunks;
                    filesProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to index file {FilePath}", filePath);
                }
            }

            var message = $"Indexed {filesProcessed} files, created {chunksCreated} searchable chunks";
            _logger.LogInformation(message);
            return (filesProcessed, chunksCreated, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository indexing failed");
            return (0, 0, $"Indexing error: {ex.Message}");
        }
    }

    public async Task<List<CodeSearchResultDto>> SearchCodeAsync(
        string query, 
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var embedding = await _llmService.GetEmbeddingAsync(query);
            if (embedding is null)
            {
                return new();
            }

            var results = await SearchQdrantAsync(embedding, CodeCollectionName, limit, cancellationToken);
            var searchResults = new List<CodeSearchResultDto>();

            foreach (var result in results)
            {
                if (result?["payload"] is JsonObject payload)
                {
                    searchResults.Add(new CodeSearchResultDto
                    {
                        Content = payload["content"]?.GetValue<string>() ?? "",
                        FilePath = payload["file_path"]?.GetValue<string>() ?? "",
                        FileName = payload["file_name"]?.GetValue<string>() ?? "",
                        StartLine = payload["start_line"]?.GetValue<int>() ?? 0,
                        EndLine = payload["end_line"]?.GetValue<int>() ?? 0,
                        Language = payload["language"]?.GetValue<string>() ?? "text"
                    });
                }
            }

            return searchResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code search failed");
            return new();
        }
    }

    private async Task<int> IndexFileAsync(string filePath, string? sessionId, CancellationToken cancellationToken)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var fileName = Path.GetFileName(filePath);
        var language = GetLanguage(fileName);

        var chunks = ChunkCode(content, fileName, filePath, 500, 100);
        int created = 0;

        foreach (var chunk in chunks)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var embedding = await _llmService.GetEmbeddingAsync(chunk.Content);
                if (embedding is null)
                    continue;

                await EnsureCollectionExistsAsync(embedding.Count);

                var pointId = GeneratePointId(chunk.Content);
                var payload = new Dictionary<string, object>
                {
                    ["content"] = chunk.Content,
                    ["file_name"] = chunk.FileName,
                    ["file_path"] = chunk.FilePath,
                    ["start_line"] = chunk.StartLine,
                    ["end_line"] = chunk.EndLine,
                    ["language"] = language,
                    ["indexed_at"] = DateTime.UtcNow.ToString("o"),
                    ["session_id"] = sessionId ?? "global"
                };

                await UpsertToQdrantAsync(pointId, embedding, payload);
                created++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to index chunk in {FilePath}", filePath);
            }
        }

        return created;
    }

    private List<(string FileName, string FilePath, string Content, int StartLine, int EndLine)> ChunkCode(
        string content,
        string fileName,
        string filePath,
        int chunkSize = 500,
        int overlap = 100)
    {
        var lines = content.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
        var chunks = new List<(string, string, string, int, int)>();

        for (int i = 0; i < lines.Length; i += (chunkSize - overlap))
        {
            var endIdx = Math.Min(i + chunkSize, lines.Length);
            var chunkLines = lines[i..endIdx];
            var chunkContent = string.Join("\n", chunkLines);

            if (string.IsNullOrWhiteSpace(chunkContent))
                continue;

            var startLine = i + 1;
            var endLine = endIdx;

            chunks.Add((fileName, filePath, chunkContent, startLine, endLine));
        }

        return chunks;
    }

    private List<string> GetCodeFiles(string rootPath)
    {
        var files = new List<string>();

        try
        {
            var di = new DirectoryInfo(rootPath);
            WalkDirectory(di, files);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error walking directory {Path}", rootPath);
        }

        return files;
    }

    private void WalkDirectory(DirectoryInfo dir, List<string> files)
    {
        if (IgnorePaths.Any(ignore => dir.Name.Equals(ignore, StringComparison.OrdinalIgnoreCase)))
            return;

        try
        {
            foreach (var file in dir.GetFiles())
            {
                if (CodeExtensions.Contains(Path.GetExtension(file.Name).ToLower()))
                {
                    files.Add(file.FullName);
                }
            }

            foreach (var subDir in dir.GetDirectories())
            {
                WalkDirectory(subDir, files);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied to directory {Path}", dir.FullName);
        }
    }

    private string GeneratePointId(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLower();
    }

    private string GetLanguage(string fileName)
    {
        return Path.GetExtension(fileName).TrimStart('.').ToLower() switch
        {
            "cs" => "csharp",
            "py" => "python",
            "js" => "javascript",
            "ts" => "typescript",
            "tsx" => "typescript",
            "jsx" => "javascript",
            "java" => "java",
            "go" => "go",
            "rs" => "rust",
            "cpp" or "c" => "c++",
            "h" => "c",
            "sql" => "sql",
            "md" => "markdown",
            "sh" or "bash" => "bash",
            _ => "text"
        };
    }

    private async Task<List<JsonNode?>> SearchQdrantAsync(
        List<float> vector,
        string collectionName,
        int limit,
        CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        var payload = JsonSerializer.Serialize(new
        {
            vector = vector,
            limit = limit,
            with_payload = true
        }, _jsonOptions);

        var url = $"{_settings.QdrantUrl}/collections/{collectionName}/points/search";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        AddQdrantHeaders(request);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Qdrant search failed: {StatusCode} {Body}", response.StatusCode, body);
                return new();
            }

            var node = JsonNode.Parse(body);
            var results = node?["result"] as JsonArray;
            return results?.ToList() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Qdrant");
            return new();
        }
    }

    private async Task UpsertToQdrantAsync(
        string pointId,
        List<float> embedding,
        Dictionary<string, object> payload)
    {
        using var client = _httpClientFactory.CreateClient();
        
        var points = new
        {
            points = new[]
            {
                new
                {
                    id = pointId,
                    vector = embedding,
                    payload = payload
                }
            }
        };

        var payloadJson = JsonSerializer.Serialize(points, _jsonOptions);
        var url = $"{_settings.QdrantUrl}/collections/{CodeCollectionName}/points";

        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(payloadJson, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        AddQdrantHeaders(request);

        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Qdrant upsert failed: {StatusCode} {Body}", response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting to Qdrant");
        }
    }

    private async Task EnsureCollectionExistsAsync(int vectorSize)
    {
        using var client = _httpClientFactory.CreateClient();
        
        // Check if collection exists
        var checkUrl = $"{_settings.QdrantUrl}/collections/{CodeCollectionName}";
        using var checkRequest = new HttpRequestMessage(HttpMethod.Get, checkUrl);
        AddQdrantHeaders(checkRequest);

        try
        {
            using var checkResponse = await client.SendAsync(checkRequest);
            if (checkResponse.IsSuccessStatusCode)
            {
                return; // Collection exists
            }
        }
        catch { }

        // Create collection
        var createPayload = JsonSerializer.Serialize(new
        {
            vectors = new
            {
                size = vectorSize,
                distance = _settings.QdrantDistance
            }
        }, _jsonOptions);

        var createUrl = $"{_settings.QdrantUrl}/collections/{CodeCollectionName}";
        using var createRequest = new HttpRequestMessage(HttpMethod.Put, createUrl)
        {
            Content = new StringContent(createPayload, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        AddQdrantHeaders(createRequest);

        try
        {
            using var response = await client.SendAsync(createRequest);
            _logger.LogInformation("Created Qdrant collection {Collection}", CodeCollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Qdrant collection");
        }
    }

    private void AddQdrantHeaders(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_settings.QdrantApiKey))
        {
            request.Headers.Add("api-key", _settings.QdrantApiKey);
        }
    }
}

public class CodeSearchResultDto
{
    public string Content { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string Language { get; set; } = string.Empty;
}
