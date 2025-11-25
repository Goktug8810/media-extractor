namespace MediaExtractor.Domain.YouTube
{
    public static class SignatureCipherExtensions
    {
        public static string ApplyTo(this SignatureCipher cipher, string decodedSignature)
        {
            return $"{cipher.Url}&{cipher.Sp}={decodedSignature}";
        }
    }
}