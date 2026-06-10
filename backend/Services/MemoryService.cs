using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RgAi.Backend.Models;

namespace RgAi.Backend.Services;

public sealed class MemoryService
{
    private readonly BackendSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ModelSelectionService _selectionService;
    private bool? _embeddingSupported;

    public MemoryService(BackendSettings settings, JsonSerializerOptions jsonOptions, IHttpClientFactory httpClientFactory, ModelSelectionService selectionService)
    {
        _settings = settings;
        _jsonOptions = jsonOptions;
        _httpClientFactory = httpClientFactory;
        _selectionService = selectionService;
    }

    public async Task<bool> CheckQdrantConnectionAsync()
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var request = CreateQdrantRequest(HttpMethod.Get, $"{_settings.QdrantUrl}/collections");
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<MemoryResult>> SearchAsync(string query, string sessionId, int limit)
    {
        var vector = await GetEmbeddingAsync(query);
        if (vector is null)
        {
            return Array.Empty<MemoryResult>();
        }

        if (!await EnsureQdrantCollectionAsync(vector.Count))
        {
            return Array.Empty<MemoryResult>();
        }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            var payload = new JsonObject
            {
                ["vector"] = JsonSerializer.SerializeToNode(vector, _jsonOptions),
                ["limit"] = limit,
                ["with_payload"] = true
            };

            var filter = BuildQdrantFilter(sessionId);
            if (filter is not null)
            {
                payload["filter"] = filter;
            }

            using var request = CreateQdrantRequest(HttpMethod.Post, $"{_settings.QdrantUrl}/collections/{_settings.QdrantCollection}/points/search", new StringContent(payload.ToJsonString(_jsonOptions), Encoding.UTF8, MediaTypeNames.Application.Json));
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<MemoryResult>();
            }

            var node = JsonNode.Parse(body);
            var result = node?["result"] ?? node;
            if (result is not JsonArray array)
            {
                return Array.Empty<MemoryResult>();
            }

            return array.Select(ParseMemoryResult).Where(memory => memory is not null).Select(memory => memory!).ToList();
        }
        catch
        {
            return Array.Empty<MemoryResult>();
        }
    }

    public async Task<bool> UpsertMemoryAsync(string prompt, string responseText, string mode, string? sessionId)
    {
        var vector = await GetEmbeddingAsync(prompt);
        if (vector is null)
        {
            return false;
        }

        if (!await EnsureQdrantCollectionAsync(vector.Count))
        {
            return false;
        }

        var idBytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{prompt}\n{responseText}\n{DateTime.UtcNow:o}"));
        var pointId = Convert.ToHexString(idBytes).ToLowerInvariant();

        var payload = new JsonObject
        {
            ["points"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = pointId,
                    ["vector"] = JsonSerializer.SerializeToNode(vector, _jsonOptions),
                    ["payload"] = new JsonObject
                    {
                        ["prompt"] = prompt,
                        ["response"] = responseText,
                        ["mode"] = mode,
                        ["timestamp"] = DateTime.UtcNow.ToString("o"),
                        ["session_id"] = string.IsNullOrWhiteSpace(sessionId)
                            ? null
                            : JsonValue.Create(sessionId)
                    }
                }
            }
        };

        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var request = CreateQdrantRequest(HttpMethod.Put, $"{_settings.QdrantUrl}/collections/{_settings.QdrantCollection}/points", new StringContent(payload.ToJsonString(_jsonOptions), Encoding.UTF8, MediaTypeNames.Application.Json));
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<float>?> GetEmbeddingAsync(string text)
    {
        if (_embeddingSupported == false)
        {
            return null;
        }

        using var client = _httpClientFactory.CreateClient();
        var payload = JsonSerializer.Serialize(new { model = _selectionService.CurrentModel, input = text }, _jsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.LlmUrl}/api/embed")
        {
            Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented)
        {
            _embeddingSupported = false;
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var node = JsonNode.Parse(body);
        var vector = ParseEmbedding(node);
        if (vector is not null)
        {
            _embeddingSupported = true;
        }

        return vector;
    }

    private async Task<bool> EnsureQdrantCollectionAsync(int vectorSize)
    {
        var info = await GetQdrantCollectionInfoAsync();
        if (info is null)
        {
            return await CreateCollectionAsync(vectorSize);
        }

        if (info["vectors"] is JsonObject vectors && vectors["size"]?.GetValue<int>() == vectorSize)
        {
            return true;
        }

        return false;
    }

    private async Task<JsonNode?> GetQdrantCollectionInfoAsync()
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var request = CreateQdrantRequest(HttpMethod.Get, $"{_settings.QdrantUrl}/collections/{_settings.QdrantCollection}/info");
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var body = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(body);
            return node?["result"] ?? node;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> CreateCollectionAsync(int vectorSize)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var payload = JsonSerializer.Serialize(new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance = _settings.QdrantDistance
                }
            }, _jsonOptions);

            using var request = CreateQdrantRequest(HttpMethod.Put, $"{_settings.QdrantUrl}/collections/{_settings.QdrantCollection}", new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json));
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private JsonNode? BuildQdrantFilter(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        return JsonNode.Parse($"{{ \"must\": [{{ \"key\": \"session_id\", \"match\": {{ \"value\": \"{sessionId}\" }} }}] }}");
    }

    private static MemoryResult? ParseMemoryResult(JsonNode? node)
    {
        if (node is not JsonObject point)
        {
            return null;
        }

        var payload = point["payload"] as JsonObject;
        if (payload is null)
        {
            return null;
        }

        var prompt = payload["prompt"]?.GetValue<string>() ?? string.Empty;
        var response = payload["response"]?.GetValue<string>() ?? string.Empty;
        var mode = payload["mode"]?.GetValue<string>() ?? string.Empty;
        var timestamp = payload["timestamp"]?.GetValue<string>() ?? string.Empty;

        return new MemoryResult(prompt, response, mode, timestamp);
    }

    private static List<float>? ParseEmbedding(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        if (node["embedding"] is JsonArray directEmbedding)
        {
            return directEmbedding.Select(item => item!.GetValue<float>()).ToList();
        }

        if (node["embeddings"] is JsonArray embeddings)
        {
            if (embeddings.FirstOrDefault() is JsonArray firstEmbedding)
            {
                return firstEmbedding.Select(item => item!.GetValue<float>()).ToList();
            }

            return embeddings.Select(item => item!.GetValue<float>()).ToList();
        }

        if (node["data"] is JsonArray dataArray && dataArray.FirstOrDefault() is JsonObject first)
        {
            if (first["embedding"] is JsonArray nestedEmbedding)
            {
                return nestedEmbedding.Select(item => item!.GetValue<float>()).ToList();
            }
        }

        return null;
    }

    private HttpRequestMessage CreateQdrantRequest(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        if (content is not null)
        {
            request.Content = content;
        }

        if (!string.IsNullOrWhiteSpace(_settings.QdrantApiKey))
        {
            request.Headers.Add("api-key", _settings.QdrantApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.QdrantApiKey);
        }

        return request;
    }
}
