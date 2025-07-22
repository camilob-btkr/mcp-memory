using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MCP.Web.UI;
using MCP.Web.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient 
    { 
        BaseAddress = new Uri("http://localhost:5194"),
        Timeout = TimeSpan.FromMinutes(5) // Longer timeout for streaming
    };
    
    // Configure headers for better streaming
    httpClient.DefaultRequestHeaders.Add("Accept", "text/event-stream");
    httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
    
    return httpClient;
});
builder.Services.AddScoped<ChatService>();

await builder.Build().RunAsync();
