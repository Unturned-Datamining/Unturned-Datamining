using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieBossQuest : MonoBehaviour
{
    private static List<Player> nearbyPlayers = new List<Player>();

    public Transform target;

    public float sqrRadius;

    public ushort teleportEffect;

    private ZombieRegion region;

    private bool isListeningPlayer;

    private bool isListeningZombie;

    private IEnumerator teleport()
    {
        yield return new WaitForSeconds(3f);
        if (!(target != null))
        {
            yield break;
        }
        nearbyPlayers.Clear();
        PlayerTool.getPlayersInRadius(base.transform.position, sqrRadius, nearbyPlayers);
        for (int i = 0; i < nearbyPlayers.Count; i++)
        {
            Player player = nearbyPlayers[i];
            if (!player.life.isDead)
            {
                player.quests.sendRemoveQuest(213);
                player.quests.setFlag(213, 1);
                player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
            }
        }
    }

    private void onPlayerLifeUpdated(Player player)
    {
        if (!(player == null) && !player.life.IsAlive && !((player.transform.position - base.transform.position).sqrMagnitude > sqrRadius))
        {
            player.quests.sendRemoveQuest(213);
        }
    }

    private void onZombieLifeUpdated(Zombie zombie)
    {
        if (zombie.isDead && !((zombie.transform.position - base.transform.position).sqrMagnitude > sqrRadius))
        {
            EffectManager.sendEffect(teleportEffect, 16f, zombie.transform.position + Vector3.up);
            StartCoroutine("teleport");
        }
    }

    private void OnEnable()
    {
        if (!Provider.isServer)
        {
            return;
        }
        if (!isListeningPlayer)
        {
            PlayerLife.onPlayerLifeUpdated = (PlayerLifeUpdated)Delegate.Combine(PlayerLife.onPlayerLifeUpdated, new PlayerLifeUpdated(onPlayerLifeUpdated));
            isListeningPlayer = true;
        }
        if (region == null)
        {
            if (LevelNavigation.tryGetBounds(base.transform.position, out var bound))
            {
                region = ZombieManager.regions[bound];
            }
            if (region != null && !isListeningZombie)
            {
                ZombieRegion zombieRegion = region;
                zombieRegion.onZombieLifeUpdated = (ZombieLifeUpdated)Delegate.Combine(zombieRegion.onZombieLifeUpdated, new ZombieLifeUpdated(onZombieLifeUpdated));
                isListeningZombie = true;
            }
        }
    }

    private void OnDisable()
    {
        if (isListeningPlayer)
        {
            PlayerLife.onPlayerLifeUpdated = (PlayerLifeUpdated)Delegate.Remove(PlayerLife.onPlayerLifeUpdated, new PlayerLifeUpdated(onPlayerLifeUpdated));
            isListeningPlayer = false;
        }
        if (isListeningZombie && region != null)
        {
            ZombieRegion zombieRegion = region;
            zombieRegion.onZombieLifeUpdated = (ZombieLifeUpdated)Delegate.Remove(zombieRegion.onZombieLifeUpdated, new ZombieLifeUpdated(onZombieLifeUpdated));
            isListeningZombie = false;
        }
        region = null;
    }
}
