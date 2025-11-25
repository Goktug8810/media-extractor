using System.Text.RegularExpressions;

namespace MediaExtractor.Application.YouTube;

public static class BaseJsUrlExtractor
{
    // YouTube HTML'de player script genelde şöyle görünür:
    // <script src="/s/player/abcd1234/player_ias.vflset/base.js"></script>
    private static readonly Regex BaseJsRegex = new(
        @"<script\s+src=""(\/s\/player\/.*?\/base\.js)""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Extract(string html)
    {
        var match = BaseJsRegex.Match(html);
        if (!match.Success)
            throw new InvalidOperationException("base.js URL could not be found in HTML.");

        string relativeUrl = match.Groups[1].Value;

        // YouTube HTML'de URL relative gelebilir → absolute'a çeviriyoruz
        return $"https://www.youtube.com{relativeUrl}";
    }
}