using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Otomasyon.Shared.Options;

namespace Otomasyon.API.Logs;

public class ActiveSegmentRotatorWorker : BackgroundService
{
    private readonly LogSpoolOptions _opt;

    public ActiveSegmentRotatorWorker(IOptions<LogSpoolOptions> opt)
        => _opt = opt.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reqDir = Path.Combine(_opt.RootPath, "request");
        var errDir = Path.Combine(_opt.RootPath, "error");
        Directory.CreateDirectory(reqDir);
        Directory.CreateDirectory(errDir);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                RotateIfNeeded(reqDir);
                RotateIfNeeded(errDir);
            }
            catch { /* worker ölmesin */ }

            await Task.Delay(_opt.RotateInterval, stoppingToken);
        }
    }

    private static void RotateIfNeeded(string dir)
    {
        var active = Path.Combine(dir, "active.ndjson");
        if (!File.Exists(active)) return;

        var fi = new FileInfo(active);
        if (fi.Length == 0) return;

        var next = Path.Combine(dir, $"spool-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.ndjson");
        File.Move(active, next, overwrite: false);
    }
}