using System.Collections.Concurrent;
using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Infrastructure.YouTube
{
    public sealed class InMemorySignaturePlanCache : ISignaturePlanCache
    {
        private readonly ConcurrentDictionary<string, SignatureDecodePlan> _cache =
            new(StringComparer.Ordinal);

        public SignatureDecodePlan? Get(string playerId)
        {
            _cache.TryGetValue(playerId, out var plan);
            return plan;
        }

        public void Set(SignatureDecodePlan plan)
        {
            _cache[plan.PlayerId] = plan;
        }
    }
}