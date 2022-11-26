using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SDG.Framework.Modules;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal static class ServerMessageHandler_ReadyToConnect
{
    private static List<ulong> pendingPackageSkins = new List<ulong>();

    [Conditional("LOG_CONNECT_ARGS")]
    private static void LogRead(string key, object value)
    {
        UnturnedLog.info("{0} = {1}", key, value);
    }

    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        if (Provider.findPendingPlayer(transportConnection) != null)
        {
            Provider.reject(transportConnection, ESteamRejection.ALREADY_PENDING);
            return;
        }
        if (Provider.findPlayer(transportConnection) != null)
        {
            Provider.reject(transportConnection, ESteamRejection.ALREADY_CONNECTED);
            return;
        }
        reader.ReadUInt8(out var value);
        reader.ReadString(out var value2);
        reader.ReadString(out var value3);
        byte[] array = new byte[20];
        reader.ReadBytes(array, 20);
        byte[] array2 = new byte[20];
        reader.ReadBytes(array2, 20);
        byte[] array3 = new byte[20];
        reader.ReadBytes(array3, 20);
        reader.ReadEnum(out var value4);
        reader.ReadUInt32(out var value5);
        reader.ReadBit(out var value6);
        reader.ReadUInt16(out var value7);
        reader.ReadString(out var value8);
        reader.ReadSteamID(out CSteamID value9);
        reader.ReadUInt8(out var value10);
        reader.ReadUInt8(out var value11);
        reader.ReadUInt8(out var value12);
        reader.ReadColor32RGB(out Color32 value13);
        reader.ReadColor32RGB(out Color32 value14);
        reader.ReadColor32RGB(out Color32 value15);
        reader.ReadBit(out var value16);
        reader.ReadUInt64(out var value17);
        reader.ReadUInt64(out var value18);
        reader.ReadUInt64(out var value19);
        reader.ReadUInt64(out var value20);
        reader.ReadUInt64(out var value21);
        reader.ReadUInt64(out var value22);
        reader.ReadUInt64(out var value23);
        pendingPackageSkins.Clear();
        reader.ReadList(pendingPackageSkins, (SystemNetPakReaderEx.ReadListItem<ulong>)reader.ReadUInt64, Provider.MAX_SKINS_LENGTH);
        reader.ReadEnum(out var value24);
        reader.ReadString(out var value25);
        reader.ReadString(out var value26);
        reader.ReadSteamID(out CSteamID value27);
        reader.ReadUInt32(out var value28);
        reader.ReadUInt8(out var value29);
        if (value29 > 8)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
            return;
        }
        byte[][] array4 = new byte[value29][];
        for (byte b = 0; b < value29; b = (byte)(b + 1))
        {
            array4[b] = new byte[20];
            reader.ReadBytes(array4[b]);
        }
        byte[] array5 = new byte[20];
        reader.ReadBytes(array5, 20);
        reader.ReadSteamID(out CSteamID value30);
        if (Provider.findPendingPlayerBySteamId(value30) != null)
        {
            Provider.reject(transportConnection, ESteamRejection.ALREADY_PENDING);
            return;
        }
        if (PlayerTool.getSteamPlayer(value30) != null)
        {
            Provider.reject(transportConnection, ESteamRejection.ALREADY_CONNECTED);
            return;
        }
        if (!Provider.modeConfigData.Players.Allow_Per_Character_Saves)
        {
            value = 0;
        }
        SteamPlayerID steamPlayerID = new SteamPlayerID(value30, value, value2, value3, value8, value9, array4);
        if (!Provider.canClientVersionJoinServer(value5))
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_VERSION, Provider.APP_VERSION);
            return;
        }
        if (value28 != Level.packedVersion)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_LEVEL_VERSION, Level.version);
            return;
        }
        if (string.IsNullOrWhiteSpace(steamPlayerID.playerName) || NameTool.containsRichText(steamPlayerID.playerName) || steamPlayerID.playerName.ContainsNewLine())
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
            return;
        }
        if (string.IsNullOrWhiteSpace(steamPlayerID.characterName) || NameTool.containsRichText(steamPlayerID.characterName) || steamPlayerID.characterName.ContainsNewLine())
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
            return;
        }
        if (string.IsNullOrWhiteSpace(steamPlayerID.nickName) || NameTool.containsRichText(steamPlayerID.nickName) || steamPlayerID.nickName.ContainsNewLine())
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_INVALID);
            return;
        }
        if (steamPlayerID.playerName.Length < 2)
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_SHORT);
            return;
        }
        if (steamPlayerID.characterName.Length < 2)
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_SHORT);
            return;
        }
        if (steamPlayerID.playerName.Length > 32)
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_LONG);
            return;
        }
        if (steamPlayerID.characterName.Length > 32)
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_LONG);
            return;
        }
        if (steamPlayerID.nickName.Length > 32)
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_LONG);
            return;
        }
        if (long.TryParse(steamPlayerID.playerName, NumberStyles.Any, CultureInfo.InvariantCulture, out var _) || double.TryParse(steamPlayerID.playerName, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_NUMBER);
            return;
        }
        if (long.TryParse(steamPlayerID.characterName, NumberStyles.Any, CultureInfo.InvariantCulture, out var _) || double.TryParse(steamPlayerID.characterName, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_NUMBER);
            return;
        }
        if (long.TryParse(steamPlayerID.nickName, NumberStyles.Any, CultureInfo.InvariantCulture, out var _) || double.TryParse(steamPlayerID.nickName, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_NUMBER);
            return;
        }
        if (Provider.filterName)
        {
            if (!NameTool.isValid(steamPlayerID.playerName))
            {
                Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
                return;
            }
            if (!NameTool.isValid(steamPlayerID.characterName))
            {
                Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
                return;
            }
            if (!NameTool.isValid(steamPlayerID.nickName))
            {
                Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_INVALID);
                return;
            }
        }
        if (steamPlayerID.steamID.m_SteamID != 76561198036822957L && steamPlayerID.steamID.m_SteamID != 76561198267201306L && (value2.Contains("Nelson", StringComparison.InvariantCultureIgnoreCase) || value3.Contains("Nelson", StringComparison.InvariantCultureIgnoreCase)))
        {
            Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
            return;
        }
        transportConnection.TryGetIPv4Address(out var address);
        Provider.checkBanStatus(steamPlayerID, address, out var isBanned, out var banReason, out var banRemainingDuration);
        if (isBanned)
        {
            Provider.notifyBannedInternal(transportConnection, banReason, banRemainingDuration);
            return;
        }
        bool flag = SteamWhitelist.checkWhitelisted(value30);
        if (Provider.isWhitelisted && !flag)
        {
            Provider.reject(transportConnection, ESteamRejection.WHITELISTED);
            return;
        }
        if (Provider.clients.Count + 1 > Provider.maxPlayers && Provider.pending.Count + 1 > Provider.queueSize)
        {
            Provider.reject(transportConnection, ESteamRejection.SERVER_FULL);
            return;
        }
        if (array.Length != 20)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_PASSWORD);
            return;
        }
        if (array2.Length != 20)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_LEVEL);
            return;
        }
        if (array3.Length != 20)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
            return;
        }
        if (Provider.configData.Server.Validate_EconInfo_Hash && !Hash.verifyHash(array5, TempSteamworksEconomy.econInfoHash) && !steamPlayerID.BypassIntegrityChecks)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ECON);
            return;
        }
        ModuleDependency[] array6;
        if (string.IsNullOrEmpty(value25))
        {
            array6 = new ModuleDependency[0];
        }
        else
        {
            string[] array7 = value25.Split(';');
            array6 = new ModuleDependency[array7.Length];
            for (int i = 0; i < array6.Length; i++)
            {
                string[] array8 = array7[i].Split(',');
                if (array8.Length == 2)
                {
                    array6[i] = new ModuleDependency();
                    array6[i].Name = array8[0];
                    uint.TryParse(array8[1], NumberStyles.Any, CultureInfo.InvariantCulture, out array6[i].Version_Internal);
                }
            }
        }
        List<Module> critMods = Provider.critMods;
        Provider.critMods.Clear();
        ModuleHook.getRequiredModules(critMods);
        bool flag2 = true;
        for (int j = 0; j < array6.Length; j++)
        {
            bool flag3 = false;
            if (array6[j] != null)
            {
                for (int k = 0; k < critMods.Count; k++)
                {
                    if (critMods[k] != null && critMods[k].config != null && critMods[k].config.Name == array6[j].Name && critMods[k].config.Version_Internal >= array6[j].Version_Internal)
                    {
                        flag3 = true;
                        break;
                    }
                }
            }
            if (!flag3)
            {
                flag2 = false;
                break;
            }
        }
        if (!flag2)
        {
            Provider.reject(transportConnection, ESteamRejection.CLIENT_MODULE_DESYNC);
            return;
        }
        bool flag4 = true;
        for (int l = 0; l < critMods.Count; l++)
        {
            bool flag5 = false;
            if (critMods[l] != null && critMods[l].config != null)
            {
                for (int m = 0; m < array6.Length; m++)
                {
                    if (array6[m] != null && array6[m].Name == critMods[l].config.Name && array6[m].Version_Internal >= critMods[l].config.Version_Internal)
                    {
                        flag5 = true;
                        break;
                    }
                }
            }
            if (!flag5)
            {
                flag4 = false;
                break;
            }
        }
        if (!flag4)
        {
            Provider.reject(transportConnection, ESteamRejection.SERVER_MODULE_DESYNC);
            return;
        }
        if (!string.IsNullOrEmpty(Provider.serverPassword) && !Hash.verifyHash(array, Provider._serverPasswordHash))
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_PASSWORD);
            return;
        }
        if (!Hash.verifyHash(array2, Level.hash) && !steamPlayerID.BypassIntegrityChecks)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_LEVEL);
            return;
        }
        if (!ReadWrite.IsAssemblyHashValid(array3, value4) && !steamPlayerID.BypassIntegrityChecks)
        {
            Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
            return;
        }
        if (value7 > Provider.configData.Server.Max_Ping_Milliseconds)
        {
            Provider.reject(transportConnection, ESteamRejection.PING, Provider.configData.Server.Max_Ping_Milliseconds.ToString());
            return;
        }
        SteamPending steamPending = new SteamPending(transportConnection, steamPlayerID, value6, value10, value11, value12, value13, value14, value15, value16, value17, value18, value19, value20, value21, value22, value23, pendingPackageSkins.ToArray(), value24, value26, value27, value4);
        byte queuePosition;
        bool flag6;
        if (!Provider.isWhitelisted && flag)
        {
            if (Provider.pending.Count == 0)
            {
                Provider.pending.Add(steamPending);
                queuePosition = 0;
                flag6 = true;
            }
            else
            {
                Provider.pending.Insert(1, steamPending);
                queuePosition = 1;
                flag6 = false;
            }
        }
        else
        {
            queuePosition = MathfEx.ClampToByte(Provider.pending.Count);
            Provider.pending.Add(steamPending);
            flag6 = queuePosition == 0;
        }
        steamPending.lastNotifiedQueuePosition = queuePosition;
        NetMessages.SendMessageToClient(EClientMessage.QueuePositionChanged, ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt8(queuePosition);
        });
        if (flag6)
        {
            Provider.verifyNextPlayerInQueue();
        }
    }
}
