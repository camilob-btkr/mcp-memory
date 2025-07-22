# Especificación de Interfaz Gráfica de Chat MCP

> Basado en la plantilla `spec_template.example.md` y adaptado para el proyecto MCP-POC existente

---

## High-Level Objective
Construir una **interfaz web de chat** en Blazor WebAssembly que permita a los usuarios interactuar con el sistema MCP a través del endpoint `/chat/{chatId}/message` del proyecto `MCP.Web.Client` existente.

## Mid-Level Objectives
- Crear un proyecto Blazor WebAssembly (.NET 9) como parte de la solución MCP-POC.
- Implementar una interfaz de chat completa con:
  - **Campo de entrada manual para Chat ID** (el usuario debe ingresar un identificador único).
  - Área de visualización del historial de mensajes con roles (Usuario/Asistente).
  - Input para enviar mensajes y recibir respuestas del asistente MCP.
  - Indicadores visuales de estado (enviando, cargando, errores).
- Integrar con el backend `MCP.Web.Client` existente mediante `HttpClient`.
- Mostrar capacidades MCP como llamadas a herramientas y respuestas estructuradas.

## Implementation Notes
- **Framework**: Blazor WebAssembly (.NET 9)
- **Proyecto**: `MCP.Web.UI` como nuevo proyecto en la solución existente **al mismo nivel** que `MCP.Web.Client` y `MCP.Web.Server`
- **Estructura**: El proyecto debe estar en `MCP-POC/MCP.Web.UI/` (misma estructura que los proyectos existentes)
- **Backend**: Consumir `MCP.Web.Client` que ya maneja Azure OpenAI, Redis y MCP
- **Puerto**: Configurar para que el frontend apunte al puerto del `MCP.Web.Client` (probablemente 5000/5001)
- **UI**: Componentes Razor con CSS moderno (puede usar Bootstrap que viene por defecto)
- **Estado**: Gestionar estado local del chat en memoria del navegador
- **Persistencia**: El historial se persiste automáticamente en Redis a través del backend
- **Chat ID**: **Campo de entrada manual** - el usuario debe escribir un identificador único para la sesión
- **⚠️ IMPORTANTE**: **NO modificar proyectos existentes** (`MCP.Web.Client`, `MCP.Web.Server`) - solo crear el nuevo proyecto frontend

## Context

### Beginning context
- Proyecto `MCP.Web.Client` funcionando con endpoint `/chat/{chatId}/message` (**NO TOCAR**)
- Proyecto `MCP.Web.Server` con herramientas MCP mock (**NO TOCAR**)
- Redis configurado para persistencia de historial
- Azure OpenAI integrado
- Solución `MCP-POC.sln` existente
- **Estructura actual**: `MCP-POC/MCP.Web.Client/` y `MCP-POC/MCP.Web.Server/`

### Ending context  
- **SOLO** nuevo proyecto `MCP.Web.UI` agregado a la solución en `MCP-POC/MCP.Web.UI/`
- **Estructura final**: `MCP-POC/MCP.Web.Client/`, `MCP-POC/MCP.Web.Server/`, `MCP-POC/MCP.Web.UI/`
- Interfaz web funcional para realizar sesiones de chat
- Usuario puede crear/continuar conversaciones con ChatID manual
- Visualización completa del historial de mensajes
- Integración transparente con el sistema MCP existente **sin modificar el backend**

## Low-Level Tasks
> Ordenados desde el inicio hasta el final - **SOLO crear nuevo proyecto, NO modificar existentes**

1. **Crear proyecto Blazor WASM en la solución**
   ```aider
   Crear proyecto Blazor WebAssembly llamado MCP.Web.UI en .NET 9 dentro de MCP-POC/MCP.Web.UI/, agregarlo a MCP-POC.sln
   FILE: CREAR MCP-POC/MCP.Web.UI/MCP.Web.UI.csproj
   FUNCTION: Configurar proyecto básico de Blazor WASM al mismo nivel que MCP.Web.Client y MCP.Web.Server
   DETAILS: Usar plantilla estándar de Blazor WASM, configurar TargetFramework net9.0, estructura de carpetas estándar
   ```

2. **Configurar Program.cs del cliente Blazor**
   ```aider
   Configurar DI para HttpClient apuntando al backend MCP.Web.Client existente
   FILE: CREAR MCP-POC/MCP.Web.UI/Program.cs  
   FUNCTION: Configurar HttpClient.BaseAddress y servicios
   DETAILS: BaseAddress debe apuntar a https://localhost:5001 (puerto del MCP.Web.Client EXISTENTE)
   ```

3. **Crear servicio de chat para comunicación con backend**
   ```aider
   Implementar servicio que encapsule llamadas HTTP al endpoint de chat EXISTENTE
   FILE: CREAR MCP-POC/MCP.Web.UI/Services/ChatService.cs
   FUNCTION: CREAR ChatService con método SendMessageAsync
   DETAILS: Manejar POST a /chat/{chatId}/message del MCP.Web.Client EXISTENTE, serialización JSON, manejo de errores
   ```

4. **Desarrollar componente principal de chat**
   ```aider
   Implementar interfaz de chat con input manual para ChatID, historial y envío de mensajes
   FILE: CREAR MCP-POC/MCP.Web.UI/Pages/Index.razor
   FUNCTION: CREAR componente de chat completo
   DETAILS: Campo de texto para ChatID manual, validación requerida, lista de mensajes, input con botón enviar, binding bidireccional
   ```

5. **Implementar modelo de datos del chat**
   ```aider
   Crear clases para representar mensajes y estado del chat en el frontend
   FILE: CREAR MCP-POC/MCP.Web.UI/Models/ChatModels.cs
   FUNCTION: CREAR ChatMessage, ChatSession, ChatState
   DETAILS: Propiedades Role, Content, Timestamp, IsLoading, ErrorMessage, validación de ChatID - SOLO para UI
   ```

6. **Agregar estilos CSS para la interfaz de chat**
   ```aider
   Estilizar componentes de chat con CSS moderno y responsivo
   FILE: CREAR/ACTUALIZAR MCP-POC/MCP.Web.UI/wwwroot/css/app.css
   FUNCTION: CREAR estilos para .chat-container, .message-user, .message-assistant, .chat-id-input
   DETAILS: Burbujas de chat diferenciadas, scroll automático, estados de carga, estilo para campo ChatID
   ```

7. **Implementar manejo de estados y errores**
   ```aider
   Agregar indicadores visuales de carga, errores y validaciones en el frontend
   FILE: ACTUALIZAR MCP-POC/MCP.Web.UI/Pages/Index.razor
   FUNCTION: ACTUALIZAR manejo de estados async y UI feedback
   DETAILS: Spinner de carga, mensajes de error, validación de ChatID obligatorio, disabled states
   ```

8. **Configurar la solución para desarrollo conjunto**
   ```aider
   Configurar profiles de launchSettings para el nuevo proyecto frontend
   FILE: CREAR MCP-POC/MCP.Web.UI/Properties/launchSettings.json
   FUNCTION: CONFIGURAR perfiles de desarrollo
   DETAILS: Puerto diferente al backend EXISTENTE, configuración HTTPS, variables de entorno
   ```
