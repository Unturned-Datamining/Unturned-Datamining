using System;
using System.IO;
using System.Runtime.InteropServices;
using BattlEye;
using SDG.NetPak;
using Steamworks;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Accepted
{
    /// <summary>
    /// Nelson 2025-06-19: using server-provided connection details is useful because
    /// it can find its public IP (e.g., joining by LAN and sharing WAN IP), and/or
    /// its fake IP (again when joining by LAN).
    /// </summary>
    internal static string RichPresenceConnectionTarget { get; private set; }

    internal static event System.Action OnGameplayConfigReceived;

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForAuthenticationResponse = false;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        bool flag = SteamNetworkingUtils.IsFakeIPv4(value);
        UnturnedLog.info("Accepted by server");
        if (!Provider.IsThirdpartyAntiCheatActiveOnCurrentServer || InitThirdpartyAntiCheat(value, value2, flag))
        {
            RichPresenceConnectionTarget = Provider.server.ToString();
            if (flag)
            {
                IPv4Address pv4Address = new IPv4Address(value);
                RichPresenceConnectionTarget = $"{pv4Address}:{value2}";
                UnturnedLog.info("Rich presence advertisement using Fake IP address (" + RichPresenceConnectionTarget + ")");
            }
            else
            {
                UnturnedLog.info("Rich presence advertisement using server code (" + RichPresenceConnectionTarget + ")");
            }
            if (OptionsSettings.ShouldHideRichPresence)
            {
                SteamFriends.SetRichPresence("connect", "");
            }
            else
            {
                SteamUser.AdvertiseGame(Provider.server, 0u, 0);
                SteamFriends.SetRichPresence("connect", "+connect " + RichPresenceConnectionTarget);
            }
            Lobbies.leaveLobby();
            SteamMatchmaking.AddFavoriteGame(Provider.APP_ID, value, (ushort)(value2 + 1), value2, Provider.STEAM_FAVORITE_FLAG_HISTORY, SteamUtils.GetServerRealTime());
            Provider.updateRichPresence();
            Provider.onClientConnected?.Invoke();
            ClientMessageHandler_Accepted.OnGameplayConfigReceived?.Invoke();
        }
    }

    private static bool InitThirdpartyAntiCheat(uint ip, ushort port, bool isIpFake)
    {
        string text = ReadWrite.PATH + "/BattlEye/BEClient_x64.so";
        if (!File.Exists(text))
        {
            text = ReadWrite.PATH + "/BattlEye/BEClient.so";
        }
        if (!File.Exists(text))
        {
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
            Provider._connectionFailureReason = "Missing BattlEye client library! (" + text + ")";
            UnturnedLog.error(Provider.connectionFailureReason);
            Provider.RequestDisconnect("BattlEye missing");
            return false;
        }
        UnturnedLog.info("Loading BattlEye client library from: " + text);
        try
        {
            Provider.battlEyeClientHandle = BEClient.dlopen(text, 2);
            if (!(Provider.battlEyeClientHandle != IntPtr.Zero))
            {
                Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                Provider._connectionFailureReason = "Failed to load BattlEye client library!";
                UnturnedLog.error(Provider.connectionFailureReason);
                Provider.RequestDisconnect("BattlEye load error");
                return false;
            }
            if (!(Marshal.GetDelegateForFunctionPointer(BEClient.dlsym(Provider.battlEyeClientHandle, "Init"), typeof(BEClient.BEClientInitFn)) is BEClient.BEClientInitFn bEClientInitFn))
            {
                BEClient.dlclose(Provider.battlEyeClientHandle);
                Provider.battlEyeClientHandle = IntPtr.Zero;
                Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                Provider._connectionFailureReason = "Failed to get BattlEye client init delegate!";
                UnturnedLog.error(Provider.connectionFailureReason);
                Provider.RequestDisconnect("BattlEye get init error");
                return false;
            }
            uint ulAddress;
            ushort usPort;
            if (isIpFake)
            {
                ulAddress = 0u;
                usPort = 0;
            }
            else
            {
                ulAddress = ((ip & 0xFF) << 24) | ((ip & 0xFF00) << 8) | ((ip & 0xFF0000) >> 8) | ((ip & 0xFF000000u) >> 24);
                usPort = (ushort)((uint)((port & 0xFF) << 8) | ((uint)(port & 0xFF00) >> 8));
            }
            Provider.battlEyeClientInitData = new BEClient.BECL_GAME_DATA();
            if (Provider._modInfo != null)
            {
                Provider.battlEyeClientInitData.pstrGameVersion = Provider._modInfo.Name + " " + Provider._modInfo.FormatModVersion();
            }
            else
            {
                Provider.battlEyeClientInitData.pstrGameVersion = Provider.APP_NAME + " " + Provider.APP_VERSION;
            }
            Provider.battlEyeClientInitData.ulAddress = ulAddress;
            Provider.battlEyeClientInitData.usPort = usPort;
            Provider.battlEyeClientInitData.pfnPrintMessage = Provider.battlEyeClientPrintMessage;
            Provider.battlEyeClientInitData.pfnRequestRestart = Provider.battlEyeClientRequestRestart;
            Provider.battlEyeClientInitData.pfnSendPacket = Provider.battlEyeClientSendPacket;
            Provider.battlEyeClientRunData = new BEClient.BECL_BE_DATA();
            if (!bEClientInitFn(2, Provider.battlEyeClientInitData, Provider.battlEyeClientRunData))
            {
                BEClient.dlclose(Provider.battlEyeClientHandle);
                Provider.battlEyeClientHandle = IntPtr.Zero;
                Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                Provider._connectionFailureReason = "Failed to call BattlEye client init!";
                UnturnedLog.error(Provider.connectionFailureReason);
                Provider.RequestDisconnect("BattlEye init error");
                return false;
            }
        }
        catch (Exception e)
        {
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
            Provider._connectionFailureReason = "Unhandled exception when loading BattlEye client library!";
            UnturnedLog.error(Provider.connectionFailureReason);
            UnturnedLog.exception(e);
            Provider.RequestDisconnect("BattlEye load exception");
            return false;
        }
        return true;
    }
}
