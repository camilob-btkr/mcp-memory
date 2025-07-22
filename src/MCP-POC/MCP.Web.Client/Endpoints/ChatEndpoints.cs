using System.Text;
using MCP.Web.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace MCP.Web.Client.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat/{chatId}/message", async (
            string chatId,
            [FromBody] string message,
            IChatClient chatClient,
            IMcpClient mcpClient,
            IChatHistoryStore historyStore,
            HttpContext httpContext) =>
        {
            var history = await historyStore.LoadAsync(chatId);

            history.Add(new ChatHistoryEntry(ChatRole.User.Value, message));

            var messages = GetChatMessagesFromHistory(history);
            var tools = await mcpClient.ListToolsAsync();

            var options = new ChatOptions { Tools = [.. tools] };
            var sb = new StringBuilder();

            // Configure SSE response
            httpContext.Response.Headers.Add("Content-Type", "text/event-stream");
            httpContext.Response.Headers.Add("Cache-Control", "no-cache");
            httpContext.Response.Headers.Add("Connection", "keep-alive");

            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
            {
                sb.Append(update);
                
                // Send each chunk as SSE
                var chunk = $"data: {System.Text.Json.JsonSerializer.Serialize(update.ToString())}\n\n";
                await httpContext.Response.WriteAsync(chunk);
                await httpContext.Response.Body.FlushAsync();
            }
            
            // Send completion signal
            await httpContext.Response.WriteAsync("data: [DONE]\n\n");
            await httpContext.Response.Body.FlushAsync();
            
            // Save complete response to history
            var assistantReply = sb.ToString();
            history.Add(new ChatHistoryEntry(ChatRole.Assistant.Value, assistantReply));
            await historyStore.SaveAsync(chatId, history);
        });

        return app;
    }

    private static List<ChatMessage> GetChatMessagesFromHistory(List<ChatHistoryEntry> history)
    {
        return history.Select(m =>
            m.Role switch
            {
                "system" => new ChatMessage(ChatRole.System, m.Content),
                "assistant" => new ChatMessage(ChatRole.Assistant, m.Content),
                "tool" => new ChatMessage(ChatRole.Tool, m.Content),
                _ => new ChatMessage(ChatRole.User, m.Content)
            }).ToList();
    }
}