namespace Wef.McpRegistry.Models;

public class McpFilter
{
    public List<string>? IncludeNames { get; set; }
    public List<string>? IncludePrefixes { get; set; }
    public List<string>? ExcludeNames { get; set; }
}