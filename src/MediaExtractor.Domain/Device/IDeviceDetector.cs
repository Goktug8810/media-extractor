namespace MediaExtractor.Domain.Device
{
    public interface IDeviceDetector
    {
        DeviceType Detect(string? userAgent);
    }
}