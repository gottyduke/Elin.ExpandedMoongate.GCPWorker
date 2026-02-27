namespace EGate.GCP.Model;

public sealed record DownloadMeta
{
    public required string Path { get; init; }
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Title { get; init; }
    public string? Cat { get; init; }
    public required string DateRaw { get; init; }
    public int Version { get; init; }
    public string? Tag { get; init; }
}