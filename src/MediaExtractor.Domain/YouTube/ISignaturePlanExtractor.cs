namespace MediaExtractor.Domain.YouTube
{
    public interface ISignaturePlanExtractor
    {
        SignatureDecodePlan Extract(string playerId, string baseJs);
    }
}