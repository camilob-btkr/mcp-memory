using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MCP.Web.Server.Tools;

[McpServerToolType]
public class MockTools
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
}