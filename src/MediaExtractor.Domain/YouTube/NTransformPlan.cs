namespace MediaExtractor.Domain.YouTube
{
    /// <summary>
    /// 'n' parametresi dönüşüm fonksiyonunun analiz sonucunu temsil eder.
    /// </summary>
    public sealed class NTransformPlan
    {
        public string FunctionName { get; }
        public string FunctionBody { get; }
        public DateTime ExtractedAt { get; }

        public NTransformPlan(string functionName, string functionBody)
        {
            FunctionName = functionName;
            FunctionBody = functionBody;
            ExtractedAt = DateTime.UtcNow;
        }

        public bool IsValid() =>
            !string.IsNullOrEmpty(FunctionName) &&
            !string.IsNullOrEmpty(FunctionBody);
    }
}