# Specification for Blazor Chat Client — Version 2

> Basado en la plantilla `spec_template.example.md`

---

## High-Level Objective
Extender la interfaz Blazor WASM para **recibir respuestas fragmentadas** en tiempo real usando SignalR.

## Mid-Level Objectives
- Agregar un proyecto de SignalR Hub en el backend.
- Mapear `/chatHub` y métodos `JoinChat(string chatId)` y `LeaveChat`.
- Instalar `Microsoft.AspNetCore.SignalR.Client` en el cliente Blazor.
- Conectarse al Hub y unirse al grupo `chatId`.
- Cambiar el flujo de `SendMessage` para invocar el hub y luego escuchar eventos `ReceiveMessage`.
- Mostrar mensajes en el historial conforme llegan en streaming.

## Implementation Notes
- **Backend**: Agregar `ChatHub : Hub`, registrar en `Program.cs` con `AddSignalR` y `MapHub<ChatHub>("/chatHub")`.  
- **Cliente**: Instalar paquete SignalR Client, usar `HubConnectionBuilder` en `Index.razor.cs`.  
- **UI**: Reemplazar fetch HTTP por suscripciones a eventos del Hub.  
- **Reutilizar** los mismos componentes Razor de la Versión 1.

## Context
**Beginning context**  
- Existe el proyecto Blazor WASM con fetch HTTP.  
- No hay SignalR configurado.

**Ending context**  
- SignalR Hub corriendo en el backend.  
- Blazor WASM conectado al Hub y recibiendo mensajes en tiempo real.

## Low-Level Tasks
1. **Agregar paquete SignalR Server**  
   ```aider
   dotnet add package Microsoft.AspNetCore.SignalR
   ```
2. **Crear `ChatHub`**  
   ```aider
   Clase ChatHub con métodos `JoinChat` y `LeaveChat`.
   ```
3. **Registrar SignalR**  
   ```aider
   En `Program.cs`: `builder.Services.AddSignalR()` y `app.MapHub<ChatHub>("/chatHub")`.
   ```
4. **Instalar SignalR Client**  
   ```aider
   dotnet add package Microsoft.AspNetCore.SignalR.Client en McpChat.Client.
   ```
5. **Configurar conexión en `Index.razor`**  
   ```aider
   Crear `HubConnection`, `StartAsync`, `InvokeAsync("JoinChat", chatId)`, `On("ReceiveMessage", ...)`.
   ```
6. **Actualizar UI**  
   ```aider
   En lugar de fetch, escuchar eventos y `appendMessage` en callback.
   ```
