namespace MediaExtractor.Domain.YouTube
{
    public interface IYouTubeHttpClient
    {
        Task<string> GetHtmlAsync(string url, string? incomingUserAgent);
        Task<(string Html, Version Protocol)> GetHtmlWithProtocolAsync(string youtubeUrl, string? incomingUa);
    }
}