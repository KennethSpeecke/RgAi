using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RgAi.Backend.Models;

namespace RgAi.Backend.Services;

public sealed class LlmService
{
    private readonly BackendSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ModelSelectionService _selectionService;

    public LlmService(BackendSettings settings, JsonSerializerOptions jsonOptions, IHttpClientFactory httpClientFactory, ModelSelectionService selectionService)
    {
        _settings = settings;
        _jsonOptions = jsonOptions;
        _httpClientFactory = httpClientFactory;
        _selectionService = selectionService;
    }

    private HttpClient CreateLlmClient() => _httpClientFactory.CreateClient("Llm");

    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            using var client = CreateLlmClient();
            var response = await client.GetAsync($"{_settings.LlmUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetAvailableModelsAsync()
    {
        try
        {
            using var client = CreateLlmClient();
            var response = await client.GetAsync($"{_settings.LlmUrl}/api/tags");
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<string>();
            }

            var payload = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(payload);
            var models = node?["models"] as JsonArray;
            if (models is null)
            {
                return Array.Empty<string>();
            }

            return models.Select(item => item?["name"]?.GetValue<string>() ?? string.Empty).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public async Task<bool> WarmUpAsync()
    {
        try
        {
            using var client = CreateLlmClient();
            var payload = JsonSerializer.Serialize(new
            {
                model = _selectionService.GenerateModel,
                prompt = "Warm up",
                stream = false,
                temperature = 0.2m,
                top_p = 1.0m,
                top_k = 1
            }, _jsonOptions);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.LlmUrl}/api/generate")
            {
                Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"ERROR: Failed to warm up LLM: {body}");
            }
            return response.IsSuccessStatusCode;
        }
        catch
        {
            Console.WriteLine("Failed to warm up LLM");
            return false;
        }
    }

    public async Task<string> GenerateAsync(string prompt, decimal temperature, decimal topP, int topK)
    {
        using var client = CreateLlmClient();
        var payload = JsonSerializer.Serialize(new
        {
            model = _selectionService.GenerateModel,
            prompt = prompt,
            stream = false,
            temperature,
            top_p = topP,
            top_k = topK
        }, _jsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.LlmUrl}/api/generate")
        {
            Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"LLM service error: {body}");
        }

        // Handle both single JSON object and newline-delimited JSON responses
        var lines = body.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var lastLine = lines.LastOrDefault() ?? string.Empty;
        
        var node = JsonNode.Parse(lastLine);
        var text = node?["response"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Empty response from LLM service.");
        }

        return text;
    }

    public async Task<JsonNode?> GetModelInfoAsync(string modelName)
    {
        using var client = CreateLlmClient();
        var payload = JsonSerializer.Serialize(new { name = modelName }, _jsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.LlmUrl}/api/show")
        {
            Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return JsonNode.Parse(body);
    }

    public async Task<(bool Success, string Message)> PullModelAsync(string modelName)
    {
        try
        {
            using var client = CreateLlmClient();
            var payload = JsonSerializer.Serialize(new { name = modelName }, _jsonOptions);
            using var response = await client.PostAsync($"{_settings.LlmUrl}/api/pull", new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json));
            var body = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, body);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> DeleteModelAsync(string modelName)
    {
        try
        {
            using var client = CreateLlmClient();
            var payload = JsonSerializer.Serialize(new { name = modelName }, _jsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_settings.LlmUrl}/api/delete")
            {
                Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, body);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<float>?> GetEmbeddingAsync(string text)
    {
        try
        {
            using var client = CreateLlmClient();
            var payload = JsonSerializer.Serialize(new { model = _selectionService.GenerateModel, input = text }, _jsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.LlmUrl}/api/embed")
            {
                Content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            using var response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented)
            {
                return null;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var node = JsonNode.Parse(body);
            return ParseEmbedding(node);
        }
        catch
        {
            return null;
        }
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
}
