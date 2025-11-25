using MediaExtractor.Application.YouTube;
using MediaExtractor.Domain.Media;

public sealed class DownloadMediaUseCase
{
    private readonly IMediaDownloader _downloader;
    private readonly OptimizeNInUrlUseCase _optimizeN;

    public DownloadMediaUseCase(IMediaDownloader downloader, OptimizeNInUrlUseCase optimizeN)
    {
        _downloader = downloader;
        _optimizeN = optimizeN;
    }

    public async Task ExecuteAsync(
        string finalUrl,
        string? baseJs,
        string ua,
        Stream output,
        CancellationToken ct)
    {
        // 1) Throttle bypass (n parametresi varsa)
        if (!string.IsNullOrEmpty(baseJs))
        {
            finalUrl = _optimizeN.Execute(finalUrl, baseJs);
            Console.WriteLine("FINAL URL = " + finalUrl);

        }

        // 2) Asıl download
        Console.WriteLine("FINAL URsL = " + finalUrl);
        await _downloader.DownloadAsync(finalUrl, ua, output, ct);
        
    }
}