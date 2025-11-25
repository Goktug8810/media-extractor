namespace MediaExtractor.Domain.YouTube
{
    public interface INParameterOptimizer
    {
        /// <summary>
        /// base.js içindeki dinamik n-transform fonksiyonunu kullanarak
        /// n parametresini optimize eder. Çözülemezse originalN döner.
        /// </summary>
        string Optimize(string baseJsContent, string originalN);
    }
}