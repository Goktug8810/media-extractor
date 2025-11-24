using MediaExtractor.Domain.Media;

namespace MediaExtractor.Application.UseCases
{
    public class SelectStreamByItagUseCase
    {
        private readonly IStreamSelector _selector;

        public SelectStreamByItagUseCase(IStreamSelector selector)
        {
            _selector = selector;
        }

        public MediaStream Execute(PlayerResponse response, int itag)
        {
            return _selector.SelectByItag(response, itag);
        }
    }
}