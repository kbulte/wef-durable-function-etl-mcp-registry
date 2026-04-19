using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Wef.McpRegistry.Models;

namespace Wef.McpRegistry.Activities;

public static class LoadToCosmosDbActivity
{
    [Function("LoadToCosmosDb")]
    public static async Task<LoadResult> Run(
        [ActivityTrigger] LoadRequest request,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("LoadToCosmosDb");
        var servers = request.Servers ?? new List<McpServerRecord>();
        if (servers.Count == 0)
        {
            logger.LogInformation("No servers to load into Cosmos DB.");
            return new LoadResult { UpsertedCount = 0 };
        }
        var connectionString = Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING");
        var databaseName = Environment.GetEnvironmentVariable("COSMOS_DB_DATABASE_NAME");
        var containerName = Environment.GetEnvironmentVariable("COSMOS_DB_CONTAINER_NAME");
        using var cosmosClient = new CosmosClient(connectionString);
        var container = cosmosClient.GetContainer(databaseName, containerName);
        var upserted = 0;
        foreach (var server in servers)
        {
            var curatedItem = CuratedMcpServer.From(server);
            await container.UpsertItemAsync(curatedItem, new PartitionKey(curatedItem.PartitionKey));
            upserted++;
        }
        logger.LogInformation("Upserted {count} MCP servers into Cosmos DB container {database}/{container}.", upserted, databaseName, containerName);
        return new LoadResult { UpsertedCount = upserted };
    }
}
