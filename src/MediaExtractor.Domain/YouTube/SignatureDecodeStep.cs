namespace MediaExtractor.Domain.YouTube
{
    public sealed record SignatureDecodeStep(SignatureOp Operation, int Argument);
}