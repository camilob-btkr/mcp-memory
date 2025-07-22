# Specification for Blazor Chat Client — Version 1

> Basado en la plantilla `spec_template.example.md`

---

## High-Level Objective
Construir una **interfaz de chat mínima** en Blazor WebAssembly que consuma el endpoint `/chat/{chatId}/message` de nuestro cliente MCP.

## Mid-Level Objectives
- Crear un proyecto Blazor WebAssembly (.NET 9).
- Implementar una página única con:
  - Campo para **Chat ID**.
  - Área para el historial de mensajes.
  - Input y botón para enviar un mensaje.
- Consumir el endpoint REST con `HttpClient` y mostrar la respuesta completa.
- Gestionar estados de carga y errores básicos.

## Implementation Notes
- **Framework**: Blazor WebAssembly (.NET 9).  
- **Proyecto**: `McpChat.Client` separado del servidor.  
- **UI**: Componentes Razor sencillos (no se requiere librería adicional).  
- **HTTP**: Usar `HttpClient` inyectado desde DI.  
- **Estilo**: CSS básico o Tailwind (opcional).

## Context
**Beginning context**  
- Existe el backend Minimal API y MCP Client en ASP.NET.  
- Aún no hay ningún proyecto de frontend.

**Ending context**  
- Carpeta `McpChat.Client` con un proyecto Blazor WASM funcionando.  
- Página `Index.razor` que actúa como chat, enviando y recibiendo mensajes.

## Low-Level Tasks
1. **Crear proyecto Blazor WASM**  
   ```aider
   Crear un proyecto Blazor WASM en .NET 9 llamado McpChat.Client
   ```
2. **Configurar `Program.cs`**  
   ```aider
   Inyectar `HttpClient` apuntando al mismo host, configurar BaseAddress
   ```
3. **Desarrollar componente de chat**  
   ```aider
   En `Index.razor`, añadir campo Chat ID, div historial, input + botón, bind de datos. 
   Detectar evento click y llamar a método `SendMessage`.
   ```
4. **Implementar método `SendMessage`**  
   ```aider
   Inyectar `HttpClient`, llamar a `POST /chat/{chatId}/message`, recibir string, actualizar historial.
   ```
5. **Manejar estados UI**  
   ```aider
   Añadir indicadores de carga y manejo de excepciones con mensaje en UI.
   ```
