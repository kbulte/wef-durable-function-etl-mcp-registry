namespace Wef.McpRegistry.Models;

/// <summary>
/// Input contract for the ETL orchestrator. Specifies criteria for fetching, filtering, and loading MCP servers from the ModelContext registry.
/// </summary>
public class EtlOrchestratorInput
{
    /// <summary>
    /// Optional ISO 8601 timestamp. If set, only servers updated since this date/time are fetched from the registry.
    /// Example: "2026-04-01T00:00:00Z"
    /// </summary>
    public string? UpdatedSince { get; set; }

    /// <summary>
    /// Optional search string to filter servers by name or description in the upstream registry.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// If true, include servers marked as deleted in the results. If false, exclude deleted servers.
    /// </summary>
    public bool IncludeDeleted { get; set; }

    /// <summary>
    /// Optional filter rules for including or excluding specific servers by name or prefix.
    /// </summary>
    public McpFilter? Filter { get; set; }
}