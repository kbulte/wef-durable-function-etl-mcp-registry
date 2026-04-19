namespace Wef.McpRegistry.Models;

public record FilterRequest(List<McpServerRecord>? Servers, McpFilter? Filter);