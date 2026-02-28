using EGate.GCP;

const string sourceBase = "http://elin.cloudfree.jp/script/uploader/";
const string targetBase = "https://api-exmoongate.elin-modding.net";

using var http = new HttpClient();
http.DefaultRequestHeaders.Add("x-debugging-key", Environment.GetEnvironmentVariable("EGateDebuggingWorkerKey"));
http.Timeout = TimeSpan.FromMinutes(5);

var client = new GcpWorkerClient(http, sourceBase, targetBase);

string[] languages = ["EN", "CN", "JP"];
Console.WriteLine($"[{DateTime.UtcNow:O}] Map sync started. Languages: {string.Join(", ", languages)}");

foreach (var lang in languages) {
    Console.WriteLine($"[{DateTime.UtcNow:O}] Fetching index for language: {lang}");
    var list = await client.GetFileListAsync(lang);
    if (list is null) {
        Console.WriteLine($"[{DateTime.UtcNow:O}] WARN: Failed to fetch index for {lang}, skipped.");
        continue;
    }
    Console.WriteLine($"[{DateTime.UtcNow:O}] Fetched {list.Count} entries for {lang}");

    var ok = 0;
    var fail = 0;

    var tasks = list.Select(async m => {
        try {
            var success = await client.DownloadAndUploadAsync(m);
            if (success) {
                Interlocked.Increment(ref ok);
                Console.WriteLine($"[{DateTime.UtcNow:O}] OK   {lang} {m.Path}");
            } else {
                Interlocked.Increment(ref fail);
                Console.WriteLine($"[{DateTime.UtcNow:O}] FAIL {lang} {m.Path}");
            }
        } catch (Exception ex) {
            Interlocked.Increment(ref fail);
            Console.WriteLine($"[{DateTime.UtcNow:O}] ERR  {lang} {m.Path} :: {ex.Message}");
        }
    });
    await Task.WhenAll(tasks);
    Console.WriteLine(
        $"[{DateTime.UtcNow:O}] Finished {lang}. Total={list.Count}, Success={ok}, Failed={fail}");
}
Console.WriteLine($"[{DateTime.UtcNow:O}] Map sync finished.");