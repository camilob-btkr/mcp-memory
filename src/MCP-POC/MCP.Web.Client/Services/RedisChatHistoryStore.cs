using System.Text.Json;
using StackExchange.Redis;

namespace MCP.Web.Client.Services;


public record ChatHistoryEntry
{
    public string Role { get; init; }
    public string Content { get; init; }
    public DateTime Timestamp { get; init; }
    public ChatHistoryEntry(string role, string content)
    {
        Role = role;
        Content = content;
        Timestamp = DateTime.UtcNow;
    }
    public ChatHistoryEntry() { }
}

public interface IChatHistoryStore
{
    Task<List<ChatHistoryEntry>> LoadAsync(string chatId);
    Task SaveAsync(string chatId, List<ChatHistoryEntry> history);
}

public class RedisChatHistoryStore : IChatHistoryStore
{
    private readonly IDatabase _db;
    private readonly TimeSpan _ttl = TimeSpan.FromHours(2);

    public RedisChatHistoryStore(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
    }

    private string Key(string chatId) => $"chat:history:{chatId}";

    public async Task<List<ChatHistoryEntry>> LoadAsync(string chatId)
    {
        var data = await _db.StringGetAsync(Key(chatId));
        if (data.IsNullOrEmpty) return new();
        return JsonSerializer.Deserialize<List<ChatHistoryEntry>>(data!)!;
    }

    public async Task SaveAsync(string chatId, List<ChatHistoryEntry> history)
    {
        var json = JsonSerializer.Serialize(history);
        await _db.StringSetAsync(Key(chatId), json, _ttl);
    }
}