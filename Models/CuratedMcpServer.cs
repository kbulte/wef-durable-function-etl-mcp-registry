using Newtonsoft.Json;

namespace Wef.McpRegistry.Models;

public class CuratedMcpServer
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "PartitionKey")]
    public string PartitionKey { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "version")]
    public string Version { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "title")]
    public string? Title { get; set; }
    [JsonProperty(PropertyName = "websiteUrl")]
    public string? WebsiteUrl { get; set; }
    [JsonProperty(PropertyName = "lastSyncedAtUtc")]
    public DateTime LastSyncedAtUtc { get; set; }

    public static CuratedMcpServer From(McpServerRecord source)
    {
        // PartitionKey is the part before the first '/'
        var partitionKey = source.Name?.Split('/')[0] ?? string.Empty;
        return new CuratedMcpServer
        {
            Id = $"{source.Name}:{source.Version}",
            PartitionKey = partitionKey,
            Name = source.Name,
            Description = source.Description,
            Version = source.Version,
            Title = source.Title,
            WebsiteUrl = source.WebsiteUrl,
            LastSyncedAtUtc = DateTime.UtcNow
        };
    }
}