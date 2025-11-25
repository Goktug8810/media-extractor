namespace MediaExtractor.Domain.YouTube
{
    public sealed class PlayerScriptResult
    {
        public string PlayerId { get; }
        public string BaseJs { get; }

        public PlayerScriptResult(string playerId, string baseJs)
        {
            PlayerId = playerId;
            BaseJs = baseJs;
        }
    }
}