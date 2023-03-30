using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class AnimalManager : SteamCaller
{
    private class ValidAnimalSpawnsInfo
    {
        public List<AnimalSpawnpoint> spawns;

        public PackInfo pack;
    }

    private static AnimalManager manager;

    private static List<Animal> _animals;

    private static List<PackInfo> _packs;

    private static int tickIndex;

    private static List<Animal> _tickingAnimals;

    public static ushort updates;

    private static ushort respawnPackIndex;

    private static float lastTick;

    private static readonly ClientStaticMethod<ushort, Vector3, byte> SendAnimalAlive = ClientStaticMethod<ushort, Vector3, byte>.Get(ReceiveAnimalAlive);

    private static readonly ClientStaticMethod<ushort, Vector3, ERagdollEffect> SendAnimalDead = ClientStaticMethod<ushort, Vector3, ERagdollEffect>.Get(ReceiveAnimalDead);

    private static uint seq;

    private static readonly ClientStaticMethod SendAnimalStates = ClientStaticMethod.Get(ReceiveAnimalStates);

    private static readonly ClientStaticMethod<ushort> SendAnimalStartle = ClientStaticMethod<ushort>.Get(ReceiveAnimalStartle);

    private static readonly ClientStaticMethod<ushort> SendAnimalAttack = ClientStaticMethod<ushort>.Get(ReceiveAnimalAttack);

    private static readonly ClientStaticMethod<ushort> SendAnimalPanic = ClientStaticMethod<ushort>.Get(ReceiveAnimalPanic);

    private static readonly ClientStaticMethod SendMultipleAnimals = ClientStaticMethod.Get(ReceiveMultipleAnimals);

    private static readonly ClientStaticMethod SendSingleAnimal = ClientStaticMethod.Get(ReceiveSingleAnimal);

    private List<Animal> animalsToSend = new List<Animal>();

    public static List<Animal> animals => _animals;

    public static List<PackInfo> packs => _packs;

    public static List<Animal> tickingAnimals => _tickingAnimals;

    public static uint maxInstances => Level.info.size switch
    {
        ELevelSize.TINY => Provider.modeConfigData.Animals.Max_Instances_Tiny, 
        ELevelSize.SMALL => Provider.modeConfigData.Animals.Max_Instances_Small, 
        ELevelSize.MEDIUM => Provider.modeConfigData.Animals.Max_Instances_Medium, 
        ELevelSize.LARGE => Provider.modeConfigData.Animals.Max_Instances_Large, 
        ELevelSize.INSANE => Provider.modeConfigData.Animals.Max_Instances_Insane, 
        _ => 0u, 
    };

    public static bool giveAnimal(Player player, ushort id)
    {
        if (Assets.find(EAssetType.ANIMAL, id) is AnimalAsset)
        {
            Vector3 vector = player.transform.position + player.transform.forward * 6f;
            Physics.Raycast(vector + Vector3.up * 16f, Vector3.down, out var hitInfo, 32f, RayMasks.BLOCK_VEHICLE);
            if (hitInfo.collider != null)
            {
                vector = hitInfo.point;
            }
            spawnAnimal(id, vector, player.transform.rotation);
            return true;
        }
        return false;
    }

    public static void spawnAnimal(ushort id, Vector3 point, Quaternion angle)
    {
        foreach (Animal animal2 in animals)
        {
            if (animal2.id == id && animal2.isDead)
            {
                animal2.sendRevive(point, UnityEngine.Random.Range(0f, 360f));
                return;
            }
        }
        if (Assets.find(EAssetType.ANIMAL, id) is AnimalAsset)
        {
            Animal animal = manager.addAnimal(id, point, angle.eulerAngles.y, isDead: false);
            AnimalSpawnpoint item = new AnimalSpawnpoint(0, point);
            PackInfo packInfo = new PackInfo();
            animal.pack = packInfo;
            packInfo.animals.Add(animal);
            packInfo.spawns.Add(item);
            packs.Add(packInfo);
            SendSingleAnimal.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), delegate(NetPakWriter writer)
            {
                WriteSingleAnimal(animal, writer);
            });
        }
    }

    public static void getAnimalsInRadius(Vector3 center, float sqrRadius, List<Animal> result)
    {
        if (animals == null)
        {
            return;
        }
        for (int i = 0; i < animals.Count; i++)
        {
            Animal animal = animals[i];
            if ((animal.transform.position - center).sqrMagnitude < sqrRadius)
            {
                result.Add(animal);
            }
        }
    }

    [Obsolete]
    public void tellAnimalAlive(CSteamID steamID, ushort index, Vector3 newPosition, byte newAngle)
    {
        ReceiveAnimalAlive(index, newPosition, newAngle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellAnimalAlive")]
    public static void ReceiveAnimalAlive(ushort index, Vector3 newPosition, byte newAngle)
    {
        if (index < animals.Count)
        {
            animals[index].tellAlive(newPosition, newAngle);
        }
    }

    [Obsolete]
    public void tellAnimalDead(CSteamID steamID, ushort index, Vector3 newRagdoll, byte newRagdollEffect)
    {
        ReceiveAnimalDead(index, newRagdoll, (ERagdollEffect)newRagdollEffect);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellAnimalDead")]
    public static void ReceiveAnimalDead(ushort index, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
    {
        if (index < animals.Count)
        {
            animals[index].tellDead(newRagdoll, newRagdollEffect);
        }
    }

    [Obsolete]
    public void tellAnimalStates(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveAnimalStates(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        if (value <= seq)
        {
            return;
        }
        seq = value;
        reader.ReadUInt16(out var value2);
        if (value2 < 1)
        {
            return;
        }
        for (ushort num = 0; num < value2; num = (ushort)(num + 1))
        {
            reader.ReadUInt16(out var value3);
            reader.ReadClampedVector3(out var value4);
            reader.ReadDegrees(out var value5);
            if (value3 < animals.Count)
            {
                animals[value3].tellState(value4, value5);
            }
        }
    }

    [Obsolete]
    public void askAnimalStartle(CSteamID steamID, ushort index)
    {
        ReceiveAnimalStartle(index);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askAnimalStartle")]
    public static void ReceiveAnimalStartle(ushort index)
    {
        if (index < animals.Count)
        {
            animals[index].askStartle();
        }
    }

    [Obsolete]
    public void askAnimalAttack(CSteamID steamID, ushort index)
    {
        ReceiveAnimalAttack(index);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askAnimalAttack")]
    public static void ReceiveAnimalAttack(ushort index)
    {
        if (index < animals.Count)
        {
            animals[index].askAttack();
        }
    }

    [Obsolete]
    public void askAnimalPanic(CSteamID steamID, ushort index)
    {
        ReceiveAnimalPanic(index);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askAnimalPanic")]
    public static void ReceiveAnimalPanic(ushort index)
    {
        if (index < animals.Count)
        {
            animals[index].askPanic();
        }
    }

    [Obsolete]
    public void tellAnimals(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveMultipleAnimals(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        for (ushort num = 0; num < value; num = (ushort)(num + 1))
        {
            ReadSingleAnimal(reader);
        }
    }

    private static void ReadSingleAnimal(NetPakReader reader)
    {
        reader.ReadUInt16(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadDegrees(out var value3);
        reader.ReadBit(out var value4);
        manager.addAnimal(value, value2, value3, value4);
    }

    [Obsolete]
    public void tellAnimal(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveSingleAnimal(in ClientInvocationContext context)
    {
        ReadSingleAnimal(context.reader);
    }

    [Obsolete]
    public void askAnimals(CSteamID steamID)
    {
    }

    internal static void SendInitialGlobalState(ITransportConnection transportConnection)
    {
        SendMultipleAnimals.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt16((ushort)animals.Count);
            for (ushort num = 0; num < animals.Count; num = (ushort)(num + 1))
            {
                WriteSingleAnimal(animals[num], writer);
            }
        });
    }

    [Obsolete]
    public void sendAnimal(Animal animal, NetPakWriter writer)
    {
    }

    private static void WriteSingleAnimal(Animal animal, NetPakWriter writer)
    {
        writer.WriteUInt16(animal.id);
        writer.WriteClampedVector3(animal.transform.position);
        writer.WriteDegrees(animal.transform.eulerAngles.y);
        writer.WriteBit(animal.isDead);
    }

    public static void sendAnimalAlive(Animal animal, Vector3 newPosition, byte newAngle)
    {
        SendAnimalAlive.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), animal.index, newPosition, newAngle);
    }

    public static void sendAnimalDead(Animal animal, Vector3 newRagdoll, ERagdollEffect newRagdollEffect = ERagdollEffect.NONE)
    {
        SendAnimalDead.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), animal.index, newRagdoll, newRagdollEffect);
    }

    public static void sendAnimalStartle(Animal animal)
    {
        SendAnimalStartle.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), animal.index);
    }

    public static void sendAnimalAttack(Animal animal)
    {
        SendAnimalAttack.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), animal.index);
    }

    public static void sendAnimalPanic(Animal animal)
    {
        SendAnimalPanic.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), animal.index);
    }

    public static void dropLoot(Animal animal)
    {
        if (animal == null || animal.asset == null || animal.transform == null)
        {
            return;
        }
        if (animal.asset.rewardID != 0)
        {
            int value = UnityEngine.Random.Range(animal.asset.rewardMin, animal.asset.rewardMax + 1);
            value = Mathf.Clamp(value, 0, 100);
            for (int i = 0; i < value; i++)
            {
                ushort num = SpawnTableTool.resolve(animal.asset.rewardID);
                if (num != 0)
                {
                    ItemManager.dropItem(new Item(num, EItemOrigin.NATURE), animal.transform.position, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                }
            }
            return;
        }
        if (animal.asset.meat != 0)
        {
            int num2 = UnityEngine.Random.Range(2, 5);
            for (int j = 0; j < num2; j++)
            {
                ItemManager.dropItem(new Item(animal.asset.meat, EItemOrigin.NATURE), animal.transform.position, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
            }
        }
        if (animal.asset.pelt != 0)
        {
            int num3 = UnityEngine.Random.Range(2, 5);
            for (int k = 0; k < num3; k++)
            {
                ItemManager.dropItem(new Item(animal.asset.pelt, EItemOrigin.NATURE), animal.transform.position, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
            }
        }
    }

    private Animal addAnimal(ushort id, Vector3 point, float angle, bool isDead)
    {
        if (Assets.find(EAssetType.ANIMAL, id) is AnimalAsset animalAsset)
        {
            GameObject original = (Dedicator.IsDedicatedServer ? animalAsset.dedicated : ((!Provider.isServer) ? animalAsset.client : animalAsset.server));
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            GameObject obj = UnityEngine.Object.Instantiate(original, point, rotation);
            obj.name = id.ToString();
            Animal animal = obj.AddComponent<Animal>();
            animal.index = (ushort)animals.Count;
            animal.id = id;
            animal.isDead = isDead;
            animal.init();
            animals.Add(animal);
            return animal;
        }
        return null;
    }

    public static Animal getAnimal(ushort index)
    {
        if (index >= animals.Count)
        {
            return null;
        }
        return animals[index];
    }

    public static void TeleportAnimalBackIntoMap(Animal animal)
    {
        Vector3? vector = null;
        if (animal.pack != null)
        {
            if (animal.pack.animals != null)
            {
                foreach (Animal animal2 in animal.pack.animals)
                {
                    if (!(animal == animal2) && !animal2.isDead)
                    {
                        Vector3 position = animal2.transform.position;
                        if (UndergroundAllowlist.IsPositionWithinValidHeight(position))
                        {
                            vector = position;
                            break;
                        }
                    }
                }
            }
            if (!vector.HasValue && animal.pack.spawns != null && animal.pack.spawns.Count > 0)
            {
                vector = animal.pack.spawns[animal.pack.spawns.GetRandomIndex()].point;
            }
        }
        if (!vector.HasValue)
        {
            if (LevelAnimals.spawns != null && LevelAnimals.spawns.Count > 0)
            {
                vector = LevelAnimals.spawns[LevelAnimals.spawns.GetRandomIndex()].point;
            }
            else
            {
                Vector3 position2 = animal.transform.position;
                position2.y = Level.HEIGHT - 10f;
                vector = position2;
            }
        }
        EffectAsset effectAsset = ZombieManager.Souls_1_Ref.Find();
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.relevantDistance = 16f;
            parameters.position = animal.transform.position + Vector3.up;
            EffectManager.triggerEffect(parameters);
        }
        animal.transform.position = vector.Value + Vector3.up;
    }

    public static void askClearAllAnimals()
    {
        foreach (Animal animal in animals)
        {
            animal.askDamage(ushort.MaxValue, Vector3.up, out var _, out var _, trackKill: false, dropLoot: false);
        }
    }

    private void respawnAnimals()
    {
        if (Level.info == null || Level.info.type == ELevelType.ARENA)
        {
            return;
        }
        if (respawnPackIndex >= packs.Count)
        {
            respawnPackIndex = (ushort)(packs.Count - 1);
        }
        PackInfo packInfo = packs[respawnPackIndex];
        respawnPackIndex++;
        if (respawnPackIndex >= packs.Count)
        {
            respawnPackIndex = 0;
        }
        if (packInfo == null)
        {
            return;
        }
        for (int i = 0; i < packInfo.animals.Count; i++)
        {
            Animal animal = packInfo.animals[i];
            if (animal == null || !animal.isDead || Time.realtimeSinceStartup - animal.lastDead < Provider.modeConfigData.Animals.Respawn_Time)
            {
                return;
            }
        }
        List<AnimalSpawnpoint> list = new List<AnimalSpawnpoint>();
        for (int j = 0; j < packInfo.spawns.Count; j++)
        {
            list.Add(packInfo.spawns[j]);
        }
        for (int k = 0; k < packInfo.animals.Count; k++)
        {
            Animal animal2 = packInfo.animals[k];
            if (!(animal2 == null))
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                AnimalSpawnpoint animalSpawnpoint = list[index];
                list.RemoveAt(index);
                Vector3 point = animalSpawnpoint.point;
                point.y += 0.1f;
                animal2.sendRevive(point, UnityEngine.Random.Range(0f, 360f));
            }
        }
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        seq = 0u;
        _animals = new List<Animal>();
        _packs = null;
        updates = 0;
        tickIndex = 0;
        _tickingAnimals = new List<Animal>();
        if (!Provider.isServer)
        {
            return;
        }
        _packs = new List<PackInfo>();
        if (LevelAnimals.spawns.Count <= 0 || Level.info == null || Level.info.type == ELevelType.ARENA)
        {
            return;
        }
        for (int i = 0; i < LevelAnimals.spawns.Count; i++)
        {
            AnimalSpawnpoint animalSpawnpoint = LevelAnimals.spawns[i];
            int num = -1;
            for (int num2 = packs.Count - 1; num2 >= 0; num2--)
            {
                List<AnimalSpawnpoint> spawns = packs[num2].spawns;
                for (int j = 0; j < spawns.Count; j++)
                {
                    if (!((spawns[j].point - animalSpawnpoint.point).sqrMagnitude < 256f))
                    {
                        continue;
                    }
                    if (num == -1)
                    {
                        spawns.Add(animalSpawnpoint);
                    }
                    else
                    {
                        List<AnimalSpawnpoint> spawns2 = packs[num].spawns;
                        for (int k = 0; k < spawns2.Count; k++)
                        {
                            spawns.Add(spawns2[k]);
                        }
                        packs.RemoveAtFast(num);
                    }
                    num = num2;
                    break;
                }
            }
            if (num == -1)
            {
                PackInfo packInfo = new PackInfo();
                packInfo.spawns.Add(animalSpawnpoint);
                packs.Add(packInfo);
            }
        }
        List<ValidAnimalSpawnsInfo> list = new List<ValidAnimalSpawnsInfo>();
        for (int l = 0; l < packs.Count; l++)
        {
            PackInfo packInfo2 = packs[l];
            List<AnimalSpawnpoint> list2 = new List<AnimalSpawnpoint>();
            for (int m = 0; m < packInfo2.spawns.Count; m++)
            {
                list2.Add(packInfo2.spawns[m]);
            }
            ValidAnimalSpawnsInfo validAnimalSpawnsInfo = new ValidAnimalSpawnsInfo();
            validAnimalSpawnsInfo.spawns = list2;
            validAnimalSpawnsInfo.pack = packInfo2;
            list.Add(validAnimalSpawnsInfo);
        }
        while (animals.Count < maxInstances && list.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            ValidAnimalSpawnsInfo validAnimalSpawnsInfo2 = list[index];
            int index2 = UnityEngine.Random.Range(0, validAnimalSpawnsInfo2.spawns.Count);
            AnimalSpawnpoint animalSpawnpoint2 = validAnimalSpawnsInfo2.spawns[index2];
            validAnimalSpawnsInfo2.spawns.RemoveAt(index2);
            if (validAnimalSpawnsInfo2.spawns.Count == 0)
            {
                list.RemoveAt(index);
            }
            Vector3 point = animalSpawnpoint2.point;
            point.y += 0.1f;
            ushort id = ((validAnimalSpawnsInfo2.pack.animals.Count <= 0) ? LevelAnimals.getAnimal(animalSpawnpoint2) : validAnimalSpawnsInfo2.pack.animals[0].id);
            Animal animal = addAnimal(id, point, UnityEngine.Random.Range(0f, 360f), isDead: false);
            if (animal != null)
            {
                animal.pack = validAnimalSpawnsInfo2.pack;
                validAnimalSpawnsInfo2.pack.animals.Add(animal);
            }
        }
        for (int num3 = packs.Count - 1; num3 >= 0; num3--)
        {
            if (packs[num3].animals.Count <= 0)
            {
                packs.RemoveAt(num3);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (packs == null)
        {
            return;
        }
        for (int i = 0; i < packs.Count; i++)
        {
            PackInfo packInfo = packs[i];
            if (packInfo == null || packInfo.spawns == null || packInfo.animals == null)
            {
                continue;
            }
            Vector3 averageSpawnPoint = packInfo.getAverageSpawnPoint();
            Vector3 averageAnimalPoint = packInfo.getAverageAnimalPoint();
            Vector3 wanderDirection = packInfo.getWanderDirection();
            Gizmos.color = Color.gray;
            for (int j = 0; j < packInfo.spawns.Count; j++)
            {
                AnimalSpawnpoint animalSpawnpoint = packInfo.spawns[j];
                if (animalSpawnpoint != null)
                {
                    Gizmos.DrawLine(averageSpawnPoint, animalSpawnpoint.point);
                }
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(averageSpawnPoint, averageAnimalPoint);
            for (int k = 0; k < packInfo.animals.Count; k++)
            {
                Animal animal = packInfo.animals[k];
                if (!(animal == null))
                {
                    Gizmos.color = (animal.isDead ? Color.red : Color.green);
                    Gizmos.DrawLine(averageAnimalPoint, animal.transform.position);
                    if (!animal.isDead)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(animal.transform.position, animal.target);
                    }
                }
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(averageAnimalPoint, averageAnimalPoint + wanderDirection * 4f);
        }
    }

    private void sendAnimalStates()
    {
        seq++;
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            if (steamPlayer == null || steamPlayer.player == null)
            {
                continue;
            }
            ushort updateCount = 0;
            animalsToSend.Clear();
            for (int j = 0; j < animals.Count; j++)
            {
                Animal animal = animals[j];
                if (!(animal == null) && animal.isUpdated)
                {
                    animalsToSend.Add(animal);
                    updateCount++;
                }
            }
            if (updateCount == 0)
            {
                continue;
            }
            SendAnimalStates.Invoke(ENetReliability.Unreliable, steamPlayer.transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt32(seq);
                writer.WriteUInt16(updateCount);
                foreach (Animal item in animalsToSend)
                {
                    writer.WriteUInt16(item.index);
                    writer.WriteClampedVector3(item.transform.position);
                    writer.WriteDegrees(item.transform.eulerAngles.y);
                }
            });
        }
        for (int k = 0; k < animals.Count; k++)
        {
            Animal animal2 = animals[k];
            if (!(animal2 == null))
            {
                animal2.isUpdated = false;
            }
        }
    }

    private void Update()
    {
        if (!Provider.isServer || !Level.isLoaded || animals == null || animals.Count == 0 || tickingAnimals == null)
        {
            return;
        }
        int num;
        int num2;
        if (Dedicator.IsDedicatedServer)
        {
            if (tickIndex >= tickingAnimals.Count)
            {
                tickIndex = 0;
            }
            num = tickIndex;
            num2 = num + 25;
            if (num2 >= tickingAnimals.Count)
            {
                num2 = tickingAnimals.Count;
            }
            tickIndex = num2;
        }
        else
        {
            num = 0;
            num2 = tickingAnimals.Count;
        }
        for (int num3 = num2 - 1; num3 >= num; num3--)
        {
            Animal animal = tickingAnimals[num3];
            if (animal == null)
            {
                UnturnedLog.error("Missing animal " + num3);
            }
            else
            {
                animal.tick();
            }
        }
        if (Dedicator.IsDedicatedServer && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
        {
            lastTick += Provider.UPDATE_TIME;
            if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
            {
                lastTick = Time.realtimeSinceStartup;
            }
            sendAnimalStates();
        }
        respawnAnimals();
    }

    private void Start()
    {
        manager = this;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Animals: {animals.Count}");
        results.Add($"Animal packs: {packs.Count}");
        results.Add($"Ticking animals: {tickingAnimals.Count}");
    }
}
