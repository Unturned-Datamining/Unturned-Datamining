using System;
using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

internal class LocalPlayerBlocklist
{
    private HashSet<CSteamID> voiceChatBlockedPlayers;

    private HashSet<CSteamID> textChatBlockedPlayers;

    private bool isDirty;

    private static LocalPlayerBlocklist instance;

    private const string RELATIVE_PATH = "/Cloud/BlockedPlayers.bin";

    public static void GetBlockStatus(CSteamID playerId, out bool isVoiceChatMuted, out bool isTextChatMuted)
    {
        LocalPlayerBlocklist localPlayerBlocklist = Get();
        isVoiceChatMuted = localPlayerBlocklist.voiceChatBlockedPlayers.Contains(playerId);
        isTextChatMuted = localPlayerBlocklist.textChatBlockedPlayers.Contains(playerId);
    }

    public static void SetVoiceChatMuted(CSteamID playerId, bool isVoiceChatMuted)
    {
        LocalPlayerBlocklist localPlayerBlocklist = Get();
        if (isVoiceChatMuted)
        {
            localPlayerBlocklist.AddVoiceChatBlock(playerId);
        }
        else
        {
            localPlayerBlocklist.RemoveVoiceChatBlock(playerId);
        }
    }

    public static void SetTextChatMuted(CSteamID playerId, bool isTextChatMuted)
    {
        LocalPlayerBlocklist localPlayerBlocklist = Get();
        if (isTextChatMuted)
        {
            localPlayerBlocklist.AddTextChatBlock(playerId);
        }
        else
        {
            localPlayerBlocklist.RemoveTextChatBlock(playerId);
        }
    }

    public static void SaveIfDirty()
    {
        if (instance == null)
        {
            UnturnedLog.info("Skipped saving blocked players");
        }
        else if (instance.isDirty)
        {
            instance.isDirty = false;
            instance.Save();
            UnturnedLog.info("Saved blocked players");
        }
    }

    private static LocalPlayerBlocklist Get()
    {
        if (instance == null)
        {
            Load();
        }
        return instance;
    }

    private static void Load()
    {
        if (ReadWrite.fileExists("/Cloud/BlockedPlayers.bin", useCloud: false, usePath: true))
        {
            try
            {
                instance = new LocalPlayerBlocklist();
                River river = new River("/Cloud/BlockedPlayers.bin", usePath: true, useCloud: false, isReading: true);
                river.readByte();
                int num = river.readInt32();
                instance.voiceChatBlockedPlayers = new HashSet<CSteamID>(num);
                for (int i = 0; i < num; i++)
                {
                    CSteamID item = river.readSteamID();
                    instance.voiceChatBlockedPlayers.Add(item);
                }
                int num2 = river.readInt32();
                instance.textChatBlockedPlayers = new HashSet<CSteamID>(num2);
                for (int j = 0; j < num2; j++)
                {
                    CSteamID item2 = river.readSteamID();
                    instance.textChatBlockedPlayers.Add(item2);
                }
                UnturnedLog.info($"Loaded blocked players (voice: {instance.voiceChatBlockedPlayers.Count} text: {instance.textChatBlockedPlayers.Count})");
                return;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Caught exception loading blocked players:");
                instance = new LocalPlayerBlocklist();
                instance.Reset();
                return;
            }
        }
        instance = new LocalPlayerBlocklist();
        instance.Reset();
    }

    private void Save()
    {
        try
        {
            River river = new River("/Cloud/BlockedPlayers.bin", usePath: true, useCloud: false, isReading: false);
            river.writeByte(1);
            river.writeInt32(voiceChatBlockedPlayers.Count);
            foreach (CSteamID voiceChatBlockedPlayer in voiceChatBlockedPlayers)
            {
                river.writeSteamID(voiceChatBlockedPlayer);
            }
            river.writeInt32(textChatBlockedPlayers.Count);
            foreach (CSteamID textChatBlockedPlayer in textChatBlockedPlayers)
            {
                river.writeSteamID(textChatBlockedPlayer);
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception saving blocked players:");
        }
    }

    private void AddVoiceChatBlock(CSteamID playerId)
    {
        isDirty |= voiceChatBlockedPlayers.Add(playerId);
    }

    private void RemoveVoiceChatBlock(CSteamID playerId)
    {
        isDirty |= voiceChatBlockedPlayers.Remove(playerId);
    }

    private void AddTextChatBlock(CSteamID playerId)
    {
        isDirty |= textChatBlockedPlayers.Add(playerId);
    }

    private void RemoveTextChatBlock(CSteamID playerId)
    {
        isDirty |= textChatBlockedPlayers.Remove(playerId);
    }

    private void Reset()
    {
        voiceChatBlockedPlayers = new HashSet<CSteamID>();
        textChatBlockedPlayers = new HashSet<CSteamID>();
    }
}
