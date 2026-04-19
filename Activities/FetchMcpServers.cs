using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Wef.McpRegistry.Models;

namespace Wef.McpRegistry.Activities;

public static class FetchMcpServersActivity
{
    [Function("FetchMcpServers")]
    public static async Task<List<McpServerRecord>> Run(
        [ActivityTrigger] FetchRequest request,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("FetchMcpServers");
        var upstreamRegistryBaseUrl = Environment.GetEnvironmentVariable("UPSTREAM_REGISTRY_BASE_URL");
        using var client = new HttpClient { BaseAddress = new Uri(upstreamRegistryBaseUrl) };

        var allServers = new List<McpServerRecord>();
        string? cursor = null;

        // Set the limit (default 100, or 3 for your test)
        int limit = 3; // You can make this configurable via FetchRequest if desired

        do
        {
            var query = new List<string> { "version=latest", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(cursor)) query.Add($"cursor={Uri.EscapeDataString(cursor)}");
            if (!string.IsNullOrWhiteSpace(request.UpdatedSince)) query.Add($"updated_since={Uri.EscapeDataString(request.UpdatedSince)}");
            if (!string.IsNullOrWhiteSpace(request.Search)) query.Add($"search={Uri.EscapeDataString(request.Search)}");
            if (request.IncludeDeleted) query.Add("include_deleted=true");

            var endpoint = $"/v0.1/servers?{string.Join("&", query)}";
            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ServerListResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload?.Servers is { Count: > 0 })
            {
                var newServers = payload.Servers.Where(s => s.Server is not null).Select(s => s.Server!).ToList();
                int spaceLeft = limit - allServers.Count;
                if (newServers.Count > spaceLeft)
                {
                    allServers.AddRange(newServers.Take(spaceLeft));
                }
                else
                {
                    allServers.AddRange(newServers);
                }
            }
            cursor = payload?.Metadata?.NextCursor;
        }
        while (!string.IsNullOrWhiteSpace(cursor) && allServers.Count < limit);

        logger.LogInformation("Fetched {count} MCP servers from upstream registry.", allServers.Count);
        return allServers;
    }
}
