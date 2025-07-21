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
            IChatHistoryStore historyStore) =>
        {
            var history = await historyStore.LoadAsync(chatId);

            history.Add(new ChatHistoryEntry(ChatRole.User.Value, message));

            var messages = GetChatMessagesFromHistory(history);
            var tools = await mcpClient.ListToolsAsync();

//             var messages = new List<ChatMessage>
//             {
// //                 new(ChatRole.System, """
// //                                                      Eres un asistente experto en gestión de usuarios, enfocado en tareas administrativas simples.
// //
// //                                                      🎯 Tus herramientas MCP disponibles son:
// //                                                      - `getFechaHoraActualUtc`: Devuelve la fecha y hora actual en formato UTC.
// //                                                      - `saludar`: Saluda a un usuario por su nombre.
// //
// //                                                      🛠️ Usa estas herramientas cada vez que sea relevante. No inventes respuestas si una herramienta está disponible para resolver la solicitud.
// //
// //                                                      📌 Responde siempre en español profesional.
// //                                      """),
//                 new(ChatRole.User, message)
//             };

            var options = new ChatOptions { Tools = [.. tools] };
            var sb = new StringBuilder();

            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
            {
                sb.Append(update);
            }
            
            var assistantReply = sb.ToString();
            
            history.Add(new ChatHistoryEntry(ChatRole.Assistant.Value,assistantReply));
            
            await historyStore.SaveAsync(chatId, history);

            return Results.Ok(assistantReply);
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