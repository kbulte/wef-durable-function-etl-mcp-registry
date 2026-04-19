# Durable Function ETL from official opensource MCP Registry to downstream Registry 

![MCP registry project vision](assets/project-vision.png)

This project follows the vision of the Model Context Protocol registry project. It uses an ETL process created with an durable Azure function to pull MCP server metadata from the upstream registry to a custom subregistry. Using the published V1.0 OpenApi definition of the official MCP registry a curated list is saved in an Azure Cosmos DB, which s used as a custom downstream registry. This ETL process is created with a durable function to extract, transform and load:
1. Fetch MCP server data from the official opensource upstream MCP registry.
2. Filter the data by include/exclude rules.
3. Upsert curated records into Azure Cosmos DB.

## ETL Flow 🔄

The durable function orchestration executes three activities:
1. `FetchMcpServers`
2. `FilterMcpServers`
3. `LoadToCosmosDb`

The HTTP starter and a daily timer trigger can both start the orchestration.

## Trigger Details 

- 🌐 HTTP trigger endpoint: `/McpRegistryEtlOrchestrator_HttpStart` (POST JSON)
- 📅 Timer trigger schedule: daily at 5:00 PM UTC (configured in timer binding)

## Example HTTP Request Body 🧾

```json
{
  "updatedSince": "2026-04-01T00:00:00Z",
  "search": "",
  "includeDeleted": false,
  "filter": {
    "includeNames": ["io.github.devantler-tech/ksail"],
    "includePrefixes": ["ai.exa/"],
    "excludeNames": ["ai.shawndurrani/mcp-merchant"]
  }
}
```

## Local Development 💻

Before running the Functions host, start the following local dependencies used by the durable function.

### Required Configuration ⚙️

Set these app settings locally in `local.settings.json` and in Azure Function App configuration:

- `UPSTREAM_REGISTRY_BASE_URL`: https://registry.modelcontextprotocol.io
- `COSMOS_DB_CONNECTION_STRING`
- `COSMOS_DB_DATABASE_NAME`
- `COSMOS_DB_CONTAINER_NAME`

### 1) Azurite (for `AzureWebJobsStorage`) 

[Install VS Code extension for Azurite](https://marketplace.visualstudio.com/items?itemName=Azurite.azurite)

**Start Azurite** 

a) From the command palette:
![Start Azurite from the command palette](assets/start-azurite.png)

b) From the project root:

```bash
azurite --location .azurite --silent --debug .azurite/debug.log
```

### 2) Durable Task Scheduler (for `DURABLE_TASK_SCHEDULER_CONNECTION_STRING`) 


Pull the official Durable Task Scheduler container image from Microsoft.

```bash
docker pull mcr.microsoft.com/dts/dts-emulator:latest
```

Start the scheduler with default task hub on port 8080 and monitoring dashboard on 8082 (Docker example):

```bash
docker run -d -p 8082:8082 -p 8080:8080 mcr.microsoft.com/dts/dts-emulator:latest
```

The dashboard will be available at http://localhost:8082

![Dashboard](assets/dts-dashboard.png)

### 3) Azure Cosmos DB Emulator (for local Cosmos access) 

Download options:
- Install with winget: `winget install Microsoft.AzureCosmosEmulator`

Start the emulator (Windows):

```powershell
Start-Process "$env:ProgramFiles\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe"
```

The emulator defaults to the endpoint `https://localhost:8081`

![Cosmos DB emulator](assets/cosmosdb-emulator.png)

Add a container and database that match the 'COSMOS_DB_*' appsettings.

![Cosmos DB container and database](assets/cosmosdb-resources.png)

### 4) Run the durable function

Build 🛠️:

```bash
dotnet build /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
```

Run Functions host ▶️:

```bash
func start
```

## Durable Payload Size Note (1 MB Limit)

If `FetchMcpServers` returns the full upstream registry (thousands of servers), the orchestrator history can exceed Durable Function payload limits before filtering reduces the data.

Recommended options (from simplest to most robust):

1. Combine fetch and filter in a single activity so only filtered results are returned to the orchestrator.
2. Keep the current split activities, but persist raw fetched data outside orchestrator history (Blob/Cosmos) and pass references between activities.
3. Use a fan-out/fan-in design with pagination/chunking so each activity payload remains small.

## Example screens

![Durable function console output](assets/console-output.png)

![Durable Task Scheduler orchestration overview](assets/dts-orchestration-overview.png)

![Durable Task Scheduler orchestration detail](assets/dts-orchestration-detail.png)

## Resources
- Azure durable task scheduler: https://learn.microsoft.com/en-us/azure/durable-task/
- Azurite local storage emulator: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
- Azure Cosmos DB Emulator: https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator
- MCP official registry docs: https://github.com/modelcontextprotocol/registry
- Model Context Protocol official OpenApi definition: https://github.com/modelcontextprotocol/registry/blob/main/docs/reference/api/openapi.yaml


