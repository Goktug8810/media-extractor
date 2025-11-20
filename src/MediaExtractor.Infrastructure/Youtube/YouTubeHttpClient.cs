using System.Net;
using System.Net.Http;
using MediaExtractor.Application.Device;
using MediaExtractor.Application.Http;
using MediaExtractor.Domain.Device;
using MediaExtractor.Domain.Media;

namespace MediaExtractor.Infrastructure.YouTube
{
    public class YouTubeHttpClient : IMediaHtmlFetcher
    {
        private readonly DeviceDetector _deviceDetector;
        private readonly GetSpoofedUserAgent _getSpoofedUserAgent;
        private readonly HttpClient _client;

        public YouTubeHttpClient(
            DeviceDetector deviceDetector,
            GetSpoofedUserAgent getSpoofedUserAgent)
        {
            _deviceDetector = deviceDetector;
            _getSpoofedUserAgent = getSpoofedUserAgent;

            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = true
            };

            _client = new HttpClient(handler);
        }

        public async Task<string> GetHtmlAsync(string url, string? ua)
        {
            var device = _deviceDetector.Detect(ua);
            var spoofUa = _getSpoofedUserAgent.Execute(device);

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("User-Agent", spoofUa);

            req.Version = HttpVersion.Version20;
            req.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            using var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        
        public async Task<(string html, Version protocol)> 
            GetHtmlWithProtocolAsync(string url, string? ua)
        {
            var device = _deviceDetector.Detect(ua);
            var spoofUa = _getSpoofedUserAgent.Execute(device);

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("User-Agent", spoofUa);

            req.Version = HttpVersion.Version20;
            req.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            using var response = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            var protocol = response.Version;
            var html = await response.Content.ReadAsStringAsync();

            return (html, protocol);
        }
        
    }
    
    
}