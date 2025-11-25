namespace MediaExtractor.Domain.YouTube;

public sealed class SignatureCipher
{
    public string Url { get; }
    public string S { get; }
    public string Sp { get; }

    public SignatureCipher(string url, string s, string? sp)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        if (string.IsNullOrWhiteSpace(s)) throw new ArgumentNullException(nameof(s));

        Url = url;
        S = s;
        Sp = string.IsNullOrWhiteSpace(sp) ? "sig" : sp;
    }
}