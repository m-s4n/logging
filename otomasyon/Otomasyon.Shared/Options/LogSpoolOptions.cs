namespace Otomasyon.Shared.Options;

public class LogSpoolOptions
{
    public string RootPath { get; set; } = null!;
    public long SegmentMaxBytes { get; set; } = 10 * 1024 * 1024;
    public int BatchSize { get; set; } = 500;
    // her 5 saniyede 1 active rotate olsun
    public TimeSpan RotateInterval { get; set; } = TimeSpan.FromSeconds(5);

}
