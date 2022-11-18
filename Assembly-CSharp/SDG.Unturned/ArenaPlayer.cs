using System;

namespace SDG.Unturned;

public class ArenaPlayer
{
    private SteamPlayer _steamPlayer;

    private bool _hasDied;

    public float lastAreaDamage;

    public float timeOutsideArea;

    public SteamPlayer steamPlayer => _steamPlayer;

    public bool hasDied => _hasDied;

    private void onLifeUpdated(bool isDead)
    {
        if (isDead)
        {
            _hasDied = true;
        }
    }

    public ArenaPlayer(SteamPlayer newSteamPlayer)
    {
        _steamPlayer = newSteamPlayer;
        _hasDied = false;
        PlayerLife life = steamPlayer.player.life;
        life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(onLifeUpdated));
    }
}
