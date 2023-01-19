using UnityEngine;

namespace SDG.Unturned;

public class GameMode
{
    public virtual GameObject getPlayerGameObject(SteamPlayerID playerID)
    {
        if (Dedicator.IsDedicatedServer)
        {
            return Object.Instantiate(Resources.Load<GameObject>("Characters/Player_Dedicated"));
        }
        if (playerID.steamID == Provider.client)
        {
            return Object.Instantiate(Resources.Load<GameObject>("Characters/Player_Server"));
        }
        return Object.Instantiate(Resources.Load<GameObject>("Characters/Player_Client"));
    }

    public GameMode()
    {
        UnturnedLog.info(this);
    }
}
