namespace MediaExtractor.Domain.Media
{
    public interface IStreamSelector
    {
        MediaStream SelectByItag(PlayerResponse response, int itag);
    }
}