using System;
using System.Web;

namespace MediaExtractor.Domain.YouTube
{
    public static class YouTubeUrlNormalizer
    {
      
        public static string Normalize(string inputUrl)
        {
            if (string.IsNullOrWhiteSpace(inputUrl))
                throw new ArgumentException("URL boş olamaz");

            // Trim + decode
            inputUrl = inputUrl.Trim();
            inputUrl = HttpUtility.UrlDecode(inputUrl);

            if (!inputUrl.StartsWith("http"))
                inputUrl = "https://" + inputUrl;

            var uri = new Uri(inputUrl);

            string host = uri.Host.ToLower();
            string path = uri.AbsolutePath;
            var query = HttpUtility.ParseQueryString(uri.Query);

            // =============== CASE 1 — youtu.be/VIDEO_ID =================
            if (host.Contains("youtu.be"))
            {
                string id = path.Trim('/');

                if (id.Length == 0)
                    throw new Exception("Geçersiz youtu.be linki");

                return BuildWatchUrl(id);
            }

            // =============== CASE 2 — /watch?v=VIDEO_ID =================
            if (path.Equals("/watch", StringComparison.OrdinalIgnoreCase))
            {
                string id = query.Get("v");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("watch URL ama v parametresi yok.");

                return BuildWatchUrl(id);
            }

            // =============== CASE 3 — /shorts/VIDEO_ID =================
            if (path.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
            {
                string id = path.Replace("/shorts/", "").Trim('/');

                if (id.Length == 0)
                    throw new Exception("Geçersiz shorts linki");

                return BuildWatchUrl(id);
            }

            // =============== CASE 4 — /embed/VIDEO_ID =================
            if (path.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
            {
                string id = path.Replace("/embed/", "").Trim('/');

                if (id.Length == 0)
                    throw new Exception("Geçersiz embed linki");

                return BuildWatchUrl(id);
            }

            // =============== CASE 5 — m.youtube.com =====================
            if (host.StartsWith("m.youtube.com"))
            {
                string id = query.Get("v");
                if (!string.IsNullOrEmpty(id))
                    return BuildWatchUrl(id);
            }

            // =============== CASE 6 — youtube.com/abc123 (rare case) ====
            // Pattern: youtube.com/<videoId>
            if (path.Length > 1 && path.Count(c => c == '/') == 1)
            {
                string maybeId = path.Trim('/');
                if (maybeId.Length >= 10 && maybeId.Length <= 20)
                {
                    return BuildWatchUrl(maybeId);
                }
            }

            throw new Exception("Bu YouTube URL formatı desteklenmiyor: " + inputUrl);
        }


        private static string BuildWatchUrl(string id)
        {
            return $"https://www.youtube.com/watch?v={id}";
        }
    }
}