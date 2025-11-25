using System.Text.RegularExpressions;
using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Infrastructure.YouTube
{
    public sealed class PlayerScriptUrlExtractor : IPlayerScriptUrlExtractor
    {
        private static readonly Regex BaseJsRegex =
            new(@"<script[^>]+src=""([^""]*?/base\.js)""", RegexOptions.Compiled);

        private static readonly Regex PlayerIdRegex =
            new(@"/player/([^/]+)/", RegexOptions.Compiled);

        public string ExtractBaseJsUrl(string html)
        {
            var match = BaseJsRegex.Match(html);
            if (!match.Success)
                throw new InvalidOperationException("base.js URL bulunamadı.");

            var url = match.Groups[1].Value;
            if (url.StartsWith("//"))
                url = "https:" + url;
            if (url.StartsWith("/"))
                url = "https://www.youtube.com" + url;

            return url;
        }

        public string ExtractPlayerIdFromUrl(string baseJsUrl)
        {
            var match = PlayerIdRegex.Match(baseJsUrl);
            if (!match.Success)
                return "unknown";

            return match.Groups[1].Value;
        }
    }
}