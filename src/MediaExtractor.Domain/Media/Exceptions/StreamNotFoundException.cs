namespace MediaExtractor.Domain.Exceptions
{
    public class StreamNotFoundException : Exception
    {
        public StreamNotFoundException(int itag)
            : base($"Stream not found for itag={itag}")
        {}
    }
}