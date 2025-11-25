using MediaExtractor.Domain.Media;

namespace MediaExtractor.Application.UseCases
{
    public class GetYoutubeHtmlUseCase
    {
        private readonly IMediaHtmlFetcher _fetcher;

        public GetYoutubeHtmlUseCase(IMediaHtmlFetcher fetcher)
        {
            _fetcher = fetcher;
        }

        public Task<(string html, Version protocol)> ExecuteWithProtocolAsync(string url, string? ua)
        {
            return _fetcher.GetHtmlWithProtocolAsync(url, ua);
        }
    }
}