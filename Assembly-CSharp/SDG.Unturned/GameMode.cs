using UnityEngine;

namespace SDG.Unturned;

public class GameMode
{
    public virtual GameObject getPlayerGameObject(SteamPlayerID playerID)
    {
        return Object.Instantiate(Resources.Load<GameObject>("Characters/Player_Dedicated"));
    }

    public GameMode()
    {
        UnturnedLog.info(this);
    }
}
