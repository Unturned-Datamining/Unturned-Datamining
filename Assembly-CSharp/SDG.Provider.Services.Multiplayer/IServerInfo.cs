using SDG.Provider.Services.Community;

namespace SDG.Provider.Services.Multiplayer;

public interface IServerInfo
{
    ICommunityEntity entity { get; }

    string name { get; }

    byte players { get; }

    byte capacity { get; }

    int ping { get; }
}
