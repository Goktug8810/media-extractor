using System;
using System.Linq;
using System.Web;
using MediaExtractor.Application.YouTube;
using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Infrastructure.YouTube;

public sealed class SignatureCipherParser : ISignatureCipherParser
{
    public SignatureCipher Parse(string signatureCipher)
    {
        if (string.IsNullOrWhiteSpace(signatureCipher))
            throw new ArgumentException("signatureCipher cannot be empty.", nameof(signatureCipher));

        // "s=xxxx&url=https%3A%2F%2F....&sp=signature"
        var query = HttpUtility.ParseQueryString(signatureCipher);

        string? url = query["url"];
        string? s   = query["s"];
        string? sp  = query["sp"];

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(s))
            throw new InvalidOperationException($"Invalid signatureCipher: '{signatureCipher}'");

        url = Uri.UnescapeDataString(url);

        return new SignatureCipher(url, s!, sp);
    }
}