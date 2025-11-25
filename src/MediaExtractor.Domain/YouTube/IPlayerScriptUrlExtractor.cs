namespace MediaExtractor.Domain.YouTube
{
    public interface IPlayerScriptUrlExtractor
    {
        string ExtractBaseJsUrl(string html);
        string ExtractPlayerIdFromUrl(string baseJsUrl);
    }
}