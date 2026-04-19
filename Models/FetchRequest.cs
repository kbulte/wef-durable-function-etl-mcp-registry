namespace Wef.McpRegistry.Models;

public record FetchRequest(string? UpdatedSince, string? Search, bool IncludeDeleted);