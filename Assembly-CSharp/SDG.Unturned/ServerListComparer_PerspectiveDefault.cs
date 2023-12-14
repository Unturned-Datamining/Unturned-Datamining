using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_PerspectiveDefault : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.cameraMode == rhs.cameraMode)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        return lhs.cameraMode.CompareTo(rhs.cameraMode);
    }
}
