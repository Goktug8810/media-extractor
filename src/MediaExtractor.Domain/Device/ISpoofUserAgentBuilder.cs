namespace MediaExtractor.Domain.Device
{
    public interface ISpoofUserAgentBuilder
    {
        string Build(DeviceType device);
    }
}