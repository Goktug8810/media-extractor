using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Application.YouTube;

public interface ISignatureCipherParser
{
    SignatureCipher Parse(string signatureCipher);
}