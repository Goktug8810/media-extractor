using MediaExtractor.Domain.Device;

namespace MediaExtractor.Application.Http
{
    public class GetSpoofedUserAgent
    {
        private readonly ISpoofUserAgentProvider _provider;

        public GetSpoofedUserAgent(ISpoofUserAgentProvider provider)
        {
            _provider = provider;
        }

        public string Execute(DeviceType deviceType)
        {
            return _provider.GetUserAgent(deviceType);
        }
    }
}