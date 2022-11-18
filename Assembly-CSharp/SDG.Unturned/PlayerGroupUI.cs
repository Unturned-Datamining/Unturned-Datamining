using System;
using System.Collections.Generic;

namespace SDG.Unturned;

internal class PlayerGroupUI : SleekWrapper
{
    private List<ISleekLabel> _groups;

    public List<ISleekLabel> groups => _groups;

    private void addGroup(SteamPlayer player)
    {
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = -100;
        sleekLabel.positionOffset_Y = -15;
        sleekLabel.sizeOffset_X = 200;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        AddChild(sleekLabel);
        sleekLabel.isVisible = false;
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
