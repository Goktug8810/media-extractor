namespace MediaExtractor.Domain.Media
{
    public interface IMediaHtmlFetcher
    {
        Task<(string html, Version protocol)> GetHtmlWithProtocolAsync(string url, string? userAgent);
    }
}