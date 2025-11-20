using MediaExtractor.Domain.Device;

namespace MediaExtractor.Application.Device
{
    public class DeviceDetector
    {
        public DeviceType Detect(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return DeviceType.IOS; // SAFE DEFAULT

            var ua = userAgent.ToLowerInvariant();

            if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod") || ua.Contains("ios"))
                return DeviceType.IOS;

            if (ua.Contains("android"))
                return DeviceType.Android;

            return DeviceType.IOS; // SAFE DEFAULT Fallback
        }
    }
}