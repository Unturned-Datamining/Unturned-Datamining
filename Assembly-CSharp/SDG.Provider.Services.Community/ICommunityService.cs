using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SDG.Provider.Services.Community;

public interface ICommunityService : IService
{
    void setStatus(string status);

    Texture2D getIcon(int id);

    Texture2D getIcon(CSteamID steamID, bool shouldCache = false);

    SteamGroup getCachedGroup(CSteamID steamID);

    SteamGroup[] getGroups();

    bool checkGroup(CSteamID steamID);
}
