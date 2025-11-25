// src/MediaExtractor.Domain/Media/IMediaDownloader.cs
namespace MediaExtractor.Domain.Media
{
    public interface IMediaDownloader
    {
        Task DownloadAsync(
            string finalUrl,
            string userAgent,
            Stream output,
            CancellationToken ct);
    }
}