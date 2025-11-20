using MediaExtractor.Domain.Device;

namespace MediaExtractor.Application.Http
{
    public class SpoofUserAgentBuilder
    {
        // iOS Safari – QUIC/HTTP3 broken HTTP/2
        private const string SafariIOS =
            "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) " +
            "AppleWebKit(605.1.15) (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";

        // Android Chrome 
        private const string ChromeAndroid =
            "Mozilla/5.0 (Linux; Android 11; Pixel 5) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/87.0.4280.101 Mobile Safari/537.36";

        public string Build(DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.IOS      => SafariIOS,
                DeviceType.Android  => ChromeAndroid,
                DeviceType.Desktop  => SafariIOS, 
                _                   => SafariIOS
            };
        }
    }
}