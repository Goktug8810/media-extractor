namespace MediaExtractor.Domain.YouTube
{
    public interface IBaseJsCache
    {
        bool TryGet(string playerId, out string baseJs);
        void Set(string playerId, string baseJs, TimeSpan ttl);
    }
}