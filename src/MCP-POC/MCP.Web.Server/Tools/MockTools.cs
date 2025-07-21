using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace MCP.Web.Server.Tools;

[McpServerToolType]
public class MockTools(IHttpContextAccessor httpContextAccessor)
{

    [McpServerTool, Description("Obtiene la fecha y hora actual en formato UTC.")]
    public string GetFechaHoraActualUtc()
    {
        return DateTime.UtcNow.ToString("O");
    }

    [McpServerTool, Description("Saluda al usuario con su nombre.")]
    public string Saludar(string nombre)
    {
        return $"¡Hola, {nombre}!, bienvenido a MCP Web Server.";
    }
    
    [McpServerTool, Description("Devuelve los encabezados HTTP de la solicitud actual.")]
    public string ObtenerEncabezados()
    {
        var context = httpContextAccessor.HttpContext;
        var sb = new StringBuilder();
    
        sb.AppendLine("Headers recibidos:");
        if (context?.Request?.Headers != null)
        {
            foreach (var header in context.Request.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value.ToArray())}");
            }
        }
        else
        {
            sb.AppendLine("(No hay headers)");
        }
    
        sb.AppendLine();
        return sb.ToString();
    }
}