namespace MediaExtractor.Domain.Media
{
    public class PlayerResponse
    {
        public string Title { get; init; } = "";
        public string VideoId { get; init; } = "";
        public IReadOnlyList<MediaStream> Streams { get; init; }
            = new List<MediaStream>();
    }
}