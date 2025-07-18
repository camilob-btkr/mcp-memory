using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace MCP.Web.Client.Endpoints;

public static class ChatEndpoints
{
    
    public static IEndpointRouteBuilder  MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat/{chatId}/message", async (
            string chatId,
            [FromBody] string message,
            IChatClient chatClient,
            IMcpClient mcpClient) =>
        {
            var tools = await mcpClient.ListToolsAsync();

            var messages = new List<ChatMessage>
            {
//                 new(ChatRole.System, """
//                                                      Eres un asistente experto en gestión de usuarios, enfocado en tareas administrativas simples.
//
//                                                      🎯 Tus herramientas MCP disponibles son:
//                                                      - `getFechaHoraActualUtc`: Devuelve la fecha y hora actual en formato UTC.
//                                                      - `saludar`: Saluda a un usuario por su nombre.
//
//                                                      🛠️ Usa estas herramientas cada vez que sea relevante. No inventes respuestas si una herramienta está disponible para resolver la solicitud.
//
//                                                      📌 Responde siempre en español profesional.
//                                      """),
                new(ChatRole.User, message)
            };

            var options = new ChatOptions { Tools = [.. tools]};
            var sb = new StringBuilder();

            await foreach (var update in chatClient.GetStreamingResponseAsync(messages,options))
            {
                sb.Append(update);
            }

            return Results.Ok(sb.ToString());
        });

        return app;
    }
}