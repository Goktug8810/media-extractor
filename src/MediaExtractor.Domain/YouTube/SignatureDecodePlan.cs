namespace MediaExtractor.Domain.YouTube
{
    public sealed class SignatureDecodePlan
    {
        public string PlayerId { get; }
        public IReadOnlyList<SignatureDecodeStep> Steps { get; }

        public SignatureDecodePlan(string playerId, IReadOnlyList<SignatureDecodeStep> steps)
        {
            PlayerId = playerId;
            Steps = steps;
        }
    }
}