namespace MCP.Web.Server;

public class HeaderPropagationMiddleware
{
    private readonly RequestDelegate _next;

    public HeaderPropagationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headersToPropagate = new[] { "Authorization", "X-Request-Id", "X-My-Header" };

        foreach (var name in headersToPropagate)
        {
            if (context.Request.Headers.TryGetValue(name, out var value))
            {
                context.Items[name] = value.ToString();
            }
        }

        await _next(context);
    }
}