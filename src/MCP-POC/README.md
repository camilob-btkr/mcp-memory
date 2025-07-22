# MCP-POC - Model Context Protocol Proof of Concept

Esta solución implementa un sistema de chat completo utilizando el protocolo MCP (Model Context Protocol) con Azure OpenAI, Redis para persistencia y una interfaz web en Blazor WebAssembly.

## Arquitectura de la Solución

La solución consta de tres proyectos principales:

```
MCP-POC/
├── MCP.Web.Server/     # Servidor MCP con herramientas personalizadas
├── MCP.Web.Client/     # Cliente API que consume Azure OpenAI y MCP
└── MCP.Web.UI/         # Interfaz web Blazor WebAssembly
```

## Prerrequisitos

- .NET 9.0 SDK
- Docker (para Redis)
- Azure OpenAI Service (cuenta y keys)

## Configuración de Redis con Docker

El proyecto utiliza Redis para persistir el historial de chat. Para ejecutar Redis usando Docker:

```bash
# Ejecutar Redis Stack (incluye Redis + RedisInsight para administración)
docker run -d --name redis-mcp -p 6379:6379 -p 8001:8001 redis/redis-stack:latest

# Verificar que Redis está funcionando
docker logs redis-mcp
```

Redis estará disponible en:
- **Redis Server**: `localhost:6379` (sin contraseña)
- **RedisInsight UI**: `http://localhost:8001` (interfaz web de administración)

Para detener Redis:
```bash
docker stop redis-mcp
docker rm redis-mcp
```

---

## MCP.Web.Server

**Puerto por defecto**: `5156` (HTTP) / `7082` (HTTPS)

### Descripción
Servidor MCP que expone herramientas personalizadas mediante el protocolo Model Context Protocol. Actúa como el backend que proporciona capacidades extendidas al asistente de IA.

### Características
- Implementa el protocolo MCP usando `ModelContextProtocol.AspNetCore`
- Expone herramientas mock personalizables
- Middleware de propagación de headers
- API de estado en `/status`

### Configuración

#### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Dependencias
- `ModelContextProtocol` (v0.3.0-preview.3)
- `ModelContextProtocol.AspNetCore` (v0.3.0-preview.3)

### Ejecución
```bash
cd MCP.Web.Server
dotnet run
```

---

## MCP.Web.Client

**Puerto por defecto**: `5194` (HTTP) / `7205` (HTTPS)

### Descripción
API cliente que orquesta la comunicación entre Azure OpenAI y el servidor MCP. Maneja el historial de chat persistente en Redis y expone endpoints REST para la interfaz web.

### Características
- Integración con Azure OpenAI usando `Azure.AI.OpenAI`
- Cliente MCP para comunicación con `MCP.Web.Server`
- Persistencia de historial de chat en Redis
- CORS configurado para Blazor WebAssembly
- Endpoint principal: `/chat/{chatId}/message`

### Configuración

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "AzureOpenAI": {
    "Endpoint": "https://tu-instancia.openai.azure.com/",
    "ApiKey": "tu-api-key-aqui",
    "Deployment": "gpt-4o"
  },
  "Mcp": {
    "ServerUrl": "http://localhost:5156"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Configuración de Azure OpenAI
1. **Endpoint**: URL de tu instancia de Azure OpenAI
2. **ApiKey**: Clave de acceso de Azure OpenAI
3. **Deployment**: Nombre del modelo desplegado (ej: gpt-4o, gpt-35-turbo)

#### Configuración de Redis
- **Redis**: Cadena de conexión a Redis (formato: `host:puerto`)

#### Configuración de MCP
- **ServerUrl**: URL del servidor MCP (`MCP.Web.Server`)

### Dependencias
- `Azure.AI.OpenAI` (v2.1.0)
- `Microsoft.Extensions.AI` (v9.7.1)
- `ModelContextProtocol` (v0.3.0-preview.3)
- `StackExchange.Redis` (v2.8.41)

### Ejecución
```bash
cd MCP.Web.Client
dotnet run
```

---

## MCP.Web.UI

**Puerto por defecto**: `5020` (HTTP) / `7020` (HTTPS)

### Descripción
Interfaz web desarrollada en Blazor WebAssembly que proporciona una experiencia de chat intuitiva para interactuar con el sistema MCP.

### Características
- Interfaz de chat moderna y responsiva
- Campo de entrada manual para Chat ID
- Visualización del historial de mensajes con roles (Usuario/Asistente)
- Indicadores visuales de estado (enviando, cargando, errores)
- Integración completa con `MCP.Web.Client`

### Configuración

La aplicación está configurada para consumir la API en `http://localhost:5194` (puerto del `MCP.Web.Client`).

Si necesitas cambiar la URL del backend, modifica el archivo `Program.cs`:

```csharp
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:5194") // Cambiar aquí si es necesario
});
```

### Dependencias
- `Microsoft.AspNetCore.Components.WebAssembly` (v9.0.4)
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer` (v9.0.4)

### Ejecución
```bash
cd MCP.Web.UI
dotnet run
```

---

## Orden de Ejecución

Para ejecutar toda la solución correctamente:

1. **Iniciar Redis**:
   ```bash
   docker run -d --name redis-mcp -p 6379:6379 redis/redis-stack:latest
   ```

2. **Iniciar MCP.Web.Server** (Terminal 1):
   ```bash
   cd MCP.Web.Server
   dotnet run
   ```

3. **Iniciar MCP.Web.Client** (Terminal 2):
   ```bash
   cd MCP.Web.Client
   dotnet run
   ```

4. **Iniciar MCP.Web.UI** (Terminal 3):
   ```bash
   cd MCP.Web.UI
   dotnet run
   ```

5. **Acceder a la aplicación**:
   - Interfaz web: `http://localhost:5020`
   - RedisInsight: `http://localhost:8001`

## URLs de la Solución

| Servicio | HTTP | HTTPS | Descripción |
|----------|------|--------|-------------|
| MCP.Web.UI | http://localhost:5020 | https://localhost:7020 | Interfaz web Blazor |
| MCP.Web.Client | http://localhost:5194 | https://localhost:7205 | API REST cliente |
| MCP.Web.Server | http://localhost:5156 | https://localhost:7082 | Servidor MCP |
| Redis | localhost:6379 | - | Base de datos Redis |
| RedisInsight | http://localhost:8001 | - | Interfaz de administración Redis |

## Uso de la Aplicación

1. Abrir la interfaz web en `http://localhost:5020`
2. Ingresar un **Chat ID único** en el campo correspondiente
3. Escribir mensajes en el chat
4. El historial se persiste automáticamente en Redis
5. El asistente responde utilizando Azure OpenAI con capacidades MCP

## Desarrollo

### Estructura del Proyecto
```
MCP-POC/
├── MCP.Web.Server/
│   ├── Tools/              # Herramientas MCP personalizadas
│   ├── Program.cs          # Configuración del servidor MCP
│   └── HeaderPropagationMiddleware.cs
├── MCP.Web.Client/
│   ├── Endpoints/          # Endpoints REST
│   ├── Services/           # Servicios (Redis, etc.)
│   └── Program.cs          # Configuración del cliente
└── MCP.Web.UI/
    ├── Pages/              # Páginas Blazor
    ├── Layout/             # Layouts de la aplicación
    ├── Services/           # Servicios del frontend
    └── Models/             # Modelos de datos
```

### Notas de Desarrollo
- **Framework**: .NET 9.0
- **Patrón**: Cliente-Servidor con MCP
- **Persistencia**: Redis para historial de chat
- **IA**: Azure OpenAI con extensiones MCP
- **Frontend**: Blazor WebAssembly
- **CORS**: Configurado para desarrollo local

## Troubleshooting

### Redis no se conecta
- Verificar que Docker esté ejecutándose
- Confirmar que el puerto 6379 esté disponible
- Revisar los logs: `docker logs redis-mcp`

### Error de Azure OpenAI
- Verificar que las credenciales sean correctas en `appsettings.json`
- Confirmar que el deployment exista en Azure
- Revisar los logs del `MCP.Web.Client`

### MCP.Web.Client no responde
- Verificar que `MCP.Web.Server` esté ejecutándose en el puerto 5156
- Confirmar la configuración de `Mcp:ServerUrl` en appsettings
- Revisar CORS si hay errores de conectividad desde Blazor
