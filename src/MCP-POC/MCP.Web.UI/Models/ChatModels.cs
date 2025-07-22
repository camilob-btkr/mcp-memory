namespace MCP.Web.UI.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ChatSession
{
    public string ChatId { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastActivity { get; set; } = DateTime.Now;
}

public class ChatState
{
    public string CurrentChatId { get; set; } = string.Empty;
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsConnected { get; set; } = true;
    
    public bool IsValidChatId()
    {
        return !string.IsNullOrWhiteSpace(CurrentChatId) && 
               CurrentChatId.Length >= 1 && 
               CurrentChatId.Length <= 50 &&
               System.Text.RegularExpressions.Regex.IsMatch(CurrentChatId, @"^[a-zA-Z0-9_-]+$");
    }
}

public static class ChatRoles
{
    public const string User = "user";
    public const string Assistant = "assistant";
    public const string System = "system";
}