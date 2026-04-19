using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Wef.McpRegistry.Models;

namespace Wef.McpRegistry.Activities;

public static class FilterMcpServersActivity
{
    [Function("FilterMcpServers")]
    public static List<McpServerRecord> Run(
        [ActivityTrigger] FilterRequest request,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("FilterMcpServers");
        var servers = request.Servers ?? new List<McpServerRecord>();
        var filter = request.Filter ?? new McpFilter();

        var includeNames = (filter.IncludeNames ?? new List<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var includePrefixes = (filter.IncludePrefixes ?? new List<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToList();
        var excludeNames = (filter.ExcludeNames ?? new List<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasIncludeCriteria = includeNames.Count > 0 || includePrefixes.Count > 0;

        var filtered = servers
            .Where(s => !excludeNames.Contains(s.Name))
            .Where(s => !hasIncludeCriteria || includeNames.Contains(s.Name) || includePrefixes.Any(p => s.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        logger.LogInformation("Filtered MCP servers from {before} down to {after}.", servers.Count, filtered.Count);
        return filtered;
    }
}
