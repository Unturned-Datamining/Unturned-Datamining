using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieSoulTeleporter : MonoBehaviour
{
    private static List<Player> nearbyPlayers = new List<Player>();

    public Transform target;

    public Transform targetBoss;

    public float sqrRadius;

    public byte soulsNeeded;

    public ushort collectEffect;

    public ushort teleportEffect;

    private ZombieRegion region;

    private byte soulsCollected;

    private bool isListening;

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
            if (player.life.isDead)
            {
                continue;
            }
            if (player.quests.getFlag(211, out var value) && value == 1 && player.quests.getFlag(212, out var value2) && value2 == 1 && player.quests.getQuestStatus(213) != ENPCQuestStatus.COMPLETED)
            {
                player.quests.sendSetFlag(214, 0);
                player.quests.sendAddQuest(213);
                player.teleportToLocationUnsafe(targetBoss.position, targetBoss.rotation.eulerAngles.y);
                continue;
            }
            player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
            if (player.equipment.isSelected)
            {
                player.equipment.dequip();
            }
            player.equipment.canEquip = false;
        }
    }

    private void onZombieLifeUpdated(Zombie zombie)
    {
        if (zombie.isDead && !((zombie.transform.position - base.transform.position).sqrMagnitude > sqrRadius))
        {
            EffectManager.sendEffect(collectEffect, 16f, zombie.transform.position + Vector3.up, (base.transform.position - zombie.transform.position + Vector3.up).normalized);
            soulsCollected++;
            if (soulsCollected >= soulsNeeded)
            {
                EffectManager.sendEffect(teleportEffect, 16f, base.transform.position);
                soulsCollected = 0;
                StartCoroutine("teleport");
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
