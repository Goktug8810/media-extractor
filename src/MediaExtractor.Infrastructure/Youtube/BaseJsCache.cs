using MediaExtractor.Domain.YouTube;
using Microsoft.Extensions.Caching.Memory;

namespace MediaExtractor.Infrastructure.YouTube
{
    public sealed class BaseJsCache : IBaseJsCache
    {
        private readonly IMemoryCache _cache;

        public BaseJsCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGet(string playerId, out string baseJs)
        {
            return _cache.TryGetValue(playerId, out baseJs!);
        }

        public void Set(string playerId, string baseJs, TimeSpan ttl)
        {
            _cache.Set(playerId, baseJs, ttl);
        }
    }
}