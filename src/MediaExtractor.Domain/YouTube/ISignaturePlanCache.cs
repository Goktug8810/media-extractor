namespace MediaExtractor.Domain.YouTube
{
    public interface ISignaturePlanCache
    {
        SignatureDecodePlan? Get(string playerId);
        void Set(SignatureDecodePlan plan);
    }
}