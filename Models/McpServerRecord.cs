namespace Wef.McpRegistry.Models;

public class McpServerRecord
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? WebsiteUrl { get; set; }
}