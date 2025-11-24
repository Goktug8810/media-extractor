using System.Text.Json;
using System.Text.RegularExpressions;
using MediaExtractor.Domain.Media;

namespace MediaExtractor.Infrastructure.YouTube
{
    public class PlayerResponseExtractor : IPlayerResponseExtractor
    {
        private static readonly Regex PlayerResponseRegex = new(
            @"(?:window\[""ytInitialPlayerResponse""\]\s*=\s*|ytInitialPlayerResponse\s*=\s*)(\{.*?""streamingData"".*?});",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public PlayerResponse Extract(string html)
        {
            var match = PlayerResponseRegex.Match(html);
            if (!match.Success)
                throw new InvalidOperationException("Playable ytInitialPlayerResponse JSON (with streamingData) not found.");

            var json = match.Groups[1].Value;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string title = "";
            string videoId = "";

            if (root.TryGetProperty("videoDetails", out var videoDetails))
            {
                if (videoDetails.TryGetProperty("title", out var titleProp))
                    title = titleProp.GetString() ?? "";

                if (videoDetails.TryGetProperty("videoId", out var vidProp))
                    videoId = vidProp.GetString() ?? "";
            }

            var streams = new List<MediaStream>();

            if (root.TryGetProperty("streamingData", out var streamingData))
            {
                if (streamingData.TryGetProperty("formats", out var formatsElement) &&
                    formatsElement.ValueKind == JsonValueKind.Array)
                {
                    ExtractStreamsFromArray(formatsElement, streams, "video+audio");
                }

                if (streamingData.TryGetProperty("adaptiveFormats", out var adaptiveElement) &&
                    adaptiveElement.ValueKind == JsonValueKind.Array)
                {
                    ExtractStreamsFromArray(adaptiveElement, streams, null);
                }
            }

            return new PlayerResponse
            {
                Title = title,
                VideoId = videoId,
                Streams = streams
            };
        }

        private static void ExtractStreamsFromArray(
            JsonElement arrayElement,
            List<MediaStream> target,
            string? typeOverride)
        {
            foreach (var item in arrayElement.EnumerateArray())
            {
                int itag = item.TryGetProperty("itag", out var itagProp)
                    ? itagProp.GetInt32()
                    : -1;

                string mime = item.TryGetProperty("mimeType", out var mimeProp)
                    ? mimeProp.GetString() ?? ""
                    : "";

                string format = mime.Split(';')[0];

                string quality = "";
                if (item.TryGetProperty("qualityLabel", out var qLabel))
                    quality = qLabel.GetString() ?? "";
                else if (item.TryGetProperty("quality", out var q))
                    quality = q.GetString() ?? "";

                string url = "";
                if (item.TryGetProperty("url", out var urlProp))
                    url = urlProp.GetString() ?? "";
                else if (item.TryGetProperty("signatureCipher", out var cipherProp))
                    url = "[signatureCipher]";

                string type;
                if (!string.IsNullOrEmpty(typeOverride))
                {
                    type = typeOverride;
                }
                else
                {
                    var lower = mime.ToLowerInvariant();
                    bool isVideo = lower.Contains("video");
                    bool isAudio = lower.Contains("audio");

                    if (isVideo && isAudio) type = "video+audio";
                    else if (isVideo)       type = "video-only";
                    else if (isAudio)       type = "audio";
                    else                    type = "unknown";
                }

                target.Add(new MediaStream
                {
                    Itag = itag,
                    Format = format,
                    Type = type,
                    Quality = quality,
                    Url = url
                });
            }
        }
    }
}