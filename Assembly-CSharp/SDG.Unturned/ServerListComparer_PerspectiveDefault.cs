using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_PerspectiveDefault : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        if (lhs.cameraMode == rhs.cameraMode)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        return lhs.cameraMode.CompareTo(rhs.cameraMode);
    }
}
