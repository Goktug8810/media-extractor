using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Domain.Media

{
    public class MediaStream
    {
        public string Format { get; init; } = "";
        public string Type { get; init; } = "";
        public string Quality { get; init; } = "";
        public int Itag { get; init; }
        public string Url { get; init; } = "";
        public SignatureCipher? Cipher { get; init; }  

    }
}