using MediaExtractor.Domain.Device;

namespace MediaExtractor.Infrastructure.YouTube
{
    public class SpoofUserAgentProvider : ISpoofUserAgentProvider
    {
        private static readonly string SafariIOS =
            "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) " +
            "AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";

        private static readonly string ChromeAndroid =
            "Mozilla/5.0 (Linux; Android 11; Pixel 5) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/87.0.4280.101 Mobile Safari/537.36";

        private static readonly string ChromeWin7 =
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36";

        public string GetUserAgent(DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.IOS     => SafariIOS,
                DeviceType.Android => ChromeAndroid,
                DeviceType.Desktop => ChromeWin7,
                _                  => SafariIOS
            };
        }
    }
}