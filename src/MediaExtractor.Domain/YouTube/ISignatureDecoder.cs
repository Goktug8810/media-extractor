namespace MediaExtractor.Domain.YouTube
{
    public interface ISignatureDecoder
    {
        string Decode(string s, SignatureDecodePlan plan);
    }
}