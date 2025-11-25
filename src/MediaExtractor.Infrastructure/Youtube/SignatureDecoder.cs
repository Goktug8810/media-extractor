using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Infrastructure.YouTube
{
    public sealed class SignatureDecoder : ISignatureDecoder
    {
        public string Decode(string s, SignatureDecodePlan plan)
        {
            var chars = s.ToCharArray();

            foreach (var step in plan.Steps)
            {
                switch (step.Operation)
                {
                    case SignatureOp.Reverse:
                        Array.Reverse(chars);
                        break;

                    case SignatureOp.Swap:
                        var idx = step.Argument % chars.Length;
                        (chars[0], chars[idx]) = (chars[idx], chars[0]);
                        break;

                    case SignatureOp.Slice:
                        chars = chars.Skip(step.Argument).ToArray();
                        break;
                }
            }

            return new string(chars);
        }
    }
}