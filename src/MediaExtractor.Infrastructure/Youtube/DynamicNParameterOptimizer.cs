using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Jint;
using MediaExtractor.Domain.YouTube;
using MediaExtractor.Infrastructure.Utils;

namespace MediaExtractor.Infrastructure.YouTube
{
    /// <summary>
    /// base.js içindeki 'n' transform fonksiyonunu dinamik olarak bulup,
    /// Jint ile çalıştırır. Eğer pattern bulunamazsa veya hata oluşursa
    /// orijinal n değeri geri döner (fail-safe).
    /// </summary>
    public sealed class DynamicNParameterOptimizer : INParameterOptimizer
    {
        // YouTube pattern’i genelde: .get("n")&&(b=FUNC(b))
        private static readonly Regex FunctionNameRegex = new(
            @"\.get\(""n""\)\s*&&\s*\(\w=([a-zA-Z0-9$]+)(?:\[(\d+)\])?\(\w\)\)",
            RegexOptions.Compiled);

        private static NTransformPlan? _cachedPlan;
        private static string? _cachedBaseJsHash;

        public string Optimize(string baseJsContent, string originalN)
        {
            if (string.IsNullOrEmpty(originalN) || string.IsNullOrEmpty(baseJsContent))
                return originalN;

            try
            {
                var plan = GetOrExtractPlan(baseJsContent);
                return ExecuteJsTransform(plan, originalN);
            }
            catch
            {
           
                return originalN;
            }
        }

        private NTransformPlan GetOrExtractPlan(string baseJs)
        {
            var hash = ComputeSha256(baseJs);

            if (_cachedPlan is not null && _cachedBaseJsHash == hash && _cachedPlan.IsValid())
                return _cachedPlan;

            var match = FunctionNameRegex.Match(baseJs);
            if (!match.Success)
                throw new InvalidOperationException("Base.js içinde 'n' transform fonksiyonu pattern'i bulunamadı.");

            var funcName = match.Groups[1].Value;

            var definitionPattern1 = $"{funcName}=function";
            var definitionPattern2 = $"function {funcName}";

            var defIndex = baseJs.IndexOf(definitionPattern1, StringComparison.Ordinal);
            if (defIndex == -1)
                defIndex = baseJs.IndexOf(definitionPattern2, StringComparison.Ordinal);

            if (defIndex == -1)
                throw new InvalidOperationException($"'{funcName}' fonksiyonunun tanımı bulunamadı.");

            var funcBody = JsCodeUtils.ExtractBalancedBlock(baseJs, defIndex)
                           ?? throw new InvalidOperationException("n transform fonksiyon gövdesi çıkarılamadı.");

            var plan = new NTransformPlan(funcName, funcBody);

            _cachedPlan = plan;
            _cachedBaseJsHash = hash;

            return plan;
        }

        private static string ExecuteJsTransform(NTransformPlan plan, string nValue)
        {
            var engine = new Engine(cfg => cfg
                .LimitRecursion(64)
                .MaxStatements(10_000));

            engine.Execute(plan.FunctionBody);

            var result = engine.Invoke(plan.FunctionName, nValue);
            return result.AsString();
        }

        private static string ComputeSha256(string text)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}