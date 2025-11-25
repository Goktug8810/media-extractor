namespace MediaExtractor.Infrastructure.Utils
{
    
    public static class JsCodeUtils
    {
        /// <summary>
        /// content[startIndex] noktasından itibaren, ilk '{' karakterini bulur,
        /// bu süslü parantezden itibaren balanced (iç içe) blok sonuna kadar substring döner.
        /// </summary>
        public static string? ExtractBalancedBlock(string content, int startIndex)
        {
            if (string.IsNullOrEmpty(content) || startIndex < 0 || startIndex >= content.Length)
                return null;

            // İlk '{' karakteri
            var openBraceIndex = content.IndexOf('{', startIndex);
            if (openBraceIndex == -1)
                return null;

            var depth = 1;
            var i = openBraceIndex + 1;

            while (i < content.Length && depth > 0)
            {
                var ch = content[i];
                if (ch == '{') depth++;
                else if (ch == '}') depth--;
                i++;
            }

            if (depth != 0)
                return null;

            return content.Substring(startIndex, i - startIndex);
        }
    }
}