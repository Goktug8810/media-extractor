using System.Text.RegularExpressions;
using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Infrastructure.YouTube
{
namespace MediaExtractor.Infrastructure.YouTube
{
    public sealed class JsSignaturePlanExtractor : ISignaturePlanExtractor
    {
        // 1) Helper object bulma (var obf_name = { ... } yapısı)
        // Groups[1]: Obje İsmi (örn. dK)
        private static readonly Regex HelperObjectRegex =
            new(@"(?:var|let|const)\s+([A-Za-z0-9$]{1,4})\s*=\s*\{([^}]+)\}",
                RegexOptions.Singleline | RegexOptions.Compiled);

        // 2) Ana çözme fonksiyonu çağrısını ve Step Array'i bulma
        // a=[["P1",0],["W1",3],["nJ",2]] yapısını arar.
        // Groups[1]: Talimat dizisinin içeriği (örn. ["P1",0],["W1",3],["nJ",2])
        private static readonly Regex StepArrayRegex =
            new(@"[a-zA-Z0-9_$]{1,3}\s*=\s*\[((?:\[[^\]]+\]\s*,?\s*)+)\];",
                RegexOptions.Singleline | RegexOptions.Compiled);

        public SignatureDecodePlan Extract(string playerId, string baseJs)
        {
            // 1) Helper object ismini ve içeriğini bul
            var helperMatch = HelperObjectRegex.Match(baseJs);
            if (!helperMatch.Success)
                throw new InvalidOperationException("Helper object not found in base.js content.");

            string helperBody = helperMatch.Groups[2].Value;

            // 2) Helper method mapping'i çıkar
            // Obfuskasyonlu isim (örn. P1) ile gerçek işlem (Reverse) eşleştirilir.
            var methodMap = ExtractHelperMethodMapping(helperBody);

            // 3) Step dizisini bul
            var stepMatch = StepArrayRegex.Match(baseJs);
            if (!stepMatch.Success)
            {
                // Alternatif olarak, ana decrypt fonksiyonunun tanımından diziyi aramayı deneyebiliriz,
                // ancak en yaygın desen (var a = [ ... ]) budur.
                throw new InvalidOperationException("Step array (instructions) not found in base.js content.");
            }

            string rawSteps = stepMatch.Groups[1].Value;

            // 4) Talimatları SignatureDecodeStep listesine dönüştür
            var steps = ExtractSteps(rawSteps, methodMap);

            return new SignatureDecodePlan(playerId, steps);
        }

        private Dictionary<string, SignatureOp> ExtractHelperMethodMapping(string helperBody)
        {
            var map = new Dictionary<string, SignatureOp>();

            // Regex'ler, obfuskasyonlu fonksiyon adını (Groups[1]) ve fonksiyonun gerçekleştirdiği işlemi yakalar.
            // Önemli Not: Regex'ler, A[x] dizisine yapılan obfuskasyonlu çağrıyı (örn: a[A[28]]()) arar.

            // Reverse: a.reverse() çağrısı. Genellikle argüman almaz.
            // Prensip: 'function(a){a[...]()}' yapısını arar.
            var reverseRegex = new Regex(@"([A-Za-z0-9$]{2,4})\s*:\s*function\s*\(\w\)\s*\{\s*\w\[[^\]]+\]\(\)", RegexOptions.Singleline | RegexOptions.Compiled);

            // Slice/Splice: a.splice(0,b) çağrısı. Baştan b kadar eleman atar.
            // Prensip: 'function(a,b){a[...](0,b)}' yapısını arar.
            var sliceRegex = new Regex(@"([A-Za-z0-9$]{2,4})\s*:\s*function\s*\(\w,\w\)\s*\{\s*\w\[[^\]]+\]\(0,\w\)", RegexOptions.Singleline | RegexOptions.Compiled);
            
            // Swap: Manuel yer değiştirme implementasyonu. 
            // Prensip: 'function(a,b){var c=a[0]; a[0]=...' yapısını arar.
            var swapRegex = new Regex(@"([A-Za-z0-9$]{2,4})\s*:\s*function\s*\(\w,\w\)\s*\{\s*var\s+\w=\w\[0\];", RegexOptions.Singleline | RegexOptions.Compiled);

            // Eşleşen metotları map'e ekle
            foreach (Match m in reverseRegex.Matches(helperBody))
            {
                string method = m.Groups[1].Value;
                if (!map.ContainsKey(method)) map.Add(method, SignatureOp.Reverse);
            }

            foreach (Match m in sliceRegex.Matches(helperBody))
            {
                string method = m.Groups[1].Value;
                if (!map.ContainsKey(method)) map.Add(method, SignatureOp.Slice);
            }

            foreach (Match m in swapRegex.Matches(helperBody))
            {
                string method = m.Groups[1].Value;
                if (!map.ContainsKey(method)) map.Add(method, SignatureOp.Swap);
            }

            if (map.Count < 3)
            {
                // Temel 3 operasyon bulunamazsa, algoritma değişmiş demektir.
                throw new InvalidOperationException($"Could not find all 3 core signature operations (Reverse, Slice, Swap) in helper object. Found {map.Count}.");
            }

            return map;
        }

        private IReadOnlyList<SignatureDecodeStep> ExtractSteps(string rawSteps, Dictionary<string, SignatureOp> methodMap)
        {
            var result = new List<SignatureDecodeStep>();

            // Talimatları ayrıştırma: ["METHOD_NAME", ARGUMENT]
            var reg = new Regex(@"\[\s*""([^""]+)""\s*,\s*(\d+)\s*\]");

            foreach (Match m in reg.Matches(rawSteps))
            {
                string method = m.Groups[1].Value;
                int arg = int.Parse(m.Groups[2].Value);

                // Hata Yönetimi: Çıkarılan metodun eşleşme tablosunda olup olmadığını kontrol et.
                if (!methodMap.ContainsKey(method))
                    throw new InvalidOperationException($"Unknown helper method '{method}' encountered in step array. Plan extraction failed.");

                result.Add(new SignatureDecodeStep(methodMap[method], arg));
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Step array was successfully extracted, but no decode steps were found within it.");
            }

            return result;
        }
    }
}

}