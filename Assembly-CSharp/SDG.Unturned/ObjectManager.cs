using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class ObjectManager : SteamCaller
{
    public const byte SAVEDATA_VERSION_INITIAL = 1;

    public const byte SAVEDATA_VERSION_REPLACE_INDEX_WITH_INSTANCE_ID = 2;

    private const byte SAVEDATA_VERSION_NEWEST = 2;

    public static readonly byte SAVEDATA_VERSION = 2;

    public static readonly byte OBJECT_REGIONS = 2;

    public static DamageObjectRequestHandler onDamageObjectRequested;

    private static ObjectManager manager;

    private static ObjectRegion[,] regions;

    private static byte updateObjects_X;

    private static byte updateObjects_Y;

    private static readonly ClientStaticMethod<byte, byte, ushort, byte, bool, Vector3> SendObjectRubble = ClientStaticMethod<byte, byte, ushort, byte, bool, Vector3>.Get(ReceiveObjectRubble);

    internal static readonly ServerStaticMethod<NetId> SendTalkWithNpcRequest = ServerStaticMethod<NetId>.Get(ReceiveTalkWithNpcRequest);

    private static readonly ServerStaticMethod<byte, byte, ushort> SendUseObjectQuest = ServerStaticMethod<byte, byte, ushort>.Get(ReceiveUseObjectQuest);

    private static readonly ServerStaticMethod<byte, byte, ushort> SendUseObjectDropper = ServerStaticMethod<byte, byte, ushort>.Get(ReceiveUseObjectDropper);

    private static readonly ClientStaticMethod<byte, byte, ushort, ushort> SendObjectResourceState = ClientStaticMethod<byte, byte, ushort, ushort>.Get(ReceiveObjectResourceState);

    private static readonly ClientStaticMethod<byte, byte, ushort, bool> SendObjectBinaryState = ClientStaticMethod<byte, byte, ushort, bool>.Get(ReceiveObjectBinaryState);

    private static readonly ServerStaticMethod<byte, byte, ushort, bool> SendToggleObjectBinaryStateRequest = ServerStaticMethod<byte, byte, ushort, bool>.Get(ReceiveToggleObjectBinaryStateRequest);

    private static readonly ClientStaticMethod<byte, byte> SendClearRegionObjects = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionObjects);

    private static readonly ClientStaticMethod SendObjects = ClientStaticMethod.Get(ReceiveObjects);

    /// <summary>
    /// Invoked when askUseObjectQuest succeeds.
    /// </summary>
    public static event Action<Player, InteractableObject> OnQuestObjectUsed;

    public static void getObjectsInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
    {
        if (LevelObjects.objects == null)
        {
            return;
        }
        for (int i = 0; i < search.Count; i++)
        {
            RegionCoordinate regionCoordinate = search[i];
            if (LevelObjects.objects[regionCoordinate.x, regionCoordinate.y] == null)
            {
                continue;
            }
            for (int j = 0; j < LevelObjects.objects[regionCoordinate.x, regionCoordinate.y].Count; j++)
            {
                LevelObject levelObject = LevelObjects.objects[regionCoordinate.x, regionCoordinate.y][j];
                if (!(levelObject.transform == null) && (levelObject.transform.position - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(levelObject.transform);
                }
            }
        }
    }

    [Obsolete]
    public void tellObjectRubble(CSteamID steamID, byte x, byte y, ushort index, byte section, bool isAlive, Vector3 ragdoll)
    {
        ReceiveObjectRubble(x, y, index, section, isAlive, ragdoll);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellObjectRubble")]
    public static void ReceiveObjectRubble(byte x, byte y, ushort index, byte section, bool isAlive, Vector3 ragdoll)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        if (!Dedicator.IsDedicatedServer)
        {
            ObjectRegion objectRegion = regions[x, y];
            if (objectRegion == null || (!Provider.isServer && !objectRegion.isNetworked))
            {
                return;
            }
        }
        if (LevelObjects.objects == null)
        {
            return;
        }
        List<LevelObject> list = LevelObjects.objects[x, y];
        if (list == null || index >= list.Count)
        {
            return;
        }
        LevelObject levelObject = list[index];
        if (levelObject != null)
        {
            InteractableObjectRubble rubble = levelObject.rubble;
            if (rubble != null)
            {
                rubble.updateRubble(section, isAlive, playEffect: true, ragdoll);
            }
        }
    }

    private static void trackKill()
    {
    }

    public static void damage(Transform obj, Vector3 direction, byte section, float damage, float times, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown, bool trackKill = true)
    {
        kill = EPlayerKill.NONE;
        xp = 0u;
        ushort pendingTotalDamage = (ushort)(damage * times);
        bool shouldAllow = true;
        onDamageObjectRequested?.Invoke(instigatorSteamID, obj, section, ref pendingTotalDamage, ref shouldAllow, damageOrigin);
        if (!shouldAllow || pendingTotalDamage < 1 || !tryGetRegion(obj, out var x, out var y, out var index))
        {
            return;
        }
        LevelObject levelObject = LevelObjects.objects[x, y][index];
        if (levelObject == null || !(levelObject.rubble != null) || !levelObject.canDamageRubble)
        {
            return;
        }
        InteractableObjectRubble rubble = levelObject.rubble;
        if (!rubble.IsSectionIndexValid(section) || rubble.isSectionDead(section))
        {
            return;
        }
        rubble.askDamage(section, pendingTotalDamage);
        if (rubble.isSectionDead(section))
        {
            kill = EPlayerKill.OBJECT;
            if (levelObject.asset != null)
            {
                xp = levelObject.asset.rubbleRewardXP;
            }
            byte[] state = levelObject.state;
            if (section == byte.MaxValue)
            {
                state[^1] = 0;
            }
            else
            {
                state[^1] = (byte)(state[^1] & ~Types.SHIFTS[section]);
            }
            SendObjectRubble.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, section, arg5: false, direction * (int)pendingTotalDamage);
        }
        if (!trackKill || levelObject.asset == null || !rubble.isAllDead())
        {
            return;
        }
        Vector3 position = obj.position;
        LevelNavigation.tryGetBounds(position, out var bound);
        Guid gUID = levelObject.asset.GUID;
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead && (steamPlayer.player.transform.position - position).sqrMagnitude < 90000f)
            {
                steamPlayer.player.quests.trackObjectKill(gUID, bound);
            }
        }
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 5)]
    public static void ReceiveTalkWithNpcRequest(in ServerInvocationContext context, NetId netId)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead)
        {
            return;
        }
        player.quests.ClearActiveNpc();
        IDialogueTarget dialogueTargetFromNetId = InteractableObjectNPC.GetDialogueTargetFromNetId(netId);
        if (dialogueTargetFromNetId != null && !((dialogueTargetFromNetId.GetDialogueTargetWorldPosition() - player.transform.position).sqrMagnitude > 400f) && dialogueTargetFromNetId.ShouldServerApproveDialogueRequest(player))
        {
            DialogueAsset dialogueAsset = dialogueTargetFromNetId.FindStartingDialogueAsset();
            if (dialogueAsset != null)
            {
                player.quests.ApproveTalkWithNpcRequest(dialogueTargetFromNetId, dialogueAsset);
            }
        }
    }

    public static void useObjectQuest(Transform transform)
    {
        if (tryGetRegion(transform, out var x, out var y, out var index))
        {
            SendUseObjectQuest.Invoke(ENetReliability.Reliable, x, y, index);
        }
    }

    [Obsolete]
    public void askUseObjectQuest(CSteamID steamID, byte x, byte y, ushort index)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveUseObjectQuest(in context, x, y, index);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = "askUseObjectQuest")]
    public static void ReceiveUseObjectQuest(in ServerInvocationContext context, byte x, byte y, ushort index)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && index < LevelObjects.objects[x, y].Count && LevelObjects.objects[x, y][index] != null && !(LevelObjects.objects[x, y][index].transform == null) && !((LevelObjects.objects[x, y][index].transform.position - player.transform.position).sqrMagnitude > 1600f))
        {
            InteractableObject interactable = LevelObjects.objects[x, y][index].interactable;
            if (interactable != null && (interactable is InteractableObjectQuest || interactable is InteractableObjectNote) && interactable.objectAsset.areConditionsMet(player) && interactable.objectAsset.areInteractabilityConditionsMet(player))
            {
                interactable.objectAsset.ApplyInteractabilityConditions(player);
                interactable.objectAsset.GrantInteractabilityRewards(player);
                ObjectManager.OnQuestObjectUsed.TryInvoke("OnQuestObjectUsed", player, interactable);
                ((InteractableObjectTriggerableBase)interactable).InvokeUsedEventForModHooks();
            }
        }
    }

    public static void useObjectDropper(Transform transform)
    {
        if (tryGetRegion(transform, out var x, out var y, out var index))
        {
            SendUseObjectDropper.Invoke(ENetReliability.Unreliable, x, y, index);
        }
    }

    [Obsolete]
    public void askUseObjectDropper(CSteamID steamID, byte x, byte y, ushort index)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveUseObjectDropper(in context, x, y, index);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 5, legacyName = "askUseObjectDropper")]
    public static void ReceiveUseObjectDropper(in ServerInvocationContext context, byte x, byte y, ushort index)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && index < LevelObjects.objects[x, y].Count && LevelObjects.objects[x, y][index] != null && !(LevelObjects.objects[x, y][index].transform == null) && !((LevelObjects.objects[x, y][index].transform.position - player.transform.position).sqrMagnitude > 400f))
        {
            InteractableObjectDropper interactableObjectDropper = LevelObjects.objects[x, y][index].interactable as InteractableObjectDropper;
            if (interactableObjectDropper != null && interactableObjectDropper.isUsable && interactableObjectDropper.objectAsset.areConditionsMet(player) && interactableObjectDropper.objectAsset.areInteractabilityConditionsMet(player))
            {
                interactableObjectDropper.objectAsset.ApplyInteractabilityConditions(player);
                interactableObjectDropper.objectAsset.GrantInteractabilityRewards(player);
                interactableObjectDropper.drop();
                interactableObjectDropper.InvokeUsedEventForModHooks();
            }
        }
    }

    [Obsolete]
    public void tellObjectResource(CSteamID steamID, byte x, byte y, ushort index, ushort amount)
    {
        ReceiveObjectResourceState(x, y, index, amount);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellObjectResource")]
    public static void ReceiveObjectResourceState(byte x, byte y, ushort index, ushort amount)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        ObjectRegion objectRegion = regions[x, y];
        if ((Provider.isServer || objectRegion.isNetworked) && index < LevelObjects.objects[x, y].Count)
        {
            InteractableObjectResource interactableObjectResource = LevelObjects.objects[x, y][index].interactable as InteractableObjectResource;
            if (interactableObjectResource != null)
            {
                interactableObjectResource.updateAmount(amount);
            }
        }
    }

    public static void updateObjectResource(Transform transform, ushort amount, bool shouldSend)
    {
        if (tryGetRegion(transform, out var x, out var y, out var index))
        {
            if (shouldSend)
            {
                SendObjectResourceState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, amount);
            }
            byte[] bytes = BitConverter.GetBytes(amount);
            LevelObjects.objects[x, y][index].state[0] = bytes[0];
            LevelObjects.objects[x, y][index].state[1] = bytes[1];
        }
    }

    public static void forceObjectBinaryState(Transform transform, bool isUsed)
    {
        if (tryGetRegion(transform, out var x, out var y, out var index))
        {
            InteractableObjectBinaryState interactableObjectBinaryState = LevelObjects.objects[x, y][index].interactable as InteractableObjectBinaryState;
            if (interactableObjectBinaryState != null && interactableObjectBinaryState.isUsable)
            {
                SendObjectBinaryState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, isUsed);
                LevelObjects.objects[x, y][index].state[0] = (byte)(interactableObjectBinaryState.isUsed ? 1u : 0u);
            }
        }
    }

    public static void toggleObjectBinaryState(Transform transform, bool isUsed)
    {
        if (tryGetRegion(transform, out var x, out var y, out var index))
        {
            SendToggleObjectBinaryStateRequest.Invoke(ENetReliability.Unreliable, x, y, index, isUsed);
        }
    }

    [Obsolete]
    public void tellToggleObjectBinaryState(CSteamID steamID, byte x, byte y, ushort index, bool isUsed)
    {
        ReceiveObjectBinaryState(x, y, index, isUsed);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellToggleObjectBinaryState")]
    public static void ReceiveObjectBinaryState(byte x, byte y, ushort index, bool isUsed)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        if (!Dedicator.IsDedicatedServer)
        {
            ObjectRegion objectRegion = regions[x, y];
            if (!Provider.isServer && !objectRegion.isNetworked)
            {
                return;
            }
        }
        if (index < LevelObjects.objects[x, y].Count)
        {
            InteractableObjectBinaryState interactableObjectBinaryState = LevelObjects.objects[x, y][index].interactable as InteractableObjectBinaryState;
            if (interactableObjectBinaryState != null)
            {
                interactableObjectBinaryState.updateToggle(isUsed);
            }
        }
    }

    [Obsolete]
    public void askToggleObjectBinaryState(CSteamID steamID, byte x, byte y, ushort index, bool isUsed)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveToggleObjectBinaryStateRequest(in context, x, y, index, isUsed);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askToggleObjectBinaryState")]
    public static void ReceiveToggleObjectBinaryStateRequest(in ServerInvocationContext context, byte x, byte y, ushort index, bool isUsed)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && index < LevelObjects.objects[x, y].Count && LevelObjects.objects[x, y][index] != null && !(LevelObjects.objects[x, y][index].transform == null))
        {
            InteractableObjectBinaryState interactableObjectBinaryState = LevelObjects.objects[x, y][index].interactable as InteractableObjectBinaryState;
            if (!(interactableObjectBinaryState == null) && interactableObjectBinaryState.isUsable && interactableObjectBinaryState.isUsed != isUsed && (interactableObjectBinaryState.modHookCounter > 0 || (!((LevelObjects.objects[x, y][index].transform.position - player.transform.position).sqrMagnitude > 400f) && !interactableObjectBinaryState.objectAsset.interactabilityRemote)) && interactableObjectBinaryState.objectAsset.areConditionsMet(player) && interactableObjectBinaryState.objectAsset.areInteractabilityConditionsMet(player))
            {
                interactableObjectBinaryState.objectAsset.ApplyInteractabilityConditions(player);
                interactableObjectBinaryState.objectAsset.GrantInteractabilityRewards(player);
                SendObjectBinaryState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, index, isUsed);
                LevelObjects.objects[x, y][index].state[0] = (byte)(interactableObjectBinaryState.isUsed ? 1u : 0u);
            }
        }
    }

    [Obsolete]
    public void tellClearRegionObjects(CSteamID steamID, byte x, byte y)
    {
        ReceiveClearRegionObjects(x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellClearRegionObjects")]
    public static void ReceiveClearRegionObjects(byte x, byte y)
    {
        if (!Provider.isServer && !regions[x, y].isNetworked)
        {
            return;
        }
        for (int i = 0; i < LevelObjects.objects[x, y].Count; i++)
        {
            LevelObject levelObject = LevelObjects.objects[x, y][i];
            if (levelObject.state != null && levelObject.state.Length != 0)
            {
                levelObject.state = levelObject.asset.getState();
                if (levelObject.interactable != null)
                {
                    levelObject.interactable.updateState(levelObject.asset, levelObject.state);
                }
                if (levelObject.rubble != null)
                {
                    levelObject.rubble.updateState(levelObject.asset, levelObject.state);
                }
            }
        }
    }

    public static void askClearRegionObjects(byte x, byte y)
    {
        if (Provider.isServer && Regions.checkSafe(x, y) && LevelObjects.objects[x, y].Count > 0)
        {
            SendClearRegionObjects.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), x, y);
        }
    }

    public static void askClearAllObjects()
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                askClearRegionObjects(b, b2);
            }
        }
    }

    [Obsolete]
    public void tellObjects(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveObjects(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        if (!Regions.checkSafe(value, value2) || regions[value, value2].isNetworked)
        {
            return;
        }
        regions[value, value2].isNetworked = true;
        ushort value3;
        while (reader.ReadUInt16(out value3) && value3 != ushort.MaxValue)
        {
            reader.ReadUInt8(out var value4);
            byte[] array = new byte[value4];
            reader.ReadBytes(array);
            LevelObject levelObject = LevelObjects.objects[value, value2][value3];
            if (levelObject.interactable != null)
            {
                levelObject.interactable.updateState(levelObject.asset, array);
            }
            if (levelObject.rubble != null)
            {
                levelObject.rubble.updateState(levelObject.asset, array);
            }
        }
    }

    [Obsolete]
    public void askObjects(CSteamID steamID, byte x, byte y)
    {
    }

    internal void askObjects(ITransportConnection transportConnection, byte x, byte y)
    {
        SendObjects.Invoke(ENetReliability.Reliable, transportConnection, SendObjects_Write, x, y);
    }

    private static void SendObjects_Write(NetPakWriter writer, byte x, byte y)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        for (ushort num = 0; num < LevelObjects.objects[x, y].Count; num++)
        {
            LevelObject levelObject = LevelObjects.objects[x, y][num];
            if (levelObject.state != null && levelObject.state.Length != 0)
            {
                writer.WriteUInt16(num);
                byte b = (byte)levelObject.state.Length;
                writer.WriteUInt8(b);
                writer.WriteBytes(levelObject.state, b);
            }
        }
        writer.WriteUInt16(ushort.MaxValue);
    }

    public static LevelObject getObject(byte x, byte y, ushort index)
    {
        if (!Regions.checkSafe(x, y))
        {
            return null;
        }
        List<LevelObject> list = LevelObjects.objects[x, y];
        if (index >= list.Count)
        {
            return null;
        }
        return list[index];
    }

    public static bool tryGetRegion(Transform transform, out byte x, out byte y, out ushort index)
    {
        x = 0;
        y = 0;
        index = 0;
        if (Regions.tryGetCoordinate(transform.position, out x, out y))
        {
            List<LevelObject> list = LevelObjects.objects[x, y];
            for (index = 0; index < list.Count; index++)
            {
                if (transform == list[index].transform)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool updateObjects()
    {
        if (Level.info == null || Level.info.type == ELevelType.ARENA)
        {
            return false;
        }
        if (LevelObjects.objects[updateObjects_X, updateObjects_Y].Count > 0)
        {
            if (regions[updateObjects_X, updateObjects_Y].updateObjectIndex >= LevelObjects.objects[updateObjects_X, updateObjects_Y].Count)
            {
                regions[updateObjects_X, updateObjects_Y].updateObjectIndex = (ushort)(LevelObjects.objects[updateObjects_X, updateObjects_Y].Count - 1);
            }
            LevelObject levelObject = LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex];
            if (levelObject == null || levelObject.asset == null)
            {
                return false;
            }
            if (levelObject.interactable != null && levelObject.asset.interactabilityReset >= 1f)
            {
                if (levelObject.asset.interactability == EObjectInteractability.BINARY_STATE)
                {
                    if (((InteractableObjectBinaryState)levelObject.interactable).checkCanReset(Provider.modeConfigData.Objects.Binary_State_Reset_Multiplier))
                    {
                        SendObjectBinaryState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, arg4: false);
                        LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state[0] = 0;
                    }
                }
                else if ((levelObject.asset.interactability == EObjectInteractability.WATER || levelObject.asset.interactability == EObjectInteractability.FUEL) && ((InteractableObjectResource)levelObject.interactable).checkCanReset((levelObject.asset.interactability == EObjectInteractability.WATER) ? Provider.modeConfigData.Objects.Water_Reset_Multiplier : Provider.modeConfigData.Objects.Fuel_Reset_Multiplier))
                {
                    ushort num = (ushort)Mathf.Min(((InteractableObjectResource)levelObject.interactable).amount + ((levelObject.asset.interactability == EObjectInteractability.WATER) ? 1 : 500), ((InteractableObjectResource)levelObject.interactable).capacity);
                    SendObjectResourceState.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, num);
                    byte[] bytes = BitConverter.GetBytes(num);
                    LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state[0] = bytes[0];
                    LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state[1] = bytes[1];
                }
            }
            if (levelObject.rubble != null && levelObject.asset.rubbleReset >= 1f && levelObject.asset.rubble == EObjectRubble.DESTROY)
            {
                byte b = levelObject.rubble.checkCanReset(Provider.modeConfigData.Objects.Rubble_Reset_Multiplier);
                if (b != byte.MaxValue)
                {
                    byte[] state = LevelObjects.objects[updateObjects_X, updateObjects_Y][regions[updateObjects_X, updateObjects_Y].updateObjectIndex].state;
                    if (levelObject.asset.RubbleRespawnAllSectionsSimultaneously)
                    {
                        state[^1] = byte.MaxValue;
                    }
                    else
                    {
                        state[^1] = (byte)(state[^1] | Types.SHIFTS[b]);
                    }
                    SendObjectRubble.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(updateObjects_X, updateObjects_Y), updateObjects_X, updateObjects_Y, regions[updateObjects_X, updateObjects_Y].updateObjectIndex, b, arg5: true, Vector3.zero);
                }
            }
            return false;
        }
        return true;
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        regions = new ObjectRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                regions[b, b2] = new ObjectRegion();
            }
        }
        updateObjects_X = 0;
        updateObjects_Y = 0;
        if (Provider.isServer)
        {
            load();
        }
    }

    private void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
    {
        if (step == 0)
        {
            for (byte b = 0; b < Regions.WORLD_SIZE; b++)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    if (Provider.isServer)
                    {
                        if (player.movement.loadedRegions[b, b2].isObjectsLoaded && !Regions.checkArea(b, b2, new_x, new_y, OBJECT_REGIONS))
                        {
                            player.movement.loadedRegions[b, b2].isObjectsLoaded = false;
                        }
                    }
                    else if (player.channel.IsLocalPlayer && regions[b, b2].isNetworked && !Regions.checkArea(b, b2, new_x, new_y, OBJECT_REGIONS))
                    {
                        regions[b, b2].isNetworked = false;
                    }
                }
            }
        }
        if (step != 4 || !Dedicator.IsDedicatedServer || !Regions.checkSafe(new_x, new_y))
        {
            return;
        }
        for (int i = new_x - OBJECT_REGIONS; i <= new_x + OBJECT_REGIONS; i++)
        {
            for (int j = new_y - OBJECT_REGIONS; j <= new_y + OBJECT_REGIONS; j++)
            {
                if (Regions.checkSafe((byte)i, (byte)j) && !player.movement.loadedRegions[i, j].isObjectsLoaded)
                {
                    player.movement.loadedRegions[i, j].isObjectsLoaded = true;
                    askObjects(player.channel.owner.transportConnection, (byte)i, (byte)j);
                }
            }
        }
    }

    private void onPlayerCreated(Player player)
    {
        PlayerMovement movement = player.movement;
        movement.onRegionUpdated = (PlayerRegionUpdated)Delegate.Combine(movement.onRegionUpdated, new PlayerRegionUpdated(onRegionUpdated));
    }

    private void Update()
    {
        if (!Level.isLoaded || !Provider.isServer)
        {
            return;
        }
        bool flag = true;
        while (flag)
        {
            flag = updateObjects();
            regions[updateObjects_X, updateObjects_Y].updateObjectIndex++;
            if (regions[updateObjects_X, updateObjects_Y].updateObjectIndex >= LevelObjects.objects[updateObjects_X, updateObjects_Y].Count)
            {
                regions[updateObjects_X, updateObjects_Y].updateObjectIndex = 0;
            }
            updateObjects_X++;
            if (updateObjects_X >= Regions.WORLD_SIZE)
            {
                updateObjects_X = 0;
                updateObjects_Y++;
                if (updateObjects_Y >= Regions.WORLD_SIZE)
                {
                    updateObjects_Y = 0;
                    flag = false;
                }
            }
        }
    }

    private void Start()
    {
        manager = this;
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
    }

    public static void load()
    {
        if (!LevelSavedata.fileExists("/Objects.dat") || Level.info.type != 0)
        {
            return;
        }
        River river = LevelSavedata.openRiver("/Objects.dat", isReading: true);
        if (river.readByte() >= 2)
        {
            for (byte b = 0; b < Regions.WORLD_SIZE; b++)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    LoadRegionV2(river, LevelObjects.objects[b, b2]);
                }
            }
        }
        else
        {
            for (byte b3 = 0; b3 < Regions.WORLD_SIZE; b3++)
            {
                for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4++)
                {
                    loadRegion(river, LevelObjects.objects[b3, b4]);
                }
            }
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = LevelSavedata.openRiver("/Objects.dat", isReading: false);
        river.writeByte(2);
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                saveRegion(river, LevelObjects.objects[b, b2]);
            }
        }
        river.closeRiver();
    }

    private static void ApplyLevelObjectLoadedState(LevelObject obj, byte[] state)
    {
        if (obj.transform == null || obj.asset == null)
        {
            UnturnedLog.warn($"Unable to load savedata for an object with asset GUID {obj.GUID:N} because its asset is missing");
            return;
        }
        if (state.Length < 1)
        {
            UnturnedLog.error("Unable to load savedata for object \"" + obj.asset.FriendlyName + "\" because loaded state array is empty (should never happen)");
            return;
        }
        if (obj.state == null || obj.state.Length < 1)
        {
            UnturnedLog.warn("Unable to load savedata for object \"" + obj.asset.FriendlyName + "\" because it doesn't have a state array");
            return;
        }
        obj.state = state;
        if (obj.interactable != null)
        {
            if (obj.interactable is InteractableObjectBinaryState)
            {
                if (obj.asset.interactabilityReset >= 1f)
                {
                    state[0] = 0;
                }
            }
            else if (obj.interactable is InteractableObjectResource)
            {
                if (obj.asset.rubble == EObjectRubble.DESTROY)
                {
                    if (state.Length < 3)
                    {
                        state = obj.asset.getState();
                        obj.state = state;
                    }
                }
                else if (state.Length < 2)
                {
                    state = obj.asset.getState();
                    obj.state = state;
                }
            }
            obj.interactable.updateState(obj.asset, state);
        }
        if (obj.rubble != null)
        {
            state[^1] = byte.MaxValue;
            obj.rubble.updateState(obj.asset, state);
        }
    }

    private static void LoadRegionV2(River river, List<LevelObject> objects)
    {
        while (true)
        {
            uint num = river.readUInt32();
            LevelObject levelObject;
            switch (num)
            {
            case uint.MaxValue:
                return;
            default:
                levelObject = LevelObjects.FindLevelObjectByInstanceId(num);
                if (levelObject == null)
                {
                    UnturnedLog.warn($"Unable to load savedata for object with instance ID {num} because it's been removed from the map");
                }
                break;
            case 0u:
            {
                ushort num2 = river.readUInt16();
                ushort num3 = river.readUInt16();
                if (num2 >= objects.Count)
                {
                    levelObject = null;
                    UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {num3} because it's been removed from the map");
                    break;
                }
                levelObject = objects[num2];
                if (levelObject != null && levelObject.id != num3)
                {
                    levelObject = null;
                    UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {num3} because the corresponding object in the map has changed");
                }
                break;
            }
            }
            byte[] state = river.readBytes();
            if (levelObject != null)
            {
                ApplyLevelObjectLoadedState(levelObject, state);
            }
        }
    }

    private static void loadRegion(River river, List<LevelObject> objects)
    {
        while (true)
        {
            ushort num = river.readUInt16();
            if (num != ushort.MaxValue)
            {
                ushort num2 = river.readUInt16();
                byte[] state = river.readBytes();
                if (num >= objects.Count)
                {
                    UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {num2} because it's been removed from the map");
                    break;
                }
                LevelObject levelObject = objects[num];
                if (num2 != levelObject.id)
                {
                    UnturnedLog.warn($"Unable to load savedata for object with legacy asset ID {num2} because the corresponding object in the map has changed");
                }
                else
                {
                    ApplyLevelObjectLoadedState(levelObject, state);
                }
                continue;
            }
            break;
        }
    }

    private static void saveRegion(River river, List<LevelObject> objects)
    {
        for (ushort num = 0; num < objects.Count; num++)
        {
            LevelObject levelObject = objects[num];
            if (levelObject.state != null && levelObject.state.Length >= 1)
            {
                river.writeUInt32(levelObject.instanceID);
                if (levelObject.instanceID < 1)
                {
                    river.writeUInt16(num);
                    river.writeUInt16(levelObject.id);
                }
                river.writeBytes(levelObject.state);
            }
        }
        river.writeUInt32(uint.MaxValue);
    }

    public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y)
    {
        return Regions.GatherRemoteClientConnections(x, y, OBJECT_REGIONS);
    }

    [Obsolete("Replaced by GatherRemoteClients")]
    public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y)
    {
        return GatherRemoteClientConnections(x, y);
    }
}
