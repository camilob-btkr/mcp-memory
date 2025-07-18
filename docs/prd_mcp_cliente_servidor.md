# 📝 Product Requirements Document (PRD)

## Proyecto: Exploración de LLMs mediante Integración con MCP (Model Context Protocol)

### Fecha: Julio 2025
### Versión: 1.0
### Responsable: Camilo Barreto

---

## 1. Objetivo del Proyecto

Diseñar e implementar un sistema basado en .NET 9 compuesto por dos componentes principales: un **cliente MCP expuesto como API REST** y un **servidor MCP**, ambos enfocados en explorar las capacidades de los Modelos de Lenguaje (LLMs) mediante integración con el estándar Model Context Protocol (MCP). El sistema busca mantener control total sobre la sesión de chat, el manejo de herramientas y el contexto conversacional, sin exponer el servidor MCP a clientes externos como Claude o Copilot.

---

## 2. Alcance del Proyecto

- Crear una API REST que actúe como cliente MCP para interactuar con el LLM y el servidor MCP.
- Implementar un servidor MCP capaz de exponer tools invocables mediante `CallToolAsync`.
- Utilizar Redis como capa de persistencia compartida para historial de chat y contexto de sesión MCP.
- Proveer una experiencia de chat personalizada y segura en la interfaz de usuario.
- Evitar la exposición directa del servidor MCP a agentes LLM externos.

---

## 3. Componentes

### 3.1 Cliente MCP (API REST en .NET 9)

#### Funcionalidad
- Exponer endpoint principal `POST /chat/{chatId}/message`.
- Cargar el historial conversacional desde Redis (`chat:history:{chatId}`).
- Generar el prompt de entrada para el LLM utilizando el historial acumulado.
- Enviar mensajes al modelo LLM (OpenAI/Azure) y procesar la respuesta.
- Detectar llamadas a herramientas (`tool_calls`) en la respuesta del modelo.
- Invocar herramientas a través del cliente MCP (`CallToolAsync`) reutilizando el `mcpSessionId` vinculado al `chatId`.
- Reinyectar resultados de herramientas al historial y reenviar al LLM para continuar la conversación.

#### Requisitos técnicos
- Framework: .NET 9
- Almacenamiento de contexto: Redis (StackExchange.Redis)
- Autenticación: No requerida en esta fase
- Historial: serializado como lista JSON con roles (`user`, `assistant`, `tool`, `system`)
- Manejo de `mcpSessionId`: Redis key `mcp:session:{chatId}`
- SDK MCP: `ModelContextProtocol` NuGet Package
- Comunicación LLM: `Microsoft.Extensions.AI.OpenAI`

#### Comportamiento esperado
- Si `chatId` no existe, se genera uno nuevo.
- Si `mcpSessionId` no existe, se crea y se vincula a `chatId`.
- Cada mensaje nuevo se almacena en Redis con TTL (ej: 2h).
- Se limita el historial reenviado al LLM (recorte a últimos 15-20 mensajes).

---

### 3.2 Servidor MCP (Tools Backend en .NET 9)

#### Funcionalidad
- Implementar herramientas invocables mediante protocolo MCP (`CallToolAsync`).
- Exponer herramientas como `buscarPedido`, `consultarInventario`, etc.
- Utilizar Redis como almacenamiento de contexto por sesión (`mcp:session:{sessionId}`).
- Permitir persistencia de datos sensibles (tokens, cadenas de conexión, variables) dentro del contexto.

#### Requisitos técnicos
- Framework: .NET 9
- Protocolo MCP: JSON-RPC 2.0
- Transporte: HTTP y/o STDIO
- SDK MCP: `ModelContextProtocol.AspNetCore`
- Almacenamiento de estado de sesión: Redis con TTL configurable (ej: 30-120 min)
- Seguridad: únicamente accesible desde el cliente MCP oficial, no expuesto a internet

#### Comportamiento esperado
- Cada `mcpSessionId` representa una sesión tool-context aislada.
- El estado se consulta y actualiza desde Redis en cada invocación.
- Las herramientas pueden fallar de forma segura si el contexto no está inicializado.
- No se expone ninguna información sensible al LLM.

---

## 4. Flujos de trabajo clave

### Flujo: procesamiento de mensaje del usuario
1. El frontend envía un mensaje al endpoint REST `/chat/{chatId}/message`.
2. Se carga el historial del chat y el `mcpSessionId` asociado.
3. Se arma el prompt completo y se envía al LLM.
4. Si el LLM responde con una tool, el cliente MCP la ejecuta.
5. La respuesta de la tool se guarda como mensaje `tool`.
6. Se reenvía el historial actualizado al LLM.
7. Se retorna la respuesta final al usuario.

---

## 5. Casos de uso

### Caso de uso 1: Inicio de conversación
- **Actor**: Usuario anónimo desde la UI
- **Precondición**: No existe `chatId`
- **Escenario**:
  1. La UI genera un nuevo `chatId`.
  2. Se envía el primer mensaje al endpoint REST con ese ID.
  3. El API genera un `mcpSessionId`, lo vincula al `chatId`, y almacena ambos en Redis.
  4. El historial se inicializa con el mensaje del usuario.
  5. Se envía al LLM y se devuelve la primera respuesta.

### Caso de uso 2: Mensaje con llamada a herramienta
- **Actor**: Usuario activo
- **Precondición**: `chatId` y `mcpSessionId` existen
- **Escenario**:
  1. Usuario envía mensaje desde la UI.
  2. El API REST consulta Redis y reconstruye el historial.
  3. Se detecta que el LLM solicita una función/tool.
  4. El cliente MCP realiza `CallToolAsync` con el contexto de sesión MCP.
  5. El resultado se inserta como mensaje de tipo `tool`.
  6. El historial se actualiza y se vuelve a enviar al LLM.
  7. Se devuelve la respuesta final al usuario.

### Caso de uso 3: Persistencia y recuperación de sesión distribuida
- **Actor**: Sistema
- **Precondición**: Redis está disponible y contiene datos válidos
- **Escenario**:
  1. Una instancia nueva del API REST recibe un mensaje con `chatId` existente.
  2. Carga el historial y `mcpSessionId` desde Redis.
  3. Continúa la conversación con el LLM sin pérdida de contexto.
  4. Las tools siguen accediendo a los datos correctos mediante el servidor MCP.

### Caso de uso 4: Expiración de sesiones
- **Actor**: Sistema / usuario pasivo
- **Precondición**: Pasó el TTL sin actividad
- **Escenario**:
  1. Redis elimina el `mcpSessionId` y/o historial por inactividad.
  2. Usuario intenta enviar un nuevo mensaje.
  3. El API REST detecta la falta de sesión y crea una nueva.
  4. El historial se reinicia.
  5. El usuario continúa sin interrupción aparente, aunque se perdió el contexto previo.

---

## 6. Consideraciones de seguridad
- El `mcpSessionId` solo se utiliza entre el cliente y el servidor MCP; nunca se expone al frontend o al LLM.
- Las cadenas de conexión, tokens u otros datos sensibles pueden residir en el contexto del servidor MCP, cifrados si es necesario.
- El acceso al servidor MCP se debe restringir a una red interna o mediante token compartido.

---

## 7. Escalabilidad y persistencia
- Redis será el punto central para almacenar estado compartido.
- Permite que el sistema escale horizontalmente sin perder contexto.
- El TTL de cada sesión puede controlarse para evitar saturación de memoria.

---

## 8. Criterios de éxito
- Las herramientas son invocadas correctamente por el LLM a través del cliente MCP.
- El contexto conversacional se mantiene entre mensajes (inclusive en ambientes distribuidos).
- Las sesiones MCP conservan el estado necesario para la ejecución de herramientas.
- La arquitectura permite modificar prompts y controlar respuestas desde el backend.

---

## 9. Futuras extensiones
- Autenticación de usuarios con `userId` y control de permisos sobre tools.
- Múltiples servidores MCP especializados por dominio (por ejemplo, inventario, ventas, logística).
- Agentes de razonamiento que combinen herramientas y contexto.
- Interfaz visual para monitorear llamadas a tools y respuestas generadas.

---

## 10. Glosario
- **LLM**: Large Language Model
- **MCP**: Model Context Protocol
- **Tool**: Funciones que el LLM puede invocar mediante protocolo MCP
- **chatId**: Identificador de la conversación (persistente)
- **mcpSessionId**: Identificador de sesión para herramientas en el servidor MCP
- **Redis**: Almacenamiento clave-valor en memoria para sesiones e historial

