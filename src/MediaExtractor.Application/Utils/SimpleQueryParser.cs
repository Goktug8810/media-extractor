using System;
using System.Collections.Generic;
using System.Text;

namespace MediaExtractor.Application.Utils
{
    /// <summary>
    /// System.Web veya ASP.NET bağımlılığı olmadan basit query string parser/builder.
    /// </summary>
    public static class SimpleQueryParser
    {
        public static Dictionary<string, List<string>> Parse(string query)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(query))
                return result;

            if (query[0] == '?')
                query = query.Substring(1);

            var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2);
                var key = Uri.UnescapeDataString(kv[0]);
                var value = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;

                if (!result.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    result[key] = list;
                }

                list.Add(value);
            }

            return result;
        }

        public static string Build(string baseUrl, IDictionary<string, string?> query)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var kv in query)
            {
                if (!first) sb.Append('&');
                first = false;

                sb.Append(Uri.EscapeDataString(kv.Key));
                sb.Append('=');

                if (kv.Value != null)
                    sb.Append(Uri.EscapeDataString(kv.Value));
            }

            var queryPart = sb.ToString();

            if (string.IsNullOrEmpty(queryPart))
                return baseUrl;

            return $"{baseUrl}?{queryPart}";
        }
    }
}