namespace Wef.McpRegistry.Models;

public class EtlRunResult
{
    public int FetchedCount { get; set; }
    public int FilteredCount { get; set; }
    public int UpsertedCount { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}