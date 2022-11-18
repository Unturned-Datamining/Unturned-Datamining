namespace SDG.Provider.Services.Matchmaking;

public class MatchmakingFilter : IMatchmakingFilter
{
    public string key { get; protected set; }

    public string value { get; protected set; }

    public MatchmakingFilter(string newKey, string newValue)
    {
        key = newKey;
        value = newValue;
    }
}
