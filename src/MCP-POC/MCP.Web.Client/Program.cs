using Azure;
using Azure.AI.OpenAI;
using MCP.Web.Client.Endpoints;
using MCP.Web.Client.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using StackExchange.Redis;

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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5020", "https://localhost:7020")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddChatClient(services =>
    new ChatClientBuilder(
            new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey)
            ).GetChatClient(deployment).AsIChatClient()
        )
        .UseFunctionInvocation()
        .Build());
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Falta la clave 'Redis' en appsettings.")));

builder.Services.AddScoped<IChatHistoryStore, RedisChatHistoryStore>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IMcpClient>(sp  =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext!;
    var httpClient = new HttpClient();
    foreach (var header in httpContext.Request.Headers)
    {
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToArray());
    }

    var transportOptions = new SseClientTransportOptions
    {
        Endpoint = new Uri(mcpUrl)
    };
    
    var transport = new SseClientTransport(transportOptions,httpClient);

    return McpClientFactory.CreateAsync(transport).GetAwaiter().GetResult();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowBlazorClient");

app.MapGet("/", () => $"Hello MCP Server Client! {DateTime.Now}");
app.MapChatEndpoints();

app.Run();

