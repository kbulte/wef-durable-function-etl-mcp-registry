# GitHub Copilot Instructions for Durable Function ETL MCP Registry

## Project Overview
This project implements an ETL pipeline using Azure Durable Functions to extract MCP servers from the official registry, filter them, and load them into Azure Cosmos DB.

## C# Code Standards

### File Organization
- **One type per file**: Every class, record, struct, interface, and enum must be in its own `.cs` file.
- **File naming**: Match the type name exactly (e.g., `McpServerRecord.cs` for class `McpServerRecord`).
- **Namespace**: All types use `namespace Wef.McpRegistry;`.

### Naming Conventions
- **Classes**: PascalCase (e.g., `McpServerRecord`, `CuratedMcpServer`)
- **Records**: PascalCase (e.g., `FetchRequest`, `LoadRequest`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `IRepository`)
- **Methods**: PascalCase (e.g., `FetchMcpServers`, `FilterMcpServers`)
- **Properties**: PascalCase (e.g., `Name`, `Description`, `UpdatedSince`)
- **Local variables**: camelCase (e.g., `allServers`, `filteredServers`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `UPSTREAM_REGISTRY_BASE_URL`)

### Nullable Reference Types
- Always use C# 8+ nullable reference types.
- Use `?` for nullable properties and parameters.
- Prefer `??` and `??=` operators for null coalescing.

## Azure Functions Best Practices

### Activity Functions
- Deterministic and idempotent (can be safely replayed).
- No side effects beyond the intended activity outcome.
- Log input/output to aid debugging and replay tracking.
- Return serializable types (POCOs, records).

### Orchestration Functions
- Keep logic simple and delegate work to activities.
- Use `context.CallActivityAsync<T>()` for activity invocation.
- Never use `DateTime.Now`; always use `context.CurrentUtcDateTime`.
- Return structured results (e.g., `EtlRunResult`).

### Triggers
- HTTP triggers: `/McpRegistryEtlOrchestrator_HttpStart` (HTTP POST with JSON body).
- Timer triggers: Cron expression in binding directly (e.g., `"0 0 17 * * *"` for 5 PM UTC daily).
- Handle JSON deserialization with `req.ReadFromJsonAsync<T>()`.

### Configuration
- Read settings via `Environment.GetEnvironmentVariable()` for app configuration.
- Never hardcode sensitive values (connection strings, keys, tokens).
- Document all required settings in `local.settings.json` with empty placeholder values.
- Settings in production are managed via Azure Key Vault or App Service Configuration.

## Project-Specific Conventions

### ETL Pipeline Flow
1. **FetchMcpServers**: Fetches servers from upstream registry (paginated, latest version).
2. **FilterMcpServers**: Filters servers by include/exclude rules.
3. **LoadToCosmosDb**: Upserts filtered servers to Cosmos DB.

### Request/Response Records
- Use records for request and response payload types.
- Example: `public record FetchRequest(string? UpdatedSince, string? Search, bool IncludeDeleted);`

### Configuration Keys
- `UPSTREAM_REGISTRY_BASE_URL`: Upstream MCP registry endpoint.
- `COSMOS_DB_CONNECTION_STRING`: Cosmos DB connection string.
- `COSMOS_DB_DATABASE_NAME`: Cosmos DB database name.
- `COSMOS_DB_CONTAINER_NAME`: Cosmos DB container name.

### Logging
- Use structured logging with parameterized messages.
- Example: `logger.LogInformation("Fetched {count} MCP servers.", servers.Count);`
- Always log activity start/completion and error conditions.

## Git and Commits
- Keep commits focused and atomic.
- Use clear, concise commit messages.
- Always build before committing: `dotnet build`.

## Testing and Validation
- Build must pass: `dotnet build /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary`.
- Test locally with `func start` or via the VS Code Azure Functions extension.
- Mock external calls (HTTP, Cosmos) in unit tests.

## Additional Guidelines
- Use async/await for all I/O-bound operations.
- Dispose of resources properly (`using` statements).
- Validate input contracts early in functions.
- Provide clear error messages with context.
