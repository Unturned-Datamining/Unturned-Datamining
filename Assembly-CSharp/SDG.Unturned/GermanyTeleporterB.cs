using System;
using System.Collections;
using UnityEngine;

namespace SDG.Unturned;

public class GermanyTeleporterB : GermanyTeleporterA
{
    public float sqrBossRadius;

    public int navIndex;

    private ZombieRegion region;

    private bool isListeningPlayer;

    private bool isListeningZombie;

    protected override IEnumerator teleport()
    {
        yield return new WaitForSeconds(1f);
        if (!(target != null))
        {
            yield break;
        }
        GermanyTeleporterA.nearbyPlayers.Clear();
        PlayerTool.getPlayersInRadius(base.transform.position, sqrRadius, GermanyTeleporterA.nearbyPlayers);
        for (int i = 0; i < GermanyTeleporterA.nearbyPlayers.Count; i++)
        {
            Player player = GermanyTeleporterA.nearbyPlayers[i];
            if (!player.life.isDead && player.quests.getQuestStatus(248) == ENPCQuestStatus.COMPLETED)
            {
                player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
            }
        }
    }

    private void onPlayerLifeUpdated(Player player)
    {
        if (!(player == null) && !player.life.IsAlive && !((player.transform.position - base.transform.position).sqrMagnitude > sqrBossRadius) && player.quests.getQuestStatus(248) != ENPCQuestStatus.COMPLETED)
        {
            player.quests.sendRemoveQuest(248);
        }
    }

    private void onZombieLifeUpdated(Zombie zombie)
    {
        if (!zombie.isDead || (zombie.transform.position - base.transform.position).sqrMagnitude > sqrBossRadius)
        {
            return;
        }
        GermanyTeleporterA.nearbyPlayers.Clear();
        PlayerTool.getPlayersInRadius(base.transform.position, sqrBossRadius, GermanyTeleporterA.nearbyPlayers);
        for (int i = 0; i < GermanyTeleporterA.nearbyPlayers.Count; i++)
        {
            Player player = GermanyTeleporterA.nearbyPlayers[i];
            if (!player.life.isDead)
            {
                player.quests.sendRemoveQuest(248);
                player.quests.sendSetFlag(248, 1);
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
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
            region = ZombieManager.regions[navIndex];
            if (region != null && !isListeningZombie)
            {
                ZombieRegion zombieRegion = region;
                zombieRegion.onZombieLifeUpdated = (ZombieLifeUpdated)Delegate.Combine(zombieRegion.onZombieLifeUpdated, new ZombieLifeUpdated(onZombieLifeUpdated));
                isListeningZombie = true;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
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
