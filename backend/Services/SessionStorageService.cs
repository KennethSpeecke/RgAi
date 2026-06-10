using System.Text.Json;
using RgAi.Backend.Models;

namespace RgAi.Backend.Services;

public sealed class SessionStorageService
{
    private readonly BackendSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _sync = new();

    public SessionStorageService(BackendSettings settings, JsonSerializerOptions jsonOptions)
    {
        _settings = settings;
        _jsonOptions = jsonOptions;
    }

    public IReadOnlyList<SessionMetadata> GetSessions()
    {
        var index = LoadSessionIndex();
        var history = LoadSessionHistory();

        var sessions = index.Values.Select(entry => new SessionMetadata(
            entry.SessionId,
            entry.Name,
            entry.CreatedAt,
            entry.UpdatedAt,
            history.TryGetValue(entry.SessionId, out var messages) ? messages.Count : 0)).ToList();

        foreach (var kvp in history)
        {
            if (!index.ContainsKey(kvp.Key))
            {
                sessions.Add(new SessionMetadata(
                    kvp.Key,
                    $"Chat {kvp.Key[..8]}",
                    null,
                    null,
                    kvp.Value.Count));
            }
        }

        return sessions.OrderByDescending(session => session.UpdatedAt?.ToString("o") ?? string.Empty).ToList();
    }

    public SessionResponse GetSession(string sessionId)
    {
        var index = LoadSessionIndex();
        var history = LoadSessionHistory();

        var metadata = index.GetValueOrDefault(sessionId) ?? new SessionIndexEntry
        {
            SessionId = sessionId,
            Name = $"Chat {sessionId[..8]}",
            CreatedAt = null,
            UpdatedAt = null
        };

        var messages = history.TryGetValue(sessionId, out var entries)
            ? entries.Select(entry => new SessionHistoryItem(entry.Role, entry.Content, entry.Timestamp?.ToString("o") ?? string.Empty)).ToList()
            : new List<SessionHistoryItem>();

        return new SessionResponse(
            new SessionMetadata(
                metadata.SessionId,
                metadata.Name,
                metadata.CreatedAt,
                metadata.UpdatedAt,
                messages.Count),
            messages);
    }

    public SessionMetadata CreateSession(string? sessionId, string? name)
    {
        var resolvedSessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId!;
        var entry = EnsureSessionMetadata(resolvedSessionId, name);
        return new SessionMetadata(entry.SessionId, entry.Name, entry.CreatedAt, entry.UpdatedAt, GetSessionHistory(resolvedSessionId).Count);
    }

    public bool RenameSession(string sessionId, string name)
    {
        var index = LoadSessionIndex();
        if (!index.TryGetValue(sessionId, out var entry))
        {
            return false;
        }

        entry.Name = name;
        entry.UpdatedAt = DateTime.UtcNow;
        SaveSessionIndex(index);
        return true;
    }

    public bool DeleteSession(string sessionId)
    {
        var index = LoadSessionIndex();
        var history = LoadSessionHistory();
        var removed = false;

        if (index.Remove(sessionId))
        {
            removed = true;
        }

        if (history.Remove(sessionId))
        {
            removed = true;
        }

        if (removed)
        {
            SaveSessionIndex(index);
            SaveSessionHistory(history);
        }

        return removed;
    }

    public void AppendHistory(string sessionId, string role, string content)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        lock (_sync)
        {
            EnsureSessionMetadata(sessionId);
            var history = LoadSessionHistory();
            if (!history.TryGetValue(sessionId, out var entries))
            {
                entries = new List<SessionHistoryEntry>();
                history[sessionId] = entries;
            }

            entries.Add(new SessionHistoryEntry
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            });

            if (entries.Count > _settings.MaxSessionHistoryLength)
            {
                history[sessionId] = entries.Skip(entries.Count - _settings.MaxSessionHistoryLength).ToList();
            }

            SaveSessionHistory(history);
        }
    }

    public string BuildSessionContext(string sessionId)
    {
        var history = GetSessionHistory(sessionId, _settings.MaxSessionContextTurns);
        if (!history.Any())
        {
            return string.Empty;
        }

        return string.Join("\n", history.Select(item => $"{(item.Role == "user" ? "User" : "Assistant")}: {item.Content}"));
    }

    public IReadOnlyList<SessionHistoryEntry> GetSessionHistory(string sessionId, int limit = 0)
    {
        var history = LoadSessionHistory();
        if (!history.TryGetValue(sessionId, out var entries))
        {
            return Array.Empty<SessionHistoryEntry>();
        }

        if (limit > 0 && entries.Count > limit)
        {
            return entries.Skip(entries.Count - limit).ToList();
        }

        return entries;
    }

    private SessionIndexEntry EnsureSessionMetadata(string sessionId, string? name = null)
    {
        var index = LoadSessionIndex();
        if (!index.TryGetValue(sessionId, out var entry))
        {
            entry = new SessionIndexEntry
            {
                SessionId = sessionId,
                Name = string.IsNullOrWhiteSpace(name) ? $"Chat {sessionId[..8]}" : name!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            entry.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(name))
            {
                entry.Name = name;
            }
        }

        index[sessionId] = entry;
        SaveSessionIndex(index);
        return entry;
    }

    private Dictionary<string, List<SessionHistoryEntry>> LoadSessionHistory()
    {
        try
        {
            if (!File.Exists(GetSessionHistoryPath()))
            {
                return new Dictionary<string, List<SessionHistoryEntry>>();
            }

            var content = File.ReadAllText(GetSessionHistoryPath());
            return JsonSerializer.Deserialize<Dictionary<string, List<SessionHistoryEntry>>>(content, _jsonOptions)
                ?? new Dictionary<string, List<SessionHistoryEntry>>();
        }
        catch
        {
            return new Dictionary<string, List<SessionHistoryEntry>>();
        }
    }

    private void SaveSessionHistory(Dictionary<string, List<SessionHistoryEntry>> history)
    {
        try
        {
            var content = JsonSerializer.Serialize(history, _jsonOptions);
            File.WriteAllText(GetSessionHistoryPath(), content);
        }
        catch
        {
        }
    }

    private Dictionary<string, SessionIndexEntry> LoadSessionIndex()
    {
        try
        {
            if (!File.Exists(GetSessionIndexPath()))
            {
                return new Dictionary<string, SessionIndexEntry>();
            }

            var content = File.ReadAllText(GetSessionIndexPath());
            return JsonSerializer.Deserialize<Dictionary<string, SessionIndexEntry>>(content, _jsonOptions)
                ?? new Dictionary<string, SessionIndexEntry>();
        }
        catch
        {
            return new Dictionary<string, SessionIndexEntry>();
        }
    }

    private void SaveSessionIndex(Dictionary<string, SessionIndexEntry> index)
    {
        try
        {
            var content = JsonSerializer.Serialize(index, _jsonOptions);
            File.WriteAllText(GetSessionIndexPath(), content);
        }
        catch
        {
        }
    }

    private string GetSessionHistoryPath() => Path.Combine(_settings.CacheDirectory, "session_history.json");
    private string GetSessionIndexPath() => Path.Combine(_settings.CacheDirectory, "session_index.json");
}
