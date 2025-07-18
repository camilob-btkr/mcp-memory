using Azure;
using Azure.AI.OpenAI;
using MCP.Web.Client.Endpoints;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

var azureConfig = builder.Configuration.GetSection("AzureOpenAI");

var endpoint = azureConfig["Endpoint"];
var apiKey = azureConfig["ApiKey"];
var deployment = azureConfig["Deployment"];

if (string.IsNullOrWhiteSpace(endpoint) ||
    string.IsNullOrWhiteSpace(apiKey) ||
    string.IsNullOrWhiteSpace(deployment))
{
    throw new InvalidOperationException("Faltan claves en la sección AzureOpenAI del appsettings.");
}

var mcpUrl = builder.Configuration["Mcp:ServerUrl"];
if (string.IsNullOrWhiteSpace(mcpUrl))
{
    throw new InvalidOperationException("Falta la clave 'Mcp:ServerUrl' en appsettings.");
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddChatClient(services =>
    new ChatClientBuilder(
            new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey)
            ).GetChatClient(deployment).AsIChatClient()
        )
        .UseFunctionInvocation()
        .Build());

builder.Services.AddSingleton<IMcpClient>(_ =>
{
    var transport = new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri(mcpUrl) // Ajusta al endpoint real del servidor MCP
    });

    return McpClientFactory.CreateAsync(transport).GetAwaiter().GetResult();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();
app.MapGet("/", () => $"Hello MCP Server Client! {DateTime.Now}");
app.MapChatEndpoints();

app.Run();

