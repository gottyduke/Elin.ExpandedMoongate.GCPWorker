using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using EGate.GCP.Model;

namespace EGate.GCP;

internal class GcpWorkerClient(HttpClient http, string sourceBase, string targetBase, int concurrency = 8)
{
    private readonly string _sourceBase = sourceBase.TrimEnd('/') + "/";
    private readonly string _targetBase = targetBase.TrimEnd('/');
    private readonly SemaphoreSlim _semaphore = new(concurrency, concurrency);

    public async Task<List<DownloadMeta>?> GetFileListAsync(string lang, CancellationToken ct = default)
    {
        var url = $"{_sourceBase}files/{lang}/index.txt";
        using var res = await http.GetAsync(url, ct);
        if (!res.IsSuccessStatusCode) {
            return null;
        }

        var text = await res.Content.ReadAsStringAsync(ct);
        var list = new List<DownloadMeta>();

        using var sr = new StringReader(text);
        var cfg = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false,
            IgnoreBlankLines = true,
            BadDataFound = null,
            MissingFieldFound = null,
        };

        using var csv = new CsvReader(sr, cfg);
        while (await csv.ReadAsync()) {
            var path = csv.GetField(0);
            if (string.IsNullOrWhiteSpace(path)) {
                continue;
            }

            var name = SafeGet(csv, 2) ?? "";
            var title = SafeGet(csv, 3) ?? "";
            var cat = SafeGet(csv, 5) ?? "";
            var dateRaw = (SafeGet(csv, 6) ?? "").Replace("\"", "");
            var versionRaw = SafeGet(csv, 8) ?? "0";
            var tag = SafeGet(csv, 9) ?? "";
            _ = int.TryParse(versionRaw, out var version);

            list.Add(new() {
                Path = path,
                Id = Path.GetFileNameWithoutExtension(path),
                Name = name,
                Title = title,
                Cat = cat,
                DateRaw = dateRaw,
                Version = version,
                Tag = tag,
            });
        }
        return list;
    }

    private static string? SafeGet(CsvReader csv, int index)
    {
        try { return csv.GetField(index); } catch { return null; }
    }

    public async Task<bool> UploadMapFileAsync(string id, byte[] bytes, CancellationToken ct = default)
    {
        var url = $"{_targetBase}/files/upload/{Uri.EscapeDataString(id)}";

        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new("application/octet-stream");
        content.Headers.Add("x-debugging-key", Environment.GetEnvironmentVariable("EGateDebuggingWorkerKey"));

        using var res = await http.PostAsync(url, content, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UploadMapMetaAsync(string id, MapMeta meta, CancellationToken ct = default)
    {
        var url = $"{_targetBase}/maps/upload/{Uri.EscapeDataString(id)}";
        var json = JsonSerializer.Serialize(meta);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("x-debugging-key", Environment.GetEnvironmentVariable("EGateDebuggingWorkerKey"));

        using var res = await http.PostAsync(url, content, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DownloadAndUploadAsync(DownloadMeta m, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try {
            var fileUrl = $"{_sourceBase}{m.Path}";
            var bytes = await http.GetByteArrayAsync(fileUrl, ct);
            var mapMeta = MapMeta.FromDownloadMeta(m);

            var fileOk = await UploadMapFileAsync(mapMeta.Id, bytes, ct);
            if (!fileOk) {
                return false;
            }

            return await UploadMapMetaAsync(mapMeta.Id, mapMeta, ct);
        } finally {
            _semaphore.Release();
        }
    }
}