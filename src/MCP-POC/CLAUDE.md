# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Model Context Protocol (MCP) proof-of-concept implementing a client-server architecture where:
- **MCP Server** exposes tools/functions callable via HTTP/SSE transport
- **MCP Client** integrates Azure OpenAI with MCP tools for AI-powered chat functionality
- **Chat API** maintains conversation history in Redis and streams AI responses

## Essential Commands

### Build Commands
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build MCP.Web.Server
dotnet build MCP.Web.Client

# Clean and rebuild
dotnet clean && dotnet build
```

### Run Commands
```bash
# Run both projects (required for full functionality)
# Terminal 1 - MCP Server (ports 5156/7082)
cd MCP.Web.Server && dotnet run

# Terminal 2 - MCP Client (ports 5194/7205)  
cd MCP.Web.Client && dotnet run

# Run with specific launch profile
dotnet run --launch-profile https
```

### Development Commands
```bash
# Restore packages
dotnet restore

# Run with hot reload
dotnet watch run

# Format code
dotnet format

# Check for outdated packages
dotnet list package --outdated
```

### Testing
No test projects currently exist. When adding tests:
```bash
# Create test project
dotnet new xunit -n MCP.Tests

# Run tests
dotnet test
```

## Architecture & Key Components

### MCP.Web.Server (Port 5156/7082)
- **Program.cs**: Configures MCP server with HTTP transport at `/mcp`
- **Tools/MockTools.cs**: MCP tool implementations (GetFechaHoraActualUtc, Saludar, ObtenerEncabezados)
- **HeaderPropagationMiddleware.cs**: Forwards Authorization, X-Request-Id, X-My-Header between services

### MCP.Web.Client (Port 5194/7205)  
- **Program.cs**: Integrates Azure OpenAI, Redis, and MCP client
- **Endpoints/ChatEndpoints.cs**: POST `/chat/{chatId}/message` - Main chat endpoint
- **Services/RedisChatHistoryStore.cs**: Chat persistence with 2-hour TTL

### Key Integration Points
1. **Azure OpenAI**: Configured in appsettings.json (Endpoint, Key, DeploymentName)
2. **Redis**: Connection string for chat history storage
3. **MCP Server URL**: Client discovers and invokes server tools dynamically
4. **Header Propagation**: Authentication and tracking headers flow between services

## Configuration

Both projects use appsettings.json (gitignored). Required settings:

### MCP.Web.Client
```json
{
  "OpenAI": {
    "Endpoint": "https://your-instance.openai.azure.com/",
    "Key": "your-api-key",
    "DeploymentName": "your-deployment"
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "McpServerUrl": "https://localhost:7082"
}
```

### MCP.Web.Server
```json
{
  "Logging": { ... },
  "AllowedHosts": "*"
}
```

## Development Workflow

1. **Start Infrastructure**: Ensure Redis is running locally
2. **Configure Settings**: Create appsettings.json files with required values
3. **Run Server First**: MCP.Web.Server must be running for client to discover tools
4. **Run Client**: MCP.Web.Client provides the chat API
5. **Test Chat**: POST to `/chat/{chatId}/message` with JSON body: `{"message": "your message"}`

## Important Patterns

### Tool Integration
- Server exposes tools via MCP protocol at `/mcp` endpoint
- Client dynamically discovers available tools on startup
- AI can invoke tools based on user requests

### Streaming Responses  
- Chat responses use Server-Sent Events (SSE) for real-time streaming
- Content-Type: `text/event-stream` for chat responses

### Session Management
- Each chat has unique ID (provided by client)
- History persisted in Redis with automatic expiration
- Sessions isolated by chatId

## Current Development Focus

Based on git status and specs:
- UI implementation planned: Blazor WebAssembly chat interface
- Specification-driven development using structured templates
- Maintaining clean separation between server, client, and future UI layers

## Common Tasks

### Add New MCP Tool
1. Add method to `Tools/MockTools.cs` with `[Tool]` attribute
2. Restart server - tools auto-discovered by client

### Modify Chat Behavior
1. Edit `ChatEndpoints.cs` for endpoint logic
2. Adjust `RedisChatHistoryStore.cs` for persistence changes

### Debug Integration
1. Check server logs for tool registration
2. Verify client logs for tool discovery
3. Monitor Redis for chat history storage
4. Use browser dev tools for SSE stream inspection