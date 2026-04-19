using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Wef.McpRegistry.Models;

namespace Wef.McpRegistry.Orchestration;

public static class McpRegistryEtlOrchestrator
{
    [Function(nameof(McpRegistryEtlOrchestrator))]
    public static async Task<EtlRunResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<EtlOrchestratorInput>() ?? new EtlOrchestratorInput();

        var fetchRequest = new FetchRequest(input.UpdatedSince, input.Search, input.IncludeDeleted);
        var fetchedServers = await context.CallActivityAsync<List<McpServerRecord>>("FetchMcpServers", fetchRequest);

        var filterRequest = new FilterRequest(fetchedServers, input.Filter);
        var filteredServers = await context.CallActivityAsync<List<McpServerRecord>>("FilterMcpServers", filterRequest);

        var loadRequest = new LoadRequest(filteredServers);
        var loadResult = await context.CallActivityAsync<LoadResult>("LoadToCosmosDb", loadRequest);

        return new EtlRunResult
        {
            FetchedCount = fetchedServers.Count,
            FilteredCount = filteredServers.Count,
            UpsertedCount = loadResult.UpsertedCount,
            StartedAtUtc = context.CurrentUtcDateTime,
            Message = $"Fetched {fetchedServers.Count}, filtered {filteredServers.Count}, upserted {loadResult.UpsertedCount}."
        };
    }

    [Function("McpRegistryEtlOrchestrator_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("McpRegistryEtlOrchestrator_HttpStart");

        var body = await req.ReadAsStringAsync();
        var input = !string.IsNullOrWhiteSpace(body)
            ? JsonSerializer.Deserialize<EtlOrchestratorInput>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new EtlOrchestratorInput()
            : new EtlOrchestratorInput();
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(McpRegistryEtlOrchestrator), input);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }

    [Function("McpRegistryEtlOrchestrator_TimerStart")]
    public static async Task TimerStart(
        [TimerTrigger("0 0 17 * * *")]
        TimerInfo timerInfo,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("McpRegistryEtlOrchestrator_TimerStart");

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(McpRegistryEtlOrchestrator),
            new EtlOrchestratorInput());

        logger.LogInformation(
            "Timer started orchestration with ID = '{instanceId}'. Next run at {nextRun}.",
            instanceId,
            timerInfo.ScheduleStatus?.Next);
    }
}
