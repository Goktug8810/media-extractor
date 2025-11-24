namespace MediaExtractor.Domain.Media
{
    public interface IPlayerResponseExtractor
    {
        PlayerResponse Extract(string html);
    }
}