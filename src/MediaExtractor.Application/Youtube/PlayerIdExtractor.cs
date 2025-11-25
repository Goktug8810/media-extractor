namespace MediaExtractor.Application.YouTube
{
    public static class PlayerIdExtractor
    {
        public static string ExtractFromUrl(string baseJsUrl)
        {
            var parts = baseJsUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var idx = Array.IndexOf(parts, "player");
            if (idx >= 0 && idx + 1 < parts.Length)
                return parts[idx + 1];

            throw new InvalidOperationException($"Could not extract playerId from: {baseJsUrl}");
        }
    }
}