using MediaExtractor.Domain.Media;

namespace MediaExtractor.Application.UseCases
{
    public class ExtractPlayerResponseUseCase
    {
        private readonly IPlayerResponseExtractor _extractor;

        public ExtractPlayerResponseUseCase(IPlayerResponseExtractor extractor)
        {
            _extractor = extractor;
        }

        public PlayerResponse Execute(string html)
        {
            return _extractor.Extract(html);
        }
    }
}