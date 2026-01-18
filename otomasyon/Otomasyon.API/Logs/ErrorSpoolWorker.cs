using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Otomasyon.DataAccess.Contexts;
using Otomasyon.Domain.Entities;
using Otomasyon.Shared.Options;

namespace Otomasyon.API.Logs;

public class ErrorSpoolWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LogSpoolOptions _opt;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public ErrorSpoolWorker(IServiceScopeFactory scopeFactory, IOptions<LogSpoolOptions> opt)
    {
        _scopeFactory = scopeFactory;
        _opt = opt.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dir = Path.Combine(_opt.RootPath, "error");
        Directory.CreateDirectory(dir);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var files = Directory.GetFiles(dir, "spool-*.ndjson").OrderBy(x => x).ToList();
                if (files.Count == 0)
                {
                    await Task.Delay(300, stoppingToken);
                    continue;
                }

                foreach (var file in files)
                {
                    await ProcessFile(file, stoppingToken);
                    File.Delete(file);
                }
            }
            catch (OperationCanceledException) { }
            catch
            {
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    private async Task ProcessFile(string file, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var batch = new List<ErrorLog>(_opt.BatchSize);

        using var fs = File.OpenRead(file);
        using var sr = new StreamReader(fs);

        while (!sr.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await sr.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            ErrorLog? item;
            try { item = JsonSerializer.Deserialize<ErrorLog>(line, _json); }
            catch { continue; }

            if (item is null) continue;

            batch.Add(item);
            if (batch.Count >= _opt.BatchSize)
            {
                await db.ErrorLogs.AddRangeAsync(batch, ct);
                await db.SaveChangesAsync(ct);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await db.ErrorLogs.AddRangeAsync(batch, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
