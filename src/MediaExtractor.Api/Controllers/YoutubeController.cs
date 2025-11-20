using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaExtractor.Application.UseCases;

namespace MediaExtractor.Api.Controllers
{
    [ApiController]
    [Route("api/youtube")]
    public class YoutubeController : ControllerBase
    {
        private readonly GetYoutubeHtmlUseCase _useCase;

        public YoutubeController(GetYoutubeHtmlUseCase useCase)
        {
            _useCase = useCase;
        }

        
    }
}