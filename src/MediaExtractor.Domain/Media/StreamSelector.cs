using MediaExtractor.Domain.Exceptions;

namespace MediaExtractor.Domain.Media
{
    public class StreamSelector : IStreamSelector
    {
        public MediaStream SelectByItag(PlayerResponse response, int itag)
        {
            var match = response.Streams
                .FirstOrDefault(s => s.Itag == itag);

            if (match == null)
                throw new StreamNotFoundException(itag);

            return match;
        }
    }
}