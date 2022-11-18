using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieSoulFlag : MonoBehaviour
{
    private static List<Player> nearbyPlayers = new List<Player>();

    public ushort flagPlaced;

    public ushort flagKills;

    public float sqrRadius;

    public byte soulsNeeded;

    public ushort collectEffect;

    public ushort teleportEffect;

    private ZombieRegion region;

    private bool isListening;

    private void onZombieLifeUpdated(Zombie zombie)
    {
        if (!zombie.isDead || (zombie.transform.position - base.transform.position).sqrMagnitude > sqrRadius)
        {
            return;
        }
        nearbyPlayers.Clear();
        PlayerTool.getPlayersInRadius(base.transform.position, sqrRadius, nearbyPlayers);
        for (int i = 0; i < nearbyPlayers.Count; i++)
        {
            Player player = nearbyPlayers[i];
            if (!player.life.isDead && player.quests.getFlag(flagPlaced, out var value) && value == 1)
            {
                EffectManager.sendEffect(collectEffect, player.channel.GetOwnerTransportConnection(), zombie.transform.position + Vector3.up, (base.transform.position - zombie.transform.position + Vector3.up).normalized);
                player.quests.getFlag(flagKills, out var value2);
                value2 = (short)(value2 + 1);
                player.quests.sendSetFlag(flagKills, value2);
                if (value2 >= soulsNeeded)
                {
                    EffectManager.sendEffect(teleportEffect, player.channel.GetOwnerTransportConnection(), base.transform.position);
                    player.quests.sendSetFlag(flagPlaced, 2);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (Provider.isServer && region == null)
        {
            if (LevelNavigation.tryGetBounds(base.transform.position, out var bound))
            {
                region = ZombieManager.regions[bound];
            }
            if (region != null && !isListening)
            {
                ZombieRegion zombieRegion = region;
                zombieRegion.onZombieLifeUpdated = (ZombieLifeUpdated)Delegate.Combine(zombieRegion.onZombieLifeUpdated, new ZombieLifeUpdated(onZombieLifeUpdated));
                isListening = true;
            }
        }
    }

    private void OnDisable()
    {
        if (isListening && region != null)
        {
            ZombieRegion zombieRegion = region;
            zombieRegion.onZombieLifeUpdated = (ZombieLifeUpdated)Delegate.Remove(zombieRegion.onZombieLifeUpdated, new ZombieLifeUpdated(onZombieLifeUpdated));
            isListening = false;
        }
        region = null;
    }
}
