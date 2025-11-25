using System.Net;
using System.Net.Http;
using MediaExtractor.Domain.Media;

namespace MediaExtractor.Infrastructure.YouTube
{
    /// <summary>
    /// YouTube CDN'den stream'i indirip verilen output stream'e pipe eder.
    /// - UA spoof dışarıdan gelir (HTML fetch ile aynı UA olmalı)
    /// - Referer & Origin: youtube
    /// - Range: bytes=0-
    /// - ResponseHeadersRead + buffered copy
    /// - Sınırlı retry + exponential backoff
    /// </summary>
    public sealed class YouTubeMediaDownloader : IMediaDownloader
    {
        private readonly HttpClient _http;

        public YouTubeMediaDownloader(HttpClient http)
        {
            _http = http;
            _http.Timeout = TimeSpan.FromSeconds(30); // per-request timeout
        }

        public async Task DownloadAsync(
            string finalUrl,
            string userAgent,
            Stream output,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(finalUrl))
                throw new ArgumentNullException(nameof(finalUrl));

            // Retry policy parametreleri
            const int maxAttempts = 3;
            var initialDelay = TimeSpan.FromSeconds(1);

            Exception? lastException = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    using var req = BuildRequest(finalUrl, userAgent);
                    using var resp = await _http.SendAsync(
                        req,
                        HttpCompletionOption.ResponseHeadersRead,
                        ct);

                    if (!resp.IsSuccessStatusCode)
                    {
                        if (IsRetriableStatus(resp.StatusCode) && attempt < maxAttempts)
                        {
                            // Backoff
                            await Task.Delay(ComputeBackoffDelay(initialDelay, attempt), ct);
                            continue;
                        }

                        var msg = $"YouTube CDN response status code: {(int)resp.StatusCode} ({resp.StatusCode}).";
                        throw new HttpRequestException(msg);
                    }

                    // Body stream'ini al ve output'a pipe et
                    await CopyResponseStreamToOutputAsync(resp, output, ct);
                    return; 
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    lastException = ex;

                    await Task.Delay(ComputeBackoffDelay(initialDelay, attempt), ct);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break;
                }
            }

            throw new HttpRequestException("YouTube media download failed after retries.", lastException);
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static HttpRequestMessage BuildRequest(string finalUrl, string userAgent)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, finalUrl);

            // UA – HTML fetch'te kullanılan spoof ile aynı
            if (!string.IsNullOrWhiteSpace(userAgent))
                req.Headers.TryAddWithoutValidation("User-Agent", userAgent);

            // Anti-hotlink için tipik header seti
            req.Headers.TryAddWithoutValidation("Referer", "https://www.youtube.com/");
            req.Headers.TryAddWithoutValidation("Origin", "https://www.youtube.com");
            req.Headers.TryAddWithoutValidation("Range", "bytes=0-");
            req.Headers.TryAddWithoutValidation("Accept", "*/*");
            req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            req.Headers.ConnectionClose = false;

            return req;
        }

        private static bool IsRetriableStatus(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.Forbidden    // 403
                || statusCode == (HttpStatusCode)429         // Too Many Requests
                || (int)statusCode >= 500;                   // 5xx
        }

        private static TimeSpan ComputeBackoffDelay(TimeSpan initialDelay, int attempt)
        {
            // Basit exponential backoff: 1s, 2s, 4s
            var factor = (int)Math.Pow(2, attempt - 1);
            return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * factor);
        }

        private static async Task CopyResponseStreamToOutputAsync(
            HttpResponseMessage resp,
            Stream output,
            CancellationToken ct)
        {
            await using var responseStream = await resp.Content.ReadAsStreamAsync(ct);

            // 64 KB buffer ile chunked copy – memory safe, streaming
            var buffer = new byte[64 * 1024];
            int read;

            while ((read = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, read), ct);
                await output.FlushAsync(ct); 
            }
        }
    }
}