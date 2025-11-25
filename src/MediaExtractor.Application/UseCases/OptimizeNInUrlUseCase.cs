using System;
using System.Collections.Generic;
using MediaExtractor.Application.Utils;
using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Application.YouTube
{
    /// <summary>
    /// finalUrl içindeki 'n' parametresini base.js’e göre optimize eden use case.
    /// Regex/JS ile dynamic çözüm, başarısız olursa URL’i değiştirmez.
    /// </summary>
    public sealed class OptimizeNInUrlUseCase
    {
        private readonly INParameterOptimizer _optimizer;

        public OptimizeNInUrlUseCase(INParameterOptimizer optimizer)
        {
            _optimizer = optimizer;
        }

        public string Execute(string url, string baseJs)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(baseJs))
                return url;

            var uri = new Uri(url);

            var queryDict = SimpleQueryParser.Parse(uri.Query);

            if (!queryDict.TryGetValue("n", out var nValues) || nValues.Count == 0)
                return url;

            var originalN = nValues[0];
            if (string.IsNullOrEmpty(originalN))
                return url;

            var optimizedN = _optimizer.Optimize(baseJs, originalN);

            if (string.IsNullOrEmpty(optimizedN) || optimizedN == originalN)
                return url;

            var newQuery = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in queryDict)
            {
                if (string.Equals(kv.Key, "n", StringComparison.OrdinalIgnoreCase))
                    newQuery[kv.Key] = optimizedN;
                else
                    newQuery[kv.Key] = kv.Value.Count > 0 ? kv.Value[0] : null;
            }

            var baseUrl = $"{uri.Scheme}://{uri.Host}";
            if (!uri.IsDefaultPort)
                baseUrl += $":{uri.Port}";
            baseUrl += uri.AbsolutePath;

            var newUrl = SimpleQueryParser.Build(baseUrl, newQuery);

            if (!string.IsNullOrEmpty(uri.Fragment))
                newUrl += uri.Fragment;

            return newUrl;
        }
    }
}