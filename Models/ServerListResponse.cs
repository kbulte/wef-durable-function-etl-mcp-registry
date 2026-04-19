namespace Wef.McpRegistry.Models;

public class ServerListResponse
{
    public List<ServerEnvelope>? Servers { get; set; }
    public RegistryMetadata? Metadata { get; set; }
}