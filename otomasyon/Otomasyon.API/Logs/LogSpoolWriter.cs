using System.Text.Json;
using Microsoft.Extensions.Options;
using Otomasyon.Domain.Entities;
using Otomasyon.Shared.Options;

namespace Otomasyon.API.Logs;

public class LogSpoolWriter : ILogSpoolWriter
{
    private readonly LogSpoolOptions _opt;
    private readonly object _lockReq = new();
    private readonly object _lockErr = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public LogSpoolWriter(IOptions<LogSpoolOptions> opt)
    {
        _opt = opt.Value;
        Directory.CreateDirectory(Path.Combine(_opt.RootPath, "request"));
        Directory.CreateDirectory(Path.Combine(_opt.RootPath, "error"));
    }

    public void Write(RequestLog log) => WriteInternal("request", log, _lockReq);
    public void Write(ErrorLog log) => WriteInternal("error", log, _lockErr);

    private void WriteInternal<T>(string folder, T log, object gate)
    {
        lock (gate)
        {
            var dir = Path.Combine(_opt.RootPath, folder);
            Directory.CreateDirectory(dir);

            var active = Path.Combine(dir, "active.ndjson");
            var line = JsonSerializer.Serialize(log, _json) + "\n";
            File.AppendAllText(active, line);

            var fi = new FileInfo(active);
            if (fi.Length >= _opt.SegmentMaxBytes)
                Rotate(dir);
        }
    }

    private void Rotate(string dir)
    {
        var active = Path.Combine(dir, "active.ndjson");
        if (!File.Exists(active)) return;

        var fi = new FileInfo(active);
        if (fi.Length == 0) return;

        var next = Path.Combine(dir, $"spool-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.ndjson");
        File.Move(active, next, overwrite: false);
    }
}