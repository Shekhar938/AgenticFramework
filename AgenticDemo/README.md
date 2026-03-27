# AgenticDemo (.NET 8 + Semantic Kernel + Agent Framework + MCP)

A production-style demo API that showcases Microsoft’s agentic ecosystem:

- **Semantic Kernel** orchestration and plugin/tool-calling
- **Microsoft Agent Framework** (`ChatCompletionAgent`) for agent behavior
- **MCP (Model Context Protocol)** dynamic tool ingestion
- **Azure OpenAI** first, **OpenAI** fallback
- Clean architecture with modular services and infrastructure

## Architecture

```text
Client
  |
  v
Api (Controllers + Middleware)
  |
  v
Application (Agent orchestration service)
  |
  v
Infrastructure (Kernel + Agent factory + local plugins)
  |
  +--> MCP client (external tools)
  |
  +--> Azure OpenAI / OpenAI model
```

### Flow (plan -> act -> complete)
1. API receives prompt (`POST /api/agent/run`).
2. `AgentOrchestrationService` creates a `ChatCompletionAgent`.
3. MCP tools are fetched dynamically and registered into the kernel.
4. Agent reasons over the prompt and auto-selects tools via `FunctionChoiceBehavior.Auto()`.
5. Local plugins (`WeatherPlugin`, `EmailPlugin`, `ActionHistoryPlugin`) and MCP tools are available for execution.
6. Final result + intermediate steps are returned.

## Project Structure

```text
/AgenticDemo
 ├── Api/
 │    ├── Controllers/
 │    ├── Middleware/
 │    └── Program.cs
 ├── Application/
 │    ├── Services/
 │    ├── Agents/
 │    └── Interfaces/
 ├── Infrastructure/
 │    ├── AI/
 │    │    ├── KernelBuilder.cs
 │    │    └── AgentFactory.cs
 │    └── Plugins/
 │         ├── WeatherPlugin.cs
 │         ├── EmailPlugin.cs
 │         └── ActionHistoryPlugin.cs
 ├── MCP/
 │    └── McpClientService.cs
 ├── Domain/
 │    └── Models.cs
 └── README.md
```

## Configuration

Set **either Azure OpenAI** OR **OpenAI** variables.

### Azure OpenAI (preferred)

```bash
AZURE_OPENAI_ENDPOINT=https://<resource-name>.openai.azure.com/
AZURE_OPENAI_API_KEY=<key>
AZURE_OPENAI_DEPLOYMENT=<deployment-name>
```

### OpenAI fallback

```bash
OPENAI_API_KEY=<key>
OPENAI_MODEL=gpt-4o-mini
```

### MCP

```bash
Mcp__BaseUrl=http://localhost:3001
```

`Mcp__BaseUrl` should point to an MCP bridge endpoint exposing:
- `GET /tools`
- `POST /invoke`

## Build & Run

```bash
dotnet restore AgenticDemo/Api/Api.csproj
dotnet build AgenticDemo/Api/Api.csproj
dotnet run --project AgenticDemo/Api/Api.csproj
```

Swagger UI is available in development mode.

## API

### Run Agent
`POST /api/agent/run`

Request:
```json
{
  "prompt": "Send weather of Delhi to my email"
}
```

Sample response:
```json
{
  "result": "Email sent with weather report",
  "steps": [
    "I will get the weather",
    "I will send the email",
    "Done"
  ]
}
```

## Demo Scenarios

Try prompts:
1. `What is weather in Patna?`
2. `Send weather of Delhi to my email at ops@example.com`
3. `Summarize last 2 actions`
4. `Use external tool to fetch latest compliance note`

## Key Components

- **Agent**: `ChatCompletionAgent` with explicit instructions and automatic function selection.
- **Plugins**:
  - `WeatherPlugin`: weather lookup (demo data).
  - `EmailPlugin`: email dispatch simulation.
  - `ActionHistoryPlugin`: short-term memory for recent actions.
- **MCP**: external tools discovered at runtime and added as kernel functions.
- **Observability**: request/response middleware + plugin/service logging.

## Notes

- This is intentionally production-style in architecture and extensibility.
- Replace plugin internals (weather/email) with real service integrations for production.
