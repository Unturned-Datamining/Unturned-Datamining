using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieManager : SteamCaller
{
    private class ZombieSpecialityWeightedRandom : IComparer<ZombieSpecialityWeightedRandom.Entry>
    {
        public struct Entry
        {
            public EZombieSpeciality value;

            public float weight;

            public Entry(EZombieSpeciality value, float weight)
            {
                this.value = value;
                this.weight = weight;
            }
        }

        private List<Entry> entries;

        public float totalWeight { get; private set; }

        public void clear()
        {
            entries.Clear();
            totalWeight = 0f;
        }

        public void add(EZombieSpeciality value, float weight)
        {
            weight = Mathf.Max(weight, 0f);
            Entry item = new Entry(value, weight);
            int num = entries.BinarySearch(item, this);
            if (num < 0)
            {
                num = ~num;
            }
            entries.Insert(num, item);
            totalWeight += weight;
        }

        public EZombieSpeciality get()
        {
            if (entries.Count < 1)
            {
                return EZombieSpeciality.NONE;
            }
            float num = UnityEngine.Random.value * totalWeight;
            foreach (Entry entry in entries)
            {
                if (num < entry.weight)
                {
                    return entry.value;
                }
                num -= entry.weight;
            }
            return entries[0].value;
        }

        public void log()
        {
            UnturnedLog.info("Entries: {0} Total Weight: {1}", entries.Count, totalWeight);
            foreach (Entry entry in entries)
            {
                UnturnedLog.info("{0}: {1}", entry.value, entry.weight);
            }
        }

        public int Compare(Entry lhs, Entry rhs)
        {
            return -lhs.weight.CompareTo(rhs.weight);
        }

        public ZombieSpecialityWeightedRandom()
        {
            entries = new List<Entry>();
            totalWeight = 0f;
        }
    }

    public static AudioClip[] roars;

    public static AudioClip[] groans;

    public static AudioClip[] spits;

    public static AudioClip[] dl_attacks;

    public static AudioClip[] dl_deaths;

    public static AudioClip[] dl_enemy_spotted;

    public static AudioClip[] dl_taunt;

    private static ZombieManager manager;

    private static ZombieRegion[] _regions;

    public static int wanderingCount;

    private static int tickIndex;

    private static List<Zombie> _tickingZombies;

    private static byte respawnZombiesBound;

    private static float lastWave;

    private static bool _waveReady;

    private static int _waveIndex;

    private static int _waveRemaining;

    private static float lastTick;

    public static WaveUpdated onWaveUpdated;

    private static readonly ClientStaticMethod<byte, bool> SendBeacon = ClientStaticMethod<byte, bool>.Get(ReceiveBeacon);

    private static readonly ClientStaticMethod<bool, int> SendWave = ClientStaticMethod<bool, int>.Get(ReceiveWave);

    private static readonly ClientStaticMethod<byte, ushort, byte, byte, byte, byte, byte, byte, Vector3, byte> SendZombieAlive = ClientStaticMethod<byte, ushort, byte, byte, byte, byte, byte, byte, Vector3, byte>.Get(ReceiveZombieAlive);

    private static readonly ClientStaticMethod<byte, ushort, Vector3, ERagdollEffect> SendZombieDead = ClientStaticMethod<byte, ushort, Vector3, ERagdollEffect>.Get(ReceiveZombieDead);

    private static uint seq;

    private static readonly ClientStaticMethod SendZombieStates = ClientStaticMethod.Get(ReceiveZombieStates);

    private static readonly ClientStaticMethod<byte, ushort, EZombieSpeciality> SendZombieSpeciality = ClientStaticMethod<byte, ushort, EZombieSpeciality>.Get(ReceiveZombieSpeciality);

    private static readonly ClientStaticMethod<byte, ushort> SendZombieThrow = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieThrow);

    private static readonly ClientStaticMethod<byte, ushort, Vector3, Vector3> SendZombieBoulder = ClientStaticMethod<byte, ushort, Vector3, Vector3>.Get(ReceiveZombieBoulder);

    private static readonly ClientStaticMethod<byte, ushort> SendZombieSpit = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieSpit);

    private static readonly ClientStaticMethod<byte, ushort> SendZombieCharge = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieCharge);

    private static readonly ClientStaticMethod<byte, ushort> SendZombieStomp = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieStomp);

    private static readonly ClientStaticMethod<byte, ushort> SendZombieBreath = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieBreath);

    private static readonly ClientStaticMethod<byte, ushort, Vector3, Vector3> SendZombieAcid = ClientStaticMethod<byte, ushort, Vector3, Vector3>.Get(ReceiveZombieAcid);

    private static readonly ClientStaticMethod<byte, ushort, Vector3> SendZombieSpark = ClientStaticMethod<byte, ushort, Vector3>.Get(ReceiveZombieSpark);

    private static readonly ClientStaticMethod<byte, ushort, byte> SendZombieAttack = ClientStaticMethod<byte, ushort, byte>.Get(ReceiveZombieAttack);

    private static readonly ClientStaticMethod<byte, ushort, byte> SendZombieStartle = ClientStaticMethod<byte, ushort, byte>.Get(ReceiveZombieStartle);

    private static readonly ClientStaticMethod<byte, ushort, byte> SendZombieStun = ClientStaticMethod<byte, ushort, byte>.Get(ReceiveZombieStun);

    private static readonly ClientStaticMethod SendZombies = ClientStaticMethod.Get(ReceiveZombies);

    private static StaticResourceRef<GameObject> dedicatedZombiePrefab = new StaticResourceRef<GameObject>("Characters/Zombie_Dedicated");

    private static StaticResourceRef<GameObject> serverZombiePrefab = new StaticResourceRef<GameObject>("Characters/Zombie_Server");

    private static StaticResourceRef<GameObject> clientZombiePrefab = new StaticResourceRef<GameObject>("Characters/Zombie_Client");

    private static ZombieSpecialityWeightedRandom zombieSpecialityTable = new ZombieSpecialityWeightedRandom();

    internal static readonly AssetReference<EffectAsset> Souls_1_Ref = new AssetReference<EffectAsset>("c17b00f2a58646c8a9ea728f6d72e54e");

    public static ZombieManager instance => manager;

    public static ZombieRegion[] regions => _regions;

    public static List<Zombie> tickingZombies => _tickingZombies;

    public static bool canSpareWanderer
    {
        get
        {
            if (wanderingCount < 8)
            {
                return tickingZombies.Count < 50;
            }
            return false;
        }
    }

    public static bool waveReady => _waveReady;

    public static int waveIndex => _waveIndex;

    public static int waveRemaining => _waveRemaining;

    public static void getZombiesInRadius(Vector3 center, float sqrRadius, List<Zombie> result)
    {
        if (regions == null || !LevelNavigation.tryGetNavigation(center, out var nav) || regions[nav] == null || regions[nav].zombies == null)
        {
            return;
        }
        for (int i = 0; i < regions[nav].zombies.Count; i++)
        {
            Zombie zombie = regions[nav].zombies[i];
            if (!(zombie == null) && (zombie.transform.position - center).sqrMagnitude < sqrRadius)
            {
                result.Add(zombie);
            }
        }
    }

    [Obsolete]
    public void tellBeacon(CSteamID steamID, byte reference, bool hasBeacon)
    {
        ReceiveBeacon(reference, hasBeacon);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellBeacon")]
    public static void ReceiveBeacon(byte reference, bool hasBeacon)
    {
        if (regions != null && reference < regions.Length && (Provider.isServer || regions[reference].isNetworked))
        {
            regions[reference].hasBeacon = hasBeacon;
        }
    }

    [Obsolete]
    public void tellWave(CSteamID steamID, bool newWaveReady, int newWave)
    {
        ReceiveWave(newWaveReady, newWave);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWave")]
    public static void ReceiveWave(bool newWaveReady, int newWave)
    {
        _waveReady = newWaveReady;
        _waveIndex = newWave;
        onWaveUpdated?.Invoke(waveReady, waveIndex);
    }

    [Obsolete]
    public void askWave(CSteamID steamID)
    {
    }

    internal static void SendInitialGlobalState(SteamPlayer client)
    {
        if (Level.info.type == ELevelType.HORDE)
        {
            SendWave.Invoke(ENetReliability.Reliable, client.transportConnection, waveReady, waveIndex);
        }
    }

    [Obsolete]
    public void tellZombieAlive(CSteamID steamID, byte reference, ushort id, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
    {
        ReceiveZombieAlive(reference, id, newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellZombieAlive")]
    public static void ReceiveZombieAlive(byte reference, ushort id, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].tellAlive(newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);
        }
    }

    [Obsolete]
    public void tellZombieDead(CSteamID steamID, byte reference, ushort id, Vector3 newRagdoll, byte newRagdollEffect)
    {
        ReceiveZombieDead(reference, id, newRagdoll, (ERagdollEffect)newRagdollEffect);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellZombieDead")]
    public static void ReceiveZombieDead(byte reference, ushort id, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].tellDead(newRagdoll, newRagdollEffect);
        }
    }

    [Obsolete]
    public void tellZombieStates(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveZombieStates(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        if (value >= regions.Length || (!Provider.isServer && !regions[value].isNetworked))
        {
            return;
        }
        reader.ReadUInt32(out var value2);
        if (value2 <= seq)
        {
            return;
        }
        seq = value2;
        reader.ReadUInt16(out var value3);
        if (value3 < 1)
        {
            return;
        }
        for (ushort num = 0; num < value3; num = (ushort)(num + 1))
        {
            reader.ReadUInt16(out var value4);
            reader.ReadClampedVector3(out var value5);
            reader.ReadDegrees(out var value6);
            if (value4 < regions[value].zombies.Count)
            {
                regions[value].zombies[value4].tellState(value5, value6);
            }
        }
    }

    [Obsolete]
    public void tellZombieSpeciality(CSteamID steamID, byte reference, ushort id, byte speciality)
    {
        ReceiveZombieSpeciality(reference, id, (EZombieSpeciality)speciality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellZombieSpeciality")]
    public static void ReceiveZombieSpeciality(byte reference, ushort id, EZombieSpeciality speciality)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].tellSpeciality(speciality);
        }
    }

    [Obsolete]
    public void askZombieThrow(CSteamID steamID, byte reference, ushort id)
    {
        ReceiveZombieThrow(reference, id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieThrow")]
    public static void ReceiveZombieThrow(byte reference, ushort id)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askThrow();
        }
    }

    [Obsolete]
    public void askZombieBoulder(CSteamID steamID, byte reference, ushort id, Vector3 origin, Vector3 direction)
    {
        ReceiveZombieBoulder(reference, id, origin, direction);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieBoulder")]
    public static void ReceiveZombieBoulder(byte reference, ushort id, Vector3 origin, Vector3 direction)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askBoulder(origin, direction);
        }
    }

    [Obsolete]
    public void askZombieSpit(CSteamID steamID, byte reference, ushort id)
    {
        ReceiveZombieSpit(reference, id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieSpit")]
    public static void ReceiveZombieSpit(byte reference, ushort id)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askSpit();
        }
    }

    [Obsolete]
    public void askZombieCharge(CSteamID steamID, byte reference, ushort id)
    {
        ReceiveZombieCharge(reference, id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieCharge")]
    public static void ReceiveZombieCharge(byte reference, ushort id)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askCharge();
        }
    }

    [Obsolete]
    public void askZombieStomp(CSteamID steamID, byte reference, ushort id)
    {
        ReceiveZombieStomp(reference, id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieStomp")]
    public static void ReceiveZombieStomp(byte reference, ushort id)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askStomp();
        }
    }

    [Obsolete]
    public void askZombieBreath(CSteamID steamID, byte reference, ushort id)
    {
        ReceiveZombieBreath(reference, id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieBreath")]
    public static void ReceiveZombieBreath(byte reference, ushort id)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askBreath();
        }
    }

    [Obsolete]
    public void askZombieAcid(CSteamID steamID, byte reference, ushort id, Vector3 origin, Vector3 direction)
    {
        ReceiveZombieAcid(reference, id, origin, direction);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieAcid")]
    public static void ReceiveZombieAcid(byte reference, ushort id, Vector3 origin, Vector3 direction)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askAcid(origin, direction);
        }
    }

    [Obsolete]
    public void askZombieSpark(CSteamID steamID, byte reference, ushort id, Vector3 target)
    {
        ReceiveZombieSpark(reference, id, target);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieSpark")]
    public static void ReceiveZombieSpark(byte reference, ushort id, Vector3 target)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askSpark(target);
        }
    }

    [Obsolete]
    public void askZombieAttack(CSteamID steamID, byte reference, ushort id, byte attack)
    {
        ReceiveZombieAttack(reference, id, attack);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieAttack")]
    public static void ReceiveZombieAttack(byte reference, ushort id, byte attack)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askAttack(attack);
        }
    }

    [Obsolete]
    public void askZombieStartle(CSteamID steamID, byte reference, ushort id, byte startle)
    {
        ReceiveZombieStartle(reference, id, startle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieStartle")]
    public static void ReceiveZombieStartle(byte reference, ushort id, byte startle)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askStartle(startle);
        }
    }

    [Obsolete]
    public void askZombieStun(CSteamID steamID, byte reference, ushort id, byte stun)
    {
        ReceiveZombieStun(reference, id, stun);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askZombieStun")]
    public static void ReceiveZombieStun(byte reference, ushort id, byte stun)
    {
        if ((Provider.isServer || regions[reference].isNetworked) && id < regions[reference].zombies.Count)
        {
            regions[reference].zombies[id].askStun(stun);
        }
    }

    [Obsolete]
    public void tellZombies(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveZombies(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        if (!regions[value].isNetworked)
        {
            regions[value].isNetworked = true;
            reader.ReadBit(out var value2);
            reader.ReadUInt16(out var value3);
            for (ushort num = 0; num < value3; num = (ushort)(num + 1))
            {
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt8(out var value6);
                reader.ReadUInt8(out var value7);
                reader.ReadUInt8(out var value8);
                reader.ReadUInt8(out var value9);
                reader.ReadUInt8(out var value10);
                reader.ReadUInt8(out var value11);
                reader.ReadClampedVector3(out var value12);
                reader.ReadDegrees(out var value13);
                reader.ReadBit(out var value14);
                manager.addZombie(value, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14);
            }
            regions[value].hasBeacon = value2;
        }
    }

    [Obsolete]
    public void askZombies(CSteamID steamID, byte bound)
    {
    }

    private void SendZombiesToPlayer(ITransportConnection transportConnection, byte bound)
    {
        ZombieRegion region = regions[bound];
        SendZombies.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt8(bound);
            writer.WriteBit(region.hasBeacon);
            writer.WriteUInt16((ushort)region.zombies.Count);
            for (ushort num = 0; num < region.zombies.Count; num = (ushort)(num + 1))
            {
                Zombie zombie = region.zombies[num];
                writer.WriteUInt8(zombie.type);
                writer.WriteUInt8((byte)zombie.speciality);
                writer.WriteUInt8(zombie.shirt);
                writer.WriteUInt8(zombie.pants);
                writer.WriteUInt8(zombie.hat);
                writer.WriteUInt8(zombie.gear);
                writer.WriteUInt8(zombie.move);
                writer.WriteUInt8(zombie.idle);
                writer.WriteClampedVector3(zombie.transform.position);
                writer.WriteDegrees(zombie.transform.eulerAngles.y);
                writer.WriteBit(zombie.isDead);
            }
        });
    }

    public static void sendZombieAlive(Zombie zombie, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
    {
        SendZombieAlive.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);
        regions[zombie.bound].onZombieLifeUpdated?.Invoke(zombie);
    }

    public static void sendZombieDead(Zombie zombie, Vector3 newRagdoll, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE)
    {
        SendZombieDead.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, newRagdoll, newRagdollEffect);
        regions[zombie.bound].onZombieLifeUpdated?.Invoke(zombie);
    }

    public static void sendZombieSpeciality(Zombie zombie, EZombieSpeciality speciality)
    {
        SendZombieSpeciality.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, speciality);
    }

    public static void sendZombieThrow(Zombie zombie)
    {
        SendZombieThrow.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
    }

    public static void sendZombieSpit(Zombie zombie)
    {
        SendZombieSpit.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
    }

    public static void sendZombieCharge(Zombie zombie)
    {
        SendZombieCharge.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
    }

    public static void sendZombieStomp(Zombie zombie)
    {
        SendZombieStomp.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
    }

    public static void sendZombieBreath(Zombie zombie)
    {
        SendZombieBreath.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
    }

    public static void sendZombieBoulder(Zombie zombie, Vector3 origin, Vector3 direction)
    {
        SendZombieBoulder.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, origin, direction);
    }

    public static void sendZombieAcid(Zombie zombie, Vector3 origin, Vector3 direction)
    {
        SendZombieAcid.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, origin, direction);
    }

    public static void sendZombieSpark(Zombie zombie, Vector3 target)
    {
        SendZombieSpark.Invoke(ENetReliability.Unreliable, GatherClientConnections(zombie.bound), zombie.bound, zombie.id, target);
    }

    public static void sendZombieAttack(Zombie zombie, byte attack)
    {
        SendZombieAttack.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, attack);
    }

    public static void sendZombieStartle(Zombie zombie, byte startle)
    {
        SendZombieStartle.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, startle);
    }

    public static void sendZombieStun(Zombie zombie, byte stun)
    {
        SendZombieStun.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, stun);
    }

    public static void dropLoot(Zombie zombie)
    {
        int value = ((zombie.isBoss || zombie.speciality == EZombieSpeciality.BOSS_ALL) ? UnityEngine.Random.Range((int)Provider.modeConfigData.Zombies.Min_Boss_Drops, (int)(Provider.modeConfigData.Zombies.Max_Boss_Drops + 1)) : ((!zombie.isMega) ? UnityEngine.Random.Range((int)Provider.modeConfigData.Zombies.Min_Drops, (int)(Provider.modeConfigData.Zombies.Max_Drops + 1)) : UnityEngine.Random.Range((int)Provider.modeConfigData.Zombies.Min_Mega_Drops, (int)(Provider.modeConfigData.Zombies.Max_Mega_Drops + 1))));
        value = Mathf.Clamp(value, 0, 100);
        if (LevelZombies.tables[zombie.type].isMega)
        {
            regions[zombie.bound].lastMega = Time.realtimeSinceStartup;
            regions[zombie.bound].hasMega = false;
        }
        if (value <= 1 && !(UnityEngine.Random.value < Provider.modeConfigData.Zombies.Loot_Chance))
        {
            return;
        }
        if (LevelZombies.tables[zombie.type].lootID != 0)
        {
            for (int i = 0; i < value; i++)
            {
                ushort num = SpawnTableTool.resolve(LevelZombies.tables[zombie.type].lootID);
                if (num != 0)
                {
                    ItemManager.dropItem(new Item(num, EItemOrigin.WORLD), zombie.transform.position, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                }
            }
        }
        else
        {
            if (LevelZombies.tables[zombie.type].lootIndex >= LevelItems.tables.Count)
            {
                return;
            }
            for (int j = 0; j < value; j++)
            {
                ushort item = LevelItems.getItem(LevelZombies.tables[zombie.type].lootIndex);
                if (item != 0)
                {
                    ItemManager.dropItem(new Item(item, EItemOrigin.WORLD), zombie.transform.position, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                }
            }
        }
    }

    public void addZombie(byte bound, byte type, byte speciality, byte shirt, byte pants, byte hat, byte gear, byte move, byte idle, Vector3 position, float angle, bool isDead)
    {
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        GameObject original = (Dedicator.IsDedicatedServer ? ((GameObject)dedicatedZombiePrefab) : ((!Provider.isServer) ? ((GameObject)clientZombiePrefab) : ((GameObject)serverZombiePrefab)));
        GameObject obj = UnityEngine.Object.Instantiate(original, position, rotation);
        obj.name = "Zombie";
        Zombie component = obj.GetComponent<Zombie>();
        component.id = (ushort)regions[bound].zombies.Count;
        component.speciality = (EZombieSpeciality)speciality;
        component.bound = bound;
        component.zombieRegion = regions[bound];
        component.type = type;
        component.shirt = shirt;
        component.pants = pants;
        component.hat = hat;
        component.gear = gear;
        component.move = move;
        component.idle = idle;
        component.isDead = isDead;
        component.init();
        regions[bound].zombies.Add(component);
    }

    public static Zombie getZombie(Vector3 point, ushort id)
    {
        if (LevelNavigation.tryGetBounds(point, out var bound))
        {
            if (id >= regions[bound].zombies.Count)
            {
                return null;
            }
            if (regions[bound].zombies[id].isDead)
            {
                return null;
            }
            return regions[bound].zombies[id];
        }
        return null;
    }

    public static ZombieDifficultyAsset getDifficultyInBound(byte bound)
    {
        if (bound < LevelNavigation.flagData.Count)
        {
            return LevelNavigation.flagData[bound].resolveDifficulty();
        }
        return null;
    }

    private static EZombieSpeciality generateZombieSpeciality(byte bound, ZombieTable table)
    {
        ZombieDifficultyAsset zombieDifficultyAsset = getDifficultyInBound(bound);
        if (zombieDifficultyAsset == null || !zombieDifficultyAsset.Overrides_Spawn_Chance)
        {
            zombieDifficultyAsset = table.resolveDifficulty();
        }
        zombieSpecialityTable.clear();
        if (zombieDifficultyAsset != null && zombieDifficultyAsset.Overrides_Spawn_Chance)
        {
            zombieSpecialityTable.add(EZombieSpeciality.CRAWLER, zombieDifficultyAsset.Crawler_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.SPRINTER, zombieDifficultyAsset.Sprinter_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.FLANKER_FRIENDLY, zombieDifficultyAsset.Flanker_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BURNER, zombieDifficultyAsset.Burner_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.ACID, zombieDifficultyAsset.Acid_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELECTRIC, zombieDifficultyAsset.Boss_Electric_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_WIND, zombieDifficultyAsset.Boss_Wind_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_FIRE, zombieDifficultyAsset.Boss_Fire_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.SPIRIT, zombieDifficultyAsset.Spirit_Chance);
            if (Level.isLoaded && LightingManager.isNighttime)
            {
                zombieSpecialityTable.add(EZombieSpeciality.DL_RED_VOLATILE, zombieDifficultyAsset.DL_Red_Volatile_Chance);
                zombieSpecialityTable.add(EZombieSpeciality.DL_BLUE_VOLATILE, zombieDifficultyAsset.DL_Blue_Volatile_Chance);
            }
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELVER_STOMPER, zombieDifficultyAsset.Boss_Elver_Stomper_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_KUWAIT, zombieDifficultyAsset.Boss_Kuwait_Chance);
        }
        else
        {
            zombieSpecialityTable.add(EZombieSpeciality.CRAWLER, Provider.modeConfigData.Zombies.Crawler_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.SPRINTER, Provider.modeConfigData.Zombies.Sprinter_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.FLANKER_FRIENDLY, Provider.modeConfigData.Zombies.Flanker_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BURNER, Provider.modeConfigData.Zombies.Burner_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.ACID, Provider.modeConfigData.Zombies.Acid_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELECTRIC, Provider.modeConfigData.Zombies.Boss_Electric_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_WIND, Provider.modeConfigData.Zombies.Boss_Wind_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_FIRE, Provider.modeConfigData.Zombies.Boss_Fire_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.SPIRIT, Provider.modeConfigData.Zombies.Spirit_Chance);
            if (Level.isLoaded && LightingManager.isNighttime)
            {
                zombieSpecialityTable.add(EZombieSpeciality.DL_RED_VOLATILE, Provider.modeConfigData.Zombies.DL_Red_Volatile_Chance);
                zombieSpecialityTable.add(EZombieSpeciality.DL_BLUE_VOLATILE, Provider.modeConfigData.Zombies.DL_Blue_Volatile_Chance);
            }
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELVER_STOMPER, Provider.modeConfigData.Zombies.Boss_Elver_Stomper_Chance);
            zombieSpecialityTable.add(EZombieSpeciality.BOSS_KUWAIT, Provider.modeConfigData.Zombies.Boss_Kuwait_Chance);
        }
        zombieSpecialityTable.add(EZombieSpeciality.NORMAL, 1f - zombieSpecialityTable.totalWeight);
        return zombieSpecialityTable.get();
    }

    private static ZombieSpawnpoint getReplacementSpawnpointInBound(byte bound)
    {
        if (bound < LevelZombies.zombies.Length)
        {
            List<ZombieSpawnpoint> list = LevelZombies.zombies[bound];
            if (list.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                return list[index];
            }
            UnturnedLog.warn("Unable to replace zombie because spawns are empty in bound {0}", bound);
        }
        else
        {
            UnturnedLog.warn("Unable to replace zombie because bound {0} is out of range", bound);
        }
        return null;
    }

    public static void teleportZombieBackIntoMap(Zombie zombie)
    {
        ZombieSpawnpoint replacementSpawnpointInBound = getReplacementSpawnpointInBound(zombie.bound);
        if (replacementSpawnpointInBound != null)
        {
            EffectAsset effectAsset = Souls_1_Ref.Find();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.relevantDistance = 16f;
                parameters.position = zombie.transform.position + Vector3.up;
                EffectManager.triggerEffect(parameters);
            }
            Vector3 position = replacementSpawnpointInBound.point + Vector3.up;
            zombie.transform.position = position;
        }
    }

    public void generateZombies(byte bound)
    {
        if (LevelNavigation.bounds.Count == 0 || LevelZombies.zombies.Length == 0 || LevelNavigation.bounds.Count != LevelZombies.zombies.Length)
        {
            return;
        }
        List<ZombieSpawnpoint> list = LevelZombies.zombies[bound];
        if (list.Count <= 0)
        {
            return;
        }
        ZombieRegion zombieRegion = regions[bound];
        zombieRegion.alive = 0;
        List<ZombieSpawnpoint> list2 = new List<ZombieSpawnpoint>();
        foreach (ZombieSpawnpoint item in list)
        {
            if (SafezoneManager.checkPointValid(item.point))
            {
                list2.Add(item);
            }
        }
        int num;
        int num2;
        if (Level.info.type == ELevelType.HORDE)
        {
            num = 40;
            num2 = -1;
        }
        else
        {
            int b = Mathf.CeilToInt((float)list.Count * Provider.modeConfigData.Zombies.Spawn_Chance);
            num = Mathf.Min(LevelNavigation.flagData[bound].maxZombies, b);
            num2 = LevelNavigation.flagData[bound].maxBossZombies;
        }
        while (list2.Count > 0 && zombieRegion.zombies.Count < num)
        {
            int index = UnityEngine.Random.Range(0, list2.Count);
            ZombieSpawnpoint zombieSpawnpoint = list2[index];
            list2.RemoveAt(index);
            byte type = zombieSpawnpoint.type;
            ZombieTable zombieTable = LevelZombies.tables[type];
            if (!canRegionSpawnZombiesFromTable(zombieRegion, zombieTable))
            {
                continue;
            }
            EZombieSpeciality eZombieSpeciality = EZombieSpeciality.NORMAL;
            if (zombieTable.isMega)
            {
                zombieRegion.lastMega = Time.realtimeSinceStartup;
                zombieRegion.hasMega = true;
                eZombieSpeciality = EZombieSpeciality.MEGA;
            }
            else if (Level.info.type == ELevelType.SURVIVAL)
            {
                eZombieSpeciality = generateZombieSpeciality(bound, zombieTable);
            }
            if (num2 < 0 || !eZombieSpeciality.IsBoss() || zombieRegion.aliveBossZombieCount < num2)
            {
                if (zombieRegion.hasBeacon)
                {
                    BeaconManager.checkBeacon(bound).spawnRemaining();
                }
                byte shirt = byte.MaxValue;
                if (zombieTable.slots[0].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[0].chance)
                {
                    shirt = (byte)UnityEngine.Random.Range(0, zombieTable.slots[0].table.Count);
                }
                byte pants = byte.MaxValue;
                if (zombieTable.slots[1].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[1].chance)
                {
                    pants = (byte)UnityEngine.Random.Range(0, zombieTable.slots[1].table.Count);
                }
                byte hat = byte.MaxValue;
                if (zombieTable.slots[2].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[2].chance)
                {
                    hat = (byte)UnityEngine.Random.Range(0, zombieTable.slots[2].table.Count);
                }
                byte gear = byte.MaxValue;
                if (zombieTable.slots[3].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[3].chance)
                {
                    gear = (byte)UnityEngine.Random.Range(0, zombieTable.slots[3].table.Count);
                }
                byte move = (byte)UnityEngine.Random.Range(0, 4);
                byte idle = (byte)UnityEngine.Random.Range(0, 3);
                Vector3 point = zombieSpawnpoint.point;
                point += new Vector3(0f, 0.5f, 0f);
                addZombie(bound, type, (byte)eZombieSpeciality, shirt, pants, hat, gear, move, idle, point, UnityEngine.Random.Range(0f, 360f), !LevelNavigation.flagData[bound].spawnZombies || Level.info.type == ELevelType.HORDE);
            }
        }
    }

    private bool canRegionSpawnZombiesFromTable(ZombieRegion region, ZombieTable table)
    {
        if (region.hasBeacon)
        {
            return !table.isMega;
        }
        if (table.isMega)
        {
            if (!region.hasMega)
            {
                return Time.realtimeSinceStartup - region.lastMega > 600f;
            }
            return false;
        }
        return true;
    }

    public void respawnZombies()
    {
        ZombieRegion zombieRegion = regions[respawnZombiesBound];
        if (Level.info.type == ELevelType.HORDE)
        {
            if (waveRemaining > 0 || zombieRegion.alive > 0)
            {
                lastWave = Time.realtimeSinceStartup;
            }
            if (waveRemaining == 0)
            {
                if (zombieRegion.alive > 0)
                {
                    return;
                }
                if (!(Time.realtimeSinceStartup - lastWave > 10f) && waveIndex != 0)
                {
                    if (waveReady)
                    {
                        _waveReady = false;
                        SendWave.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), waveReady, waveIndex);
                    }
                    return;
                }
                if (!waveReady)
                {
                    _waveReady = true;
                    _waveIndex++;
                    _waveRemaining = (int)Mathf.Ceil(Mathf.Pow(waveIndex + 5, 1.5f));
                    SendWave.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), waveReady, waveIndex);
                }
            }
        }
        if (!LevelNavigation.flagData[respawnZombiesBound].spawnZombies || zombieRegion.zombies.Count <= 0 || (!Dedicator.IsDedicatedServer && !zombieRegion.hasBeacon && Level.info.type != ELevelType.HORDE) || (zombieRegion.hasBeacon && BeaconManager.checkBeacon(respawnZombiesBound).getRemaining() == 0))
        {
            return;
        }
        if (zombieRegion.respawnZombieIndex >= zombieRegion.zombies.Count)
        {
            zombieRegion.respawnZombieIndex = (ushort)(zombieRegion.zombies.Count - 1);
        }
        Zombie zombie = zombieRegion.zombies[zombieRegion.respawnZombieIndex];
        zombieRegion.respawnZombieIndex++;
        if (zombieRegion.respawnZombieIndex >= zombieRegion.zombies.Count)
        {
            zombieRegion.respawnZombieIndex = 0;
        }
        if (!zombie.isDead)
        {
            return;
        }
        float num = Provider.modeConfigData.Zombies.Respawn_Day_Time;
        if (zombieRegion.hasBeacon)
        {
            num = Provider.modeConfigData.Zombies.Respawn_Beacon_Time;
        }
        else if (LightingManager.isFullMoon)
        {
            num = Provider.modeConfigData.Zombies.Respawn_Night_Time;
        }
        if (!(Time.realtimeSinceStartup - zombie.lastDead > num))
        {
            return;
        }
        ZombieSpawnpoint zombieSpawnpoint = LevelZombies.zombies[respawnZombiesBound][UnityEngine.Random.Range(0, LevelZombies.zombies[respawnZombiesBound].Count)];
        if (!SafezoneManager.checkPointValid(zombieSpawnpoint.point))
        {
            return;
        }
        for (ushort num2 = 0; num2 < zombieRegion.zombies.Count; num2 = (ushort)(num2 + 1))
        {
            if (!zombieRegion.zombies[num2].isDead && (zombieRegion.zombies[num2].transform.position - zombieSpawnpoint.point).sqrMagnitude < 4f)
            {
                return;
            }
        }
        byte b = zombieSpawnpoint.type;
        ZombieTable zombieTable = LevelZombies.tables[b];
        if (!canRegionSpawnZombiesFromTable(zombieRegion, zombieTable))
        {
            return;
        }
        EZombieSpeciality eZombieSpeciality = EZombieSpeciality.NORMAL;
        if (zombieRegion.hasBeacon ? (BeaconManager.checkBeacon(respawnZombiesBound).getRemaining() == 1) : zombieTable.isMega)
        {
            if (!zombieTable.isMega)
            {
                for (byte b2 = 0; b2 < LevelZombies.tables.Count; b2 = (byte)(b2 + 1))
                {
                    ZombieTable zombieTable2 = LevelZombies.tables[b2];
                    if (zombieTable2.isMega)
                    {
                        b = b2;
                        zombieTable = zombieTable2;
                        break;
                    }
                }
            }
            zombieRegion.lastMega = Time.realtimeSinceStartup;
            zombieRegion.hasMega = true;
            eZombieSpeciality = EZombieSpeciality.MEGA;
        }
        else if (Level.info.type == ELevelType.SURVIVAL)
        {
            eZombieSpeciality = generateZombieSpeciality(respawnZombiesBound, zombieTable);
        }
        int maxBossZombies = LevelNavigation.flagData[respawnZombiesBound].maxBossZombies;
        if (maxBossZombies < 0 || !eZombieSpeciality.IsBoss() || zombieRegion.aliveBossZombieCount < maxBossZombies)
        {
            if (zombieRegion.hasBeacon)
            {
                BeaconManager.checkBeacon(respawnZombiesBound).spawnRemaining();
            }
            byte shirt = byte.MaxValue;
            if (zombieTable.slots[0].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[0].chance)
            {
                shirt = (byte)UnityEngine.Random.Range(0, zombieTable.slots[0].table.Count);
            }
            byte pants = byte.MaxValue;
            if (zombieTable.slots[1].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[1].chance)
            {
                pants = (byte)UnityEngine.Random.Range(0, zombieTable.slots[1].table.Count);
            }
            byte hat = byte.MaxValue;
            if (zombieTable.slots[2].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[2].chance)
            {
                hat = (byte)UnityEngine.Random.Range(0, zombieTable.slots[2].table.Count);
            }
            byte gear = byte.MaxValue;
            if (zombieTable.slots[3].table.Count > 0 && UnityEngine.Random.value < zombieTable.slots[3].chance)
            {
                gear = (byte)UnityEngine.Random.Range(0, zombieTable.slots[3].table.Count);
            }
            Vector3 point = zombieSpawnpoint.point;
            point += new Vector3(0f, 0.5f, 0f);
            zombie.sendRevive(b, (byte)eZombieSpeciality, shirt, pants, hat, gear, point, UnityEngine.Random.Range(0f, 360f));
            if (Level.info.type == ELevelType.HORDE)
            {
                _waveRemaining--;
            }
        }
    }

    private void onBoundUpdated(Player player, byte oldBound, byte newBound)
    {
        if (player.channel.isOwner && LevelNavigation.checkSafe(oldBound) && regions[oldBound].isNetworked)
        {
            regions[oldBound].destroy();
            regions[oldBound].isNetworked = false;
        }
        if (!Provider.isServer)
        {
            return;
        }
        if (LevelNavigation.checkSafe(oldBound) && player.movement.loadedBounds[oldBound].isZombiesLoaded)
        {
            player.movement.loadedBounds[oldBound].isZombiesLoaded = false;
        }
        if (LevelNavigation.checkSafe(newBound) && !player.movement.loadedBounds[newBound].isZombiesLoaded)
        {
            if (player.channel.isOwner)
            {
                generateZombies(newBound);
                regions[newBound].isNetworked = true;
            }
            else
            {
                SendZombiesToPlayer(player.channel.owner.transportConnection, newBound);
            }
            player.movement.loadedBounds[newBound].isZombiesLoaded = true;
        }
    }

    private void onPlayerCreated(Player player)
    {
        PlayerMovement movement = player.movement;
        movement.onBoundUpdated = (PlayerBoundUpdated)Delegate.Combine(movement.onBoundUpdated, new PlayerBoundUpdated(onBoundUpdated));
    }

    private void onLevelLoaded(int level)
    {
        if (level > Level.BUILD_INDEX_MENU)
        {
            seq = 0u;
            if (LevelNavigation.bounds == null)
            {
                return;
            }
            _regions = new ZombieRegion[LevelNavigation.bounds.Count];
            for (byte b = 0; b < regions.Length; b = (byte)(b + 1))
            {
                regions[b] = new ZombieRegion(b);
                Vector3 center = LevelNavigation.bounds[b].center;
                regions[b].isRadioactive = VolumeManager<DeadzoneVolume, DeadzoneVolumeManager>.Get().IsNavmeshCenterInsideAnyVolume(center);
            }
            wanderingCount = 0;
            tickIndex = 0;
            _tickingZombies = new List<Zombie>();
            respawnZombiesBound = 0;
            _waveReady = false;
            _waveIndex = 0;
            _waveRemaining = 0;
            onWaveUpdated = null;
            if (Dedicator.IsDedicatedServer)
            {
                if (LevelNavigation.bounds.Count == 0 || LevelZombies.zombies.Length == 0 || LevelNavigation.bounds.Count != LevelZombies.zombies.Length)
                {
                    return;
                }
                for (byte b2 = 0; b2 < LevelNavigation.bounds.Count; b2 = (byte)(b2 + 1))
                {
                    generateZombies(b2);
                }
            }
        }
        if (level > Level.BUILD_INDEX_SETUP && !Dedicator.IsDedicatedServer)
        {
            ZombieClothing.build();
        }
    }

    private void onDayNightUpdated(bool isDaytime)
    {
        if (!isDaytime)
        {
            return;
        }
        ZombieRegion[] array = regions;
        for (int i = 0; i < array.Length; i++)
        {
            foreach (Zombie zombie in array[i].zombies)
            {
                if (zombie.speciality.IsDLVolatile())
                {
                    zombie.killWithFireExplosion();
                }
            }
        }
    }

    private void onPostLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_MENU || regions == null)
        {
            return;
        }
        for (int i = 0; i < regions.Length; i++)
        {
            regions[i].init();
            if (Provider.isServer)
            {
                InteractableBeacon interactableBeacon = BeaconManager.checkBeacon((byte)i);
                if (interactableBeacon != null)
                {
                    interactableBeacon.init(regions[i].alive);
                }
                regions[i].hasBeacon = interactableBeacon != null;
            }
        }
        LightingManager.onDayNightUpdated = (DayNightUpdated)Delegate.Combine(LightingManager.onDayNightUpdated, new DayNightUpdated(onDayNightUpdated));
    }

    private void onBeaconUpdated(byte nav, bool hasBeacon)
    {
        if (Provider.isServer && regions != null && nav < regions.Length)
        {
            if (hasBeacon)
            {
                BeaconManager.checkBeacon(nav).init(regions[nav].alive);
            }
            SendBeacon.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(nav), nav, hasBeacon);
        }
    }

    private void updateRegionsAndSendZombieStates()
    {
        for (byte regionIndex = 0; regionIndex < regions.Length; regionIndex++)
        {
            ZombieRegion region = regions[regionIndex];
            region.UpdateRegion();
            if (region.updates <= 0)
            {
                continue;
            }
            if (Dedicator.IsDedicatedServer)
            {
                seq++;
                SendZombieStates.Invoke(ENetReliability.Unreliable, GatherRemoteClientConnections(regionIndex), delegate(NetPakWriter writer)
                {
                    writer.WriteUInt8(regionIndex);
                    writer.WriteUInt32(seq);
                    writer.WriteUInt16(region.updates);
                    foreach (Zombie zombie in region.zombies)
                    {
                        if (zombie.isUpdated)
                        {
                            zombie.isUpdated = false;
                            writer.WriteUInt16(zombie.id);
                            writer.WriteClampedVector3(zombie.transform.position);
                            writer.WriteDegrees(zombie.transform.eulerAngles.y);
                        }
                    }
                });
                region.updates = 0;
                continue;
            }
            foreach (Zombie zombie2 in region.zombies)
            {
                if (zombie2.isUpdated)
                {
                    zombie2.isUpdated = false;
                }
            }
            region.updates = 0;
        }
    }

    private void Update()
    {
        if (!Level.isLoaded || !Provider.isServer || LevelNavigation.bounds == null || LevelNavigation.bounds.Count == 0 || LevelZombies.zombies == null || LevelZombies.zombies.Length == 0 || LevelNavigation.bounds.Count != LevelZombies.zombies.Length || regions == null || tickingZombies == null)
        {
            return;
        }
        int num;
        int num2;
        if (Dedicator.IsDedicatedServer)
        {
            if (tickIndex >= tickingZombies.Count)
            {
                tickIndex = 0;
            }
            num = tickIndex;
            num2 = num + 50;
            if (num2 >= tickingZombies.Count)
            {
                num2 = tickingZombies.Count;
            }
            tickIndex = num2;
        }
        else
        {
            num = 0;
            num2 = tickingZombies.Count;
        }
        for (int num3 = num2 - 1; num3 >= num; num3--)
        {
            Zombie zombie = tickingZombies[num3];
            if (zombie == null)
            {
                UnturnedLog.error("Missing zombie " + num3);
            }
            else
            {
                zombie.tick();
            }
        }
        if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
        {
            lastTick += Provider.UPDATE_TIME;
            if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
            {
                lastTick = Time.realtimeSinceStartup;
            }
            updateRegionsAndSendZombieStates();
        }
        respawnZombies();
        respawnZombiesBound++;
        if (respawnZombiesBound >= LevelZombies.zombies.Length)
        {
            respawnZombiesBound = 0;
        }
    }

    private void Start()
    {
        manager = this;
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        Level.onPostLevelLoaded = (PostLevelLoaded)Delegate.Combine(Level.onPostLevelLoaded, new PostLevelLoaded(onPostLevelLoaded));
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
        BeaconManager.onBeaconUpdated = (BeaconUpdated)Delegate.Combine(BeaconManager.onBeaconUpdated, new BeaconUpdated(onBeaconUpdated));
        if (!Dedicator.IsDedicatedServer)
        {
            roars = new AudioClip[16];
            for (int i = 0; i < roars.Length; i++)
            {
                roars[i] = (AudioClip)Resources.Load("Sounds/Zombies/Roars/Roar_" + i);
            }
            groans = new AudioClip[5];
            for (int j = 0; j < groans.Length; j++)
            {
                groans[j] = (AudioClip)Resources.Load("Sounds/Zombies/Groans/Groan_" + j);
            }
            spits = new AudioClip[4];
            for (int k = 0; k < spits.Length; k++)
            {
                spits[k] = (AudioClip)Resources.Load("Sounds/Zombies/Spits/Spit_" + k);
            }
            dl_attacks = new AudioClip[6];
            for (int l = 0; l < dl_attacks.Length; l++)
            {
                dl_attacks[l] = Resources.Load<AudioClip>("Sounds/Zombies/DL_Volatile/volatile00_attack_0" + l);
            }
            dl_deaths = new AudioClip[4];
            for (int m = 0; m < dl_deaths.Length; m++)
            {
                dl_deaths[m] = Resources.Load<AudioClip>("Sounds/Zombies/DL_Volatile/volatile00_death_0" + m);
            }
            dl_enemy_spotted = new AudioClip[4];
            for (int n = 0; n < dl_enemy_spotted.Length; n++)
            {
                dl_enemy_spotted[n] = Resources.Load<AudioClip>("Sounds/Zombies/DL_Volatile/volatile00_enemy_spotted_0" + n);
            }
            dl_taunt = new AudioClip[4];
            for (int num = 0; num < dl_taunt.Length; num++)
            {
                dl_taunt[num] = Resources.Load<AudioClip>("Sounds/Zombies/DL_Volatile/volatile_taunt_0" + num);
            }
        }
    }

    public static PooledTransportConnectionList GatherClientConnections(byte bound)
    {
        PooledTransportConnectionList pooledTransportConnectionList = TransportConnectionListPool.Get();
        foreach (SteamPlayer client in Provider.clients)
        {
            if (client.player != null && client.player.movement.bound == bound)
            {
                pooledTransportConnectionList.Add(client.transportConnection);
            }
        }
        return pooledTransportConnectionList;
    }

    [Obsolete("Replaced by GatherClientConnections")]
    public static IEnumerable<ITransportConnection> EnumerateClients(byte bound)
    {
        return GatherClientConnections(bound);
    }

    public static PooledTransportConnectionList GatherRemoteClientConnections(byte bound)
    {
        PooledTransportConnectionList pooledTransportConnectionList = TransportConnectionListPool.Get();
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!client.IsLocalPlayer && client.player != null && client.player.movement.bound == bound)
            {
                pooledTransportConnectionList.Add(client.transportConnection);
            }
        }
        return pooledTransportConnectionList;
    }

    [Obsolete("Replaced by GatherRemoteClientConnections")]
    public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte bound)
    {
        return GatherRemoteClientConnections(bound);
    }
}
