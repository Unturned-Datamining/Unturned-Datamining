using SDG.Framework.IO.Streams;

namespace SDG.Provider.Services.Community;

public interface ICommunityEntity : INetworkStreamable
{
    bool isValid { get; }
}
