namespace MediaExtractor.Domain.Device
{
    public interface ISpoofUserAgentProvider
    {
        string GetUserAgent(DeviceType deviceType);
    }
}