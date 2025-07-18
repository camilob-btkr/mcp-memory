using MCP.Web.Server.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<MockTools>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// map endpoints
app.MapGet("/", () => $"Hello MCP Server! {DateTime.Now}");
app.MapMcp();

app.Run();

