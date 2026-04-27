using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieRegion
{
    public HyperUpdated onHyperUpdated;

    public ZombieLifeUpdated onZombieLifeUpdated;

    private List<Zombie> _zombies;

    public FlagData flagData;

    public ushort updates;

    public ushort respawnZombieIndex;

    /// <summary>
    /// Number of alive zombies.
    /// </summary>
    public int alive;

    public bool isNetworked;

    public float lastMega;

    public bool hasMega;

    private bool _hasBeacon;

    public bool isRadioactive;

    private Zombie bossZombie;

    /// <summary>
    /// Last time a quest boss was spawned.
    /// </summary>
    private float lastBossTime = -1f;

    private bool isInRegionsWithPlayersSet;

    private int _playerCountInRegion;

    internal int aliveBossZombieCount;

    public List<Zombie> zombies => _zombies;

    public byte nav { get; protected set; }

    public bool hasBeacon
    {
        get
        {
            return _hasBeacon;
        }
        set
        {
            if (value != _hasBeacon)
            {
                _hasBeacon = value;
                onHyperUpdated?.Invoke(isHyper);
            }
        }
    }

    public bool isHyper
    {
        get
        {
            if (!LightingManager.isFullMoon)
            {
                return hasBeacon;
            }
            return true;
        }
    }

    public bool HasInfiniteAgroRange
    {
        get
        {
            if (!hasBeacon)
            {
                if (flagData != null)
                {
                    return flagData.hyperAgro;
                }
                return false;
            }
            return true;
        }
    }

    public int PlayerCountInRegion
    {
        get
        {
            return _playerCountInRegion;
        }
        internal set
        {
            if (value == _playerCountInRegion)
            {
                return;
            }
            _playerCountInRegion = value;
            if (_playerCountInRegion < 1)
            {
                if (isInRegionsWithPlayersSet)
                {
                    isInRegionsWithPlayersSet = false;
                    ZombieManager.regionsWithPlayers.Remove(nav);
                }
            }
            else if (!isInRegionsWithPlayersSet)
            {
                isInRegionsWithPlayersSet = true;
                ZombieManager.regionsWithPlayers.Add(nav);
            }
        }
    }

    public int GetAliveBossZombieCount()
    {
        return aliveBossZombieCount;
    }

    /// <summary>
    /// Allow another quest to spawn a boss zombie immediately.
    /// </summary>
    public void resetQuestBossTimer()
    {
        lastBossTime = -1f;
    }

    /// <summary>
    /// Kills the boss zombie if nobody is around, if the boss was killed it calls UpdateBoss.
    /// </summary>
    public void UpdateRegion()
    {
        if (bossZombie == null)
        {
            return;
        }
        bool flag = false;
        bool flag2 = false;
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead)
            {
                if (steamPlayer.player.movement.bound == nav)
                {
                    flag = true;
                }
                if (steamPlayer.player.movement.nav == nav)
                {
                    flag2 = true;
                }
                if (flag && flag2)
                {
                    break;
                }
            }
        }
        if (flag)
        {
            if (bossZombie.isDead)
            {
                bossZombie = null;
                if (flag2)
                {
                    UpdateBoss();
                }
            }
        }
        else
        {
            bossZombie.askDamage(50000, Vector3.up, out var _, out var _, trackKill: false, dropLoot: false);
            resetQuestBossTimer();
        }
    }

    public Zombie FindBestZombieToRespawnDifferentSpeciality(EZombieSpeciality speciality)
    {
        foreach (Zombie zombie in zombies)
        {
            if (zombie != null && zombie.isDead)
            {
                return zombie;
            }
        }
        if (speciality != EZombieSpeciality.NORMAL)
        {
            foreach (Zombie zombie2 in zombies)
            {
                if (zombie2 != null && !zombie2.isHunting && zombie2.speciality == EZombieSpeciality.NORMAL)
                {
                    return zombie2;
                }
            }
        }
        foreach (Zombie zombie3 in zombies)
        {
            if (zombie3 != null && !zombie3.isHunting && zombie3.speciality != speciality)
            {
                return zombie3;
            }
        }
        foreach (Zombie zombie4 in zombies)
        {
            if (zombie4 != null && zombie4.speciality != speciality)
            {
                return zombie4;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks for players in the area with quests and spawns boss zombies accordingly.
    /// </summary>
    public void UpdateBoss()
    {
        if (bossZombie != null)
        {
            return;
        }
        bool flag = lastBossTime < 0f || Time.time - lastBossTime > Provider.modeConfigData.Zombies.Quest_Boss_Respawn_Interval;
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            if (steamPlayer.player == null || steamPlayer.player.movement == null || steamPlayer.player.life == null || steamPlayer.player.life.isDead || steamPlayer.player.movement.nav != nav)
            {
                continue;
            }
            for (int j = 0; j < steamPlayer.player.quests.questsList.Count; j++)
            {
                PlayerQuest playerQuest = steamPlayer.player.quests.questsList[j];
                if (playerQuest == null || playerQuest.asset == null)
                {
                    continue;
                }
                for (int k = 0; k < playerQuest.asset.conditions.Length; k++)
                {
                    if (!(playerQuest.asset.conditions[k] is NPCZombieKillsCondition nPCZombieKillsCondition) || nPCZombieKillsCondition.nav != nav || !nPCZombieKillsCondition.spawn || nPCZombieKillsCondition.isConditionMet(steamPlayer.player))
                    {
                        continue;
                    }
                    bool usesBossInterval = nPCZombieKillsCondition.usesBossInterval;
                    if (usesBossInterval && !flag)
                    {
                        continue;
                    }
                    int num = Mathf.Min(zombies.Count, nPCZombieKillsCondition.spawnQuantity);
                    int num2 = 0;
                    foreach (Zombie zombie2 in zombies)
                    {
                        if (zombie2 != null && !zombie2.isDead && zombie2.speciality == nPCZombieKillsCondition.zombie)
                        {
                            num2++;
                        }
                    }
                    int num3 = LevelZombies.FindTableIndexByUniqueId(nPCZombieKillsCondition.LevelTableUniqueId);
                    ZombieTable zombieTable = ((num3 >= 0) ? LevelZombies.tables[num3] : null);
                    int num4 = num2;
                    while (num4 < num)
                    {
                        Zombie zombie = FindBestZombieToRespawnDifferentSpeciality(nPCZombieKillsCondition.zombie);
                        if (zombie == null)
                        {
                            break;
                        }
                        Vector3 position = zombie.transform.position;
                        if (zombie.isDead)
                        {
                            for (int l = 0; l < 10; l++)
                            {
                                ZombieSpawnpoint zombieSpawnpoint = LevelZombies.zombies[nav][UnityEngine.Random.Range(0, LevelZombies.zombies[nav].Count)];
                                if (SafezoneManager.checkPointValid(zombieSpawnpoint.point))
                                {
                                    break;
                                }
                                position = zombieSpawnpoint.point;
                                position.y += 0.1f;
                            }
                        }
                        byte type = zombie.type;
                        byte shirt = zombie.shirt;
                        byte pants = zombie.pants;
                        byte hat = zombie.hat;
                        byte gear = zombie.gear;
                        if (zombieTable != null)
                        {
                            type = (byte)num3;
                            zombieTable.GetSpawnClothingParameters(out shirt, out pants, out hat, out gear);
                        }
                        num4++;
                        zombie.sendRevive(type, (byte)nPCZombieKillsCondition.zombie, shirt, pants, hat, gear, position, UnityEngine.Random.Range(0f, 360f));
                        if (usesBossInterval)
                        {
                            bossZombie = zombie;
                        }
                    }
                    UnturnedLog.info("Spawned " + num4 + " " + nPCZombieKillsCondition.zombie.ToString() + " zombies in nav " + nav + " for quest " + playerQuest.id + ", isBoss " + usesBossInterval + " boss = " + bossZombie);
                }
            }
        }
    }

    private void onMoonUpdated(bool isFullMoon)
    {
        onHyperUpdated?.Invoke(isHyper);
    }

    public void destroy()
    {
        for (ushort num = 0; num < zombies.Count; num++)
        {
            UnityEngine.Object.Destroy(zombies[num].gameObject);
        }
        zombies.Clear();
        hasMega = false;
    }

    public void init()
    {
        LightingManager.onMoonUpdated = (MoonUpdated)Delegate.Combine(LightingManager.onMoonUpdated, new MoonUpdated(onMoonUpdated));
    }

    public ZombieRegion(byte newNav)
    {
        _zombies = new List<Zombie>();
        nav = newNav;
        if (nav < LevelNavigation.flagData.Count)
        {
            flagData = LevelNavigation.flagData[nav];
        }
        updates = 0;
        respawnZombieIndex = 0;
        alive = 0;
        isNetworked = false;
        lastMega = -1000f;
        hasMega = false;
    }
}
