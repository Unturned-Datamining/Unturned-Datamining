using System;
using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// HUD with projected labels for teammates.
/// </summary>
internal class PlayerGroupUI : SleekWrapper
{
    private List<ISleekLabel> _groups;

    public List<ISleekLabel> groups => _groups;

    private void addGroup(SteamPlayer player)
    {
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = -100f;
        sleekLabel.PositionOffset_Y = -15f;
        sleekLabel.SizeOffset_X = 200f;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(sleekLabel);
        sleekLabel.IsVisible = false;
        groups.Add(sleekLabel);
    }

    private void onEnemyConnected(SteamPlayer player)
    {
        addGroup(player);
    }

    private void onEnemyDisconnected(SteamPlayer player)
    {
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            if (Provider.clients[i] == player)
            {
                RemoveChild(groups[i]);
                groups.RemoveAt(i);
                break;
            }
        }
    }

    public override void OnDestroy()
    {
        Provider.onEnemyConnected = (Provider.EnemyConnected)Delegate.Remove(Provider.onEnemyConnected, new Provider.EnemyConnected(onEnemyConnected));
        Provider.onEnemyDisconnected = (Provider.EnemyDisconnected)Delegate.Remove(Provider.onEnemyDisconnected, new Provider.EnemyDisconnected(onEnemyDisconnected));
    }

    public PlayerGroupUI()
    {
        _groups = new List<ISleekLabel>();
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            addGroup(Provider.clients[i]);
        }
        Provider.onEnemyConnected = (Provider.EnemyConnected)Delegate.Combine(Provider.onEnemyConnected, new Provider.EnemyConnected(onEnemyConnected));
        Provider.onEnemyDisconnected = (Provider.EnemyDisconnected)Delegate.Combine(Provider.onEnemyDisconnected, new Provider.EnemyDisconnected(onEnemyDisconnected));
    }
}
