using System.Text.Json.Serialization;
using EGate.GCP.Helper;

namespace EGate.GCP.Model;

public sealed record MapMeta
{
    // 0
    // on elin server stored as files/XX/id.z
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    // 1 normal id
    // 2
    [JsonPropertyName("author")]
    public required string Author { get; init; }

    // 3
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    // 4
    [JsonPropertyName("language")]
    public string? Lang { get; init; }

    // 5
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    // 6
    [JsonPropertyName("created_at")]
    public required string Date { get; init; }

    // 7 - IP - redacted
    // 8
    [JsonPropertyName("version")]
    public required int Version { get; init; }

    // 9
    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    // stats
    [JsonPropertyName("visit_count")]
    public int VisitCount { get; init; }

    [JsonPropertyName("rating_count")]
    public int RatingCount { get; init; }

    [JsonPropertyName("rating_average")]
    public float RatingAverage { get; init; }

    // d1 -> r2
    [JsonPropertyName("file_key")]
    public string? FileKey { get; init; }

    [JsonPropertyName("file_size")]
    public int FileSize { get; init; }

    [JsonPropertyName("preview_key")]
    public string? PreviewKey { get; init; }

    // from DownloadMeta
    public static MapMeta FromDownloadMeta(DownloadMeta meta)
    {
        return new() {
            Id = $"{meta.Name}/{meta.Title}",
            Author = meta.Name,
            Title = meta.Title,
            Lang = meta.Path.ExtractInBetween("/", "/"),
            Category = meta.Cat,
            Date = meta.DateRaw,
            Version = meta.Version,
            Tag = meta.Tag,
        };
    }
}