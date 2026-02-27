using EGate.GCP.Helper;

namespace EGate.GCP.Model;

public sealed record MapMeta
{
    public required string Id { get; init; }
    public required string Author { get; init; }
    public required string Title { get; init; }
    public string? Lang { get; init; }
    public string? Cat { get; init; }
    public required DateTime Date { get; init; }
    public required int Version { get; init; }
    public string? Tag { get; init; }
    public required bool IsOfficial { get; init; }

    public int VisitCount { get; init; }
    public int RatingCount { get; init; }
    public float RatingAverage { get; init; }
    public string? FileKey { get; init; }
    public int FileSize { get; init; }

    public static MapMeta FromDownloadMeta(DownloadMeta m) => new()
    {
        Id = $"{m.Path}-{m.Name}-{m.DateRaw}".SanitizeFileName('-'),
        Author = m.Name,
        Title = m.Title,
        Lang = m.Path.ExtractInBetween("/", "/"),
        Cat = m.Cat,
        Date = DateTime.TryParse(m.DateRaw, out var d) ? d : DateTime.UtcNow,
        Version = m.Version,
        Tag = m.Tag,
        IsOfficial = true,
    };
}