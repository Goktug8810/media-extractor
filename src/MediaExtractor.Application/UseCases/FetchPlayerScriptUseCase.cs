using MediaExtractor.Domain.YouTube;
using MediaExtractor.Domain.Media;

using IPlayerScriptUrlExtractor = MediaExtractor.Domain.YouTube.IPlayerScriptUrlExtractor;

namespace MediaExtractor.Application.UseCases
{
    public sealed class FetchPlayerScriptUseCase
    {
        private readonly IMediaHtmlFetcher _fetcher;
        private readonly IPlayerScriptUrlExtractor _extractor;
        private readonly IBaseJsCache _cache;

        public FetchPlayerScriptUseCase(
            IMediaHtmlFetcher fetcher,
            IPlayerScriptUrlExtractor extractor,
            IBaseJsCache cache)
        {
            _fetcher = fetcher;
            _extractor = extractor;
            _cache = cache;
        }

        public async Task<PlayerScriptResult> ExecuteAsync(string html, string ua)
        {
            //  BaseJS URL extract
            var baseJsUrl = _extractor.ExtractBaseJsUrl(html);

            var playerId = _extractor.ExtractPlayerIdFromUrl(baseJsUrl);

            if (_cache.TryGet(playerId, out var cachedJs))
            {
                return new PlayerScriptResult(playerId, cachedJs);
            }

            var (baseJs, _) = await _fetcher.GetHtmlWithProtocolAsync(baseJsUrl, ua);

            _cache.Set(playerId, baseJs, TimeSpan.FromHours(6));

            return new PlayerScriptResult(playerId, baseJs);
        }
    }
}