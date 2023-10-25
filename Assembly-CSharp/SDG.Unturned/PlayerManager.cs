using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerManager : SteamCaller
{
    [Obsolete]
    public static ushort updates;

    private static float lastTick;

    private static uint seq;

    private static double lastReceivePlayerStates;

    private static readonly ClientStaticMethod SendPlayerStates = ClientStaticMethod.Get(ReceivePlayerStates);

    private List<SteamPlayer> playersToSend = new List<SteamPlayer>();

    private float lastSendOverflowWarning;

    /// <summary>
    /// Whether local client is currently penalized for potentially using a lag switch. Server has an equivalent check which reduces
    /// damage dealt, whereas the clientside check stops shooting in order to prevent abuse of inbound-only lagswitches. For example,
    /// if a cheater freezes enemy positions by dropping inbound traffic while still sending movement and shooting outbound traffic.
    /// </summary>
    internal static bool IsClientUnderFakeLagPenalty
    {
        get
        {
            bool flag = false;
            flag |= Provider.isServer;
            flag |= !Provider.isPvP;
            flag |= Provider.clients.Count < 2;
            if (Time.realtimeSinceStartupAsDouble - lastReceivePlayerStates > 2.0)
            {
                return !flag;
            }
            return false;
        }
    }

    [Obsolete]
    public void tellPlayerStates(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceivePlayerStates(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        if (value <= seq)
        {
            return;
        }
        seq = value;
        lastReceivePlayerStates = Time.realtimeSinceStartupAsDouble;
        reader.ReadUInt16(out var value2);
        if (value2 < 1)
        {
            return;
        }
        for (ushort num = 0; num < value2; num++)
        {
            reader.ReadUInt8(out var value3);
            reader.ReadClampedVector3(out var value4);
            reader.ReadUInt8(out var value5);
            reader.ReadUInt8(out var value6);
            SteamPlayer steamPlayer = PlayerTool.findSteamPlayerByChannel(value3);
            if (steamPlayer != null && !(steamPlayer.player == null) && !(steamPlayer.player.movement == null))
            {
                steamPlayer.player.movement.tellState(value4, value5, value6);
            }
        }
    }

    private void onLevelLoaded(int level)
    {
        if (level > Level.BUILD_INDEX_SETUP)
        {
            seq = 0u;
        }
    }

    private void sendPlayerStates()
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
            playersToSend.Clear();
            for (int j = 0; j < Provider.clients.Count; j++)
            {
                if (j != i)
                {
                    SteamPlayer steamPlayer2 = Provider.clients[j];
                    if (steamPlayer2 != null && !(steamPlayer2.player == null) && !(steamPlayer2.player.movement == null) && steamPlayer2.player.movement.updates != null && steamPlayer2.player.movement.updates.Count != 0)
                    {
                        playersToSend.Add(steamPlayer2);
                        updateCount += (ushort)steamPlayer2.player.movement.updates.Count;
                    }
                }
            }
            SendPlayerStates.Invoke(ENetReliability.Unreliable, steamPlayer.transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt32(seq);
                writer.WriteUInt16(updateCount);
                foreach (SteamPlayer item in playersToSend)
                {
                    for (int l = 0; l < item.player.movement.updates.Count; l++)
                    {
                        PlayerStateUpdate playerStateUpdate = item.player.movement.updates[l];
                        writer.WriteUInt8((byte)item.channel);
                        writer.WriteClampedVector3(playerStateUpdate.pos);
                        writer.WriteUInt8(playerStateUpdate.angle);
                        writer.WriteUInt8(playerStateUpdate.rot);
                    }
                }
                if (writer.errors != 0 && Time.realtimeSinceStartup - lastSendOverflowWarning > 1f)
                {
                    lastSendOverflowWarning = Time.realtimeSinceStartup;
                    CommandWindow.LogWarningFormat("Error {0} writing player states. The player count ({1}) is probably too high. No this is not a bug introduced in the update, rather a warning of a previously silent bug.", writer.errors, Provider.clients.Count);
                }
            });
        }
        for (int k = 0; k < Provider.clients.Count; k++)
        {
            SteamPlayer steamPlayer3 = Provider.clients[k];
            if (steamPlayer3 != null && !(steamPlayer3.player == null) && !(steamPlayer3.player.movement == null) && steamPlayer3.player.movement.updates != null && steamPlayer3.player.movement.updates.Count != 0)
            {
                steamPlayer3.player.movement.updates.Clear();
            }
        }
    }

    private void Update()
    {
        if (Provider.isServer && Level.isLoaded && Dedicator.IsDedicatedServer && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
        {
            lastTick += Provider.UPDATE_TIME;
            if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
            {
                lastTick = Time.realtimeSinceStartup;
            }
            sendPlayerStates();
        }
    }

    private void Start()
    {
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Players: {Provider.clients?.Count}");
    }
}
