using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MCP.Web.UI.Services;

public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ChatResponse> SendMessageAsync(string chatId, string message)
    {
        if (string.IsNullOrWhiteSpace(chatId))
        {
            throw new ArgumentException("Chat ID is required", nameof(chatId));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }

        var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"chat/{chatId}/message", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            // El backend devuelve un string JSON, necesitamos deserializarlo
            var actualContent = JsonSerializer.Deserialize<string>(responseContent, _jsonOptions);
            
            return new ChatResponse
            {
                Success = true,
                Content = actualContent ?? responseContent,
                IsStreaming = false
            };
        }
        catch (HttpRequestException ex)
        {
            return new ChatResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
        catch (TaskCanceledException)
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Request timed out"
            };
        }
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(string chatId, string message)
    {
        if (string.IsNullOrWhiteSpace(chatId))
        {
            throw new ArgumentException("Chat ID is required", nameof(chatId));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }

        var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"chat/{chatId}/message")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error: {response.StatusCode} - {errorContent}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, bufferSize: 1024);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!string.IsNullOrEmpty(line) && line.StartsWith("data: "))
            {
                var data = line.Substring(6).Trim();
                if (data == "[DONE]")
                {
                    break;
                }
                
                if (!string.IsNullOrEmpty(data))
                {
                    string? chunk = null;
                    try
                    {
                        // Deserializar el string JSON que viene en el SSE
                        chunk = JsonSerializer.Deserialize<string>(data, _jsonOptions);
                    }
                    catch
                    {
                        // Si falla la deserialización, usar el dato tal cual
                        chunk = data.Trim('"'); // Remove quotes if present
                    }
                    
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        yield return chunk;
                        await Task.Delay(1); // Small delay to allow UI updates
                    }
                }
            }
        }
    }
}

public class ChatResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? Error { get; set; }
    public bool IsStreaming { get; set; }
}