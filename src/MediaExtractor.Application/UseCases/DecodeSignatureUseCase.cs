using MediaExtractor.Domain.YouTube;

namespace MediaExtractor.Application.YouTube
{
    public sealed class DecodeSignatureUseCase
    {
        private readonly ISignatureDecoder _decoder;
        private readonly ISignaturePlanExtractor _planExtractor;
        private readonly ISignaturePlanCache _planCache;

        public DecodeSignatureUseCase(
            ISignatureDecoder decoder,
            ISignaturePlanExtractor planExtractor,
            ISignaturePlanCache planCache)
        {
            _decoder = decoder;
            _planExtractor = planExtractor;
            _planCache = planCache;
        }

        public string Execute(SignatureCipher cipher, string baseJs, string playerId)
        {
            // 1) Cache’de plan var mı?
            var cachedPlan = _planCache.Get(playerId);
            if (cachedPlan is not null)
            {
                return _decoder.Decode(cipher.S, cachedPlan);
            }

            // 2) base.js’den yeni plan çıkar
            var plan = _planExtractor.Extract(playerId, baseJs);

            // 3) Plan'i cache'e koy
            _planCache.Set(plan);

            // 4) Decode et
            return _decoder.Decode(cipher.S, plan);
        }

        
    }
}